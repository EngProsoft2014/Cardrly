namespace Cardrly.Models.AccountLinks
{
    public class AccountLinkRequest
    {
        public int TypeLink { get; set; } = default!;
        public string? ImgName { get; set; } = default!;
        public byte[]? ImgFile { get; set; }
        public string? Extension { get; set; } = string.Empty;
        public string Title { get; set; } = default!;
    }
}
