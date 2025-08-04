using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Fiap.Cloud.Games.API.Controllers;       // seu UsuarioController
using Fiap.Cloud.Games.Application.Interfaces;// IUsuarioService  
using Fiap.Cloud.Games.Domain.Enums;
using Fiap.Cloud.Games.Application.DTOs.Jogo;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Fiap.Cloud.Games.Tests.Application
{
    //Teste Controller
    public class JogoControllerTests
    {
        private readonly Mock<IJogoService> _serviceMock;
        private readonly JogoController _controller;

        public JogoControllerTests()
        {
            _serviceMock = new Mock<IJogoService>();
            _controller = new JogoController(_serviceMock.Object);
        }

        private void SetUserContext(string role = "Usuario", int? userId = 1)
        {
            var claims = new List<Claim>
        {
            new(ClaimTypes.Role, role)
        };

            if (userId.HasValue)
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.ToString()!));

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task Cadastrar_DeveRetornarOk()
        {
            var dto = new JogoDto
            {
                Nome = "Test Game",
                Descricao = "Descrição",
                Preco = 10,
                Tipo = TipoJogo.Puzzle,
                Ativo = true,
                Publicado = false
            };

            _serviceMock.Setup(s => s.CadastrarJogo(dto)).Returns(Task.CompletedTask);

            var result = await _controller.Cadastrar(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Jogo cadastrado com sucesso.", ((dynamic)okResult.Value!).Mensagem);
        }

        [Fact]
        public async Task Cadastrar_DeveLancarExcecao_SeServiceFalhar()
        {
            SetUserContext("Admin");

            var dto = new JogoDto { Nome = "Falha", Descricao = "Teste", Preco = 100, Tipo = TipoJogo.Puzzle, Ativo = true, Publicado = false };

            _serviceMock
                .Setup(s => s.CadastrarJogo(dto))
                .ThrowsAsync(new Exception("Erro ao cadastrar"));

            var exception = await Assert.ThrowsAsync<Exception>(() => _controller.Cadastrar(dto));
            Assert.Equal("Erro ao cadastrar", exception.Message);
        }

        [Fact]
        public async Task Cadastrar_DeveRetornarBadRequest_SeModeloInvalido()
        {
            // Arrange
            _controller.ModelState.AddModelError("Nome", "O campo Nome é obrigatório");

            var dto = new JogoDto(); // sem preencher os campos obrigatórios

            // Act
            var result = await _controller.Cadastrar(dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsAssignableFrom<SerializableError>(badRequest.Value);
        }

        [Fact]
        public async Task Adquirir_DeveRetornarOk_SeUsuarioAutenticado()
        {
            SetUserContext("Usuario", 99);

            _serviceMock.Setup(s => s.AdquirirJogo(It.IsAny<int>(), 99)).Returns(Task.CompletedTask);

            var result = await _controller.Adquirir(123);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Jogo adquirido com sucesso.", ((dynamic)okResult.Value!).Mensagem);
        }

        [Fact]
        public async Task Adquirir_DeveRetornarUnauthorized_SeUsuarioNaoAutenticado()
        {
            // Sem contexto de usuário autenticado
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await _controller.Adquirir(1);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Usuário não autenticado.", ((dynamic)unauthorized.Value!).Mensagem);
        }

        [Fact]
        public async Task Listar_DeveRetornarOkComJogos()
        {
            SetUserContext("Usuario");

            _serviceMock.Setup(s => s.ListarJogos(It.IsAny<string>()))
                .ReturnsAsync([]);

            var result = await _controller.Listar();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<JogoDto[]>(ok.Value);
        }

        [Fact]
        public async Task Publicar_DeveRetornarOk()
        {
            _serviceMock.Setup(s => s.PublicarJogo(1)).Returns(Task.CompletedTask);

            var result = await _controller.Publicar(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Jogo publicado com sucesso.", ((dynamic)ok.Value!).Mensagem);
        }

        [Fact]
        public async Task Ativar_DeveRetornarOk()
        {
            _serviceMock.Setup(s => s.AtivarJogo(1)).Returns(Task.CompletedTask);

            var result = await _controller.Ativar(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            dynamic value = ok.Value!;
            Assert.Equal("Jogo ativado com sucesso.", value.Mensagem);
        }

        [Fact]
        public async Task Desativar_DeveRetornarOk()
        {
            _serviceMock.Setup(s => s.DesativarJogo(1)).Returns(Task.CompletedTask);

            var result = await _controller.Desativa(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Jogo desativado com sucesso.", ((dynamic)ok.Value!).Mensagem);
        }

        [Fact]
        public async Task BibliotecaUsuario_DeveRetornarOk_SeUsuarioAutenticado()
        {
            SetUserContext("Usuario", 2);
            _serviceMock.Setup(s => s.ListarJogosAdquiridosUsuario(2))
                        .ReturnsAsync([]);

            var result = await _controller.BibliotecaUsuario();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<JogoDto[]>(ok.Value);
        }

        [Fact]
        public async Task BibliotecaUsuario_DeveRetornarUnauthorized_SeUsuarioInvalido()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await _controller.BibliotecaUsuario();

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Usuário não autenticado.", ((dynamic)unauthorized.Value!).Mensagem);
        }

        [Fact]
        public async Task BibliotecaUsuario_DeveRetornarUnauthorized_SeUserIdInvalido()
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "abc") }; // não numérico
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            var result = await _controller.BibliotecaUsuario();
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Usuário inválido.", ((dynamic)unauthorized.Value!).Mensagem);
        }

        [Fact]
        public async Task Listar_DevePassarRoleCorretamente()
        {
            SetUserContext("Admin");

            _serviceMock.Setup(s => s.ListarJogos("Admin"))
                .ReturnsAsync([]);

            var result = await _controller.Listar();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<JogoDto[]>(ok.Value);
            _serviceMock.Verify(s => s.ListarJogos("Admin"), Times.Once);
        }

        [Fact]
        public async Task Publicar_JogoDeveLancarExcecao_SeServiceFalhar()
        {
            _serviceMock.Setup(s => s.PublicarJogo(1))
                        .ThrowsAsync(new Exception("Falha ao publicar"));

            var exception = await Assert.ThrowsAsync<Exception>(() => _controller.Publicar(1));

            Assert.Equal("Falha ao publicar", exception.Message);
        }

        [Fact]
        public async Task Ativar_JogoDeveLancarExcecao_SeServiceFalhar()
        {
            _serviceMock.Setup(s => s.AtivarJogo(1))
                        .ThrowsAsync(new Exception("Erro ao ativar"));

            var exception = await Assert.ThrowsAsync<Exception>(() => _controller.Ativar(1));

            Assert.Equal("Erro ao ativar", exception.Message);
        }

        [Fact]
        public async Task Desativar_JogoDeveLancarExcecao_SeServiceFalhar()
        {
            _serviceMock.Setup(s => s.DesativarJogo(1))
                        .ThrowsAsync(new Exception("Erro ao desativar"));

            var exception = await Assert.ThrowsAsync<Exception>(() => _controller.Desativa(1));

            Assert.Equal("Erro ao desativar", exception.Message);
        }

        [Fact]
        public async Task Listar_DeveRetornarJogos_ParaUsuarioAnonimo()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext() // sem claims = anônimo
            };

            _serviceMock.Setup(s => s.ListarJogos("Anonimo"))
                        .ReturnsAsync([]);

            var result = await _controller.Listar();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<JogoDto[]>(ok.Value);
            _serviceMock.Verify(s => s.ListarJogos("Anonimo"), Times.Once);
        }

        [Fact]
        public async Task Cadastrar_DeveLancarExcecao_SeUsuarioNaoAdmin()
        {
            SetUserContext("Usuario"); // papel errado

            var dto = new JogoDto
            {
                Nome = "Game",
                Descricao = "desc",
                Preco = 10,
                Tipo = TipoJogo.RPG,
                Ativo = true,
                Publicado = true
            };

            // Simula falha pois role não é Admin (não será barrado aqui diretamente)
            _serviceMock.Setup(s => s.CadastrarJogo(dto))
                        .ThrowsAsync(new UnauthorizedAccessException("Acesso negado"));

            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.Cadastrar(dto));
            Assert.Equal("Acesso negado", ex.Message);
        }
    }
}
