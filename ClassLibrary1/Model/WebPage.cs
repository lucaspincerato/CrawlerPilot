using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WebCrawler.Model
{
    internal class WebPage
    {

        public string URL { get; set; }
        public string HTMLString { get; set; }
        //public HTMLFile HTMLFile {get;set;}

        internal WebPage(string URL)
        {
            this.URL = URL;
        }

        internal WebPage(string URL, string HTMLString)
        {
            this.URL = URL;
            this.HTMLString = HTMLString;
        }

        internal WebPage(string URL, string HTMLString, string SaveRootPath)
        {
            this.URL = URL;
            this.HTMLString = HTMLString;

            SavePageAsHTMLFile(SaveRootPath);
        }

        private void SavePageAsHTMLFile(string SaveRootPath)
        {
            string cleanSavePath = "";

            try
            {
                cleanSavePath = SaveRootPath + Controller.Utils.CleanFileName(this.URL);
                File.WriteAllText(cleanSavePath + ".html", this.HTMLString);
            }
            catch (PathTooLongException ex)
            {
                cleanSavePath = SaveRootPath + Guid.NewGuid();
                File.WriteAllText(cleanSavePath + ".html", this.HTMLString);
            }

        }
    }
}
