using HtmlAgilityPack;

namespace TesteWebScrapping
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string baseUrl = @"https://www.lopes.com.br";

            var html = @$"{baseUrl}/busca/lancamento/br/sp/sao-paulo/tipo/apartamento?estagio=real_estate_parent&tipo=APARTMENT";

            HtmlWeb web = new HtmlWeb();

            var htmlDoc = web.Load(html);

            //Para cada página, repetir o processo
            var nodePages = htmlDoc.DocumentNode.SelectNodes("//a[@class='page-link ng-star-inserted']");
            List<string> pageLinks = nodePages.GetLinksByList();
            int currentPage = 1;
            List<PropertyInfo> propertyInfos = new List<PropertyInfo>();

            while (true)
            {
                if (currentPage == 1)
                {
                    SetPropertyInfoByPage(web, htmlDoc, propertyInfos, baseUrl);
                }
                else if (IsLastPage(pageLinks, currentPage))
                {
                    string? currentPageLink = pageLinks.SearchPageLink(currentPage);

                    if (currentPageLink == null)
                    {
                        break;
                    }

                    //Atualiza os nodes com a página atual
                    var nextUrl = baseUrl + NormalizeUrl(currentPageLink);
                    var currentPageHtml = web.Load(nextUrl);
                    var nextNodePages = currentPageHtml.DocumentNode.SelectNodes("//a[@class='page-link ng-star-inserted']");
                    pageLinks = nextNodePages.GetLinksByList();

                    if (nextNodePages == null || nextNodePages.Count == 0)
                    {
                        break;
                    }

                    //Atualiza as informações da página atual
                    SetPropertyInfoByPage(web, currentPageHtml, propertyInfos, baseUrl);
                }
                else
                {
                    string? currentPageLink = pageLinks.SearchPageLink(currentPage);

                    if (currentPageLink == null)
                    {
                        break;
                    }

                    var nextUrl = baseUrl + NormalizeUrl(currentPageLink);
                    var nextHtmlDoc = web.Load(nextUrl);
                    SetPropertyInfoByPage(web, nextHtmlDoc, propertyInfos, baseUrl);
                }
                 currentPage++;
            }
        }

        private static bool IsLastPage(List<string> pageLinks, int currentPage)
        {
            int lastPageNumber;

            if (currentPage < 10)
            {
                var numberPosition = pageLinks.Last().IndexOf("?") - 1;
                lastPageNumber = int.Parse(pageLinks.Last()[numberPosition].ToString());
            }
            else
            {
                var numberPosition = pageLinks.Last().IndexOf("?") - 2;
                lastPageNumber = int.Parse(pageLinks.Last().Substring(numberPosition, 2));
            }
            
            return currentPage == lastPageNumber;

        }
        private static string NormalizeUrl(string url)
        {
            string normalizedUrl = url;

            if (url.StartsWith("https"))
            {
                normalizedUrl = url.Substring(url.IndexOf("r") + 1);
            }

            return normalizedUrl;
        }
        private static void SetPropertyInfoByPage(HtmlWeb web, HtmlDocument htmlDoc, List<PropertyInfo> propertyInfos, string baseUrl)
        {
            var node = htmlDoc.DocumentNode.SelectNodes("//a[@class='card__link lead-button ng-star-inserted']");

            List<string> imoveisLinks = node.GetLinksByList();
            //Para cada link, acessa a página do imóvel e pega as informações
            SetPropertyInfo(web, imoveisLinks, propertyInfos, baseUrl);
        }

        private static void SetPropertyInfo(HtmlWeb web, List<string> propertiesLinks, List<PropertyInfo> propertyInfo, string baseUrl)
        {
            foreach (var link in propertiesLinks)
            {
                string PropertyUrl = baseUrl + link;
                var nextHtmlDoc = web.Load(PropertyUrl);
                var PropertyInfoNode = nextHtmlDoc.DocumentNode.SelectSingleNode("//ul[@class='attributes-content layout-name']");
                propertyInfo.Add(new PropertyInfo
                {
                    Area = PropertyInfoNode.ChildNodes.Where(c => c.InnerText.Contains("Área")).FirstOrDefault()?.InnerText.Trim(),
                    Address = nextHtmlDoc.DocumentNode.SelectSingleNode("//p[@class='details__address']")?.InnerText.Trim(),
                    ParkingSpots = PropertyInfoNode.ChildNodes.Where(c => c.InnerText.Contains("Vagas")).FirstOrDefault()?.InnerText.Trim(),
                    Rooms = PropertyInfoNode.ChildNodes.Where(c => c.InnerText.Contains("Dormitórios")).FirstOrDefault()?.InnerText.Trim(),
                });
            }
        }
    }
}
