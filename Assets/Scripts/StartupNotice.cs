using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartupNotice : MonoBehaviour 
{
    public Text titleText;
    public Button button;
    public Image noticeSprite;

    public static void ShowNote(string type, string img, System.Action action)
    {
        StartupNotice Instance = (Instantiate(Resources.Load("StartupNotice")) as GameObject).GetComponent<StartupNotice>();

        Instance.Setup(type, img, action);
    }

    public void Setup(string type, string img, System.Action action)
    {
        titleText.text = type;
        NetworkHelper.DownloadSprite(this, img, sprite =>
        {
            noticeSprite.sprite = sprite;
        });


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
