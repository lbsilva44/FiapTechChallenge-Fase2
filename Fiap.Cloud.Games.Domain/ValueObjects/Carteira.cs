
namespace Fiap.Cloud.Games.Domain.ValueObjects;

public class Carteira
{
    public decimal Saldo { get; private set; }
    protected Carteira() { }                       // EF

    public Carteira(decimal saldoInicial)
    {
        if (saldoInicial < 0) throw new ArgumentException("Saldo negativo.");
        Saldo = saldoInicial;
    }
    public void Depositar(decimal v) { if (v <= 0) throw new(); Saldo += v; }
    public void Debitar(decimal v) { if (v <= 0 || v > Saldo) throw new(); Saldo -= v; }
}
