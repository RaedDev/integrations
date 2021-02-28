using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Extensions;

public class Chat : MonoBehaviour
{
    public static Chat Instance;

    public InputField inputField;
    public GameObject messagePrefab;
    public Transform content;
    public ScrollRect scrollRect;

    public Button closeOpenButton;
    RectTransform panel;
    RectTransform btnRect;

    User other;
    bool isOpen = false;
    string conversationId = "";

    void Awake()
    {
        Instance = this;
        panel = transform.GetChild(0).GetComponent<RectTransform>();
        panel.gameObject.SetActive(true);
        btnRect = closeOpenButton.GetComponent<RectTransform>();
        Close();
    }

    public void Setup(User otherUser)
    {
        other = otherUser;
        Clean();

        FirebaseManager.GetConversationWith(otherUser, conv =>
        {
            conversationId = conv;
            FirebaseManager.GetConversationRef(conversationId).ChildAdded += MessageAdded;
            Open();
        });
    }

    void Clean()
    {
        // Clean UI
        foreach (Transform t in content)
        {
            Destroy(t.gameObject);
        }

        // Clean events
        if(conversationId != "")
        {
            FirebaseManager.GetConversationRef(conversationId).ChildAdded -= MessageAdded;
        }
    }

    void MessageAdded(object sender, Firebase.Database.ChildChangedEventArgs args)
    {
        if (args.Snapshot != null)
        {
            var dict = args.Snapshot.Value as Dictionary<string, object>;
            string msg = ((dict["sender"] as string) == other.uid ? other.username : FirebaseManager.user.username) + ": " + dict["message"];

            _SendMessage(msg);
        }
    }

    public void SendMessage()
    {
        string message = inputField.text;
        if (message.Trim() == "") return;

        //_SendMessage(FirebaseManager.user.username + ": " + message);
        var chatMessage = new Dictionary<string, object>()
        {
            { "message", message },
            { "sender", FirebaseManager.user.uid },
            { "timestamp", Firebase.Database.ServerValue.Timestamp},
        };

        FirebaseManager.GetConversationRef(conversationId).Push().SetValueAsync(chatMessage).ContinueWithOnMainThread(task =>
        {
        });
    }

    void _SendMessage(string message)
    {
        var o = Instantiate(messagePrefab, content);
        o.GetComponent<Text>().text = message;

        Canvas.ForceUpdateCanvases();

        o.GetComponent<ContentSizeFitter>().SetLayoutVertical();

        scrollRect.content.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
        scrollRect.content.GetComponent<ContentSizeFitter>().SetLayoutVertical();

        scrollRect.verticalNormalizedPosition = 0;
    }

    public void Close()
    {
        isOpen = false;
        panel.anchoredPosition = new Vector2(-120, panel.anchoredPosition.y);
        closeOpenButton.GetComponentInChildren<Text>().text = "Open";
        btnRect.anchoredPosition = new Vector3(33, btnRect.anchoredPosition.y);
    }

    public void Open()
    {
        isOpen = true;
        panel.anchoredPosition = new Vector2(120, panel.anchoredPosition.y);
        closeOpenButton.GetComponentInChildren<Text>().text = "Close";
        btnRect.anchoredPosition = new Vector3(-33, btnRect.anchoredPosition.y);
    }

    public void Toggle()
    {
        if (isOpen) Close(); else Open();
    }
}
