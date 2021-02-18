using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SigninPanel : MonoBehaviour
{
    public GameObject emailPanel;
    public GameObject newUserPanel;

    public InputField emailField;
    public InputField passwordField;

    public void SigninWithGoogle()
    {
        FirebaseManager.Test();
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

        FirebaseManager.SigninWithEmail(emailField.text, passwordField.text, OnLoggedIn);
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

        FirebaseManager.SignupWithEmail(emailField.text, passwordField.text, OnLoggedIn);
    }

    void OnLoggedIn(LoginResult result)
    {
        if (result == LoginResult.NewUser)
        {
            newUserPanel.SetActive(true);
        }
        else if (result == LoginResult.Successful)
        {
            print("Done!");
        }
    }
    
    public void Back()
    {
        emailPanel.SetActive(false);
    }
}
