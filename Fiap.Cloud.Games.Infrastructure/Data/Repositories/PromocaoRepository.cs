using Fiap.Cloud.Games.Domain.Entities;
using Fiap.Cloud.Games.Domain.Interfaces;
using Fiap.Cloud.Games.Domain.Interfaces.Repositories;
using Fiap.Cloud.Games.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fiap.Cloud.Games.Infrastructure.Data.Repositories;

public class PromocaoRepository(BaseDbContext context) : IPromocaoRepository
{
    public async Task<Promocao?> ObterPorIdAsync(int id, bool incluirJogos = false)
    {
        return await context.Promocoes
            .Include(p => p.Jogos)
                .ThenInclude(pj => pj.Jogo) // <- necessário para acessar `j.Jogo` depois
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Promocao>> ListarAsync(bool somenteAtivas, bool isAdmin)
    {
        var query = context.Promocoes.Include(p => p.Jogos).ThenInclude(pj => pj.Jogo).AsQueryable();

        if (!isAdmin || somenteAtivas)
            query = query.Where(p => p.Ativa && p.DataFim > DateTime.UtcNow);

        return await query.ToListAsync();
    }

    public async Task AdicionarAsync(Promocao promocao)
    {
        await context.AddAsync(promocao);
        await context.SaveChangesAsync();
    }

    public Task RemoverAsync(Promocao promocao)
    {
        context.Remove(promocao);
        return Task.CompletedTask;
    }

    public async Task SalvarAsync()
    {
        await context.SaveChangesAsync();
    }
}
