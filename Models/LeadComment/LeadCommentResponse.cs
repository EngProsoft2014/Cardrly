﻿
using Cardrly.Helpers;

namespace Cardrly.Models.LeadComment
{
    public class LeadCommentResponse
    {
        public string? Id { get; set; }
        public string? LeadId { get; set; } = default!;
        public string? LeadName { get; set; } = default!;
        public string? CardId { get; set; } = default!;
        public string? CardPersonName { get; set; } = default!;
        public string? CardUrlImgProfile { get; set; } = default!;
        public string? CardUrlImgProfileVM { get { return Utility.ServerUrl + CardUrlImgProfile; } }
        public string Comment { get; set; } = default!;
        public bool ActiveDelete { get; set; } = default!;
        public DateTime CreatedDate { get; set; } 
    }
}
