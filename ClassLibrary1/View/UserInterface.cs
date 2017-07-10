using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler
{
    public class UserInterface
    {
        public static void CrawlFromSeed(string seedURL, bool DepthFirst, bool SameDomainOnly)
        {
            Model.Crawler crawler = new Model.Crawler(seedURL,100, DepthFirst, SameDomainOnly);


        }
    }
}
