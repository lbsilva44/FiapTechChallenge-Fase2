
using Fiap.Cloud.Games.Application.DTOs.Jogo;
using Fiap.Cloud.Games.Application.Interfaces;
using Fiap.Cloud.Games.Domain.Entities;
using Fiap.Cloud.Games.Domain.Interfaces.Repositories;
using Fiap.Cloud.Games.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fiap.Cloud.Games.Application.Services;

public class PromocaoService(IPromocaoRepository promocaoRepository, IJogoRepository jogoRepository,IUsuarioRepository usuarioRepository) : IPromocaoService
{
    public async Task<int> CriarPromocao(CriarPromocaoDto dto)
    {
        // validações básicas
        if (dto.DescontoPercentual is < 0 or > 100)
            throw new ArgumentException("Desconto deve estar entre 0 e 100 %.");

        if (dto.DataFimUtc <= dto.DataInicioUtc)
            throw new ArgumentException("Data Fim deve ser maior que Data Início.");

        if (dto.JogosIds is null || dto.JogosIds.Count == 0)
            throw new ArgumentException("Informe pelo menos um jogo para a promoção.");

        var jogosValidos = await jogoRepository.ListarPorIds(dto.JogosIds);

        var idsValidos = jogosValidos
            .Where(j => j.Publicado && j.Ativo)
            .Select(j => j.Id)
            .ToList();

        if (idsValidos.Count == 0)
            throw new ArgumentException("Nenhum dos jogos informados é válido para promoção.");

        var promocao = new Promocao(
            dto.Titulo,
            dto.Descricao,
            dto.DescontoPercentual,
            dto.DataInicioUtc.DateTime,
            dto.DataFimUtc.DateTime);

        foreach (var jogoId in idsValidos)
            promocao.AdicionarJogo(jogoId);

        await promocaoRepository.AdicionarAsync(promocao);
        return promocao.Id;
    }

    public async Task AtivarPromocao(int id)
    {
        var promocao = await promocaoRepository.ObterPorIdAsync(id)
            ?? throw new ArgumentException("Promoção não encontrada.");

        promocao.Ativar();
        await promocaoRepository.SalvarAsync();
    }

    public async Task ExcluirPromocao(int id)
    {
        var promocao = await promocaoRepository.ObterPorIdAsync(id)
            ?? throw new ArgumentException("Promoção não encontrada.");

        await promocaoRepository.RemoverAsync(promocao);
        await promocaoRepository.SalvarAsync();
    }

    public async Task<IEnumerable<PromocaoDto>> ListarPromocao(bool somenteAtivas, bool isAdmin)
    {
        var promocoes = await promocaoRepository.ListarAsync(somenteAtivas, isAdmin);

        if (!isAdmin || somenteAtivas)
            promocoes = promocoes.Where(p => p.Ativa && !p.Expirada).ToList();

        return promocoes.Select(p => new PromocaoDto
        {
            Id = p.Id,
            Titulo = p.Titulo,
            Descricao = p.Descricao,
            DescontoPercentual = p.DescontoPercentual,
            DataInicioUtc = p.DataInicio,
            DataFimUtc = p.DataFim,
            Ativa = p.Ativa,
            Expirada = p.Expirada,
            Jogos = p.Jogos.Select(j => new JogoDto
            {
                Id = j.Jogo!.Id,
                Nome = j.Jogo.Nome,
                Descricao = j.Jogo.Descricao,
                Preco = j.Jogo.Preco,
                Tipo = j.Jogo.Tipo,
                Ativo = j.Jogo.Ativo,
                Publicado = j.Jogo.Publicado
            })
        });
    }

    public async Task AdicionarJogoPromocao(int promocaoId, int jogoId)
    {
        var promocao = await promocaoRepository.ObterPorIdAsync(promocaoId)
            ?? throw new ArgumentException("Promoção não encontrada.");

        var jogo = await jogoRepository.ObterPorId(jogoId);
        if (jogo == null || !jogo.Ativo || !jogo.Publicado)
            throw new ArgumentException("Jogo não encontrado ou inativo.");

        promocao.AdicionarJogo(jogoId);
        await promocaoRepository.SalvarAsync();
    }

    public async Task AdquirirPromocao(int usuarioId, int promocaoId)
    {
        var usuario = await usuarioRepository.ObterPorId(usuarioId)
            ?? throw new ArgumentException("Usuário não encontrado.");

        var promocao = await promocaoRepository.ObterPorIdAsync(promocaoId)
            ?? throw new ArgumentException("Promoção não encontrada.");

        if (!promocao.Ativa)
            throw new InvalidOperationException("Promoção não está ativa.");

        var jogosPendentes = promocao.Jogos
            .Select(j => j.Jogo!)
            .Where(j => !usuario.PossuiJogo(j.Id))
            .ToList();

        if (jogosPendentes.Count == 0)
            throw new InvalidOperationException("Usuário já possui todos os jogos da promoção.");

        decimal total = jogosPendentes
            .Sum(j => promocao.CalcularPrecoComDesconto(j));

        if (!usuario.TemSaldoSuficiente(total))
            throw new InvalidOperationException($"Saldo insuficiente: custo total R$ {total:F2}, saldo R$ {usuario.Carteira.Saldo:F2}.");

        foreach (var jogo in jogosPendentes)
        {
            usuario.ComprarPromocao(jogo, promocao.CalcularPrecoComDesconto(jogo));
        }

        await usuarioRepository.SalvarAsync();
    }
}
