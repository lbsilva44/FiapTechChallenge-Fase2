
namespace Fiap.Cloud.Games.Application.DTOs.Jogo;

public class JogoBibliotecaDto
{
    public int JogoId { get; init; }
    public string Titulo { get; init; } = string.Empty;
    public decimal Preco { get; init; }
    public DateTime DataAquisicao { get; init; }
}