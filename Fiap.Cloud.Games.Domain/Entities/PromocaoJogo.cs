
namespace Fiap.Cloud.Games.Domain.Entities;

public class PromocaoJogo
{
    public int PromocaoId { get; private set; }
    public Promocao? Promocao { get; private set; }

    public int JogoId { get; private set; }
    public Jogo? Jogo { get; private set; }

    protected PromocaoJogo() { }
    public PromocaoJogo(int promocaoId, int jogoId)
    {
        PromocaoId = promocaoId;
        JogoId = jogoId;
    }
}