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

    public static void Start(string caminhoDaPastaOrigemCsv, string caminhoArquivoFinalCsv)
    {
        CultureInfo cultura = new CultureInfo("pt-BR");
        Thread.CurrentThread.CurrentCulture = cultura;
        Thread.CurrentThread.CurrentUICulture = cultura;
        List<Transaction> transactions = new();
        string[] arquivos = Directory.GetFiles(caminhoDaPastaOrigemCsv, "*.csv");

        try
        {
            foreach (var caminho in arquivos)
            {
                using var arquivo = new StreamReader(caminho);
                var linha = arquivo.ReadLine();
                while (!arquivo.EndOfStream)
                {
                    linha = arquivo.ReadLine();
                    var campos = linha.Split(',');
                    var data = campos[0].Trim();
                    var valor = campos[1].Trim();
                    var info = campos[3].Trim();
                    decimal amount = Decimal.Parse(valor, CultureInfo.InvariantCulture);
                    DateTime date = DateTime.Parse(data);

                    transactions.Add(new Transaction(date, amount, info));
                }
            }

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
        Console.WriteLine("Aperte uma tecla para sair");
        Console.ReadKey();
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

public record Transaction(DateTime Data, decimal Valor, Categoria Categoria, string? Descricao)
{
    public Transaction() : this(default, default, default, default) { }
    public Transaction(DateTime data, decimal valor, string descricao)
        : this(data, valor, valor < 0 ? Categoria.Despesa : Categoria.Receita, descricao)
    {

    }

}

public enum Categoria
{
    Despesa,
    Receita
}

public class TransactionMap : ClassMap<Transaction>
{
    public TransactionMap()
    {
        Map(m => m.Data).TypeConverterOption.Format("dd/MM/yyyy");
        Map(m => m.Valor);
        Map(m => m.Categoria);
        Map(m => m.Descricao);
    }
}
