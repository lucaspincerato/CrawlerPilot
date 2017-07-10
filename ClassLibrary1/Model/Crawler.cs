using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Web;



namespace WebCrawler.Model
{
    internal class Crawler
    {
        public string SeedURL { get; set; }
        public string DomainName {get;set;}
        private int PageLimit { get; set; }
        private string ArchiveRootPath { get; set; }
        public bool DepthFirst { get; set; }
        public bool SameDomainOnly { get; set; }
        public List<QueuedURL> CrawlFrontier { get; set; }
        public List<WebPage> WebPageList { get; set; }

        public Crawler(string SeedURL, int PageLimit, bool DepthFirst, bool SameDomainOnly)
        {
            //Intancia duas listas propriedades
            CrawlFrontier = new List<QueuedURL>();
            WebPageList = new List<WebPage>();

            //Atribui a SeedURL caso passe nas validações
            if (CheckInputURL(SeedURL))
                this.SeedURL = SeedURL;
            else
            {
                Logger.Log("URL inserida inválida!");
                return;
            }

            this.DetectDomain();

            //Insere URL inicial na CrawlFrontier
            CrawlFrontier.Add(new QueuedURL(this.SeedURL, 0));

            //Atribui o limite de páginas a serem pesquisadas e o modo de busca (Profundidade ou Largura)
            this.PageLimit = PageLimit;
            this.ArchiveRootPath = string.Format(@"C:\Users\lucas_000\Desktop\CodeHome\WebCrawlerPilot\CrawlArchive\{0}\", this.DomainName + DateTime.Now.Millisecond);
            Directory.CreateDirectory(ArchiveRootPath);
            this.DepthFirst = DepthFirst;
            this.SameDomainOnly = SameDomainOnly;

            //Inicia processo de crawling
            Crawl(this.SeedURL);
        }

        
        private void DetectDomain()
        {
            this.DomainName = new Uri(this.SeedURL).Host;
        }
        
        private bool CheckInputURL(string URL)
        {
            return true;
        }

        private void Crawl(string URLToCrawl)
        {
            Logger.Log("Iniciando processo de crawl da página #" + (this.WebPageList.Count+1));

            string HTMLString = GetPage(URLToCrawl);

            WebPageList.Add(new WebPage(URLToCrawl, HTMLString, this.ArchiveRootPath));

            List<string> URLsFound = ScrapeURLs(HTMLString);

            PrioritizeCrawlFrontier(URLsFound);
            
            Logger.Log((CrawlFrontier.Count - WebPageList.Count) + " páginas pendentes\n");

            if (CheckConditionsForNextCrawl())
                Crawl(CurrentHighestPriorityQueuedURL().URL);
            else return;
        }

        private string GetPage(string WebPageURL)
        {
            WebClient client = new WebClient();
            String html = "";
                
            try
            {
                html = client.DownloadString(WebPageURL);
                Logger.Log("Página baixada com sucesso :" + WebPageURL + "\n");
            }
            catch(WebException ex)
            {
                Logger.Log("Falha no acesso à página :" + WebPageURL + "\n");
            }
            
            return html;
        }

        private List<string> ScrapeURLs(string HTMLString)
        {
            List<string> URLsFoundList = new List<string>();
            
            MatchCollection matches1 = Regex.Matches(HTMLString, @"""http://(.+?)""", RegexOptions.Singleline);
            MatchCollection matches2 = Regex.Matches(HTMLString, @"""https://(.+?)""", RegexOptions.Singleline);
            
            foreach(Match m in matches1)
            {
                URLsFoundList.Add(m.Value.Replace(@"""",""));
            }
            foreach(Match m in matches2)
            {
                URLsFoundList.Add(m.Value.Replace(@"""",""));
            }
            
            Logger.Log(URLsFoundList.Count + " novas páginas encontradas\n");

            URLsFoundList = FilterScrapedURLs(URLsFoundList);

            Logger.Log(URLsFoundList.Count + " restantes após filtro\n");

            return URLsFoundList;
        }

        private List<string> FilterScrapedURLs(List<string> URLsList)
        {
            List<string> output = URLsList;

            //Filtro de não-repetição
            output = output.Where(x => ! CrawlFrontier.Select(y=>y.URL).Contains(x)).ToList();

            //Filtro opcional de domínio principal
            if(this.SameDomainOnly)
            output = output.Where(x => x.Contains(this.DomainName)).ToList();
            
            return output;
        }

        private void PrioritizeCrawlFrontier(List<string> NewURLs)
        {
            foreach(string URL in NewURLs)
            {
                CrawlFrontier.Add(new QueuedURL(URL, CrawlFrontier.Max(x=>x.priority)+1));
            }
            
            QueuedURL TopPriorityURL = CurrentHighestPriorityQueuedURL();

            TopPriorityURL.priority += CrawlFrontier.Count + 1;
        }

        private QueuedURL CurrentHighestPriorityQueuedURL()
        {
            int minPriority = CrawlFrontier.Min(x => x.priority);
            return CrawlFrontier.Where(x => x.priority == minPriority).First();
        }

        bool CheckConditionsForNextCrawl()
        {
            if (
                //WebPageList.Count < this.PageLimit && 
                UncrawledFrontier())
                return true;
            else
                return false;
        }

        bool UncrawledFrontier()
        {
            foreach(string URL in CrawlFrontier.Select(x => x.URL))
            {
                if(!WebPageList.Select(x => x.URL).Contains(URL))
                    return true;
            }

            return false;
        }
    }
}
