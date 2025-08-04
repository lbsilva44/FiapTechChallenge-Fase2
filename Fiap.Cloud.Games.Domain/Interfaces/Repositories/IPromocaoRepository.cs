using Fiap.Cloud.Games.Domain.Entities;

namespace Fiap.Cloud.Games.Domain.Interfaces.Repositories;

public interface IPromocaoRepository
{
    Task<Promocao?> ObterPorIdAsync(int id, bool incluirJogos = false);
    Task<IEnumerable<Promocao>> ListarAsync(bool somenteAtivas, bool isAdmin);
    Task AdicionarAsync(Promocao promocao);
    Task RemoverAsync(Promocao promocao);
    Task SalvarAsync(); // para SaveChanges
}
