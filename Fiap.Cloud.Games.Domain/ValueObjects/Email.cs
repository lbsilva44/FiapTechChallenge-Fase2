
using System.Text.RegularExpressions;

namespace Fiap.Cloud.Games.Domain.ValueObjects;

public class Email
{
    public string? Valor { get; private set; }

    protected Email() { }

    public Email(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("E-mail é obrigatório.");

        try
        {
            var addr = new System.Net.Mail.MailAddress(valor);
            if (addr.Address != valor || string.IsNullOrWhiteSpace(valor) || !Regex.IsMatch(valor, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("E-mail inválido.");
        }
        catch
        {
            throw new ArgumentException("E-mail inválido.");
        }

        Valor = valor;
    }

    public override string ToString() => Valor!;

    public static implicit operator string(Email email) => email.Valor!;
}