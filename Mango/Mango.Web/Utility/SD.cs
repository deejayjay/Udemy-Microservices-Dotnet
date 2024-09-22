namespace Mango.Web.Utility
{
    public class SD
    {
        public enum ApiType
        {
            GET,
            POST,
            PUT,
            DELETE
        }

        public static string CouponApiBase { get; set; }
        public static string AuthApiBase { get; set; }


        public const string RoleAdmin = "ADMIN";
        public const string RoleCustomer = "CUSTOMER";

        public const string TokenCookie = "JWTToken";
    }
}
