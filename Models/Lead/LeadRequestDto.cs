﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Models.Lead
{
    public class LeadRequestDto
    {
        public string? CardId { get; set; }
        public string? UrlImgProfile { get; set; } = default!;
        public string? PersonName { get; set; } = default!;
        public string? LeadCategoryId { get; set; }
        public string FullName { get; set; } = default!;
        public string? Email { get; set; } = default!;
        public string? Address { get; set; } = default!;
        public string? Phone { get; set; } = default!;
        public string? Company { get; set; } = default!;
        public string? Website { get; set; } = default!;
        public string? JobTitle { get; set; } = default!;
        public byte[]? ImgFile { get; set; }
        public string? Extension { get; set; } = string.Empty;

    }
}
