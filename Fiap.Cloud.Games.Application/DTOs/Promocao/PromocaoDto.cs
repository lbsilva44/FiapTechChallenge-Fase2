
namespace Fiap.Cloud.Games.Application.DTOs.Jogo;

public class PromocaoDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal DescontoPercentual { get; set; }
    public DateTime DataInicioUtc { get; set; }
    public DateTime DataFimUtc { get; set; }
    public bool Ativa { get; set; }
    public bool Expirada { get; set; }
    public IEnumerable<JogoDto> Jogos { get; set; } = new List<JogoDto>();
}
