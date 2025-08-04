using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace Fiap.Cloud.Games.Application.Helper;

public  static class Criptografia
{
    public static string Criptografar(string text, byte[] key)
    {
        byte[] iv = new byte[16];
        byte[] array;

        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;

            using ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using MemoryStream memoryStream = new();
            using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
            using (StreamWriter streamWriter = new(cryptoStream))
            {
                streamWriter.Write(text);
            }

            array = memoryStream.ToArray();
        }

        return Convert.ToBase64String(array);
    }

    public static string Descriptografar(string cryptText, byte[] key)
    {
        byte[] iv = new byte[16];
        byte[] buffer = Convert.FromBase64String(cryptText);

        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        using ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using MemoryStream memoryStream = new(buffer);
        using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
        using StreamReader streamReader = new(cryptoStream);

        return streamReader.ReadToEnd();
    }

    public static byte[] CriarChave(string password, byte[] salt, int keyBytes = 32)
    {
        const int Iterations = 300;
        using Rfc2898DeriveBytes keyGenerator = new(password, salt, Iterations, HashAlgorithmName.SHA1);

        return keyGenerator.GetBytes(keyBytes);
    }

    public static string GerarHash(string senha)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);

        byte[] hash = KeyDerivation.Pbkdf2(
            password: senha,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 30000,
            numBytesRequested: 32);

        byte[] hashBytes = new byte[48];
        Buffer.BlockCopy(salt, 0, hashBytes, 0, 16);
        Buffer.BlockCopy(hash, 0, hashBytes, 16, 32);

        return Convert.ToBase64String(hashBytes);
    }

    public static bool VerificarSenha(string senhaInformada, string hashArmazenado)
    {
        byte[] hashBytes = Convert.FromBase64String(hashArmazenado);

        byte[] salt = new byte[16];
        Buffer.BlockCopy(hashBytes, 0, salt, 0, 16);

        byte[] hash = new byte[32];
        Buffer.BlockCopy(hashBytes, 16, hash, 0, 32);

        byte[] hashDaSenhaInformada = KeyDerivation.Pbkdf2(
            password: senhaInformada,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 30000,
            numBytesRequested: 32);

        return CryptographicOperations.FixedTimeEquals(hashDaSenhaInformada, hash);
    }
}
