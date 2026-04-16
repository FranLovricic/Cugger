namespace Cugger.Models
{
    public class BreadcrumbItem
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public bool IsCurrent { get; set; }

        public BreadcrumbItem(string title, string url, bool isCurrent = false)
        {
            Title = title;
            Url = url;
            IsCurrent = isCurrent;
        }
    }
}
