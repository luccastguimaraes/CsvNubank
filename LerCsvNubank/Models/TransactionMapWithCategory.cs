using CsvHelper.Configuration;

namespace LerCsvNubank.Models;

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
