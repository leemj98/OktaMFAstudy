namespace WebApplication4.Models
{
    public class FactorResponse
    {
        public string status { get; set; }
        public string stateToken { get; set; }
        public Embedded _embedded { get; set; }
        public ErrorCause[] errorCauses { get; set; }
    }
}
