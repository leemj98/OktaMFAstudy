using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebApplication4.Services;

namespace WebApplication4.Controllers
{
    public class AccountController : Controller
    {
        private readonly OktaService _oktaService;

        public AccountController()
        {
            _oktaService = new OktaService();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(string username, string password)
        {
            try
            {
                var authResponse = await _oktaService.AuthenticateUser(username, password);

                if (authResponse.status == "SUCCESS")
                {
                    return RedirectToAction("Index", "Home");
                }
                else if (authResponse.status == "MFA_ENROLL" || authResponse.status == "MFA_REQUIRED")
                {
                    ViewBag.StateToken = authResponse.stateToken;
                    ViewBag.Factors = authResponse._embedded.factors;
                    return View("SelectMfa");
                }
                else
                {
                    ViewBag.ErrorMessage = "로그인 실패: " + (authResponse.errorCauses != null ? authResponse.errorCauses[0].errorSummary : "알 수 없는 오류");
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "로그인 중 오류 발생: " + ex.Message;
            }

            return View();
        }

        public ActionResult SelectMfa()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> SelectMfa(string stateToken, string factorId)
        {
            try
            {
                if (factorId.Contains("sms"))
                {
                    var response = await _oktaService.SendSmsCode(stateToken, factorId);
                    ViewBag.StateToken = stateToken;
                    ViewBag.FactorId = factorId;
                    return View("MfaVerify");
                }
                else if (factorId.Contains("token:software:totp"))
                {
                    var enrollResponse = await _oktaService.EnrollMfa(stateToken, factorId);
                    ViewBag.QrCodeUrl = enrollResponse._embedded.activation._links.qrcode.href;
                    ViewBag.StateToken = stateToken;
                    ViewBag.FactorId = factorId;
                    return View("MfaEnroll");
                }
                else
                {
                    ViewBag.ErrorMessage = "알 수 없는 MFA 방법입니다.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "MFA 선택 중 오류 발생: " + ex.Message;
            }

            return View("Login");
        }

        public ActionResult MfaVerify()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> MfaVerify(string stateToken, string factorId, string passCode)
        {
            try
            {
                var verifyResponse = await _oktaService.VerifyFactor(stateToken, factorId, passCode);

                if (verifyResponse.status == "SUCCESS")
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.ErrorMessage = "MFA 인증 실패: " + verifyResponse.errorCauses[0].errorSummary;
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "MFA 인증 중 오류 발생: " + ex.Message;
            }

            ViewBag.StateToken = stateToken;
            ViewBag.FactorId = factorId;
            return View();
        }

        public ActionResult MfaEnroll()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> MfaEnroll(string stateToken, string factorId)
        {
            try
            {
                var enrollResponse = await _oktaService.EnrollMfa(stateToken, factorId);

                if (enrollResponse.status == "WAITING")
                {
                    ViewBag.QrCodeUrl = enrollResponse._embedded.activation._links.qrcode.href;
                }
                else
                {
                    ViewBag.ErrorMessage = "MFA 등록 실패: " + enrollResponse.errorCauses[0].errorSummary;
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "MFA 등록 중 오류 발생: " + ex.Message;
            }

            ViewBag.StateToken = stateToken;
            ViewBag.FactorId = factorId;
            return View();
        }
    }
}
