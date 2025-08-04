
namespace Fiap.Cloud.Games.Domain.ValueObjects;

public class Senha
{
    public string Valor { get; private set; }

    protected Senha() { }

    public Senha(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor) || valor.Length < 8)
            throw new ArgumentException("A senha deve ter no mínimo 8 caracteres.");

        bool temMaiuscula = valor.Any(char.IsUpper);
        bool temMinuscula = valor.Any(char.IsLower);
        bool temNumero = valor.Any(char.IsDigit);
        bool temEspecial = valor.Any(c => !char.IsLetterOrDigit(c));

        if (!(temMaiuscula && temMinuscula && temNumero && temEspecial))
            throw new ArgumentException(
                "A senha deve conter letra maiúscula, letra minúscula, número e caractere especial.");

        Valor = valor;         //aqui você pode aplicar hash futuramente
    }

    public override string ToString() => Valor;

    public static implicit operator string(Senha senha) => senha.Valor;
}