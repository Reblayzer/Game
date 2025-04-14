using UnityEngine;
using System.Collections.Generic;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance;

    public User currentUser;
    public string currentWorld; // e.g. "World1", "World2"
    public double treasuryBalance = 1000000000;
    public HashSet<string> treasuryOwnedPlots = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keeps this alive between scenes
        }
        else
        {
            Destroy(gameObject); // Only one allowed
        }
        if (treasuryOwnedPlots.Count == 0)
        {
            treasuryOwnedPlots.Add("Plot1");
            treasuryOwnedPlots.Add("Plot2");
            treasuryOwnedPlots.Add("Plot3");
            treasuryOwnedPlots.Add("Plot4");
        }
    }
}