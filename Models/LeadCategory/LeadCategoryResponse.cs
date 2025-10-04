
namespace Cardrly.Models.LeadCategory
{
    public class LeadCategoryResponse
    {
        public string? Id { set; get; }
        public string? Name { set; get; }
        public bool Active { get; set; } = default!;
    }
}
