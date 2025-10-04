
namespace Cardrly.Models.Lead
{
    public class PagingLstLeadResponse
    {
        public PagingLst<LeadResponse> pagingLst { get; set; }
        public List<LeadCategoryGroupResponse> LeadCategoryGroup { get; set; }
    }
}
