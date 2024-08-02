namespace LerCsvNubank.Models;

public readonly record struct Relatorio(decimal TotalEntradas, decimal TotalSaidas)
{
    public decimal Entrada => TotalEntradas;
    public decimal Saida => -TotalSaidas;
    public decimal Saldo => TotalEntradas + TotalSaidas;

    public override string? ToString()
    {
        return $"Total de Entradas: {Entrada}\nTotal de Saídas: {Saida}\nSaldo: {Saldo}";
    }
}
