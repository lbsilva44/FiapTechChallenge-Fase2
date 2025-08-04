using Fiap.Cloud.Games.Domain.Entities;
using Fiap.Cloud.Games.Domain.Interfaces.Repositories;
using Fiap.Cloud.Games.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fiap.Cloud.Games.Infrastructure.Data.Repositories;

public class UsuarioRepository(BaseDbContext context) : IUsuarioRepository
{
    public async Task<bool> EmailExiste(string email) =>
        await context.Usuarios.AnyAsync(u => u.Email.Valor == email);

    public async Task<Usuario?> ObterPorEmail(string email) =>
        await context.Usuarios.FirstOrDefaultAsync(u => u.Email.Valor == email);

    public async Task<Usuario?> ObterPorId(int id) =>
        await context.Usuarios.FindAsync(id);

    public async Task<IEnumerable<Usuario>> ListarTodos() =>
        await context.Usuarios.AsNoTracking().ToListAsync();

    public async Task<Usuario?> ObterComCarteira(int id) =>
        await context.Usuarios
            .Include(u => u.Carteira)
            .FirstOrDefaultAsync(u => u.Id == id);

    public async Task<Usuario?> ObterComMovimentosEBiblioteca(int id) =>
        await context.Usuarios
            .Include(u => u.Movimentos)
            .Include(u => u.Bibliotecas)
                .ThenInclude(b => b.Jogo)
            .FirstOrDefaultAsync(u => u.Id == id);

    public void Adicionar(Usuario usuario) =>
        context.Usuarios.Add(usuario);

    public async Task SalvarAsync() =>
        await context.SaveChangesAsync();
}
