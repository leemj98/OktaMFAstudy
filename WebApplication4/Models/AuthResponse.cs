namespace WebApplication4.Models
{
    public class AuthResponse
    {
        public string status { get; set; }
        public string stateToken { get; set; }
        public Embedded _embedded { get; set; }
        public ErrorCause[] errorCauses { get; set; }
    }

    public class Embedded
    {
        public Factor[] factors { get; set; }
        public Activation activation { get; set; }
    }

    public class Factor
    {
        public string id { get; set; }
        public string factorType { get; set; }
    }

    public class Activation
    {
        public Links _links { get; set; }
    }

    public class Links
    {
        public Link qrcode { get; set; }
    }

    public class Link
    {
        public string href { get; set; }
    }

    public class ErrorCause
    {
        public string errorSummary { get; set; }
    }
}
