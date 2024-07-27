using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;
namespace LerCsvNubank;

public static class CsvNubank
{
    public static void Start(string caminhoDaPastaOrigemCsv, string caminhoArquivoFinalCsv)
    {
        CultureInfo cultura = new CultureInfo("pt-BR");
        Thread.CurrentThread.CurrentCulture = cultura;
        Thread.CurrentThread.CurrentUICulture = cultura;
        List<Transaction> transactions = new();
        string[] arquivos = Directory.GetFiles(caminhoDaPastaOrigemCsv, "*.csv");
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ",",
            HasHeaderRecord = true
        };
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
        Console.ReadKey();
    }
}

public class Transaction
{
    public DateTime Data { get; set; }
    public decimal Valor { get; set; }
    public Categoria Categoria { get; set; }
    public string Descricao { get; set; }

    public Transaction(DateTime data, decimal valor, string descricao)
    {
        Data = data;
        Valor = valor;
        Descricao = descricao;
        if (valor < 0) Categoria = Categoria.Despesa;
        if (valor > 0) Categoria = Categoria.Receita;
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
