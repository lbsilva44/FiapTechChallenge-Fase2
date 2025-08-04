using Fiap.Cloud.Games.Application.DTOs.Jogo;


namespace Fiap.Cloud.Games.Application.DTOs.Usuario;

public class UsuarioDetalhesDto
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime DataCadastro { get; init; }
    public string Role { get; init; } = string.Empty;

    public decimal Saldo { get; init; }

    public List<JogoBibliotecaDto> Biblioteca { get; init; } = [];
    public List<MovimentoCarteiraDto> Movimentos { get; init; } = [];
}