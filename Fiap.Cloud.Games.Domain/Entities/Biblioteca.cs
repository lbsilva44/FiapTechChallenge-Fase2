
namespace Fiap.Cloud.Games.Domain.Entities;

public class Biblioteca
{
    public int Id { get; private set; }
    public int UsuarioId { get; private set; }
    public Usuario? Usuario { get; private set; }
    public int JogoId { get; private set; }
    public DateTime DataAquisicao { get; private set; } = DateTime.UtcNow;
    public Jogo Jogo { get; private set; } = null!;

    // Construtor vazio para EF
    protected Biblioteca() { }

    public Biblioteca(int usuarioId, int jogoId)
    {
        UsuarioId = usuarioId;
        JogoId = jogoId;
        DataAquisicao = DateTime.UtcNow;
    }
}