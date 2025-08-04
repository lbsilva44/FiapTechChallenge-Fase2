
namespace Fiap.Cloud.Games.Domain.Entities;

public class Promocao
{
    public int Id { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public decimal DescontoPercentual { get; private set; }               // 0-100
    public DateTime DataInicio { get; private set; }
    public DateTime DataFim { get; private set; }
    public bool Ativa { get; private set; } = false;
    public bool Expirada => DateTime.UtcNow > DataFim;

    public ICollection<PromocaoJogo> Jogos { get; private set; } = new List<PromocaoJogo>();

    protected Promocao() { }

    public Promocao(string titulo, string descricao,
                    decimal descontoPercentual,
                    DateTime dataInicio, DateTime dataFim)
    {
        if (descontoPercentual is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(descontoPercentual));
        if (dataFim <= dataInicio) throw new ArgumentException("DataFim deve ser > DataInicio");

        Titulo = titulo;
        Descricao = descricao;
        DescontoPercentual = descontoPercentual;
        DataInicio = dataInicio;
        DataFim = dataFim;
    }

    public void Ativar()
    {
        if (Expirada)
            throw new InvalidOperationException("Promoção expirada.");
        if (Ativa)
            throw new InvalidOperationException("Promoção já está ativa.");

        Ativa = true;
    }

    public void AdicionarJogo(int jogoId)
    {
        if (Ativa)
            throw new InvalidOperationException("Promoção já ativada; não é possível adicionar novos jogos.");
        if (Expirada)
            throw new InvalidOperationException("Promoção expirada; não é possível adicionar novos jogos.");
        if (Jogos.Any(j => j.JogoId == jogoId))
            throw new InvalidOperationException("Esse jogo já está associado à promoção.");

        Jogos.Add(new PromocaoJogo(Id, jogoId));
    }

    public decimal CalcularPrecoComDesconto(Jogo jogo)
    {
        if (!Ativa) throw new InvalidOperationException("Promoção inativa.");
        if (Expirada) throw new InvalidOperationException("Promoção expirada.");

        return jogo.Preco * (1 - DescontoPercentual / 100m);
    }
}