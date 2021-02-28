using UnityEngine;
using UnityEngine.UI;

public class StorePanel : MonoBehaviour
{
    public static StorePanel Instance;
    public Button liteButton;
    public Button vipButton;

    public static void Show()
    {
        if (Instance == null)
        {
            Instance = (Instantiate(Resources.Load("StorePanel")) as GameObject).GetComponent<StorePanel>();
        }

        Instance.gameObject.SetActive(true);
    }

    public static void Hide()
    {
        if(Instance == null)
        {
            Debug.LogError("Trying to hide the store panel but it's not shown in the first place!");
            return;
        }
        Instance.gameObject.SetActive(false);
    }

    public void Back()
    {
        Hide();
    }

    void Start()
    {
        bool hasLite = FirebaseManager.user.isLite;
        bool hasVip = FirebaseManager.user.isVIP;

        liteButton.interactable = !hasLite || !hasVip;
        vipButton.interactable = !hasVip;
    }
}
