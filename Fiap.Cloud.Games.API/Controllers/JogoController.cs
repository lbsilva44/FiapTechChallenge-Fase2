using Fiap.Cloud.Games.Application.DTOs.Comum;
using Fiap.Cloud.Games.Application.DTOs.Jogo;
using Fiap.Cloud.Games.Application.Interfaces;
using Fiap.Cloud.Games.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fiap.Cloud.Games.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JogoController(IJogoService jogoService) : ControllerBase
{
    #region ── Acesso Admin ─────────────────────────────────────────────────────────────
    [Tags("🎮 Gerenciamento de Jogos")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Cadastrar um novo jogo")]
    [EndpointDescription("Adiciona um novo jogo no sistema. Somente Admin.")]
    [HttpPost]
    public async Task<IActionResult> Cadastrar([FromBody] JogoDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await jogoService.CadastrarJogo(dto);

        return Ok(new ResponseMensagem("Jogo cadastrado com sucesso."));
    }

    [Tags("🎮 Gerenciamento de Jogos")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Publicar um jogo")]
    [EndpointDescription("Torna o jogo visível para aquisição.")]
    [HttpPatch("{idJogo}/publicar")]
    public async Task<IActionResult> Publicar(int idJogo)
    {
        await jogoService.PublicarJogo(idJogo);
        return Ok(new ResponseMensagem("Jogo publicado com sucesso."));
    }

    [Tags("🎮 Gerenciamento de Jogos")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Ativar jogo")]
    [EndpointDescription("Ativa um jogo previamente desativado.")]
    [HttpPatch("{idJogo}/ativar")]
    public async Task<IActionResult> Ativar(int idJogo)
    {
        await jogoService.AtivarJogo(idJogo);
        return Ok(new ResponseMensagem("Jogo ativado com sucesso."));
    }

    [Tags("🎮 Gerenciamento de Jogos")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Desativar jogo")]
    [EndpointDescription("Desativa um jogo para que não possa mais ser adquirido.")]
    [HttpPatch("{idJogo}/desativar")]
    public async Task<IActionResult> Desativa(int idJogo)
    {
        await jogoService.DesativarJogo(idJogo);
        return Ok(new ResponseMensagem("Jogo desativado com sucesso."));
    }
    #endregion

    #region ── Acesso Publico ─────────────────────────────────────────────────────────────
    [Tags("🎮 Gerenciamento de Jogos")]
    [AllowAnonymous]
    [EndpointSummary("Listar jogos")]
    [EndpointDescription("Lista os jogos publicados e ativos. Admin vê todos.")]
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var TipoAcesso = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "Anonimo";
        var jogos = await jogoService.ListarJogos(TipoAcesso);
        return Ok(jogos);
    }

    [Tags("🎮 Gerenciamento de Jogos")]
    [AllowAnonymous]
    [EndpointSummary("Listar tipos de jogo")]
    [EndpointDescription("Retorna os tipos de jogo disponíveis.")]
    [HttpGet("tipos")]
    public IActionResult Tipojogo()
    {
        var tipos = Enum.GetNames(typeof(TipoJogo));
        return Ok(tipos);
    }
    #endregion

    #region ── Acesso Usuario ─────────────────────────────────────────────────────────────
    [Tags("🛍️ Aquisição de Jogos")]
    [Authorize(Roles = "Usuario,Admin")]
    [EndpointSummary("Adquirir jogo")]
    [EndpointDescription("Adquire um jogo para a biblioteca do usuário logado.")]
    [HttpPost("{idJogo}/adquirir")]
    public async Task<IActionResult> Adquirir(int idJogo)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return Unauthorized(new ResponseMensagem("Usuário não autenticado."));

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int idUsuario))
            return Unauthorized(new ResponseMensagem("Usuário inválido."));

        await jogoService.AdquirirJogo(idJogo, idUsuario);
        return Ok(new ResponseMensagem("Jogo adquirido com sucesso."));
    }

    [Tags("🛍️ Aquisição de Jogos")]
    [Authorize(Roles = "Usuario,Admin")]
    [EndpointSummary("Biblioteca do usuário")]
    [EndpointDescription("Lista os jogos adquiridos pelo usuário logado.")]
    [HttpGet("biblioteca")]
    public async Task<IActionResult> BibliotecaUsuario()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return Unauthorized(new ResponseMensagem("Usuário não autenticado."));

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int idUsuario))
            return Unauthorized(new ResponseMensagem("Usuário inválido."));

        var jogos = await jogoService.ListarJogosAdquiridosUsuario(idUsuario);
        return Ok(jogos);
    }
    #endregion
}
