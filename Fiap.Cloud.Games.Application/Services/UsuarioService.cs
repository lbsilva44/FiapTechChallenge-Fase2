using Fiap.Cloud.Games.Application.Interfaces;
using Fiap.Cloud.Games.Domain.Entities;
using Fiap.Cloud.Games.Domain.Enums;
using Fiap.Cloud.Games.Application.DTOs.Usuario;
using Fiap.Cloud.Games.Application.DTOs.Jogo;
using Fiap.Cloud.Games.Application.Helper;
using Fiap.Cloud.Games.Domain.Interfaces.Repositories;

namespace Fiap.Cloud.Games.Application.Services;

public class UsuarioService(IUsuarioRepository repo, IJwtService jwtService) : IUsuarioService
{
    public async Task<Usuario> RegistrarUsuario(RegistroUsuarioDto user)
    {
        if (await repo.EmailExiste(user.Email))
            throw new Exception("Email já cadastrado.");

        string hash = Criptografia.GerarHash(user.Senha);
        var usuario = new Usuario(user.Nome, user.Email, hash, 0);
        repo.Adicionar(usuario);
        await repo.SalvarAsync();
        return usuario;
    }

    public async Task<string?> LoginUsuario(LoginUsuarioDto user)
    {
        var usuario = await repo.ObterPorEmail(user.Email);

        if (usuario is null || !Criptografia.VerificarSenha(user.Senha, usuario.Senha.Valor))
            return null;
        return jwtService.GerarToken(usuario);
    }

    public async Task<bool> AlterarSenha(AlterarSenhaDto dto)
    {
        var usuario = await repo.ObterPorEmail(dto.Email);
        if (usuario is null) return false;

        var senhaValida = Criptografia.VerificarSenha(dto.SenhaAtual, usuario.Senha.Valor);
        if (!senhaValida) return false;

        var novaSenhaHash = Criptografia.GerarHash(dto.NovaSenha);
        usuario.AlterarSenha(novaSenhaHash);

        await repo.SalvarAsync();
        return true;
    }

    public async Task<bool> RedefinirSenha(RedefinirSenhaDto dto)
    {
        var usuario = await repo.ObterPorEmail(dto.Email);
        if (usuario is null) return false;

        var novaSenhaHash = Criptografia.GerarHash(dto.NovaSenha);
        usuario.AlterarSenha(novaSenhaHash);

        await repo.SalvarAsync();
        return true;
    }

    public async Task<bool> AlterarAcessoUsuario(int idUsuario, string novaRole)
    {
        var usuario = await repo.ObterPorId(idUsuario)
            ?? throw new ArgumentException("Usuário não encontrado.");

        TipoAcesso roleValida;

        if (int.TryParse(novaRole, out var numero))
        {
            if (!Enum.IsDefined(typeof(TipoAcesso), numero))
                throw new ArgumentException("Tipo de acesso inválido. Aceito apenas 'Admin' (1) ou 'Usuario' (2).");

            roleValida = (TipoAcesso)numero;
        }
        else
        {
            if (!Enum.TryParse<TipoAcesso>(novaRole, true, out roleValida))
                throw new ArgumentException("Tipo de acesso inválido. Aceito apenas 'Admin' ou 'Usuario'.");
        }

        usuario.AlterarRole(roleValida.ToString());
        await repo.SalvarAsync();
        return true;
    }

    public async Task<IEnumerable<UsuarioDto>> ListarUsuarios()
    {
        var usuarios = await repo.ListarTodos();

        return usuarios.Select(u => new UsuarioDto
        {
            Id = u.Id,
            Nome = u.Nome,
            Email = u.Email.Valor!,
            Role = u.Role,
            Ativo = u.Ativo,
            DataCadastro = u.DataCadastro
        });
    }

    public async Task<bool> DesativarUsuario(int idUsuario)
    {
        var usuario = await repo.ObterPorId(idUsuario)
            ?? throw new ArgumentException("Usuário não encontrado.");

        if (!usuario.Ativo)
            throw new InvalidOperationException("Usuário já está desativado.");

        usuario.Desativar();
        await repo.SalvarAsync();
        return true;
    }

    public async Task<bool> AtivarUsuario(int idUsuario)
    {
        var usuario = await repo.ObterPorId(idUsuario)
            ?? throw new ArgumentException("Usuário não encontrado.");

        if (usuario.Ativo)
            throw new InvalidOperationException("Usuário já está ativo.");

        usuario.Ativar();
        await repo.SalvarAsync();
        return true;
    }

    public async Task Depositar(int usuarioId, decimal valor)
    {
        var usuario = await repo.ObterComCarteira(usuarioId)
            ?? throw new ArgumentException("Usuário não encontrado.");

        usuario.Depositar(valor);
        await repo.SalvarAsync();
    }

    public async Task<UsuarioDetalhesDto?> ObterDetalhesUsuario(int usuarioId)
    {
        var usuario = await repo.ObterComMovimentosEBiblioteca(usuarioId);
        if (usuario is null) return null;

        return new UsuarioDetalhesDto
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email.Valor!,
            DataCadastro = usuario.DataCadastro,
            Role = usuario.Role,
            Saldo = usuario.Carteira.Saldo,

            Biblioteca = usuario.Bibliotecas
                .Select(b => new JogoBibliotecaDto
                {
                    JogoId = b.JogoId,
                    Titulo = b.Jogo.Nome,
                    Preco = b.Jogo.Preco,
                    DataAquisicao = b.DataAquisicao
                })
                .ToList(),

            Movimentos = usuario.Movimentos
                .OrderByDescending(m => m.DataHora)
                .Select(m => new MovimentoCarteiraDto
                {
                    Id = m.Id,
                    Tipo = m.Tipo.ToString(),
                    Valor = m.Valor,
                    SaldoAntes = m.SaldoAntes,
                    SaldoDepois = m.SaldoDepois,
                    JogoId = m.JogoId,
                    DataHora = m.DataHora
                })
                .ToList()
        };
    }
}
