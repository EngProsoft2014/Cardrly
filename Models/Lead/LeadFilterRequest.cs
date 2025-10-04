

namespace Cardrly.Models.Lead
{
    public class LeadFilterRequest
    {
        public int? PageNumber { get; set; } = 1;
        public int? Pagesize { get; set; } = 25;
        public string? sortbydir { get; set; } = "A-Z";
        public int? sortby { get; set; } = 2;
        public string? Categorynm { get; set; } = "";
        public string? SearchLead { get; set; } = "";

        public string? rangetype { get; set; } = "None";
        public DateOnly? fromdt { get; set; }
        public DateOnly? todt { get; set; }
        public bool shareto { get; set; } = false;
        public string? CardId { get; set; } = "";

    }
}
