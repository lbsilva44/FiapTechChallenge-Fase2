using FluentAssertions;
using Fiap.Cloud.Games.Domain.Entities;
namespace Fiap.Cloud.Games.Tests.Domain;

    public class PromocaoTests
    {
        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public void Ctor_Deve_lancar_se_desconto_fora_de_0_a_100(decimal desconto)
        {
            // Arrange
            var inicio = DateTime.UtcNow;
            var fim = inicio.AddDays(1);

            // Act
            Action act = () => new Promocao("T", "D", desconto, inicio, fim);

            // Assert
            act.Should()
               .Throw<ArgumentOutOfRangeException>()
               .WithParameterName("descontoPercentual");
        }

        [Fact]
        public void Ctor_Deve_lancar_se_dataFim_menor_ou_igual_dataInicio()
        {
            // Arrange
            var inicio = DateTime.UtcNow;
            var fim = inicio;

            // Act
            Action act = () => new Promocao("T", "D", 10m, inicio, fim);

            // Assert
            act.Should()
               .Throw<ArgumentException>()
               .WithMessage("*DataFim deve ser > DataInicio*");
        }

        [Fact]
        public void AdicionarJogo_Deve_armazenar_um_jogo()
        {
            // Arrange
            var promo = new Promocao("T", "D", 10m, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
            var jogoId = 42;

            // Act
            promo.AdicionarJogo(jogoId);

            // Assert
            promo.Jogos.Should().ContainSingle()
                 .Which.JogoId.Should().Be(jogoId);
        }

        [Fact]
        public void Nao_deve_associar_jogo_duplicado()
        {
            // Arrange
            var promo = new Promocao("T", "D", 10m, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
            var jogoId = 42;
            promo.AdicionarJogo(jogoId);

            // Act
            Action act = () => promo.AdicionarJogo(jogoId);

            // Assert
            act.Should()
               .Throw<InvalidOperationException>()
               .WithMessage("Esse jogo já está associado à promoção.");
        }

        [Fact]
        public void Ativar_Deve_marcar_como_ativa_se_nao_expirada()
        {
            // Arrange
            var promo = new Promocao("T", "D", 10m,
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(1));

            // Act
            promo.Ativar();

            // Assert
            promo.Ativa.Should().BeTrue();
        }

        [Fact]
        public void Ativar_Deve_lancar_se_expirada()
        {
            // Arrange
            var promo = new Promocao("T", "D", 10m,
                DateTime.UtcNow.AddDays(-5),
                DateTime.UtcNow.AddDays(-1));

            // Act
            Action act = () => promo.Ativar();

            // Assert
            act.Should()
               .Throw<InvalidOperationException>()
               .WithMessage("Promoção expirada.");
        }
    }

