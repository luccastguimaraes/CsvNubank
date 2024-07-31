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
        List<Transaction> transactions = await LoadCsvAsync(caminhoDaPastaOrigemCsv);

        await WriteTransactionsToCsvAsync(caminhoArquivoFinalCsv, transactions);
    }


    private static async Task WriteTransactionsToCsvAsync(string caminhoArquivoFinalCsv, List<Transaction> transactions)
    {
        try
        {
            transactions = transactions.OrderByDescending(t => t.Data).ToList();
            using var fw = new FileStream(caminhoArquivoFinalCsv, FileMode.Create);
            using var sw = new StreamWriter(fw);
            using var csv = new CsvWriter(sw, config);
            csv.Context.RegisterClassMap<TransactionMapWithCategory>();
            csv.WriteHeader<Transaction>();
            csv.NextRecord();
            await csv.WriteRecordsAsync(transactions);
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao escrever as transações no arquivo CSV: {ex.Message}\n", ex);
        }
    }

    private static async Task ProcessCsvFileWithCsvHelperAsync(string arquivo, List<Transaction> transactions)
    {
        try
        {
            using var sr = new StreamReader(arquivo);
            using var csv = new CsvReader(sr, config);
            csv.Read();
            csv.ReadHeader();
            RegisterAppropriateClassMap(csv.Context, csv.HeaderRecord);
            var records = csv.GetRecordsAsync<Transaction>();
            await foreach (var record in records)
            {
                lock (transactions) transactions.Add(record);
            }
        }
        catch (Exception e)
        {
            throw new Exception($"Erro ao processar o arquivo {arquivo}: {e.Message} \n", e);
        }
    }

    private static void RegisterAppropriateClassMap(CsvContext context, string[]? headers)
    {
        if (headers.Contains("Categoria"))
        {
            context.RegisterClassMap<TransactionMapWithCategory>();
        }
        else
        {
            context.RegisterClassMap<TransactionMapWithoutCategory>();
        }
    }

    private static async Task ProcessCsvFileManuallyAsync(string arquivo, List<Transaction> transactions)
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
                //var newTransaction = new Transaction(identificador, date, amount, info);
                //lock (transactions) transactions.Add(newTransaction);
            }
        }
        catch(Exception e)
        {
            throw new Exception($"Erro ao processar o arquivo {arquivo} manualmente", e);
        }
    }
   

    
    public static async Task<List<Transaction>> LoadCsvAsync(string caminhoArquivoFinalCsv)
    {
        try
        {
            List<Transaction> registros = new();
            if (File.Exists(caminhoArquivoFinalCsv)) await ProcessCsvFileWithCsvHelperAsync(caminhoArquivoFinalCsv, registros);
            else await ProcessCsvFilesInDirectoryAsync(caminhoArquivoFinalCsv, registros);
            return registros;
        }
        catch (Exception ex) 
        {
            throw new Exception($"Erro ao carregar as transações do arquivo CSV: {ex.Message} \n", ex);
        }
    }

    private static async Task ProcessCsvFilesInDirectoryAsync(string caminhoArquivoFinalCsv, List<Transaction> registros)
    {
        if (Directory.Exists(caminhoArquivoFinalCsv))
        {
            string[] arquivos = Directory.GetFiles(caminhoArquivoFinalCsv, "*.csv");  // Filtrando apenas arquivos CSV
            var tasks = arquivos.Select(arquivo => ProcessCsvFileWithCsvHelperAsync(arquivo, registros)).ToList();
            await Task.WhenAll(tasks);
        }
        else throw new FileNotFoundException($"Caminho especificado não encontrado: {caminhoArquivoFinalCsv}");
    }
}

public enum Categoria
{
    Despesa,
    Receita
}

public sealed class TransactionMapWithCategory : ClassMap<Transaction>
{
    public TransactionMapWithCategory()
    {
        Map(m => m.Identificador);
        Map(m => m.Data).TypeConverterOption.Format("dd/MM/yyyy");
        Map(m => m.Valor);
        Map(m => m.Categoria).Convert(args =>
        {
            var valor = args.Row.GetField<decimal>("Valor");
            return valor < 0 ? Categoria.Despesa : Categoria.Receita;
        });
        Map(m => m.Descricao);
    }
}

public sealed class TransactionMapWithoutCategory : ClassMap<Transaction>
{
    public TransactionMapWithoutCategory()
    {
        Map(m => m.Identificador);
        Map(m => m.Data).TypeConverterOption.Format("dd/MM/yyyy");
        Map(m => m.Valor);
        Map(m => m.Categoria).Convert(args =>
        {
            var valor = args.Row.GetField<decimal>("Valor");
            return valor < 0 ? Categoria.Despesa : Categoria.Receita;
        });
        Map(m => m.Descricao).Index(3);
    }
}


public readonly record struct Transaction
{
    public string Identificador { get; init; }
    public DateTime Data { get; init; }
    public decimal Valor { get; init; }
    public Categoria Categoria { get; init; }
    public string Descricao { get; init; }

    public Transaction Default() => new()
    {
        Identificador = default,
        Data = default,
        Valor = default,
        Categoria = default,
        Descricao = default
    };

    public Transaction WithValues(string identificador, DateTime data, decimal valor, string descricao) => new()
    {
        Identificador = identificador,
        Data = data,
        Valor = valor,
        Categoria = valor < 0 ? Categoria.Despesa : Categoria.Receita,
        Descricao = descricao
    };
}
