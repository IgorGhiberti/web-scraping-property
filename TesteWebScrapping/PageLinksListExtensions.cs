
namespace TesteWebScrapping
{
    public static class PageLinksListExtensions
    {
        public static string? SearchPageLink(this List<string> pageLinks, int currentPage)
        {
            return pageLinks.FirstOrDefault(p => p.Contains($"pagina/{currentPage}"));
        }
    }
}
