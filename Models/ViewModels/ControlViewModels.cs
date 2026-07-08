namespace Cugger.Models.ViewModels
{
    public class AutocompleteSelectViewModel
    {
        public string FieldName { get; set; } = "";
        public string Endpoint { get; set; } = "";
        public string Placeholder { get; set; } = "Traži...";
        public int? InitialValueId { get; set; }
        public string? InitialLabel { get; set; }
        public int MinChars { get; set; } = 1;
    }

    public class DateTimePickerViewModel
    {
        public string FieldName { get; set; } = "";
        public DateTime? InitialValue { get; set; }
        public bool IncludeTime { get; set; }
        public DateTime? MinDate { get; set; }
        public DateTime? MaxDate { get; set; }
    }

    public class LookupResult
    {
        public int Id { get; set; }
        public string Label { get; set; } = "";
        public string? SubLabel { get; set; }
    }

    /// <summary>Jedan rezultat globalne pretrage (stranica, izbornik ili zapis iz baze).</summary>
    public class GlobalSearchItem
    {
        public string Label { get; set; } = "";
        public string? SubLabel { get; set; }
        public string Url { get; set; } = "";
        public string Icon { get; set; } = "";
    }

    /// <summary>Grupa rezultata globalne pretrage (npr. "Stranice", "Piva", ...).</summary>
    public class GlobalSearchGroup
    {
        public string Name { get; set; } = "";
        public List<GlobalSearchItem> Items { get; set; } = new();
    }
}
