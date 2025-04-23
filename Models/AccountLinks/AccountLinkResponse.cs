using Cardrly.Enums;
using Cardrly.Helpers;

namespace Cardrly.Models.AccountLinks
{
    public class AccountLinkResponse
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public EnumTypeLink TypeLink { get; set; } = default!;
        public string ImgName { get; set; } = default!;
        public string? UrlImgName { get; set; } = default!;
        public string? UrlImgNameVM { get { return !string.IsNullOrEmpty(UrlImgName) ? Utility.ServerUrl + UrlImgName : ""; } }
        public string Title { get; set; } = default!;
        public bool? Active { get; set; } = default!;
    }
}
