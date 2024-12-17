using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Mode_s.CardLink
{
    public class CardLinkResponse
    {
        public string Id { get; set; } = default!;
        public string CardId { get; set; } = default!;
        public string CardName { get; set; } = default!;
        public string AccountLinkId { get; set; } = default!;
        public string AccountLinkTitle { get; set; } = default!;
        public string? AccountLinkUrlImgName { get; set; } = default!;
        public string ValueOf { get; set; } = default!;
        public bool? Active { get; set; } = default!;
    }
}
