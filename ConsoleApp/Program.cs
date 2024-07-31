using LerCsvNubank;
using System.Collections.Generic;
using System.Diagnostics;

namespace ConsoleApp;

public class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            await ShowMenuAndProcessTasks();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        Console.ReadKey();
    }




    private static async Task ShowMenuAndProcessTasks()
    {
        string pastaCaminho = @"C:\Users\DELL\Desktop\Arquivos\Csv\Csv_Bruto\";
        string arquivoNu = @"C:\Users\DELL\Desktop\Arquivos\Csv\NU4.csv";
        while (true)
        {
            Console.WriteLine("Escolha uma opção:");
            Console.WriteLine("1 - Processar arquivos do diretorio");
            Console.WriteLine("2 - Processar arquivo NU4.csv");
            Console.WriteLine("3 - Processar ambos os arquivos novamente");
            Console.WriteLine("0 - Sair");
            Console.Write("Opção: ");
            string opcao = Console.ReadLine();
            Console.Clear();

            Stopwatch stopwatch = new Stopwatch();
            switch (opcao)
            {
                case "1":
                    stopwatch.Start();
                    List<Transaction> transactions = await CsvNubank.LoadCsvAsync(pastaCaminho);
                    PrintTransactionsTable(transactions);
                    stopwatch.Stop();
                    Console.WriteLine("Tempo para arquivos do diretorio: " + stopwatch.Elapsed.TotalMilliseconds + "ms");
                    break;
                case "2":
                    stopwatch.Start();
                    List<Transaction> transactions2 = await CsvNubank.LoadCsvAsync(arquivoNu);
                    PrintTransactionsTable(transactions2);
                    stopwatch.Stop();
                    Console.WriteLine("Tempo para arquivo NU4.csv: " + stopwatch.Elapsed.TotalMilliseconds + "ms");
                    break;
                case "3":
                    stopwatch.Start();
                    await CsvNubank.LoadCsvAsync(pastaCaminho);
                    await CsvNubank.LoadCsvAsync(arquivoNu);
                    stopwatch.Stop();
                    Console.WriteLine("Tempo total para ambos os arquivos: " + stopwatch.Elapsed.TotalMilliseconds + "ms");
                    break;
                case "0":
                    return; 
                default:
                    Console.WriteLine("Opção inválida. Tente novamente.");
                    break;
            }

            Console.ReadKey();
        }
    }





    private static void PrintTransactionsTable(List<Transaction> transactions)
    {
        //int infoWidth = transactions.Max(t => t.Descricao.ToString().Length);
        int valorWidth = transactions.Max(t => t.Valor.ToString().Length) + 1;
        int dateWidth = transactions.Max(t => t.Data.ToString("dd/MM/yyyy").Length); 

        string header = $"|{"DATA".PadRight(dateWidth)}|{"VALOR".PadLeft(valorWidth)}|";
        Console.WriteLine(header);
        Console.WriteLine(new string('-', header.Length));
        foreach (Transaction transaction in transactions)
        {
            break;
            string data = transaction.Data.ToString("dd/MM/yyyy").PadRight(dateWidth);
            string valor = transaction.Valor.ToString().PadLeft(valorWidth);
            Console.WriteLine($"|{data}|{valor}|");
        }
        foreach (Transaction transaction in transactions)
        {
            Console.WriteLine(transaction);
        }
    }
}
