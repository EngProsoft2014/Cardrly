

namespace Cardrly.Models.CardLink
{
    public class CardLinkRequest
    {
        public string AccountLinkId { get; set; } = default!;
        public int CardLinkType { get; set; } = default!;
        public string ValueOf { get; set; } = default!;
    }
}
