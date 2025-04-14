using System;
using System.Collections.Generic;

[Serializable]
public class UserWalletData
{
  public string username;
  public string walletAddress;
  public string encryptedPrivateKey; // AES encrypted
  public string salt;
  public Dictionary<string, WorldData> worlds = new();
}
