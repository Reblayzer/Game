using System.IO;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.KeyStore;
using Nethereum.Signer;
using UnityEngine;

public class WalletManager : MonoBehaviour
{
    private Web3 web3;
    private string currentWalletAddress;

    public static string GetWalletPath(string username)
    {
        return Path.Combine(Application.persistentDataPath, $"wallet-{username}.json");
    }

    public void CreateWallet(string username, string password)
    {
        var ecKey = EthECKey.GenerateKey();
        var privateKeyBytes = ecKey.GetPrivateKeyAsBytes();
        var address = ecKey.GetPublicAddress();

        currentWalletAddress = address;

        var keyStoreService = new KeyStoreService();
        var json = keyStoreService.EncryptAndGenerateDefaultKeyStoreAsJson(password, privateKeyBytes, address);

        var path = GetWalletPath(username);
        File.WriteAllText(path, json);

        Debug.Log($"✅ Wallet created and saved: {address} to {path}");
    }

    public bool LoadWallet(string username, string password)
    {
        string userWalletPath = GetWalletPath(username);

        if (!File.Exists(userWalletPath))
        {
            Debug.LogError("❌ Wallet not found");
            return false;
        }

        var keyStoreService = new KeyStoreService();
        var json = File.ReadAllText(userWalletPath);
        var privateKey = keyStoreService.DecryptKeyStoreFromJson(password, json);

        var account = new Account(privateKey);
        web3 = new Web3(account, "https://rpc-testnet.hydrachain.org");
        currentWalletAddress = account.Address;

        Debug.Log($"🔓 Wallet loaded: {currentWalletAddress}");
        return true;
    }

    public Web3 GetWeb3() => web3;
    public string GetWalletAddress() => currentWalletAddress;
}
