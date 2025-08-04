using Fiap.Cloud.Games.Application.DTOs.Jogo;
using Fiap.Cloud.Games.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fiap.Cloud.Games.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class PromocaoController(IPromocaoService promocaoService) : ControllerBase
{
    #region ── Acesso Admin ─────────────────────────────────────────────────────────────
    [Tags("🎯 Gerenciamento de Promoções")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Criar promoção de jogos no sistema")]
    [EndpointDescription("Cria promoção de jogos no sistema, somente Admin pode realizar cadastro")]
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarPromocaoDto dto)
    {
        try
        {
            var id = await promocaoService.CriarPromocao(dto);
            return Ok(new { Mensagem = $"Promoção cadastrada com sucesso. id:{id}" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Mensagem = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Mensagem = "Erro interno ao criar promoção." });
        }
    }

    [Tags("🎯 Gerenciamento de Promoções")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Ativa promoção de jogos no sistema")]
    [EndpointDescription("Ativa promoção de jogos no sistema, somente Admin pode realizar ativação")]
    [HttpPatch("{idPromocao}/ativar")]
    public async Task<IActionResult> Ativar(int idPromocao)
    {
        try
        {
            await promocaoService.AtivarPromocao(idPromocao);
            return Ok(new { Mensagem = $"Promoção ativada com sucesso." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Mensagem = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Mensagem = "Erro interno ao ativar promoção." });
        }
    }

    [Tags("🎯 Gerenciamento de Promoções")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Adiciona jogos a promoção")]
    [EndpointDescription("Adiciona jogos a promoção somente se não for ativada ainda, somente Admin pode realizar adição")]
    [HttpPost("{idPromocao}/{jogoId}/Adicina")]
    public async Task<IActionResult> AdicionarJogo(int idPromocao, int jogoId)
    {
        try
        {
            await promocaoService.AdicionarJogoPromocao(idPromocao, jogoId);
            return Ok(new { Mensagem = $"Jogo Adicionado a promoção com sucesso." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Mensagem = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Mensagem = "Erro interno ao adicionar jogo na promoção." });
        }
    }

    [Tags("🎯 Gerenciamento de Promoções")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Exclui a promoção de jogos do sistema")]
    [EndpointDescription("Exclui a promoção de jogos do sistema estando ativa ou não, somente Admin pode realizar exclusão")]
    [HttpPatch("{idPromocao}/exclui")]
    public async Task<IActionResult> Excluir(int idPromocao)
    {
        try
        {
            await promocaoService.ExcluirPromocao(idPromocao);
            return Ok(new { Mensagem = $"Promoção excluida com sucesso." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Mensagem = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Mensagem = "Erro interno ao excluir promoção." });
        }
    }
    #endregion

    #region ── Acesso Publico ─────────────────────────────────────────────────────────────

    [Tags("📣 Promoções Disponíveis")]
    [AllowAnonymous]
    [EndpointSummary("Lista promoção de jogos no sistema")]
    [EndpointDescription("Lista promoção de jogos no sistema, somente Admin pode visualizar as que estão ativas e inativas")]
    [HttpGet]
    public async Task<IActionResult> ListarPromocao([FromQuery] bool somenteAtivas = true)
    {
        bool isAdmin = User.IsInRole("Admin");
        // Para não-admin força o filtro de ativas
        var lista = await promocaoService.ListarPromocao(somenteAtivas: !isAdmin || somenteAtivas, isAdmin: isAdmin);
        return Ok(lista);
    }
    #endregion

    #region ── Acesso Usuario ─────────────────────────────────────────────────────────────
    [Tags("🛍️ Aquisição de Promoções")]
    [Authorize(Roles = "Usuario,Admin")]
    [EndpointSummary("Adquirir promoção com os jogos")]
    [EndpointDescription("Adquiri a promoção dos jogos, precisa esta logado no sistema para adquirir")]
    [HttpPost("{idPromocao}/comprar")]
    public async Task<IActionResult> AdquirirPromocao(int idPromocao)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return Unauthorized(new { Mensagem = "Usuário não autenticado." });

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int idUsuario))
            return Unauthorized(new { Mensagem = "Usuário inválido." });

        try
        {
            await promocaoService.AdquirirPromocao(idUsuario, idPromocao);
            return Ok(new { Mensagem = "Promoção adquirida com sucesso." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Mensagem = ex.Message });
        }
    }
    #endregion
}