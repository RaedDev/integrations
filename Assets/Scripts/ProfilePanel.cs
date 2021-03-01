using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Firebase.Extensions;

public class ProfilePanel : MonoBehaviour
{
    public Text nameText;
    public Text bioText;
    public Image image;

    public Transform requestsContent;
    public GameObject requestPrefab;

    public Transform friendsContent;
    public GameObject friendPrefab;

    public static User currentUser;

    public void Setup(User user)
    {
        currentUser = user;
        gameObject.SetActive(true);

        nameText.text = user.username;
        bioText.text = user.bio;

        SetupFriendRequests(user);
        SetupFriendList(user);


        NetworkHelper.DownloadSprite(this, user.img, sprite => image.sprite = sprite);
    }

    void SetupFriendRequests(User user)
    {
        foreach(Transform t in requestsContent)
        {
            Destroy(t.gameObject);
        }

        if (user.requests != null)
        {
            foreach (var req in user.requests)
            {
                FirebaseManager.GetUser(req, u =>
                {
                    var o = Instantiate(requestPrefab, requestsContent);
                    o.GetComponent<FriendRequestItem>().Setup(req, u.username);
                });
            }
        }
    }

    void SetupFriendList(User user)
    {
        foreach (Transform t in friendsContent)
        {
            Destroy(t.gameObject);
        }

        foreach (string friend in user.friends)
        {
            FirebaseManager.usersCollection.Document(friend).GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    ErrorHandler.Show("Unable to find friend data, please check your internet connection");
                    return;
                }
                var u = task.Result.ConvertTo<User>();
                u.uid = task.Result.Id;
                var o = Instantiate(friendPrefab, friendsContent);
                o.GetComponent<FriendListItem>().Setup(u.uid, u.username);
            });
        }
    }
}
