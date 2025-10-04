
namespace Cardrly.Helpers
{
    public class QueryStringHelper
    {
        public static string ToQueryString(object obj)
        {
            var properties = obj.GetType().GetProperties()
                .Where(p => p.GetValue(obj) != null) // Ignore null properties
                .Select(p => $"{Uri.EscapeDataString(p.Name)}={Uri.EscapeDataString(p.GetValue(obj)!.ToString()!)}");

            return string.Join("&", properties);
        }
    }
}
