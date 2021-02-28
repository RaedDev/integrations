using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorPanel : MonoBehaviour
{
    public static ErrorPanel Instance;

    public Text messageText;
    public Text descText;
    public Button button;

    public static void ShowError(string message, string description, System.Action action)
    {
        if (Instance == null)
        {
            Instance = (Instantiate(Resources.Load("ErrorPanel")) as GameObject).GetComponent<ErrorPanel>();
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
