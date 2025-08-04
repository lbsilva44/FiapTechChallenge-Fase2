using Fiap.Cloud.Games.API.Extensions;
using Fiap.Cloud.Games.API.Middlewares;
using Fiap.Cloud.Games.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Audit.Core;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using Serilog.Formatting.Compact;
using Fiap.Cloud.Games.Domain.Entities;
using Fiap.Cloud.Games.Domain.Enums;

var builder = WebApplication.CreateBuilder(args);

#region ── Serilog ─────────────────────────────────────────────────────────────
var columnOptions = new ColumnOptions
{
    // garante que o sink use a coluna "Timestamp" exata do seu DB
    TimeStamp = { ColumnName = "Timestamp" }
};
columnOptions.Store.Remove(StandardColumn.MessageTemplate);
columnOptions.Store.Remove(StandardColumn.LogEvent);

var sinkOptions = new MSSqlServerSinkOptions
{
    TableName = "Logs",
    AutoCreateSqlTable = false,
    SchemaName = "dbo",
    BatchPostingLimit = 50,
    BatchPeriod = TimeSpan.FromSeconds(2)
};

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .WriteTo.MSSqlServer(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
        sinkOptions: sinkOptions,
        columnOptions: columnOptions
    )
    .CreateLogger();
builder.Host.UseSerilog();
#endregion

builder.Host.UseSerilog();

#region ── Services ────────────────────────────────────────────────────────────
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddJwtAuthentication(builder.Configuration);
#endregion

var app = builder.Build();

#region ── Pipeline ────────────────────────────────────────────────────────────
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseRedocConfiguration();
//}

// Audit.NET (ativa / desativa via appsettings)
if (!builder.Configuration.GetValue<bool>("AuditEnabled"))
{
    Configuration.Setup()
                 .UseDynamicProvider(_ => { })
                 .WithCreationPolicy(EventCreationPolicy.Manual);
}

// logs de requisição padrão Serilog
app.UseSerilogRequestLogging();

// HTTPS ↓ 
app.UseHttpsRedirection();

// Middleware global de exceções (Problem+JSON)
app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
#endregion

#region ── EF Core Migration on startup ───────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BaseDbContext>();
    await db.Database.MigrateAsync();
    if (!db.Usuarios.Any())
    {
        var usuarioAdmin = Usuario.Criar("Admin", "admin@fcg.com", "Admin@123!", 0, "Admin");
        db.Usuarios.Add(usuarioAdmin);

        var usuarioComum = Usuario.Criar("Usuario", "user@fcg.com", "Senha@123!", 0, "Usuario");
        db.Usuarios.Add(usuarioComum);
        await db.SaveChangesAsync();
    }
}
#endregion

await app.RunAsync();