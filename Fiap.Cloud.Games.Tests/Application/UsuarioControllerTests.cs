using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Fiap.Cloud.Games.API.Controllers;
using Fiap.Cloud.Games.Application.Interfaces;
using Fiap.Cloud.Games.Domain.Entities;
using Fiap.Cloud.Games.Application.DTOs.Usuario;
using Fiap.Cloud.Games.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Fiap.Cloud.Games.Tests.Application
{
    //Teste Controller
    public class UsuarioControllerTests
    {
        private readonly Mock<IUsuarioService> _serviceMock;
        private readonly UsuarioController _controller;

        public UsuarioControllerTests()
        {
            _serviceMock = new Mock<IUsuarioService>();
            _controller = new UsuarioController(_serviceMock.Object);
        }

        [Fact]
        public async Task Registrar_Deve_retornar_Ok_com_mensagem_sucesso()
        {
            // Arrange
            var dto = new RegistroUsuarioDto
            {
                Nome = "Teste Usuário",
                Email = "teste@exemplo.com",
                Senha = "Senha@123",
                // demais campos se houver
            };

            _serviceMock
                .Setup(s => s.RegistrarUsuario(dto))
                .ReturnsAsync(new Usuario(dto.Nome, dto.Email, dto.Senha));

            // Act
            var result = await _controller.Criar(dto);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeEquivalentTo(new { Mensagem = "Usuário cadastrado com sucesso." });

            _serviceMock.Verify(s => s.RegistrarUsuario(dto), Times.Once);
        }
        [Fact]
        public async Task Registrar_Deve_retornar_BadRequest_quando_usuario_nao_for_criado()
        {
            // Arrange
            var dto = new RegistroUsuarioDto
            {
                Nome = "Invalido",
                Email = "emailinvalido.com",
                Senha = "123"
            };

            _serviceMock
                .Setup(s => s.RegistrarUsuario(dto))
                .ReturnsAsync((Usuario)null!);

            // Act
            var result = await _controller.Criar(dto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            _serviceMock.Verify(s => s.RegistrarUsuario(dto), Times.Once);
        }
        [Fact]
        public async Task Registrar_Deve_retornar_BadRequest_quando_modelo_invalido()
        {
            // Arrange
            var dto = new RegistroUsuarioDto(); // vazio

            _controller.ModelState.AddModelError("Email", "O campo Email é obrigatório");

            // Act
            var result = await _controller.Criar(dto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Theory]
        [InlineData("lbsilva44@gmail.com", "Le@156487812335")]
        public async Task Login_Deve_retornar_Ok_com_token(string email, string senha)
        {
            // Arrange
            var dto = new LoginUsuarioDto { Email = email, Senha = senha };
            var fakeToken = "jwt-token-123";

            _serviceMock
                .Setup(s => s.LoginUsuario(dto))
                .ReturnsAsync(fakeToken);

            // Act
            var result = await _controller.Login(dto);

            // Assert  
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeEquivalentTo(new
            {
                Mensagem = "Usuário logado com sucesso.",
                Token = fakeToken
            });

            _serviceMock.Verify(s => s.LoginUsuario(dto), Times.Once);
        }
        [Fact]
        public async Task Login_Deve_retornar_Unauthorized_quando_token_for_nulo()
        {
            // Arrange
            var dto = new LoginUsuarioDto
            {
                Email = "naoexiste@teste.com",
                Senha = "SenhaErrada"
            };

            _serviceMock
                .Setup(s => s.LoginUsuario(dto))
                .ReturnsAsync((string)null!);

            // Act
            var result = await _controller.Login(dto);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
            _serviceMock.Verify(s => s.LoginUsuario(dto), Times.Once);
        }
        [Fact]
        public async Task Login_Deve_retornar_BadRequest_quando_modelo_invalido()
        {
            var dto = new LoginUsuarioDto(); // Campos vazios
            _controller.ModelState.AddModelError("Email", "Campo obrigatório");

            var result = await _controller.Login(dto);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AlterarAcessoUsuario_Deve_retornar_Ok()
        {
            var dto = new AlterarAcessoUsuarioDto { NovaRole = "Admin" };
            int idUsuario = 1;

            _serviceMock.Setup(s => s.AlterarAcessoUsuario(idUsuario, dto.NovaRole)).ReturnsAsync(true);

            var result = await _controller.AlterarAcessoUsuario(idUsuario, dto);

            result.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().BeEquivalentTo(new { Mensagem = "Tipo de acesso alterado com sucesso." });

            _serviceMock.Verify(s => s.AlterarAcessoUsuario(idUsuario, dto.NovaRole), Times.Once);
        }
        [Fact]
        public async Task AlterarAcessoUsuario_Deve_retornar_BadRequest_quando_falha()
        {
            var dto = new AlterarAcessoUsuarioDto { NovaRole = "Admin" };
            int idUsuario = 1;

            _serviceMock.Setup(s => s.AlterarAcessoUsuario(idUsuario, dto.NovaRole))
                        .ReturnsAsync(false); // <- simula falha

            var result = await _controller.AlterarAcessoUsuario(idUsuario, dto);

            result.Should().BeOfType<BadRequestObjectResult>();
        }
        [Fact]
        public async Task AlterarAcessoUsuario_Deve_retornar_BadRequest_se_model_invalido()
        {
            var dto = new AlterarAcessoUsuarioDto(); // vazio
            _controller.ModelState.AddModelError("NovaRole", "Campo obrigatório");

            var result = await _controller.AlterarAcessoUsuario(1, dto);

            result.Should().BeOfType<BadRequestObjectResult>();
        }
        [Fact]
        public void AlterarAcessoUsuario_Deve_ter_autorizacao_para_admin()
        {
            var method = typeof(UsuarioController).GetMethod("AlterarAcessoUsuario");
            var attr = method!.GetCustomAttributes(typeof(AuthorizeAttribute), false).FirstOrDefault() as AuthorizeAttribute;

            attr.Should().NotBeNull();
            attr!.Roles.Should().Contain("Admin");
        }

        [Fact]
        public void ListarRoles_Deve_retornar_lista_enum()
        {
            var result = _controller.ListarRoles();

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var value = ok.Value;

            var prop = value!.GetType().GetProperty("RolesDisponiveis");
            var roles = prop?.GetValue(value) as string[];

            roles.Should().NotBeNull();
            roles!.Should().Contain(nameof(TipoAcesso.Admin));
        }
        [Fact]
        public void ListarRoles_Deve_retornar_roles_definidos_em_TipoAcesso()
        {
            var result = _controller.ListarRoles() as OkObjectResult;
            var value = result!.Value;

            var roles = value!.GetType().GetProperty("RolesDisponiveis")?.GetValue(value) as string[];

            var expectedRoles = Enum.GetNames(typeof(TipoAcesso));
            roles.Should().BeEquivalentTo(expectedRoles);
        }

        [Fact]
        public async Task ListarUsuarios_Deve_retornar_lista()
        {
            var lista = new List<UsuarioDto>
            {
        new() { Nome = "Leo", Email = "leo@ex.com" },
        new() { Nome = "Maria", Email = "maria@ex.com" }
    };

            _serviceMock.Setup(s => s.ListarUsuarios())
                        .ReturnsAsync(lista);

            var result = await _controller.ListarUsuarios();

            result.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().BeEquivalentTo(lista);
        }
        [Fact]
        public async Task ListarUsuarios_Deve_retornar_lista_vazia()
        {
            _serviceMock.Setup(s => s.ListarUsuarios())
                        .ReturnsAsync([]);

            var result = await _controller.ListarUsuarios();

            result.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().BeEquivalentTo(new List<UsuarioDto>());
        }

        [Fact]
        public async Task GetDetalhes_Deve_retornar_NotFound_quando_usuario_nao_existe()
        {
            var detalhes = new UsuarioDetalhesDto
            {
                Id = 1,
                Nome = "Leonardo",
                Email = "leo@teste.com",
                Role = "Usuario"
            };

            _serviceMock.Setup(s => s.ObterDetalhesUsuario(1))
                        .ReturnsAsync((UsuarioDetalhesDto)null!);

            var result = await _controller.ObterDetalhes(1);

            result.Should().BeOfType<NotFoundObjectResult>();
        }
        [Fact]
        public async Task GetDetalhes_Deve_retornar_Ok_quando_usuario_encontrado()
        {
            var detalhes = new UsuarioDetalhesDto
            {
                Id = 1,
                Nome = "Fulano",
                Email = "fulano@teste.com",
                Role = "Usuario"
                // Preencha os campos reais da sua classe
            };

            _serviceMock.Setup(s => s.ObterDetalhesUsuario(1))
                        .ReturnsAsync(detalhes);

            var result = await _controller.ObterDetalhes(1);

            result.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().BeEquivalentTo(detalhes);
        }
        [Fact]
        public async Task GetDetalhes_Deve_retornar_BadRequest_para_id_invalido()
        {
            var result = await _controller.ObterDetalhes(0);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Depositar_Deve_retornar_Ok()
        {
            int id = 1;
            decimal valor = 50.0m;

            _serviceMock.Setup(s => s.Depositar(id, valor)).Returns(Task.CompletedTask);

            var result = await _controller.Depositar(id, valor);

            result.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().BeEquivalentTo(new { Mensagem = "Depósito realizado com sucesso." });

            _serviceMock.Verify(s => s.Depositar(id, valor), Times.Once);
        }
        [Fact]
        public async Task Depositar_Deve_retornar_BadRequest_quando_valor_invalido()
        {
            int id = 1;
            decimal valor = 0m;

            // Supondo que o serviço recuse depósitos de valor zero
            _serviceMock.Setup(s => s.Depositar(id, valor)).ThrowsAsync(new ArgumentException("Valor inválido"));

            var result = await _controller.Depositar(id, valor);

            result.Should().BeOfType<BadRequestObjectResult>();
        }
        [Fact]
        public async Task Depositar_Deve_retornar_BadRequest_para_valor_negativo()
        {
            int id = 1;
            decimal valor = -10.0m;

            _serviceMock.Setup(s => s.Depositar(id, valor))
                        .ThrowsAsync(new ArgumentException("Valor inválido"));

            var result = await _controller.Depositar(id, valor);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task DesativarUsuario_Deve_retornar_Ok()
        {
            int id = 1;

            _serviceMock.Setup(s => s.DesativarUsuario(id)).ReturnsAsync(true);

            var result = await _controller.DesativarUsuario(id);

            result.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().BeEquivalentTo(new { Mensagem = "Usuário desativado com sucesso." });
        }
        [Fact]
        public async Task AtivarUsuario_Deve_retornar_Ok()
        {
            int id = 1;

            _serviceMock.Setup(s => s.AtivarUsuario(id)).ReturnsAsync(true);

            var result = await _controller.AtivarUsuario(id);

            result.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().BeEquivalentTo(new { Mensagem = "Usuário ativado com sucesso." });
        }
        [Fact]
        public async Task DesativarUsuario_Deve_retornar_BadRequest_quando_falha()
        {
            int id = 1;

            _serviceMock.Setup(s => s.DesativarUsuario(id)).ReturnsAsync(false);

            var result = await _controller.DesativarUsuario(id);

            result.Should().BeOfType<BadRequestObjectResult>();
        }
        [Fact]
        public async Task AtivarUsuario_Deve_retornar_BadRequest_quando_falha()
        {
            int id = 1;

            _serviceMock.Setup(s => s.AtivarUsuario(id)).ReturnsAsync(false);

            var result = await _controller.AtivarUsuario(id);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AlterarSenha_DeveRetornarOk_QuandoSenhaAlteradaComSucesso()
        {
            // Arrange
            var dto = new AlterarSenhaDto
            {
                Email = "usuario@teste.com",
                SenhaAtual = "SenhaAntiga@123",
                NovaSenha = "NovaSenha@456"
            };

            _serviceMock.Setup(s => s.AlterarSenha(dto)).ReturnsAsync(true);

            // Act
            var result = await _controller.AlterarSenha(dto);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeEquivalentTo(new { Mensagem = "Senha alterada com sucesso." });

            _serviceMock.Verify(s => s.AlterarSenha(dto), Times.Once);
        }
        [Fact]
        public async Task AlterarSenha_DeveRetornarUnauthorized_QuandoCredenciaisInvalidas()
        {
            // Arrange
            var dto = new AlterarSenhaDto
            {
                Email = "usuario@teste.com",
                SenhaAtual = "SenhaErrada",
                NovaSenha = "NovaSenha@456"
            };

            _serviceMock.Setup(s => s.AlterarSenha(dto)).ReturnsAsync(false);

            // Act
            var result = await _controller.AlterarSenha( dto);

            // Assert
            var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            unauthorized.Value.Should().BeEquivalentTo(new { Mensagem = "Credenciais inválidas ou senha atual incorreta." });

            _serviceMock.Verify(s => s.AlterarSenha(dto), Times.Once);
        }
        [Fact]
        public async Task AlterarSenha_DeveRetornarBadRequest_QuandoModelStateInvalido()
        {
            // Arrange
            var dto = new AlterarSenhaDto(); // inválido
            _controller.ModelState.AddModelError("Email", "Campo obrigatório");

            // Act
            var result = await _controller.AlterarSenha(dto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            _serviceMock.Verify(s => s.AlterarSenha(It.IsAny<AlterarSenhaDto>()), Times.Never);
        }
        [Fact]
        public async Task RedefinirSenha_DeveRetornarOk_QuandoSenhaRedefinidaComSucesso()
        {
            // Arrange
            var dto = new RedefinirSenhaDto
            {
                Email = "teste@email.com",
                NovaSenha = "NovaSenha@123"
            };

            _serviceMock
                .Setup(s => s.RedefinirSenha(dto))
                .ReturnsAsync(true);

            // Act
            var resultado = await _controller.RedefinirSenha(dto);

            // Assert
            resultado.Should().BeOfType<OkObjectResult>();
            var resposta = resultado as OkObjectResult;
            resposta?.Value?.ToString().Should().Contain("Senha redefinida com sucesso");
        }
        [Fact]
        public async Task RedefinirSenha_DeveRetornarNotFound_QuandoUsuarioNaoEncontrado()
        {
            // Arrange
            var dto = new RedefinirSenhaDto
            {
                Email = "inexistente@email.com",
                NovaSenha = "NovaSenha@123"
            };

            _serviceMock
                .Setup(s => s.RedefinirSenha(dto))
                .ReturnsAsync(false);

            // Act
            var resultado = await _controller.RedefinirSenha( dto);

            // Assert
            resultado.Should().BeOfType<NotFoundObjectResult>();
            var resposta = resultado as NotFoundObjectResult;
            resposta?.Value?.ToString().Should().Contain("Usuário não encontrado");
        }
        [Fact]
        public async Task RedefinirSenha_DeveRetornarBadRequest_QuandoModelStateInvalido()
        {
            // Arrange
            var dto = new RedefinirSenhaDto
            {
                Email = "", // inválido
                NovaSenha = ""
            };
            _controller.ModelState.AddModelError("Email", "Email é obrigatório");

            // Act
            var resultado = await _controller.RedefinirSenha(dto);

            // Assert
            resultado.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
