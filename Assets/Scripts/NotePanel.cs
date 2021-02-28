using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotePanel : MonoBehaviour
{
    public static NotePanel Instance;

    public Text messageText;
    public Text descText;
    public Button button;

    public static void ShowNote(string message, string description, System.Action action)
    {
        if (Instance == null)
        {
            Instance = (Instantiate(Resources.Load("NotePanel")) as GameObject).GetComponent<NotePanel>();
        }

        Instance.Setup(message, description, action);
    }

    public void Setup(string message, string description, System.Action action)
    {
        gameObject.SetActive(true);

        messageText.text = message;
        descText.text = description;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            action?.Invoke();
        });
    }
}
