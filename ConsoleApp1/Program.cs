using System;

namespace WebCrawlerInterface
{
    class Program
    {
        static void Main(string[] args)
        {
            MainFunction();
        }

        private static void MainFunction()
        {
            string seedURL;
            bool depth = false;
            bool sameDomainOption = false;

            Console.WriteLine("Por favor, insira a URL inicial do crawl: ");
            seedURL = Console.ReadLine();

            Console.WriteLine("\nPor favor, selecione uma das lógicas de busca abaixo:\n1: Profundidade\n2: Largura");
            int depthSelection = int.Parse(Console.ReadLine());

            if (depthSelection != 1 && depthSelection != 2)
            {
                Console.WriteLine("Opção inválida selecionada!");
                Console.ReadKey();
                return;
            }

            if (depthSelection == 1)
                depth = true;

            
            Console.WriteLine("\nPor favor, selecione uma das lógicas de domínio abaixo:\n1: Mesmo domínio\n2: Número de páginas visitadas");
            int sameDomainOptionSelection = int.Parse(Console.ReadLine());

            if (sameDomainOptionSelection != 1 && sameDomainOptionSelection != 2)
            {
                Console.WriteLine("Opção inválida selecionada!");
                Console.ReadKey();
                return;
            }

            if (sameDomainOptionSelection == 1)
                sameDomainOption = true;

            WebCrawler.UserInterface.CrawlFromSeed(seedURL, depth, sameDomainOption);


            Console.WriteLine("\nProcesso de Crawl finalizado!\n\n");
            Console.ReadKey();

            MainFunction();
        }

    }
}
