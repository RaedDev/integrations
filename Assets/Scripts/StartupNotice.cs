using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartupNotice : MonoBehaviour 
{
    public Text titleText;
    public Button button;

    public static void ShowNote(string type, string description, System.Action action)
    {
        StartupNotice Instance = (Instantiate(Resources.Load("StartupNotice")) as GameObject).GetComponent<StartupNotice>();

        Instance.Setup(type, action);
    }

    public void Setup(string type, System.Action action)
    {
        titleText.text = type;

        button.onClick.AddListener(() =>
        {
            Destroy(gameObject);
            action?.Invoke();

            if(type == "offer")
            {
                if(FirebaseManager.user != null)
                {
                    StorePanel.Show();
                }
            }
        });
    }
}
