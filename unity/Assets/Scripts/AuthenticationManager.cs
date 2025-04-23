using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AuthenticationManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField loginUsernameInput;
    public TMP_InputField loginPrivateKeyInput;
    public TMP_InputField registerUsernameInput;
    public TMP_InputField registerWalletAddressInput;
    public WalletManager walletManager;
    public Button registerButton;
    public Button loginButton;
    public TMP_Text loginFeedback;
    public TMP_Text registerFeedback;

    private List<User> users = new List<User>();

    void Start()
    {
        registerButton.onClick.AddListener(Register);
        loginButton.onClick.AddListener(Login);
    }

    void Register()
    {
        string username = registerUsernameInput.text.Trim();
        string walletAddress = registerWalletAddressInput.text.Trim();



        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(walletAddress))
        {
            registerFeedback.text = "Username and Private key are required.";
            return;
        }

        foreach (var user in users)
        {
            if (user.username == username)
            {
                registerFeedback.text = "Username already exists.";
                return;
            }
        }

        WalletStorage.SaveWallet(username, walletAddress);
        var walletData = WalletStorage.LoadWallet(username, walletAddress);
        if (walletData == null)
        {
            registerFeedback.text = "PlayerNFT creation failed.";
            return;
        }

        var newUser = new User(username, walletAddress)
        {
            hasWallet = true,
            walletAddress = walletData.walletAddress
        };

        users.Add(newUser);
        registerFeedback.text = $"{username} successfully registered!";
        ClearInputs();
    }

    void Login()
    {
        string username = loginUsernameInput.text.Trim();
        string privateKey = loginPrivateKeyInput.text.Trim();

        foreach (var user in users)
        {
            if (user.username == username)
            {
                if (user.walletAddress == privateKey)
                {
                    // 🧠 Save user to session
                    SessionManager.Instance.currentUser = user;

                    // 🔐 Load wallet from keystore
                    bool walletLoaded = walletManager.LoadWallet(username, privateKey);

                    if (!walletLoaded)
                    {
                        loginFeedback.text = "Login error";
                        return;
                    }

                    loginFeedback.text = $"Login successful. Welcome, {username}!";
                    ClearInputs();
                    SceneManager.LoadScene("WorldSelection");
                    return;
                }
                else
                {
                    loginFeedback.text = "Incorrect Private key.";
                    return;
                }
            }
        }

        loginFeedback.text = "Username does not exist.";
    }


    void ClearInputs()
    {
        loginUsernameInput.text = "";
        loginPrivateKeyInput.text = "";
    }
}
