using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;
namespace LerCsvNubank;

public static class CsvNubank
{

    public static CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        Delimiter = ",",
        HasHeaderRecord = true,
        HeaderValidated = null, // Ignora a validação do cabeçalho
        MissingFieldFound = null // Ignora campos faltantes
    };

    public static async Task StartAsync(string caminhoDaPastaOrigemCsv, string caminhoArquivoFinalCsv)
    {
        CultureInfo cultura = new CultureInfo("pt-BR");
        Thread.CurrentThread.CurrentCulture = cultura;
        Thread.CurrentThread.CurrentUICulture = cultura;
        List<Transaction> transactions = new();
        string[] arquivos = Directory.GetFiles(caminhoDaPastaOrigemCsv, "*.csv");

        var tasks = arquivos.Select(arquivo => ProcessCsvFileAsync(arquivo, transactions)).ToList();

        await Task.WhenAll(tasks);

        try
        {
            using var fw = new FileStream(caminhoArquivoFinalCsv, FileMode.Create);
            using var sw = new StreamWriter(fw);
            using var csv = new CsvWriter(sw, config);
            csv.Context.RegisterClassMap<TransactionMap>();
            csv.WriteHeader<Transaction>();
            csv.NextRecord();
            csv.WriteRecords(transactions);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex);
        }
        
    }

    private static async Task ProcessCsvFileAsync(string arquivo, List<Transaction> transactions)
    {
        try
        {
            using var sr = new StreamReader(arquivo);
            var linha = await sr.ReadLineAsync();
            while (!sr.EndOfStream)
            {
                linha = await sr.ReadLineAsync();
                var campos = linha.Split(',');
                var data = campos[0].Trim();
                var valor = campos[1].Trim();
                var identificador = campos[2].Trim();
                var info = campos[3].Trim();
                decimal amount = Decimal.Parse(valor, CultureInfo.InvariantCulture);
                DateTime date = DateTime.Parse(data);
                var newTransaction = new Transaction(identificador, date, amount, info);
                lock (transactions) transactions.Add(newTransaction);
            }
        }
        catch(Exception e)
        {
            Console.WriteLine($"Erro ao processar o arquivo {arquivo}: {e.Message}");
            Console.WriteLine(e);
        }
    }

    public static List<Transaction> LoadCsv(string caminhoArquivoFinalCsv)
    {
        List<Transaction> registros = new();
        try
        {
            using var sr = new StreamReader(caminhoArquivoFinalCsv);
            using var csv = new CsvReader(sr, config);
            csv.Context.RegisterClassMap<TransactionMap>();
            registros = csv.GetRecords<Transaction>().ToList();
        }
        catch (Exception ex) 
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex);
        }
        return registros;

    }

}

public record Transaction(string? Identificador, DateTime Data, decimal Valor, Categoria Categoria, string? Descricao)
{
    public Transaction() : this(default, default, default, default, default) { }
    public Transaction(string identificador, DateTime data, decimal valor, string descricao)
        : this(identificador, data, valor, valor < 0 ? Categoria.Despesa : Categoria.Receita, descricao)
    {

    }

}

public enum Categoria
{
    Despesa,
    Receita
}

public sealed class TransactionMap : ClassMap<Transaction>
{
    public TransactionMap()
    {
        Map(m => m.Identificador);
        Map(m => m.Data).TypeConverterOption.Format("dd/MM/yyyy");
        Map(m => m.Valor);
        Map(m => m.Categoria);
        Map(m => m.Descricao);
    }
}
