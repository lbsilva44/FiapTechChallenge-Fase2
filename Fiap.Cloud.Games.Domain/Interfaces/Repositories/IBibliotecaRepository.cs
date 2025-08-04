using Fiap.Cloud.Games.Domain.Entities;
namespace Fiap.Cloud.Games.Domain.Interfaces.Repositories;

public interface IBibliotecaRepository
{
    Task<bool> UsuarioPossuiJogo(int usuarioId, int jogoId);
    Task Adicionar(Biblioteca biblioteca);
}
