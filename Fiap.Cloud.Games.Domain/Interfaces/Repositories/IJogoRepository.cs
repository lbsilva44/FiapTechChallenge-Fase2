
using Fiap.Cloud.Games.Domain.Entities;

namespace Fiap.Cloud.Games.Domain.Interfaces.Repositories;

public interface IJogoRepository
{
    Task<bool> ExisteJogoAtivoComNome(string nome);
    Task<Jogo?> ObterPorId(int id);
    Task<IEnumerable<Jogo>> ListarTodos();
    Task<IEnumerable<Jogo>> ListarPublicadosAtivos();
    Task<IEnumerable<Jogo>> ListarJogosAdquiridos(int usuarioId);
    Task<IEnumerable<Jogo>> ListarPorIds(IEnumerable<int> ids);
    Task Adicionar(Jogo jogo);
    Task SalvarAsync();
}
