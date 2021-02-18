using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateUserPanel : MonoBehaviour
{
    public InputField username;
    public InputField bio;

    public ProfilePanel profilePanel;

    public void Submit()
    {
        User u = new User() { bio = bio.text, username = username.text };
        FirebaseManager.SetUser(u);

        profilePanel.gameObject.SetActive(true);
        profilePanel.Setup(u);
    }
}
