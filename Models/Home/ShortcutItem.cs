using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly.Models.Home
{
    public partial class ShortcutItem : ObservableObject
    {
        public int Id { get; set; }
        [ObservableProperty] 
        string pageName;
        [ObservableProperty] 
        string iconGlyph;
        [ObservableProperty] 
        bool isChecked;
    }

}
