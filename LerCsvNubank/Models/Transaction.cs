namespace LerCsvNubank.Models;

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


public enum Categoria
{
    Despesa,
    Receita
}
