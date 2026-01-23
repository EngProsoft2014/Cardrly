namespace Cardrly.Models.TimeSheetBranch
{
    public class CreateTimeSheetBranch
    {
        public string Name { get; set; } = default!;
        public string Address { get; set; } = default!;
        public double Latitude { get; set; } = default!;
        public double Longitude { get; set; } = default!;
    }
}
