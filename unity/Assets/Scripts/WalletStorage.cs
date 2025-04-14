using UnityEngine;
using System.IO;
using Nethereum.Signer;
using Nethereum.KeyStore;
using System;

public static class WalletStorage
{
  private static readonly string WalletDir = Application.persistentDataPath;

  public static void SaveWallet(string username, string password)
  {
    var ecKey = EthECKey.GenerateKey();
    var privateKeyBytes = ecKey.GetPrivateKeyAsBytes();
    var address = ecKey.GetPublicAddress();

    var keyStoreService = new KeyStoreService();
    var json = keyStoreService.EncryptAndGenerateDefaultKeyStoreAsJson(password, privateKeyBytes, address);

    var path = GetWalletPath(username);
    File.WriteAllText(path, json);

    Debug.Log($"✅ Wallet saved for {username}: {address}");
  }

  public static WalletData LoadWallet(string username, string password)
  {
    var path = GetWalletPath(username);
    if (!File.Exists(path))
    {
      Debug.LogError("❌ Wallet not found");
      return null;
    }

    try
    {
      var json = File.ReadAllText(path);
      var keyStoreService = new KeyStoreService();
      var privateKey = keyStoreService.DecryptKeyStoreFromJson(password, json);
      var ecKey = new EthECKey(privateKey, true);
      var address = ecKey.GetPublicAddress();

      Debug.Log("🔓 Decrypted private key successfully");
      return new WalletData
      {
        privateKey = privateKey,
        walletAddress = address
      };
    }
    catch (System.Exception ex)
    {
      Debug.LogError($"❌ Decryption failed: {ex.Message}");
      return null;
    }
  }

  public static string GetWalletPath(string username)
  {
    return Path.Combine(WalletDir, $"wallet-{username}.json");
  }
}

