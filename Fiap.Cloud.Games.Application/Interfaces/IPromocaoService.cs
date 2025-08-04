
using Fiap.Cloud.Games.Application.DTOs.Jogo;
using Fiap.Cloud.Games.Domain.Entities;

namespace Fiap.Cloud.Games.Application.Interfaces;

public interface IPromocaoService
{
    Task<int> CriarPromocao(CriarPromocaoDto dto);
    Task AtivarPromocao(int id);
    Task ExcluirPromocao(int id);
    Task AdicionarJogoPromocao(int promocaoId, int jogoId);
    Task<IEnumerable<PromocaoDto>> ListarPromocao(bool somenteAtivas, bool isAdmin);
    Task AdquirirPromocao(int usuarioId, int promocaoId);
}