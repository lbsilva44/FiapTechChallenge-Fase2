

namespace Fiap.Cloud.Games.Application.DTOs.Usuario;

public class MovimentoCarteiraDto
{
    public int Id { get; init; }
    public string Tipo { get; init; } = string.Empty;   // "Deposito", "Compra"
    public decimal Valor { get; init; }
    public decimal SaldoAntes { get; init; }
    public decimal SaldoDepois { get; init; }
    public int? JogoId { get; init; }
    public DateTimeOffset DataHora { get; init; }
}
