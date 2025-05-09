namespace ArelScoreWebUI.Middleware
{
    public static class VerificationCodeGenerator
    {
        public static string GenerateCode(int length = 6)
        {
            var random = new Random();
            return new string(Enumerable.Range(0, length)
                .Select(_ => random.Next(0, 10).ToString()[0])
                .ToArray());
        }
    }
}
