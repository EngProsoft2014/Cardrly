using Cardrly.Mode_s.CardLink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Mode_s.Card
{
    public class CardDetailsResponse
    {
        public string Id { get; set; } = default!;
        public string CardName { get; set; } = default!;
        public string? Cardlayout { get; set; } = default!;
        public string? ImgProfile { get; set; } = default!;
        public string? UrlImgProfile { get; set; } = default!;
        public string? ImgCover { get; set; } = default!;
        public string? UrlImgCover { get; set; } = default!;
        public string? PersonName { get; set; } = default!;
        public string? PersonNikeName { get; set; } = default!;
        public string? location { get; set; } = default!;
        public string? JobTitle { get; set; } = default!;
        public string? Bio { get; set; } = default!;
        public string? CardTheme { get; set; } = default!;
        public string? LinkColor { get; set; } = default!;
        public string? FontStyle { get; set; } = default!;
        public bool? Active { get; set; }

        public List<CardLinkResponse>? CardLinks { get; set; }
    }
}
