
using Fiap.Cloud.Games.Domain.Enums;

namespace Fiap.Cloud.Games.Application.DTOs.Jogo;

public class JogoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public TipoJogo Tipo { get; set; }
    public bool? Ativo { get; set; } 
    public bool? Publicado { get; set; }
}
