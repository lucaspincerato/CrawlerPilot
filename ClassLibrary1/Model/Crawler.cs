using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler.Model
{
    internal class Crawler
    {
        public string SeedURL { get; set; }
        private int PageLimit { get; set; }
        private string ArchiveRootPath { get; set; }
        public bool DepthFirst { get; set; }
        public List<QueuedURL> CrawlFrontier { get; set; }
        public List<WebPage> WebPageList { get; set; }

        public Crawler(string SeedURL, int PageLimit, bool DepthFirst)
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

            //Insere primeira página na lista de páginas
            WebPageList.Add(new WebPage(this.SeedURL));

            //Insere URL inicial na CrawlFrontier
            CrawlFrontier.Add(new QueuedURL(this.SeedURL, 0));

            //Atribui o limite de páginas a serem pesquisadas e o modo de busca (Profundidade ou Largura)
            this.PageLimit = PageLimit;
            this.ArchiveRootPath = string.Format(@"C:\Users\lucas_000\Desktop\CodeHome\WebCrawlerPilot\CrawlArchive\{0}\", Controller.Utils.CleanFileName(this.SeedURL) + DateTime.Now.Millisecond);
            Directory.CreateDirectory(ArchiveRootPath);
            this.DepthFirst = DepthFirst;

            //Inicia processo de crawling
            Crawl(this.SeedURL);
        }
        
        private bool CheckInputURL(string URL)
        {
            return true;
        }

        private void Crawl(string URLToCrawl)
        {
            string HTMLString = GetPage(URLToCrawl);

            WebPageList.Add(new WebPage(URLToCrawl, HTMLString, this.ArchiveRootPath + (WebPageList.Count + 1)));

            List<string> URLsFound = ScrapeURLs(HTMLString);

            PrioritizeCrawlFrontier(URLsFound);

            if (CheckConditionsForNextCrawl())
                Crawl(CurrentHighestPriorityQueuedURL().URL);
            else return;
        }

        private string GetPage(string WebPageURL)
        {
            WebClient client = new WebClient();
            String html = client.DownloadString(WebPageURL);

            //teste
            return html;
        }

        private List<string> ScrapeURLs(string HTMLString)
        {
            List<string> URLsFoundList = new List<string>();

            MatchCollection matches = Regex.Matches(HTMLString, @"""http://(.+?)""", RegexOptions.Singleline);

            foreach(Match m in matches)
            {
                URLsFoundList.Add(m.Value.Replace(@"""",""));
            }

            return URLsFoundList;
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
            if (WebPageList.Count < this.PageLimit)
                return true;
            else
                return false;
        }
    }
}
