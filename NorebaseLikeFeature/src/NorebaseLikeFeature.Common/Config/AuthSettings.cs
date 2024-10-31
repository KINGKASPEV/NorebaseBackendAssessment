namespace NorebaseLikeFeature.Common.Config
{
    public class AuthSettings
    {
        public int TokenLifeTimeDays { get; set; }
        public int TokenLifeTimeInHours { get; set; }

        public string SecretKey { get; set; } = String.Empty;

        public string Issuer { get; set; } = String.Empty;

        public string Audience { get; set; } = String.Empty;
    }
}
