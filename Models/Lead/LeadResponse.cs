using Cardrly.Helpers;
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
        //public string? Id { get; set; }
        //[DisplayName("Category")]
        //public string? LeadCategoryId { get; set; }
        //public IList<SelectListCategory>? LeadCategories { get; set; }
        //public string FullName { get; set; } = default!;
        //public string? Email { get; set; } = default!;
        //public string? Address { get; set; } = default!;
        //public string? Phone { get; set; } = default!;
        //public string? Company { get; set; } = default!;
        //public string? Website { get; set; } = default!;
        //public string? JobTitle { get; set; } = default!;
        //public string? ImgProfile { get; set; } = default!;
        //public string? UrlImgProfile { get; set; } = default!;
        //public string? UrlImgProfileVM { get { return !string.IsNullOrEmpty(UrlImgProfile) ? Utility.ServerUrl + UrlImgProfile : "usericon.png"; } }
        //public bool? Active { get; set; } = default!;
        //public bool? IsShareToUsers { get; set; } = default!;
        //public DateTime CreatedDate { get; set; }


        public string Id { get; set; }
        public string? CardName { get; set; }

        public string? LeadCategoryId { get; set; }
        public string? LeadCategoryName { get; set; }
        public string FullName { get; set; } = default!;
        public string? Email { get; set; } = default!;
        public string? Address { get; set; } = default!;
        public string? Phone { get; set; } = default!;
        public string? Company { get; set; } = default!;
        public string? Website { get; set; } = default!;
        public string? JobTitle { get; set; } = default!;
        public string? ImgProfile { get; set; } = default!;
        public string? UrlImgProfile { get; set; } = default!;
        public string? UrlImgProfileVM { get { return !string.IsNullOrEmpty(UrlImgProfile) ? Utility.ServerUrl + UrlImgProfile : "usericon.png"; } }
        public bool? Active { get; set; } = default!;
        public DateTime CreatedDate { get; set; }
        public bool? IsShareToUsers { get; set; } = default!;
        public int? LeadCommentCount { get; set; } = default!;
        public bool UnsubscribeEmail { get; set; }
        public bool UnsubscribeSms { get; set; }

        public bool IsNotification { get; set; } = false;

    }
}
