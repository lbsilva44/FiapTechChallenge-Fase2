
using Fiap.Cloud.Games.Domain.Entities;

namespace Fiap.Cloud.Games.Application.Interfaces;

public interface IJwtService
{
    string GerarToken(Usuario usuario);
}