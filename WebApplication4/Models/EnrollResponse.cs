namespace WebApplication4.Models
{
    public class EnrollResponse
    {
        public string status { get; set; }
        public Embedded _embedded { get; set; }
        public ErrorCause[] errorCauses { get; set; }
    }
}
