using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;
using LerCsvNubank.Models;
using System;
using System.Reflection;
namespace LerCsvNubank;

public static class CsvNubank
{
    private static List<Transaction> _registros = new();

    public static CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        Delimiter = ",",
        HasHeaderRecord = true,
        HeaderValidated = null, // Ignora a validação do cabeçalho
        MissingFieldFound = null // Ignora campos faltantes
    };


    public static async Task WriteTransactionsToCsvAsync(string caminhoArquivoFinalCsv)
    {
        try
        {
            _registros = _registros.OrderByDescending(t => t.Data).ToList();
            using var fw = new FileStream(caminhoArquivoFinalCsv, FileMode.Create);
            using var sw = new StreamWriter(fw);
            using var csv = new CsvWriter(sw, config);
            csv.Context.RegisterClassMap<TransactionMapWithCategory>();
            csv.WriteHeader<Transaction>();
            csv.NextRecord();
            await csv.WriteRecordsAsync(_registros);
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao escrever as transações no arquivo CSV: {ex.Message}\n", ex);
        }
    }

    private static async Task ProcessCsvFileWithCsvHelperAsync(string arquivo)
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
                lock (_registros) _registros.Add(record);
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

    private static async Task ProcessCsvFileManuallyAsync(string arquivo)
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
    /*
    private static async Task ProcessCsvFileManuallyAsync2(string arquivo)
    {
        try
        {
            using var sr = new StreamReader(arquivo);
            var linha = await sr.ReadLineAsync();
            var header = linha.Split(',');
            while (!sr.EndOfStream)
            {
                linha = await sr.ReadLineAsync();
                var campos = linha.Split(',');
                // var newTransaction = await MapCsvAsync(header, campos);
                // lock (_registros) _registros.Add(newTransaction);
            }
        }
        catch (Exception e)
        {
            throw new Exception($"Erro ao processar o arquivo {arquivo} manualmente", e);
        }
    }

    private static async Task MapCsvAsync(string[] header, string[] campos)
    {
        foreach (string prop in header)
        {
            PropertyInfo? propInfo = typeof(T).GetProperty(prop);
            if (typeof(T).GetProperties().Contains(propInfo))
            {
                typeof(T).GetProperties();
                typeof(T).GetFields();
            }
            if (propInfo != null)
            {
                var propType = propInfo.PropertyType;

            }

        }
    }

     */
    public static async Task<List<Transaction>> LoadCsvAsync(string caminhoArquivoFinalCsv)
    {
        try
        {
            _registros.Clear();
            if (File.Exists(caminhoArquivoFinalCsv)) await ProcessCsvFileWithCsvHelperAsync(caminhoArquivoFinalCsv);
            else await ProcessCsvFilesInDirectoryAsync(caminhoArquivoFinalCsv);
            return _registros;
        }
        catch (Exception ex) 
        {
            throw new Exception($"Erro ao carregar as transações do arquivo CSV: {ex.Message} \n", ex);
        }
    }

    private static async Task ProcessCsvFilesInDirectoryAsync(string caminhoArquivoFinalCsv)
    {
        if (Directory.Exists(caminhoArquivoFinalCsv))
        {
            string[] arquivos = Directory.GetFiles(caminhoArquivoFinalCsv, "*.csv");  // Filtrando apenas arquivos CSV
            var tasks = arquivos.Select(arquivo => ProcessCsvFileWithCsvHelperAsync(arquivo)).ToList();
            await Task.WhenAll(tasks);
        }
        else throw new FileNotFoundException($"Caminho especificado não encontrado: {caminhoArquivoFinalCsv}");
    }
}
