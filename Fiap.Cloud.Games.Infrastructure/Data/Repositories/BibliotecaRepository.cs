using Fiap.Cloud.Games.Domain.Entities;
using Fiap.Cloud.Games.Domain.Interfaces.Repositories;
using Fiap.Cloud.Games.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fiap.Cloud.Games.Infrastructure.Data.Repositories;

public class BibliotecaRepository(BaseDbContext context) : IBibliotecaRepository
{
    public async Task<bool> UsuarioPossuiJogo(int usuarioId, int jogoId)
    {
        return await context.Bibliotecas
            .AnyAsync(b => b.UsuarioId == usuarioId && b.JogoId == jogoId);
    }

    public async Task Adicionar(Biblioteca biblioteca)
    {
        await context.Bibliotecas.AddAsync(biblioteca);
    }
}
