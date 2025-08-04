using Fiap.Cloud.Games.Application.Interfaces;
using Fiap.Cloud.Games.Application.Services;
using Fiap.Cloud.Games.Domain.Interfaces.Repositories;
using Fiap.Cloud.Games.Infrastructure.Data;
using Fiap.Cloud.Games.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fiap.Cloud.Games.API.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BaseDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IJogoService, JogoService>();
        services.AddScoped<IPromocaoService, PromocaoService>();
        services.AddScoped<IJwtService, JwtService>();

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IPromocaoRepository, PromocaoRepository>();
        services.AddScoped<IBibliotecaRepository, BibliotecaRepository>();
        services.AddScoped<IJogoRepository, JogoRepository>();

        return services;
    }
}