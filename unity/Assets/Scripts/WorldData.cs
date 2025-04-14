using System.Collections.Generic;

[System.Serializable]
public class WorldData
{
    public double balance = 0;
    public HashSet<string> ownedPlots = new HashSet<string>();
    public Dictionary<string, int> plotCosts = new Dictionary<string, int>();
}