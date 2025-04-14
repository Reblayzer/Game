using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class User
{
    public string username;
    public string password;
    public Dictionary<string, WorldData> worlds = new Dictionary<string, WorldData>();
    public HashSet<string> fundedWorlds = new HashSet<string>();

    public string walletAddress;
    public bool hasWallet;

    public User(string username, string password)
    {
        this.username = username;
        this.password = password;
        worlds["World1"] = new WorldData();
        worlds["World2"] = new WorldData();
    }
}
