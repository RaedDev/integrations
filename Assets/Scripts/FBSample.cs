using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FBSample : MonoBehaviour
{
    public LeaderboardPanel leaderboardPanel;

    void Awake()
    {
        FirebaseManager.InitializeFirebase(OnSignedIn, OnSignedOut);
    }

    void OnSignedIn(LoginResult res)
    {
        FindObjectOfType<SigninPanel>().OnLoggedIn(res);
    }

    void OnSignedOut()
    {
        ErrorPanel.ShowError("Signed out", "", () =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });
    }

    public void SignOut()
    {
        FirebaseManager.SignOut();
    }

    public void ShowStore()
    {
        StorePanel.Show();
    }

    public void HideStore()
    {
        StorePanel.Hide();
    }

    public void ShowLeaderboard()
    {
        Leaderboard.GetLeaderboard("test_leaderboard", records =>
        {
            leaderboardPanel.Setup(records);
        });
    }

    public void SubmitScore()
    {
        Leaderboard.SubmitScore("test_leaderboard", Random.Range(1, 10000));
    }

    public void AddFriend()
    {
        if(ProfilePanel.currentUser != null)
        {
            FirebaseManager.AddFriend(ProfilePanel.currentUser.uid);
            Debug.Log("Friend Request sent");
        }
        else
        {
            Debug.Log("No user found");
        }
    }

    public void Link()
    {

    }
}
