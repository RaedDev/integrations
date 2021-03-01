using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SigninPanel : MonoBehaviour
{
    public GameObject emailPanel;
    public GameObject newUserPanel;
    public ProfilePanel profilePanel;

    public InputField emailField;
    public InputField passwordField;

    void Start()
    {
        if(FirebaseManager.auth.CurrentUser != null)
        {
            if(FirebaseManager.user != null)
            {
                OnLoggedIn(LoginResult.Successful);
            }
            else
            {
                OnLoggedIn(LoginResult.NewUser);
            }
        }
    }

    public void SigninWithGoogle()
    {
        FirebaseManager.AddUser();
    }

    public void SigninWithApple()
    {

    }

    public void GoToSignWithEmail()
    {
        emailPanel.SetActive(true);
    }

    public void SigninWithEmail()
    {
        if (!emailField.text.Contains("@"))
        {
            Debug.LogError("Email invalid");
        }

        if(passwordField.text.Length < 6)
        {
            Debug.LogError("Password is too short");
        }

        FirebaseManager.SigninWithEmail(emailField.text, passwordField.text);

        LoadingPanel.ShowLoading();
    }

    public void SignupWithEmail()
    {
        if (!emailField.text.Contains("@"))
        {
            Debug.LogError("Email invalid");
        }

        if (passwordField.text.Length < 6)
        {
            Debug.LogError("Password is too short");
        }

        FirebaseManager.SignupWithEmail(emailField.text, passwordField.text);
    }

    public void SignInAnonymously()
    {
        LoadingPanel.ShowLoading();
        FirebaseManager.SigninAnonymously();
    }

    public void OnLoggedIn(LoginResult result)
    {
        if (result == LoginResult.NewUser)
        {
            newUserPanel.SetActive(true);
        }
        else if (result == LoginResult.Successful)
        {
            FirebaseManager.GetUser(FirebaseManager.auth.CurrentUser.UserId, u =>
            {
                profilePanel.Setup(FirebaseManager.user);
            });
        }
    }
    
    public void Back()
    {
        emailPanel.SetActive(false);
    }
}
