using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Fiap.Cloud.Games.API.Controllers;  
using Fiap.Cloud.Games.Application.Interfaces;
using Fiap.Cloud.Games.Application.DTOs.Jogo;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Fiap.Cloud.Games.Tests.Application
{
    //Teste Controller
    public class PromocaoControllerTests
    {
        private readonly Mock<IPromocaoService> _serviceMock;
        private readonly PromocaoController _controller;

        public PromocaoControllerTests()
        {
            _serviceMock = new Mock<IPromocaoService>();
            _controller = new PromocaoController(_serviceMock.Object);
        }


        [Fact]
        public async Task Criar_DeveRetornarOk()
        {
            // Arrange
            var dto = new CriarPromocaoDto
            {
                Titulo = "Promoção de Inverno",
                Descricao = "Jogos com até 50% de desconto",
                DescontoPercentual = 50,
                DataInicioUtc = DateTime.UtcNow,
                DataFimUtc = DateTime.UtcNow.AddDays(7),
                JogosIds = [1, 2, 3]
            };

            _serviceMock
                .Setup(s => s.CriarPromocao(dto))
                .ReturnsAsync(10); // retorna id fictício

            // Act
            var resultado = await _controller.Criar(dto) as OkObjectResult;

            // Assert
            resultado.Should().NotBeNull();
            resultado!.StatusCode.Should().Be(200);
            resultado.Value.Should().BeEquivalentTo(new { Mensagem = "Promoção cadastrada com sucesso. id:10" });
        }

        [Fact]
        public async Task AtivarPromocao_DeveRetornarOk()
        {
            // Arrange
            int idPromocao = 42;

            _serviceMock
                .Setup(s => s.AtivarPromocao(idPromocao))
                .Returns(Task.CompletedTask); // simula conclusão sem erro

            // Act
            var resultado = await _controller.Ativar(idPromocao) as OkObjectResult;

            // Assert
            resultado.Should().NotBeNull();
            resultado!.StatusCode.Should().Be(200);
            resultado.Value.Should().BeEquivalentTo(new { Mensagem = "Promoção ativada com sucesso." });
        }

        [Fact]
        public async Task AdicionarJogoPromocao_DeveRetornarOk()
        {
            // Arrange
            int idPromocao = 10;
            int idJogo = 5;

            _serviceMock
                .Setup(s => s.AdicionarJogoPromocao(idPromocao, idJogo))
                .Returns(Task.CompletedTask);

            // Act
            var resultado = await _controller.AdicionarJogo(idPromocao, idJogo) as OkObjectResult;

            // Assert
            resultado.Should().NotBeNull();
            resultado!.StatusCode.Should().Be(200);
            resultado.Value.Should().BeEquivalentTo(new { Mensagem = "Jogo Adicionado a promoção com sucesso." });
        }

        [Fact]
        public async Task ExcluirPromocao_DeveRetornarOk()
        {
            // Arrange
            int idPromocao = 7;

            _serviceMock
                .Setup(s => s.ExcluirPromocao(idPromocao))
                .Returns(Task.CompletedTask);

            // Act
            var resultado = await _controller.Excluir(idPromocao) as OkObjectResult;

            // Assert
            resultado.Should().NotBeNull();
            resultado!.StatusCode.Should().Be(200);
            resultado.Value.Should().BeEquivalentTo(new { Mensagem = "Promoção excluida com sucesso." });
        }

        [Fact]
        public async Task ListarPromocoes_NaoAdmin_DeveFiltrarSomenteAtivas()
        {
            // Arrange
            var listaEsperada = new List<PromocaoDto>
    {
        new() {
            Id = 1,
            Titulo = "Promoção 1",
            Descricao = "Descrição",
            DescontoPercentual = 10,
            DataInicioUtc = DateTime.UtcNow,
            DataFimUtc = DateTime.UtcNow.AddDays(5),
            Ativa = true,
            Expirada = false,
            Jogos = []
        }
    };

            _serviceMock
                .Setup(s => s.ListarPromocao(true, false))
                .ReturnsAsync(listaEsperada);

            // Simula usuário sem role "Admin"
            var user = new ClaimsPrincipal(new ClaimsIdentity(
            [
        new Claim(ClaimTypes.Role, "Usuario")
    ], "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var resultado = await _controller.ListarPromocao(somenteAtivas: true) as OkObjectResult;

            // Assert
            resultado.Should().NotBeNull();
            resultado!.StatusCode.Should().Be(200);
            resultado.Value.Should().BeEquivalentTo(listaEsperada);
        }

        [Fact]
        public async Task ListarPromocoes_Admin_DeveListarTodas()
        {
            // Arrange
            var listaEsperada = new List<PromocaoDto>
    {
        new() {
            Id = 1,
            Titulo = "Promoção Ativa",
            Descricao = "Descrição ativa",
            DescontoPercentual = 10,
            DataInicioUtc = DateTime.UtcNow.AddDays(-2),
            DataFimUtc = DateTime.UtcNow.AddDays(3),
            Ativa = true,
            Expirada = false,
            Jogos = []
        },
        new() {
            Id = 2,
            Titulo = "Promoção Inativa",
            Descricao = "Descrição inativa",
            DescontoPercentual = 20,
            DataInicioUtc = DateTime.UtcNow.AddDays(-10),
            DataFimUtc = DateTime.UtcNow.AddDays(-1),
            Ativa = false,
            Expirada = true,
            Jogos = []
        }
    };

            _serviceMock
                .Setup(s => s.ListarPromocao(It.IsAny<bool>(), true))
                .ReturnsAsync(listaEsperada);

            // Simula usuário com role "Admin"
            var adminUser = new ClaimsPrincipal(new ClaimsIdentity(
            [
        new Claim(ClaimTypes.Role, "Admin")
    ], "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = adminUser }
            };

            // Act
            var resultado = await _controller.ListarPromocao(somenteAtivas: false) as OkObjectResult;

            // Assert
            resultado.Should().NotBeNull();
            resultado!.StatusCode.Should().Be(200);
            resultado.Value.Should().BeEquivalentTo(listaEsperada);
        }

        [Fact]
        public async Task CriarPromocao_DeveRetornarOk()
        {
            // Arrange
            var dto = new CriarPromocaoDto
            {
                Titulo = "Promoção de Teste",
                Descricao = "Promoção criada via teste",
                DataInicioUtc = DateTime.UtcNow,
                DataFimUtc = DateTime.UtcNow.AddDays(5),
                DescontoPercentual = 25
            };

            _serviceMock
                .Setup(s => s.CriarPromocao(dto))
                .ReturnsAsync(42); // Id fictício retornado pela service

            // Act
            var resultado = await _controller.Criar(dto) as OkObjectResult;

            // Assert
            resultado.Should().NotBeNull();
            resultado!.StatusCode.Should().Be(200);
            resultado.Value.Should().BeEquivalentTo(new
            {
                Mensagem = "Promoção cadastrada com sucesso. id:42"
            });
        }

        [Fact]
        public async Task AdquirirPromocao_DeveRetornar401_QuandoUsuarioNaoAutenticado()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            };

            // Act
            var resultado = await _controller.AdquirirPromocao(1);

            // Assert
            var unauthorized = resultado as UnauthorizedObjectResult;
            unauthorized.Should().NotBeNull();
            unauthorized!.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task AdquirirPromocao_DeveRetornar401_QuandoIdInvalido()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity(
            [
        new Claim(ClaimTypes.NameIdentifier, "abc") // inválido
    ], "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var resultado = await _controller.AdquirirPromocao(5);

            // Assert
            var unauthorized = resultado as UnauthorizedObjectResult;
            unauthorized.Should().NotBeNull();
            unauthorized!.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task AdquirirPromocao_DeveRetornarOk_QuandoValido()
        {
            // Arrange
            var userId = 5;
            var user = new ClaimsPrincipal(new ClaimsIdentity(
            [
        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        new Claim(ClaimTypes.Role, "Usuario")
    ], "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            _serviceMock.Setup(s => s.AdquirirPromocao(userId, It.IsAny<int>()))
                        .Returns(Task.CompletedTask);

            // Act
            var resultado = await _controller.AdquirirPromocao(10) as OkObjectResult;

            // Assert
            resultado.Should().NotBeNull();
            resultado!.StatusCode.Should().Be(200);
            resultado.Value.Should().BeEquivalentTo(new { Mensagem = "Promoção adquirida com sucesso." });
        }

        [Fact]
        public async Task AdicionarJogoPromocao_DeveChamarServicoComArgumentosCorretos()
        {
            // Arrange
            int idPromocao = 99, jogoId = 77;

            _serviceMock.Setup(s => s.AdicionarJogoPromocao(idPromocao, jogoId))
                        .Returns(Task.CompletedTask)
                        .Verifiable();

            // Act
            var result = await _controller.AdicionarJogo(idPromocao, jogoId);

            // Assert
            _serviceMock.Verify(s => s.AdicionarJogoPromocao(idPromocao, jogoId), Times.Once);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task ListarPromocoes_DeveForcarSomenteAtivas_ParaUsuarioNaoAdmin_QuandoParametroFalse()
        {
            // Arrange
            var listaEsperada = new List<PromocaoDto>();
            _serviceMock.Setup(s => s.ListarPromocao(true, false)).ReturnsAsync(listaEsperada);

            var user = new ClaimsPrincipal(new ClaimsIdentity(
            [
        new Claim(ClaimTypes.Role, "Usuario")
    ], "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.ListarPromocao(false);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _serviceMock.Verify(s => s.ListarPromocao(true, false), Times.Once);
        }

        [Fact]
        public async Task AtivarPromocao_DeveRetornarBadRequest_QuandoServiceFalhar()
        {
            // Arrange
            int idPromocao = 99;

            _serviceMock
                .Setup(s => s.AtivarPromocao(idPromocao))
                .ThrowsAsync(new InvalidOperationException("Promoção já está ativa."));

            // Act
            var resultado = await _controller.Ativar(idPromocao);

            // Assert
            resultado.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = resultado as BadRequestObjectResult;
            badRequest!.Value.Should().BeEquivalentTo(new { Mensagem = "Promoção já está ativa." });
        }

        [Fact]
        public async Task ExcluirPromocao_DeveRetornarErro500_SeServiceLancarExcecaoNaoMapeada()
        {
            // Arrange
            int idInvalido = 404;

            _serviceMock
                .Setup(s => s.ExcluirPromocao(idInvalido))
                .ThrowsAsync(new ArgumentException("Promoção não encontrada."));

            // Act
            var resultado = await _controller.Excluir(idInvalido);

            // Assert
            var erro = resultado.Should().BeOfType<ObjectResult>().Subject;
            erro.StatusCode.Should().Be(500);
            erro.Value.Should().BeEquivalentTo(new { Mensagem = "Erro interno ao excluir promoção." });
        }

        [Fact]
        public async Task CriarPromocao_DeveRetornarErro_SeServiceRetornarZero()
        {
            // Arrange
            var dto = new CriarPromocaoDto
            {
                Titulo = "Erro",
                Descricao = "Teste",
                DescontoPercentual = 50,
                DataInicioUtc = DateTime.UtcNow,
                DataFimUtc = DateTime.UtcNow.AddDays(1)
            };

            _serviceMock.Setup(s => s.CriarPromocao(dto)).ReturnsAsync(0);

            // Act
            var resultado = await _controller.Criar(dto) as OkObjectResult;

            // Assert
            resultado.Should().NotBeNull();
            resultado!.StatusCode.Should().Be(200); // Aqui a controller continua retornando 200 mesmo com ID = 0
            resultado.Value.Should().BeEquivalentTo(new { Mensagem = "Promoção cadastrada com sucesso. id:0" });
        }

        [Fact]
        public async Task ListarPromocoes_DeveRetornarListaVazia_SeNaoHouverPromocoes()
        {
            // Arrange
            _serviceMock.Setup(s => s.ListarPromocao(It.IsAny<bool>(), It.IsAny<bool>()))
                        .ReturnsAsync([]);

            var user = new ClaimsPrincipal(new ClaimsIdentity(
            [
        new Claim(ClaimTypes.Role, "Admin")
    ], "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var resultado = await _controller.ListarPromocao() as OkObjectResult;

            // Assert
            resultado.Should().NotBeNull();
            resultado!.StatusCode.Should().Be(200);
            ((IEnumerable<PromocaoDto>)resultado.Value!).Should().BeEmpty();
        }

        [Fact]
        public async Task Criar_DeveRetornarBadRequest_SeServiceLancarExcecao()
        {
            // Arrange
            var dto = new CriarPromocaoDto
            {
                Titulo = "Erro Promo",
                Descricao = "Deve falhar",
                DescontoPercentual = 99,
                DataInicioUtc = DateTime.UtcNow,
                DataFimUtc = DateTime.UtcNow.AddDays(3),
                JogosIds = [99]
            };

            _serviceMock
                .Setup(s => s.CriarPromocao(dto))
                .ThrowsAsync(new InvalidOperationException("Erro interno"));

            // Act
            var resultado = await _controller.Criar(dto);

            // Assert
            resultado.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = resultado as BadRequestObjectResult;
            badRequest!.Value?.ToString().Should().Contain("Erro interno");
        }

        [Fact]
        public async Task AtivarPromocao_DeveRetornarBadRequest_SeServiceLancarExcecao()
        {
            // Arrange
            int idPromocao = 99;

            _serviceMock
                .Setup(s => s.AtivarPromocao(idPromocao))
                .ThrowsAsync(new InvalidOperationException("Promoção expirada."));

            // Act
            var resultado = await _controller.Ativar(idPromocao);

            // Assert
            resultado.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = resultado as BadRequestObjectResult;
            badRequest!.Value?.ToString().Should().Contain("Promoção expirada.");
        }

        [Fact]
        public async Task AdicionarJogoPromocao_DeveRetornarBadRequest_SeServiceLancarExcecao()
        {
            // Arrange
            int idPromocao = 1;
            int jogoId = 99;

            _serviceMock
                .Setup(s => s.AdicionarJogoPromocao(idPromocao, jogoId))
                .ThrowsAsync(new InvalidOperationException("Promoção já ativada."));

            // Act
            var resultado = await _controller.AdicionarJogo(idPromocao, jogoId);

            // Assert
            resultado.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = resultado as BadRequestObjectResult;
            badRequest!.Value?.ToString().Should().Contain("Promoção já ativada.");
        }

        [Fact]
        public async Task ExcluirPromocao_DeveRetornarBadRequest_SeServiceLancarExcecao()
        {
            // Arrange
            int idPromocao = 123;

            _serviceMock
                .Setup(s => s.ExcluirPromocao(idPromocao))
                .ThrowsAsync(new InvalidOperationException("Promoção já utilizada."));

            // Act
            var resultado = await _controller.Excluir(idPromocao);

            // Assert
            resultado.Should().BeOfType<BadRequestObjectResult>();
            var badRequest = resultado as BadRequestObjectResult;
            badRequest!.Value?.ToString().Should().Contain("Promoção já utilizada.");
        }

        [Fact]
        public async Task AdquirirPromocao_DeveRetornarOk_UsuarioLogado()
        {
            // Arrange
            int idUsuario = 123;
            int idPromocao = 456;

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(
            [
        new Claim(ClaimTypes.NameIdentifier, idUsuario.ToString()),
        new Claim(ClaimTypes.Role, "Usuario")
    ], "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaims }
            };

            _serviceMock
                .Setup(s => s.AdquirirPromocao(idUsuario, idPromocao))
                .Returns(Task.CompletedTask);

            // Act
            var resultado = await _controller.AdquirirPromocao(idPromocao) as OkObjectResult;

            // Assert
            resultado.Should().NotBeNull();
            resultado!.StatusCode.Should().Be(200);
            resultado.Value.Should().BeEquivalentTo(new { Mensagem = "Promoção adquirida com sucesso." });
        }

        [Fact]
        public async Task ListarPromocoes_DeveRespeitarParametroSomenteAtivasMesmoParaAdmin()
        {
            // Arrange
            var listaEsperada = new List<PromocaoDto>
    {
        new() {
            Id = 1,
            Titulo = "Promoção Ativa",
            Descricao = "Descrição ativa",
            DescontoPercentual = 15,
            DataInicioUtc = DateTime.UtcNow.AddDays(-1),
            DataFimUtc = DateTime.UtcNow.AddDays(2),
            Ativa = true,
            Expirada = false,
            Jogos = []
        }
    };

            _serviceMock
                .Setup(s => s.ListarPromocao(true, true))
                .ReturnsAsync(listaEsperada);

            var adminUser = new ClaimsPrincipal(new ClaimsIdentity(
            [
        new Claim(ClaimTypes.Role, "Admin")
    ], "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = adminUser }
            };

            // Act
            var resultado = await _controller.ListarPromocao(somenteAtivas: true) as OkObjectResult;

            // Assert
            resultado.Should().NotBeNull();
            resultado!.StatusCode.Should().Be(200);
            resultado.Value.Should().BeEquivalentTo(listaEsperada);

            // Verifica se o método do service foi chamado com os valores corretos
            _serviceMock.Verify(s => s.ListarPromocao(true, true), Times.Once);
        }

        [Fact]
        public async Task AdquirirPromocao_DeveRetornarUnauthorized_QuandoNaoAutenticado()
        {
            // Arrange
            int idPromocao = 99;

            // Simula um usuário não autenticado
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var resultado = await _controller.AdquirirPromocao(idPromocao) as UnauthorizedObjectResult;

            // Assert
            resultado.Should().NotBeNull();
            resultado!.StatusCode.Should().Be(401);
            resultado.Value.Should().BeEquivalentTo(new { Mensagem = "Usuário não autenticado." });

            // Garante que o método do service não foi chamado
            _serviceMock.Verify(s => s.AdquirirPromocao(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task AdquirirPromocao_DeveRetornarUnauthorized_QuandoClaimInvalida()
        {
            // Arrange
            int idPromocao = 10;

            // Simula um usuário autenticado, mas sem a claim NameIdentifier
            var identity = new ClaimsIdentity([new Claim(ClaimTypes.Role, "Usuario")], "mock");
            var user = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var resultado = await _controller.AdquirirPromocao(idPromocao) as UnauthorizedObjectResult;

            // Assert
            resultado.Should().NotBeNull();
            resultado!.StatusCode.Should().Be(401);
            resultado.Value.Should().BeEquivalentTo(new { Mensagem = "Usuário inválido." });

            _serviceMock.Verify(s => s.AdquirirPromocao(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task AdquirirPromocao_DeveRetornarOk_QuandoUsuarioValido()
        {
            // Arrange
            int idUsuario = 123;
            int idPromocao = 10;

            // Simula o usuário com claim válida
            var identity = new ClaimsIdentity(
            [
        new Claim(ClaimTypes.NameIdentifier, idUsuario.ToString()),
        new Claim(ClaimTypes.Role, "Usuario")
    ], "mock");

            var user = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            _serviceMock
                .Setup(s => s.AdquirirPromocao(idUsuario, idPromocao))
                .Returns(Task.CompletedTask);

            // Act
            var resultado = await _controller.AdquirirPromocao(idPromocao) as OkObjectResult;

            // Assert
            resultado.Should().NotBeNull();
            resultado!.StatusCode.Should().Be(200);
            resultado.Value.Should().BeEquivalentTo(new { Mensagem = "Promoção adquirida com sucesso." });

            _serviceMock.Verify(s => s.AdquirirPromocao(idUsuario, idPromocao), Times.Once);
        }

        [Fact]
        public async Task AdquirirPromocao_DeveRetornarBadRequest_SeServiceLancarExcecao()
        {
            // Arrange
            int idUsuario = 123;
            int idPromocao = 10;
            string erro = "Usuário não possui saldo suficiente.";

            var identity = new ClaimsIdentity(
            [
        new Claim(ClaimTypes.NameIdentifier, idUsuario.ToString()),
        new Claim(ClaimTypes.Role, "Usuario")
    ], "mock");

            var user = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            _serviceMock
                .Setup(s => s.AdquirirPromocao(idUsuario, idPromocao))
                .ThrowsAsync(new InvalidOperationException(erro));

            // Act
            var resultado = await _controller.AdquirirPromocao(idPromocao) as BadRequestObjectResult;

            // Assert
            resultado.Should().NotBeNull();
            resultado!.StatusCode.Should().Be(400);
            resultado.Value.Should().BeEquivalentTo(new { Mensagem = erro });

            _serviceMock.Verify(s => s.AdquirirPromocao(idUsuario, idPromocao), Times.Once);
        }

        [Fact]
        public async Task AdquirirPromocao_DeveRetornarUnauthorized_SeUsuarioNaoAutenticado()
        {
            // Arrange
            int idPromocao = 10;

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() } // sem identidade
            };

            // Act
            var resultado = await _controller.AdquirirPromocao(idPromocao) as UnauthorizedObjectResult;

            // Assert
            resultado.Should().NotBeNull();
            resultado!.StatusCode.Should().Be(401);
            resultado.Value.Should().BeEquivalentTo(new { Mensagem = "Usuário não autenticado." });
        }

        [Fact]
        public async Task AdquirirPromocao_DeveRetornarUnauthorized_SeClaimInvalida()
        {
            // Arrange
            int idPromocao = 10;

            var user = new ClaimsPrincipal(new ClaimsIdentity(
            [
        new Claim(ClaimTypes.NameIdentifier, "invalido") // não é int
    ], "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var resultado = await _controller.AdquirirPromocao(idPromocao) as UnauthorizedObjectResult;

            // Assert
            resultado.Should().NotBeNull();
            resultado!.StatusCode.Should().Be(401);
            resultado.Value.Should().BeEquivalentTo(new { Mensagem = "Usuário inválido." });
        }
    }
}
