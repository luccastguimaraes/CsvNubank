using LerCsvNubank;
using System.Diagnostics;
using LerCsvNubank.Models;
using System.Collections.Generic;

namespace ConsoleApp;

public class Program
{
    private static List<Transaction> _transactions = new();
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
    }

    private static void Banner()
    {
        Console.Clear();
        Console.WriteLine(@"


    ____        __       ____              __      
   / __ )__  __/ /____  / __ )____ _____  / /__    
  / __  / / / / __/ _ \/ __  / __ `/ __ \/ //_/    
 / /_/ / /_/ / /_/  __/ /_/ / /_/ / / / / ,<       
/_____/\__, /\__/\___/_____/\__,_/_/ /_/_/|_|      
      /____/                                       
                                
        ");
        Console.WriteLine();
    }

    private static async Task ShowMenuAndProcessTasks()
    {
        var loadingTask = CarregarAsync();

        while (true)
        {
            Menu();
            string opcao = Console.ReadLine() ?? "-1";
            if (opcao == "0") return;
            if (!loadingTask.IsCompleted)
            {
                Console.WriteLine("Aguarde, carregando dados...");
                await loadingTask;
            }
            Execute(opcao);
        }
    }

    private static void Execute(string opcao)
    {
        Banner();
        Stopwatch stopwatch = new Stopwatch();
        switch (opcao)
        {
            case "1":
                stopwatch.Restart();
                FilterTransactionsByMonthAndYear();
                stopwatch.Stop();
                Console.WriteLine("Tempo para Gerar Relatorio Mensal: " + stopwatch.Elapsed.TotalMilliseconds + "ms");
                break;
            case "2":
                stopwatch.Restart();
                FilterTransactionsByYear();
                stopwatch.Stop();
                Console.WriteLine("Tempo para Gerar Relatorio Anual: " + stopwatch.Elapsed.TotalMilliseconds + "ms");
                break;
            case "3":
                stopwatch.Restart();
                AllTransactions();
                stopwatch.Stop();
                Console.WriteLine("Tempo total para Gerar Relatorio De Todos os Periodos: " + stopwatch.Elapsed.TotalMilliseconds + "ms");
                break;
            default:
                Console.WriteLine("Opção inválida. Tente novamente.");
                Console.WriteLine("Aperte qualquer tecla para Continuar");
                break;
        }
        Console.ReadKey();
    }

    private static async Task CarregarAsync()
    {
        string caminhoOrigem = @"C:\Users\DELL\Desktop\Arquivos\Csv\Csv_Bruto\";
        //string caminhoFinal = @"C:\Users\DELL\Desktop\Arquivos\Csv\NUBANK.csv";
        //await CsvNubank.StartAsync(caminhoOrigem, caminhoFinal);
        var transactions = await CsvNubank.LoadCsvAsync(caminhoOrigem);
        _transactions = await Task.Run(() => transactions.OrderBy(t => t.Data).ToList());
    }

    private static void Menu()
    {
        Banner();
        Console.WriteLine("Escolha uma opção:");
        Console.WriteLine("1 - Gerar Relatorio Mensal");
        Console.WriteLine("2 - Gerar Relatorio Anual");
        Console.WriteLine("3 - Gerar Relatorio De Todos os Periodos");
        Console.WriteLine("0 - Sair");
        Console.Write("Digite o número da opção desejada: ");
    }

    private static void AllTransactions()
    {
        var groupedTransactions = _transactions.ToList();
        Console.WriteLine($"Relatorio de Todos os Periodos: ");
        Console.WriteLine(new string('-', 10));
        Relatorio relatorio = new
            (
                groupedTransactions.Where(t => t.Categoria == Categoria.Receita).Sum(t => t.Valor),
                groupedTransactions.Where(t => t.Categoria == Categoria.Despesa).Sum(t => t.Valor)
            );
        Console.WriteLine(relatorio);
        Console.WriteLine();
        Console.WriteLine("Aperte qualquer tecla para Continuar");
    }

    private static void FilterTransactionsByYear()
    {
        var groupedTransactions = _transactions
            .GroupBy(t => new { t.Data.Year })
            .ToList();
        Console.WriteLine("Selecione o Ano que deseja Analizar:");
        int menuOption = 1;
        foreach (var group in groupedTransactions)
        {
            Console.WriteLine($"{menuOption++,2} - Ano: {group.Key.Year}");
        }
        Console.WriteLine("0 - Sair");
        Console.Write("Digite sua opção: ");
        if (int.TryParse(Console.ReadLine(), out int selectedOption) && selectedOption <= groupedTransactions.Count && selectedOption >= 0)
        {
            if (selectedOption == 0) return;
            Banner();
            var selectedGroup = groupedTransactions[selectedOption - 1];
            Console.WriteLine(new string('-', 10));
            Console.WriteLine($"Relatorio do Periodo {selectedGroup.Key.Year}");
            Relatorio relatorio = new
                (
                    selectedGroup.Where(t => t.Categoria == Categoria.Receita).Sum(t => t.Valor),
                    selectedGroup.Where(t => t.Categoria == Categoria.Despesa).Sum(t => t.Valor)
                );
            Console.WriteLine(relatorio);
            Console.WriteLine();
            Console.WriteLine("Aperte qualquer tecla para Continuar");
        }
        else Console.WriteLine("Opção inválida.");
    }

    private static void FilterTransactionsByMonthAndYear()
    {
        var groupedTransactions = _transactions
            .GroupBy(t => new { t.Data.Year, t.Data.Month })
            .ToList();
        Console.WriteLine("Selecione um Mes para Analizar:");
        int menuOption = 1;
        foreach (var group in groupedTransactions)
        {
            Console.WriteLine($"{menuOption++,2} - Mês/Ano: {group.Key.Month,2}/{group.Key.Year}");
        }
        Console.WriteLine("0 - Sair");
        Console.Write("Digite sua opção: ");
        if (int.TryParse(Console.ReadLine(), out int selectedOption) && selectedOption <= groupedTransactions.Count && selectedOption >= 0)
        {
            if (selectedOption == 0) return;
            Banner();
            var selectedGroup = groupedTransactions[selectedOption - 1];
            Console.WriteLine(new string('-', 10));
            Console.WriteLine($"Relatorio do Periodo {selectedGroup.Key.Month}/{selectedGroup.Key.Year}");
            Relatorio relatorio = new
                (
                    selectedGroup.Where(t => t.Categoria == Categoria.Receita).Sum(t => t.Valor),
                    selectedGroup.Where(t => t.Categoria == Categoria.Despesa).Sum(t => t.Valor)
                );
            Console.WriteLine(relatorio);
            Console.WriteLine();
            Console.WriteLine("Aperte qualquer tecla para Continuar");
        }
        else Console.WriteLine("Opção inválida.");
    }

    private static void PrintTransactionsTable()
    {
        //int infoWidth = transactions.Max(t => t.Descricao.ToString().Length);
        int valorWidth = _transactions.Max(t => t.Valor.ToString().Length) + 1;
        int dateWidth = _transactions.Max(t => t.Data.ToString("dd/MM/yyyy").Length);

        string header = $"|{"DATA".PadRight(dateWidth)}|{"VALOR".PadLeft(valorWidth)}|";
        Console.WriteLine(header);
        Console.WriteLine(new string('-', header.Length));
        foreach (Transaction transaction in _transactions)
        {
            break;
            string data = transaction.Data.ToString("dd/MM/yyyy").PadRight(dateWidth);
            string valor = transaction.Valor.ToString().PadLeft(valorWidth);
            Console.WriteLine($"|{data}|{valor}|");
        }
        foreach (Transaction t in _transactions)
        {
            Console.WriteLine($"Identificador: {t.Identificador}, Data: {t.Data:dd/MM/yyyy}, Valor: {t.Valor:C2}, Categoria: {t.Categoria}, Descricao: {t.Descricao}");
        }
    }
}
