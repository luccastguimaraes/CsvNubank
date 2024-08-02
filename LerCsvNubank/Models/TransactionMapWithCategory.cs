using CsvHelper.Configuration;

namespace LerCsvNubank.Models;

public sealed class TransactionMapWithCategory : ClassMap<Transaction>
{
    public TransactionMapWithCategory()
    {
        Map(m => m.Identificador);
        Map(m => m.Data).TypeConverterOption.Format("dd/MM/yyyy");
        Map(m => m.Valor);
        Map(m => m.Categoria);
        Map(m => m.Descricao);
    }
}
