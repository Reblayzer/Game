using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class EncryptionUtils
{
  public static string Encrypt(string plainText, string password, out string saltOut)
  {
    using var aes = Aes.Create();
    byte[] salt = new byte[16];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(salt);

    saltOut = Convert.ToBase64String(salt);
    var key = new Rfc2898DeriveBytes(password, salt, 10000);
    aes.Key = key.GetBytes(32);
    aes.GenerateIV();

    using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
    using var ms = new MemoryStream();
    ms.Write(aes.IV, 0, 16);
    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
    using (var sw = new StreamWriter(cs)) sw.Write(plainText);

    return Convert.ToBase64String(ms.ToArray());
  }

  public static string Decrypt(string cipherText, string password, string saltIn)
  {
    byte[] cipherBytes = Convert.FromBase64String(cipherText);
    byte[] salt = Convert.FromBase64String(saltIn);

    using var aes = Aes.Create();
    var key = new Rfc2898DeriveBytes(password, salt, 10000);
    aes.Key = key.GetBytes(32);

    using var ms = new MemoryStream(cipherBytes);
    byte[] iv = new byte[16];
    ms.Read(iv, 0, 16);
    aes.IV = iv;

    using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
    using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
    using var sr = new StreamReader(cs);

    return sr.ReadToEnd();
  }
}
