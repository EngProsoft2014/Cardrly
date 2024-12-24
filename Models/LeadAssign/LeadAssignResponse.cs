
namespace Cardrly.Models.LeadAssign
{
    public class LeadAssignResponse
    {
        public string? Id { get; set; } = "";
        public string? LeadId { get; set; } = default!;
        public string? LeadName { get; set; } = default!;
        public string? UserId { get; set; } = default!;
        public string? CardPersonName { get; set; } = default!;
        public bool AllowChangeIsShareToUsers { get; set; } = default!;
        public bool IsShareToUsers { get; set; } = default!;
        public bool IsAdded { get; set; } = default!;

    }
}
