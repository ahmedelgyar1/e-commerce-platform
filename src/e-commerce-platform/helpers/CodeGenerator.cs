namespace e_commerce_platform.helpers
{
    public static  class CodeGenerator
    {
        public static string Generate6DigitOtp()
        {
            var random = new Random();
            var otp = "";
            for (int i = 0; i < 6; i++)
            {
                otp += random.Next(0, 10).ToString();
            }
            return otp;
        }
    }
}
