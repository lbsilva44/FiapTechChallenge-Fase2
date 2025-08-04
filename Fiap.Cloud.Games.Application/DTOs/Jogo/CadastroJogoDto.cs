
namespace Fiap.Cloud.Games.Application.DTOs.Jogo;

public class CadastroJogoDto
{
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int Tipo { get; set; } // Recebemos o valor numérico do Enum no request
}
