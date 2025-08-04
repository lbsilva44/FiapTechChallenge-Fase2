using Fiap.Cloud.Games.Domain.Entities;

namespace Fiap.Cloud.Games.Domain.Interfaces.Repositories;

public interface IUsuarioRepository
{
    Task<bool> EmailExiste(string email);
    Task<Usuario?> ObterPorEmail(string email);
    Task<Usuario?> ObterPorId(int id);
    Task<IEnumerable<Usuario>> ListarTodos();
    Task<Usuario?> ObterComMovimentosEBiblioteca(int id);
    Task<Usuario?> ObterComCarteira(int id);
    void Adicionar(Usuario usuario);
    Task SalvarAsync();
}
