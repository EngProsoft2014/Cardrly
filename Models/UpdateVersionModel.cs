using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Models
{
    public class UpdateVersionModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string VersionNumber { get; set; }
        public string VersionBuild { get; set; }
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public DateTime ReleaseDate { get; set; }
    }
}
