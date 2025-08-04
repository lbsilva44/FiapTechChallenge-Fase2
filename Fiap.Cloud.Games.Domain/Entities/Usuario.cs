using Fiap.Cloud.Games.Domain.ValueObjects;

namespace Fiap.Cloud.Games.Domain.Entities;


public class Usuario
{
    public int Id { get; private set; }
    public Nome Nome { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public Senha Senha { get; private set; } = null!;
    public string Role { get; private set; } = "Usuario";
    public bool Ativo { get; private set; } = true;
    public DateTime DataCadastro { get; private set; } = DateTime.UtcNow;
    public Carteira Carteira { get; private set; } = new Carteira(0);

    private readonly List<MovimentoCarteira> _movimentos = [];
    public IReadOnlyCollection<MovimentoCarteira> Movimentos => _movimentos.AsReadOnly();

    private readonly List<Biblioteca> _bibliotecas = [];
    public IReadOnlyCollection<Biblioteca> Bibliotecas => _bibliotecas.AsReadOnly();

    // Construtor para EF
    protected Usuario() { }

    // Construtor para criação
    public Usuario(string nome, string email, string senha, decimal saldoInicial = 0, string role = "Usuario")
    {
        Nome = new  Nome(nome);
        Email = new Email(email);
        Senha = new Senha(senha);
        Carteira = new Carteira(saldoInicial);
        Role = role;
    }



    // quando comprar um jogo:
    public void Desativar() => Ativo = false;
    public void Ativar() => Ativo = true;
    public void AlterarRole(string novaRole) => Role = novaRole;
    public void AlterarSenha(string novaSenha) => Senha = new Senha(novaSenha);
    public void Depositar(decimal valor)
   {
        var antes = Carteira.Saldo;
        Carteira.Depositar(valor);
        _movimentos.Add(new MovimentoCarteira(Id, TipoMovimentoCarteira.Deposito,
                                              valor, antes, Carteira.Saldo));
    }
    private void Comprar(Jogo jogo, decimal valorASerDebitado)
    {
        if (valorASerDebitado < 0)
            throw new InvalidOperationException("Valor da compra não pode ser negativo.");

        if (PossuiJogo(jogo.Id))
            throw new InvalidOperationException("Usuário já possui este jogo.");

        var antes = Carteira.Saldo;

        if (valorASerDebitado > 0)
        {
            if (!TemSaldoSuficiente(valorASerDebitado))
                throw new InvalidOperationException("Saldo insuficiente.");

            Carteira.Debitar(valorASerDebitado);
        }

        _movimentos.Add(new MovimentoCarteira(
            usuarioId: Id,
            tipo: valorASerDebitado > 0 ? TipoMovimentoCarteira.Retirada : TipoMovimentoCarteira.RecebidoGratis,
            valor: valorASerDebitado,
            saldoAntes: antes,
            saldoDepois: Carteira.Saldo,
            jogoId: jogo.Id
        ));

        _bibliotecas.Add(new Biblioteca(
            usuarioId: Id,
            jogoId: jogo.Id
        ));
    }
    public static Usuario Criar(string nome, string email, string senha, decimal saldoInicial = 0m, string role = "Usuario")
    {
        return new Usuario(nome, email, senha, saldoInicial, role);
    }

    // Compra individual pelo preço original
    public void ComprarJogo(Jogo jogo)=> Comprar(jogo, jogo.Preco);

    public void ComprarPromocao(Jogo jogo, decimal valorPromocional)=> Comprar(jogo, valorPromocional);

    public bool TemSaldoSuficiente(decimal valor)
    {
        return Carteira.Saldo >= valor;
    }

    public bool PossuiJogo(int jogoId)
    {
        return _bibliotecas.Any(b => b.JogoId == jogoId);
    }

    // Compra em lote para promoção
    public void ComprarPromocao(Promocao promocao)
    {
        if (!promocao.Ativa)
            throw new InvalidOperationException("Promoção não está ativa.");

        foreach (var pj in promocao.Jogos)
        {
            var jogo = pj.Jogo ??
                throw new InvalidOperationException("Jogo não carregado na promoção.");

            if (_bibliotecas.Any(b => b.JogoId == jogo.Id))
                continue;

            var precoPromocional = jogo.Preco * (1 - promocao.DescontoPercentual / 100m);
            Comprar(jogo, precoPromocional);
        }
    }
    public void AdicionarBiblioteca(Biblioteca entrada) => _bibliotecas.Add(entrada);
}
