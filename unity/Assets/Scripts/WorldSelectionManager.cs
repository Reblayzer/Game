using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class WorldSelectionManager : MonoBehaviour
{
    public TMP_Text treasuryWalletText;

    void Start()
    {}

    public void SelectWorld(string worldName)
    {
        SessionManager.Instance.currentWorld = worldName;

        var user = SessionManager.Instance.currentUser;
        var worldData = user.worlds[worldName];

        // Only fund once
        if (!user.fundedWorlds.Contains(worldName))
        {
            double startupFunds = 2000;

            worldData.balance += startupFunds;
            SessionManager.Instance.treasuryBalance -= startupFunds;

            user.fundedWorlds.Add(worldName);
        }

        SceneManager.LoadScene("WorldOverview");
    }
}
