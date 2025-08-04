using Fiap.Cloud.Games.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Fiap.Cloud.Games.Tests.Domain;

public class UsuarioTests
{
    //Teste Entidades
    #region ── Acesso Publico ─────────────────────────────────────────────────────────────
    [Theory]
    [InlineData("Leonardo Silva", "lbsilva44@gmail.com", "Le@156487812335")]
    public void Registrar_Usuario_Dados_Validos(string nome, string email, string senha)
    {
        // Act & Assert (não lança)
        Action act = () => new Usuario(nome, email, senha);
        act.Should().NotThrow();

        // Arrange a instância
        var usuario = new Usuario(nome, email, senha);

        // Verifico o nome
        usuario.Nome.Valor.Should().Be(nome);

        // Verifico o Email (ValueObject)
        usuario.Email.Valor.Should().Be(email);

        // Verifico a Senha:
        // • se o VO expuser .Valor com a string pura:
        usuario.Senha.Valor.Should().Be(senha);
    }

    [Theory]
    [InlineData("", "n@f.com", "Senha@123", "Nome inválido.")]
    [InlineData("Ana", "invalid@", "Senha@123", "E-mail inválido.")]
    [InlineData("Ana", "a@b.com", "1234", "A senha deve ter no mínimo 8 caracteres.")]
    public void Registrar_Usuario_Dados_Invalidos(
    string nome, string email, string senha, string mensagemEsperada)
    {
        // Act
        Action act = () => new Usuario(nome, email, senha);

        // Assert
        act.Should()
           .Throw<ArgumentException>()                // tipo de exceção genérico
           .WithMessage($"{mensagemEsperada}*");                // mensagem que contenha “inválido”
    }
    #endregion
}
