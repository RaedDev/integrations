using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendRequestItem : MonoBehaviour
{
    public Text usernameText;
    string uid;

    public void Setup(string _uid, string username)
    {
        uid = _uid;
        usernameText.text = username;
    }

    public void Accept()
    {
        FirebaseManager.AcceptFriend(uid);
        Destroy(gameObject);
    }

    public void Reject()
    {
        FirebaseManager.RejectFriend(uid);
        Destroy(gameObject);
    }
}
