
namespace Fiap.Cloud.Games.Domain.Entities;

public enum TipoMovimentoCarteira
{
    Deposito = 1,
    Retirada = 2
}

public class MovimentoCarteira
{
    public int Id { get; private set; }
    public int UsuarioId { get; private set; }
    public TipoMovimentoCarteira Tipo { get; private set; }
    public decimal Valor { get; private set; }
    public decimal SaldoAntes { get; private set; }
    public decimal SaldoDepois { get; private set; }
    public DateTimeOffset DataHora { get; private set; } = DateTimeOffset.UtcNow;
    public int? JogoId { get; private set; }

    protected MovimentoCarteira() { } // EF

                                      // Construtor para depósitos
    public MovimentoCarteira(int usuarioId, TipoMovimentoCarteira tipo,
                             decimal valor, decimal saldoAntes, decimal saldoDepois)
        : this(usuarioId, tipo, valor, saldoAntes, saldoDepois, null) { }

    // Construtor para compras
    public MovimentoCarteira(int usuarioId, TipoMovimentoCarteira tipo,
                             decimal valor, decimal saldoAntes, decimal saldoDepois,
                             int? jogoId)
    {
        UsuarioId = usuarioId;
        Tipo = tipo;
        Valor = valor;
        SaldoAntes = saldoAntes;
        SaldoDepois = saldoDepois;
        JogoId = jogoId;
        DataHora = DateTimeOffset.UtcNow;
    }
}
