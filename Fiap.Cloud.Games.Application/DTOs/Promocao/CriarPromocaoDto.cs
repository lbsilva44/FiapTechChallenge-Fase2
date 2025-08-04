
namespace Fiap.Cloud.Games.Application.DTOs.Jogo;

public class CriarPromocaoDto
{
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal DescontoPercentual { get; set; }
    public DateTimeOffset DataInicioUtc { get; set; }
    public DateTimeOffset DataFimUtc { get; set; }
    public List<int> JogosIds { get; set; } = new();
}
