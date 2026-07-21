namespace PetShop_Upgrade.Options
{
    public class GoogleAuthOptions
    {
        public const string SectionName = "GoogleAuth";
        public string ClientId { get; set; } = default!;
        public string ClientSecret { get; set; } = default!;
        public string RedirectUri { get; set; } = default!;
    }
}
