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
            if (CheckInputURLAndDetectDomain(SeedURL))
                this.SeedURL = SeedURL;
            else
            {
                Logger.Log("URL inserida inválida!");
                return;
            }

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
        
        private bool CheckInputURLAndDetectDomain(string URL)
        {
            try
            {
                this.DomainName = new Uri(URL).Host;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //Método principal de crawling
        private void Crawl(string URLToCrawl)
        {
            Logger.Log("Iniciando processo de crawl da página #" + (this.WebPageList.Count+1));

            //Pega a página
            string HTMLString = GetPage(URLToCrawl);

            //Adiciona a página à lista de páginas
            WebPageList.Add(new WebPage(URLToCrawl, HTMLString, this.ArchiveRootPath));

            //Faz a triagem de novas URLs dentro da página baixada (com filtro)
            List<string> URLsFound = ScrapeURLs(HTMLString, true);

            //Insere as novas URLs na fronteira, de forma priorizada
            PrioritizeCrawlFrontier(URLsFound,this.DepthFirst);
            
            //Loga quantas páginas ainda faltam para  completar o processo
            Logger.Log((CrawlFrontier.Count - WebPageList.Count) + " páginas pendentes\n");

            //Chama o próprio método novamente caso atendidas as condições para continuar
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

        private List<string> ScrapeURLs(string HTMLString, bool Filter)
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
            
            if (Filter)
            {
                URLsFoundList = FilterScrapedURLs(URLsFoundList);

                Logger.Log(URLsFoundList.Count + " restantes após filtro\n"); 
            }

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

            //Filtro de teste pra tirar as páginas do blog stone para fins de teste
            //output = output.Where(x => !x.ToLower().Contains("blog")).ToList();

            return output;
        }

        private void PrioritizeCrawlFrontier(List<string> NewURLs, bool DepthFirst)
        {
            //Adiciona cada URL nova na fronteira de forma priorizada
            //Busca em largura
            if (! DepthFirst)
            {
                foreach (string URL in NewURLs)
                {
                    CrawlFrontier.Add(new QueuedURL(URL, CrawlFrontier.Max(x => x.priority) + 1));
                } 
            }
            //Busca em profundidade
            else
            {
                //Pega a prioridade do nó pai
                double RootScore = this.CurrentHighestPriorityQueuedURL().priority;
                //Pega a prioridad do nó imediatamente mais prioritário
                double RootCeiling = this.CurrentSecondHighestPriorityQueuedURL().priority;

                foreach (string URL in NewURLs)
                {
                    //Divide a diferença entre o score do nó pai e do nó seguinte (com um Epsilon para corrigir) 
                    //pelo número de elementos na nova lista e soma esse adicional ao score de cada um dos elementos
                    double URLPriority = ((RootCeiling - RootScore - double.Epsilon) / NewURLs.Count) * (NewURLs.IndexOf(URL) + 1);
                    
                    CrawlFrontier.Add(new QueuedURL(URL, URLPriority));
                }
            }
            
            //Salva referência da URL de maior prioridade da fronteira
            QueuedURL TopPriorityURL = CurrentHighestPriorityQueuedURL();

            //Joga a URL de maior prioridade para o fim da fila
            TopPriorityURL.priority += CrawlFrontier.Count + 1;
        }

        private QueuedURL CurrentHighestPriorityQueuedURL()
        {
            double minPriority = CrawlFrontier.Min(x => x.priority);
            return CrawlFrontier.Where(x => x.priority == minPriority).First();
        }
        
        private QueuedURL CurrentSecondHighestPriorityQueuedURL()
        {
            double minPriority = CrawlFrontier.Min(x => x.priority);
            if(minPriority!=0)
            {
                double secondMinPriority = CrawlFrontier.Where(x => x.priority != minPriority).Min(x => x.priority);
                return CrawlFrontier.Where(x => x.priority == secondMinPriority).First();
            }
            return new QueuedURL("",1); //Na primeira iteração em que não existe uma segunda URL, retornar score 1
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
