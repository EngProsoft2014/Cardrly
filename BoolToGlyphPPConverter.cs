using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly
{
    public class BoolToGlyphPPConverter : IValueConverter
    {
        public string CloseGlyph { get; set; } = "\uf054";   // close
        public string OpenGlyph { get; set; } = "\uf078";  // open
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? OpenGlyph : CloseGlyph;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

    }

}
