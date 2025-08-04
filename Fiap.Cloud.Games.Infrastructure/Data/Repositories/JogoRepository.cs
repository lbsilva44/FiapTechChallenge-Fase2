using Fiap.Cloud.Games.Domain.Entities;
using Fiap.Cloud.Games.Domain.Interfaces;
using Fiap.Cloud.Games.Domain.Interfaces.Repositories;
using Fiap.Cloud.Games.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fiap.Cloud.Games.Infrastructure.Data.Repositories;

public class JogoRepository(BaseDbContext context) : IJogoRepository
{
    public async Task<bool> ExisteJogoAtivoComNome(string nome)
    {
        return await context.Jogos.AnyAsync(x => x.Nome == nome && x.Ativo);
    }

    public async Task<Jogo?> ObterPorId(int id)
    {
        return await context.Jogos.FindAsync(id);
    }

    public async Task<IEnumerable<Jogo>> ListarTodos()
    {
        return await context.Jogos.ToListAsync();
    }

    public async Task<IEnumerable<Jogo>> ListarPublicadosAtivos()
    {
        return await context.Jogos
            .Where(j => j.Publicado && j.Ativo)
            .ToListAsync();
    }

    public async Task<IEnumerable<Jogo>> ListarJogosAdquiridos(int usuarioId)
    {
        return await context.Bibliotecas
            .Where(b => b.UsuarioId == usuarioId)
            .Include(b => b.Jogo)
            .Select(b => b.Jogo)
            .ToListAsync();
    }

    public async Task Adicionar(Jogo jogo)
    {
        context.Jogos.Add(jogo);
        await Task.CompletedTask;
    }

    public async Task SalvarAsync()
    {
        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Jogo>> ListarPorIds(IEnumerable<int> ids)
    {
        return await context.Jogos
            .Where(j => ids.Contains(j.Id))
            .ToListAsync();
    }
}
