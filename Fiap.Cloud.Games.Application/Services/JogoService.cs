using Fiap.Cloud.Games.Domain.Entities;
using Fiap.Cloud.Games.Application.Interfaces;
using Fiap.Cloud.Games.Application.DTOs.Jogo;
using Fiap.Cloud.Games.Domain.Enums;
using Fiap.Cloud.Games.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Fiap.Cloud.Games.Domain.Interfaces.Repositories;

namespace Fiap.Cloud.Games.Application.Services;

public class JogoService(IJogoRepository jogoRepository, IUsuarioRepository usuarioRepository, IBibliotecaRepository bibliotecaRepository) : IJogoService
{
    public async Task CadastrarJogo(JogoDto dto)
    {
        if (!Enum.IsDefined(typeof(TipoJogo), dto.Tipo))
            throw new ArgumentException("Tipo de jogo inválido.");

        var existe = await jogoRepository.ExisteJogoAtivoComNome(dto.Nome);

        var jogo = new Jogo(dto.Nome, dto.Descricao, dto.Preco, dto.Tipo);
        jogo.ValidarCadastroDuplicado(existe);

        await jogoRepository.Adicionar(jogo);
        await jogoRepository.SalvarAsync();
    }

    public async Task PublicarJogo(int id)
    {
        var jogo = await jogoRepository.ObterPorId(id)
                   ?? throw new ArgumentException("Jogo não encontrado.");

        jogo.Publicar();
        await jogoRepository.SalvarAsync();
    }

    public async Task AtivarJogo(int id)
    {
        var jogo = await jogoRepository.ObterPorId(id)
                   ?? throw new ArgumentException("Jogo não encontrado.");

        jogo.Ativar();
        await jogoRepository.SalvarAsync();
    }

    public async Task DesativarJogo(int id)
    {
        var jogo = await jogoRepository.ObterPorId(id)
                   ?? throw new ArgumentException("Jogo não encontrado.");

        jogo.Desativar();
        await jogoRepository.SalvarAsync();
    }

    public async Task<IEnumerable<JogoDto>> ListarJogos(string role)
    {
        IEnumerable<Jogo> jogos;

        bool isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);

        jogos = isAdmin
            ? await jogoRepository.ListarTodos()
            : await jogoRepository.ListarPublicadosAtivos();

        return jogos.Select(j => new JogoDto
        {
            Id = j.Id,
            Nome = j.Nome,
            Descricao = j.Descricao,
            Preco = j.Preco,
            Tipo = j.Tipo,
            Ativo = j.Ativo,
            Publicado = isAdmin ? j.Publicado : null
        });
    }

    public async Task AdquirirJogo(int idJogo, int idUsuario)
    {
        var jogo = await jogoRepository.ObterPorId(idJogo)
                   ?? throw new Exception("Jogo não encontrado.");

        jogo.ValidarCompra();

        var usuario = await usuarioRepository.ObterComCarteira(idUsuario)
                      ?? throw new Exception("Usuário não encontrado.");

        var jaTem = await bibliotecaRepository.UsuarioPossuiJogo(idUsuario, idJogo);
        if (jaTem)
            throw new InvalidOperationException("Você já possui este jogo na sua biblioteca.");

        usuario.ComprarJogo(jogo);

        await jogoRepository.SalvarAsync(); // Ou um UnitOfWork se você quiser centralizar os commits

    }

    public async Task<IEnumerable<JogoDto>> ListarJogosAdquiridosUsuario(int idUsuario)
    {
        var jogos = await jogoRepository.ListarJogosAdquiridos(idUsuario);

        return jogos.Select(j => new JogoDto
        {
            Id = j.Id,
            Nome = j.Nome,
            Descricao = j.Descricao,
            Preco = j.Preco,
            Tipo = j.Tipo,
            Ativo = j.Ativo,
            Publicado = j.Publicado
        });
    }

}