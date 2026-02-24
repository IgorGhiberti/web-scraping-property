using HtmlAgilityPack;

namespace TesteWebScrapping
{
    public static class HtmlNodeCollectionExtensions
    {
        public static List<string> GetLinksByList(this HtmlNodeCollection node)
        {
            List<string> linksList = new List<string>();
            
            foreach (var item in node)
            {
                linksList.Add(item.GetAttributeValue("href", string.Empty));
            }

            return linksList;
        }
    }
}
