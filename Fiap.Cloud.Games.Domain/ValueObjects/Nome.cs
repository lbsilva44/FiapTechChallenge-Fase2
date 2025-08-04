using Microsoft.EntityFrameworkCore;

namespace Fiap.Cloud.Games.Domain.ValueObjects
{
    /// <summary>
    /// Value Object para o nome do usuário.
    /// O atributo [Owned] diz ao EF Core que este tipo é incorporado (não tem chave primária).
    /// </summary>
    [Owned]                                 // <- linha fundamental
    public class Nome
    {
        public string Valor { get; private set; } = null!;

        // construtor sem parâmetros exigido pelo EF
        protected Nome() { }

        public Nome(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException("Nome inválido.", nameof(valor));

            Valor = valor.Trim();
        }

        public override string ToString() => Valor;

        // conversão implícita para string
        public static implicit operator string(Nome nome) => nome.Valor;
    }
}
