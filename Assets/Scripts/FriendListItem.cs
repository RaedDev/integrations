using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;

public class FriendListItem : MonoBehaviour {

    public Text usernameText;
    string uid;

    public void Setup(string _uid, string username)
    {
        uid = _uid;
        usernameText.text = username;
    }

    public void SendGift()
    {
        FirebaseManager.SendGift(uid, ProductIDs.vip.ToString(), OnSuccess, OnError);
    }

    public void Message()
    {
        FirebaseManager.GetUser(uid, user =>
        {
            Chat.Instance.Setup(user);
        });
    }

    void OnSuccess()
    {
        NotePanel.ShowNote("Success", "Successfully sent gift to " + usernameText.text, null);
    }

    void OnError()
    {
        SendGift();
    }
}
