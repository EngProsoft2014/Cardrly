using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cardrly
{
    public class BoolToGlyphConverter : IValueConverter
    {
        public string RecordGlyph { get; set; } = "\uf130";  // microphone
        //public string StopGlyph { get; set; } = "\uf28d";   // stop
        public string PauseGlyph { get; set; } = "\uf28b";   // pause

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? PauseGlyph : RecordGlyph;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
