using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartupNotice : MonoBehaviour 
{
    public Text titleText;
    public Button button;

    public static void ShowNote(string title, string description, System.Action action)
    {
        StartupNotice Instance = (Instantiate(Resources.Load("StartupNotice")) as GameObject).GetComponent<StartupNotice>();

        Instance.Setup(title, action);
    }

    public void Setup(string title, System.Action action)
    {
        titleText.text = title;

        button.onClick.AddListener(() =>
        {
            if(action == null)
            {
                Destroy(gameObject);
            } 
            else
            {
                action();
            }
        });
    }
}
