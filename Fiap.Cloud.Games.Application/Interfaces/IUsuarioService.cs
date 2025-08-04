using Fiap.Cloud.Games.Application.DTOs.Usuario;
using Fiap.Cloud.Games.Domain.Entities;

namespace Fiap.Cloud.Games.Application.Interfaces;

public interface IUsuarioService 
{
    Task<Usuario> RegistrarUsuario(RegistroUsuarioDto user);
    Task<string?> LoginUsuario(LoginUsuarioDto user);
    Task<bool> AlterarSenha(AlterarSenhaDto user);
    Task<bool> RedefinirSenha(RedefinirSenhaDto user);
    Task<bool> AlterarAcessoUsuario(int idUsuario, string TipoAcesso);
    Task<IEnumerable<UsuarioDto>> ListarUsuarios();
    Task<bool> DesativarUsuario( int idUsuario);
    Task<bool> AtivarUsuario(int idUsuario);
    Task Depositar(int usuarioId, decimal valor);
    Task<UsuarioDetalhesDto?> ObterDetalhesUsuario(int usuarioId);
}
