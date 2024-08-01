using LerCsvNubank;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Drawing;
using LerCsvNubank.Models;

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
        List<Transaction> transactions = await CsvNubank.LoadCsvAsync(pastaCaminho);
        transactions = transactions.OrderBy(t => t.Data).ToList();
        while (true)
        {
            Console.WriteLine("Escolha uma opção:");
            Console.WriteLine("1 - Gerar Relatorio Mensal");
            Console.WriteLine("2 - Gerar Relatorio Anual");
            Console.WriteLine("3 - Gerar Relatorio De Todos os Periodos");
            Console.WriteLine("0 - Sair");
            Console.Write("Opção: ");
            string opcao = Console.ReadLine() ?? "-1";
            Console.Clear();

            Stopwatch stopwatch = new Stopwatch();
            switch (opcao)
            {
                case "1":
                    stopwatch.Start();
                    FilterTransactionsByMonthAndYear(transactions);
                    stopwatch.Stop();
                    Console.WriteLine("Tempo para Gerar Relatorio Mensal: " + stopwatch.Elapsed.TotalMilliseconds + "ms");
                    break;
                case "2":
                    stopwatch.Start();
                    FilterTransactionsByYear(transactions);
                    stopwatch.Stop();
                    Console.WriteLine("Tempo para Gerar Relatorio Anual: " + stopwatch.Elapsed.TotalMilliseconds + "ms");
                    break;
                case "3":
                    stopwatch.Start();
                    AllTransactions(transactions);
                    stopwatch.Stop();
                    Console.WriteLine("Tempo total para Gerar Relatorio De Todos os Periodos: " + stopwatch.Elapsed.TotalMilliseconds + "ms");
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Opção inválida. Tente novamente.");
                    Console.WriteLine("Aperte qualquer tecla para Continuar");
                    Console.ReadKey();
                    break;
            }
            Console.Clear();
        }
    }

    private static void AllTransactions(List<Transaction> transactions)
    {
        Console.Clear();
        var groupedTransactions = transactions.ToList();
        Console.WriteLine($"Relatorio de Todos os Periodos: ");
        Console.WriteLine(new string('-', 10));
        RelatorioMensal relatorio = new
            (
                groupedTransactions.Where(t => t.Categoria == Categoria.Receita).Sum(t => t.Valor),
                groupedTransactions.Where(t => t.Categoria == Categoria.Despesa).Sum(t => t.Valor)
            );
        Console.WriteLine(relatorio);
        Console.WriteLine();
        Console.WriteLine("Aperte qualquer tecla para Continuar");
        Console.ReadKey();

    }

    private static void FilterTransactionsByYear(List<Transaction> transactions)
    {
        Console.Clear();
        var groupedTransactions = transactions
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
            Console.Clear();
            var selectedGroup = groupedTransactions[selectedOption - 1];
            Console.WriteLine(new string('-', 10));
            Console.WriteLine($"Relatorio do Periodo {selectedGroup.Key.Year}");
            RelatorioMensal relatorio = new
                (
                    selectedGroup.Where(t => t.Categoria == Categoria.Receita).Sum(t => t.Valor),
                    selectedGroup.Where(t => t.Categoria == Categoria.Despesa).Sum(t => t.Valor)
                );
            Console.WriteLine(relatorio);
            Console.WriteLine();
            Console.WriteLine("Aperte qualquer tecla para Continuar");
            Console.ReadKey();
        }
        else Console.WriteLine("Opção inválida.");
    }

    private static void FilterTransactionsByMonthAndYear(List<Transaction> transactions)
    {
        Console.Clear();
        var groupedTransactions = transactions
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
            Console.Clear();
            var selectedGroup = groupedTransactions[selectedOption - 1];
            Console.WriteLine(new string('-', 10));
            Console.WriteLine($"Relatorio do Periodo {selectedGroup.Key.Month}/{selectedGroup.Key.Year}");
            RelatorioMensal relatorio = new
                (
                    selectedGroup.Where(t => t.Categoria == Categoria.Receita).Sum(t => t.Valor),
                    selectedGroup.Where(t => t.Categoria == Categoria.Despesa).Sum(t => t.Valor)
                );
            Console.WriteLine(relatorio);
            Console.WriteLine();
            Console.WriteLine("Aperte qualquer tecla para Continuar");
            Console.ReadKey();
        }
        else Console.WriteLine("Opção inválida.");
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
        foreach (Transaction t in transactions)
        {
            Console.WriteLine($"Identificador: {t.Identificador}, Data: {t.Data:dd/MM/yyyy}, Valor: {t.Valor:C2}, Categoria: {t.Categoria}, Descricao: {t.Descricao}");
        }
    }
}
