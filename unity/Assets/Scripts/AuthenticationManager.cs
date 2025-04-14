using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AuthenticationManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public WalletManager walletManager;
    public Button registerButton;
    public Button loginButton;
    public TMP_Text feedbackText;

    private List<User> users = new List<User>();

    void Start()
    {
        registerButton.onClick.AddListener(Register);
        loginButton.onClick.AddListener(Login);
    }

    void Register()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            feedbackText.text = "Nickname and Password are required.";
            return;
        }

        foreach (var user in users)
        {
            if (user.username == username)
            {
                feedbackText.text = "Nickname already exists.";
                return;
            }
        }

        WalletStorage.SaveWallet(username, password);
        var walletData = WalletStorage.LoadWallet(username, password);
        if (walletData == null)
        {
            feedbackText.text = "⚠️ Wallet creation failed.";
            return;
        }

        var newUser = new User(username, password)
        {
            hasWallet = true,
            walletAddress = walletData.walletAddress
        };

        users.Add(newUser);
        feedbackText.text = $"✅ Registered! Wallet: {walletData.walletAddress}";
        ClearInputs();
    }

    void Login()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text.Trim();

        foreach (var user in users)
        {
            if (user.username == username)
            {
                if (user.password == password)
                {
                    // 🧠 Save user to session
                    SessionManager.Instance.currentUser = user;

                    // 🔐 Load wallet from keystore
                    bool walletLoaded = walletManager.LoadWallet(username, password);

                    if (!walletLoaded)
                    {
                        feedbackText.text = "Wallet load failed. Corrupted or incorrect password?";
                        return;
                    }

                    feedbackText.text = $"Login successful. Welcome, {username}!";
                    ClearInputs();
                    SceneManager.LoadScene("WorldSelection");
                    return;
                }
                else
                {
                    feedbackText.text = "Incorrect password.";
                    return;
                }
            }
        }

        feedbackText.text = "User does not exist.";
    }


    void ClearInputs()
    {
        usernameInput.text = "";
        passwordInput.text = "";
    }
}
