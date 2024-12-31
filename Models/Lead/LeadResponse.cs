using Cardrly.Models.LeadCategory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Models.Lead
{
    public class LeadResponse
    {
        public string? Id { get; set; }
        [DisplayName("Category")]
        public string? LeadCategoryId { get; set; }
        public IList<SelectListCategory>? LeadCategories { get; set; }
        public string FullName { get; set; } = default!;
        public string? Email { get; set; } = default!;
        public string? Address { get; set; } = default!;
        public string? Phone { get; set; } = default!;
        public string? Company { get; set; } = default!;
        public string? Website { get; set; } = default!;
        public string? JobTitle { get; set; } = default!;
        public string? ImgProfile { get; set; } = default!;
        public string? UrlImgProfile { get; set; } = default!;
        public bool Active { get; set; } = default!;
        public bool? IsShareToUsers { get; set; } = default!;


    }
}
