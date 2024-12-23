using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Models.LeadComment
{
    public class LeadCommentResponse
    {
        public string? Id { get; set; }
        public string? LeadId { get; set; } = default!;
        public string? LeadName { get; set; } = default!;
        public string? UserId { get; set; } = default!;
        public string? CardPersonName { get; set; } = default!;
        public string Comment { get; set; } = default!;
        public DateTime CreatedDate { get; set; } = default!;

    }
}
