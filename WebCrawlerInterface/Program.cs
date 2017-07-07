using System;

namespace WebCrawlerInterface
{
    class Program
    {
        static void Main(string[] args)
        {
            string seedURL;
            bool depth = false;

            Console.WriteLine("Por favor, insira a URL inicial do crawl: ");
            seedURL = Console.ReadLine();

            Console.WriteLine("\nPor favor, selecione uma das lógicas de busca abaixo:\n1 - Profundidade\n2-Largura");
            int depthSelection = Console.Read();

            if(depthSelection != 1 && depthSelection != 2)
            {
                Console.WriteLine("Opção inválida selecionada!");
                Console.ReadKey();
                return;
            }

            if (depthSelection == 1)
                depth = true;

            WebCrawler.UserInterface.CrawlFromSeed(seedURL, depth);


            Console.WriteLine("\nProcesso de Crawl finalizado!");
        }
        
    }
}