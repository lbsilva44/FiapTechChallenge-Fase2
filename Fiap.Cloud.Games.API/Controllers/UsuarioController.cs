using Microsoft.AspNetCore.Mvc;
using Fiap.Cloud.Games.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Fiap.Cloud.Games.Domain.Enums;
using Fiap.Cloud.Games.Application.DTOs.Usuario;
using Fiap.Cloud.Games.Application.DTOs.Comum;

namespace Fiap.Cloud.Games.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuarioController(IUsuarioService usuarioService) : ControllerBase
{
    #region ─── Acesso Público ────────────────────────────────────────────

    [Tags("🔐 Autenticação e Registro")]
    [EndpointSummary("Registrar novo usuário")]
    [EndpointDescription("Cria um novo usuário com perfil padrão de acesso.")]
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] RegistroUsuarioDto user)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var usuarioCriado = await usuarioService.RegistrarUsuario(user);
        if (usuarioCriado == null)
            return BadRequest(new ResponseMensagem("Não foi possível registrar o usuário."));

        return Ok(new ResponseMensagem("Usuário cadastrado com sucesso."));
    }

    [Tags("🔐 Autenticação e Registro")]
    [EndpointSummary("Login no sistema")]
    [EndpointDescription("Realiza autenticação do usuário com e-mail e senha.")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUsuarioDto user)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var token = await usuarioService.LoginUsuario(user);
        if (string.IsNullOrEmpty(token))
            return Unauthorized(new ResponseMensagem("Credenciais inválidas."));

        return Ok(new { Mensagem = "Usuário logado com sucesso.", Token = token });
    }

    [Tags("🔐 Autenticação e Registro")]
    [EndpointSummary("Alterar senha do usuário")]
    [EndpointDescription("Altera a senha do usuário autenticado.")]
    [HttpPost("senha")]
    public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (dto.SenhaAtual == dto.NovaSenha)
            return BadRequest(new ResponseMensagem("A nova senha deve ser diferente da senha atual."));

        var sucesso = await usuarioService.AlterarSenha(dto);
        if (!sucesso)
            return Unauthorized(new ResponseMensagem("Credenciais inválidas ou senha atual incorreta."));

        return Ok(new ResponseMensagem("Senha alterada com sucesso."));
    }

    [Tags("🔐 Autenticação e Registro")]
    [EndpointSummary("Redefinir senha")]
    [EndpointDescription("Redefine a senha de acesso sem a senha atual, utilizando apenas o e-mail.")]
    [HttpPost("senha/redefinir")]
    [AllowAnonymous]
    public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var sucesso = await usuarioService.RedefinirSenha(dto);
        if (!sucesso)
            return NotFound(new ResponseMensagem("Usuário não encontrado."));

        return Ok(new ResponseMensagem("Senha redefinida com sucesso."));
    }

    #endregion
    #region ─── Acesso Admin ───────────────────────────────────────────────

    [Tags("🛠️ Administração de Usuários")]
    [EndpointSummary("Alterar tipo de acesso do usuário")]
    [EndpointDescription("Atualiza o tipo de acesso de um usuário existente (Admin ou Usuario).")]
    [Authorize(Roles = "Admin")]
    [HttpPatch("{idUsuario}/acesso")]
    public async Task<IActionResult> AlterarAcessoUsuario(int idUsuario, [FromBody] AlterarAcessoUsuarioDto dto)
    {
        var sucesso = await usuarioService.AlterarAcessoUsuario(idUsuario, dto.NovaRole);
        if (!sucesso)
            return BadRequest(new ResponseMensagem("Não foi possível alterar o tipo de acesso."));

        return Ok(new ResponseMensagem("Tipo de acesso alterado com sucesso."));
    }

    [Tags("🛠️ Administração de Usuários")]
    [EndpointSummary("Listar tipos de acesso disponíveis")]
    [EndpointDescription("Lista os tipos de acesso disponíveis para atribuição a um usuário.")]
    [Authorize(Roles = "Admin")]
    [HttpGet("roles")]
    public IActionResult ListarRoles()
    {
        var roles = Enum.GetNames(typeof(TipoAcesso));
        return Ok(new { RolesDisponiveis = roles });
    }

    [Tags("🛠️ Administração de Usuários")]
    [EndpointSummary("Listar todos os usuários")]
    [EndpointDescription("Retorna a lista de todos os usuários cadastrados.")]
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> ListarUsuarios()
    {
        var usuarios = await usuarioService.ListarUsuarios();
        return Ok(usuarios);
    }

    [Tags("🛠️ Administração de Usuários")]
    [EndpointSummary("Desativar usuário")]
    [EndpointDescription("Desativa um usuário pelo ID.")]
    [Authorize(Roles = "Admin")]
    [HttpPatch("{idUsuario}/desativar")]
    public async Task<IActionResult> DesativarUsuario(int idUsuario)
    {
        var sucesso = await usuarioService.DesativarUsuario(idUsuario);
        if (!sucesso)
            return BadRequest(new ResponseMensagem("Não foi possível desativar o usuário."));

        return Ok(new ResponseMensagem("Usuário desativado com sucesso."));
    }

    [Tags("🛠️ Administração de Usuários")]
    [EndpointSummary("Ativar usuário")]
    [EndpointDescription("Ativa um usuário previamente desativado.")]
    [Authorize(Roles = "Admin")]
    [HttpPatch("{idUsuario}/ativar")]
    public async Task<IActionResult> AtivarUsuario(int idUsuario)
    {
        var sucesso = await usuarioService.AtivarUsuario(idUsuario);
        if (!sucesso)
            return BadRequest(new ResponseMensagem("Não foi possível ativar o usuário."));

        return Ok(new ResponseMensagem("Usuário ativado com sucesso."));
    }
    #endregion
    #region ─── Acesso do Próprio Usuário ─────────────────────────────────

    [Tags("👤 Conta do Usuário")]
    [EndpointSummary("Depositar saldo na carteira")]
    [EndpointDescription("Realiza um depósito de saldo na carteira do usuário.")]
    [Authorize(Roles = "Usuario,Admin")]
    [HttpPost("{id}/deposito")]
    public async Task<IActionResult> Depositar(int id, [FromBody] decimal valor)
    {
        try
        {
            await usuarioService.Depositar(id, valor);
            return Ok(new ResponseMensagem("Depósito realizado com sucesso."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ResponseMensagem(ex.Message));
        }
    }

    [Tags("👤 Conta do Usuário")]
    [EndpointSummary("Detalhes do perfil do usuário")]
    [EndpointDescription("Obtém os detalhes do perfil de um usuário específico.")]
    [Authorize(Roles = "Usuario,Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> ObterDetalhes(int id)
    {
        if (id <= 0)
            return BadRequest(new ResponseMensagem("ID inválido."));

        var detalhes = await usuarioService.ObterDetalhesUsuario(id);
        return detalhes is null
            ? NotFound(new ResponseMensagem("Usuário não encontrado."))
            : Ok(detalhes);
    }

    #endregion
}

