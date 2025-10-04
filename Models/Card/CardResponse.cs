using Cardrly.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Models.Card
{
    public class CardResponse
    {
        public string Id { get; set; } = default!;
        public string CardName { get; set; } = default!;
        public string? Cardlayout { get; set; } = default!;
        public string? ImgProfile { get; set; } = default!;
        public string? UrlImgProfile { get; set; } = default!;
        public string? UrlImgProfileVM { get { return Utility.ServerUrl + UrlImgProfile; } }
        public string? ImgCover { get; set; } = default!;
        public string? UrlImgCover { get; set; } = default!;
        public string? UrlImgCoverVM { get { return Utility.ServerUrl + UrlImgCover; } }
        public string? PersonName { get; set; } = default!;
        public string? PersonNikeName { get; set; } = default!;
        public string? location { get; set; } = default!;
        public string? JobTitle { get; set; } = default!;
        public string? Bio { get; set; } = default!;
        public string? CardTheme { get; set; } = default!;
        public string? LinkColor { get; set; } = default!;
        public string? FontStyle { get; set; } = default!;
        public string? CardUrl { get; set; } = default!;
        public bool? IsAddLeadFromProfileCard { get; set; }
        public string? CardUrlVM { get { return $"https://app.cardrly.com/profile/{Id}"; } }
        public bool? Active { get; set; }

        public string? CalendarEmail { get; set; } = default!;
        public string? EmbeddedCode { get; set; } = default!;
        public bool ShowCompanyName { get; set; } = false;
    }
}
