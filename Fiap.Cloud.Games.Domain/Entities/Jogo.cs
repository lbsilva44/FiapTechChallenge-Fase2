
using Fiap.Cloud.Games.Domain.Enums;

namespace Fiap.Cloud.Games.Domain.Entities;

public class Jogo
{
    public int Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public decimal Preco { get; private set; }
    public TipoJogo Tipo { get; private set; }
    public bool Publicado { get; private set; } = false;
    public bool Ativo { get; private set; } = true;
    public DateTime DataCadastro { get; private set; } = DateTime.UtcNow;

    protected Jogo() { }

    private readonly List<PromocaoJogo> _promocaoJogos = new();
    public IReadOnlyCollection<PromocaoJogo> PromocaoJogos => _promocaoJogos.AsReadOnly();

    public Jogo(string nome, string descricao, decimal preco, TipoJogo tipo)
    {
        Nome = nome;
        Descricao = descricao;
        Preco = preco;
        Tipo = tipo;
    }

    public void Publicar()
    {
        if (Publicado)
            throw new InvalidOperationException("O jogo já está publicado.");

        if (!Ativo)
            throw new InvalidOperationException("Não é possível publicar um jogo inativo.");

        Publicado = true;
    }

    public void Ativar()
    {
        if (Ativo)
            throw new InvalidOperationException("O jogo já está ativo.");

        Ativo = true;
    }

    public void Desativar()
    {
        if (!Ativo)
            throw new InvalidOperationException("O jogo já está desativado.");

        Ativo = false;
    }
    
    public void ValidarCadastroDuplicado(bool jogoComMesmoNomeAtivo)
    {
        if (jogoComMesmoNomeAtivo)
            throw new InvalidOperationException("Já existe um jogo ativo com o mesmo nome.");
    }

    public void ValidarCompra()
    {
        if (!Publicado || !Ativo)
            throw new InvalidOperationException("O jogo não está disponível para compra.");
    }
}
