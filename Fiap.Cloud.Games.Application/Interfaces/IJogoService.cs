using Fiap.Cloud.Games.Application.DTOs.Jogo;

namespace Fiap.Cloud.Games.Application.Interfaces;

public interface IJogoService
{
    Task CadastrarJogo(JogoDto dto);

    Task PublicarJogo(int id);

    Task AtivarJogo(int id);

    Task DesativarJogo(int id);

    Task<IEnumerable<JogoDto>> ListarJogos(string role);

    Task AdquirirJogo(int idJogo, int idUsuario);

    Task<IEnumerable<JogoDto>> ListarJogosAdquiridosUsuario(int idUsuario);
}