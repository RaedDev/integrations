using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingPanel : MonoBehaviour 
{
    public static LoadingPanel Instance;
    public static void ShowLoading()
    {
        if (Instance == null)
        {
            Instance = (Instantiate(Resources.Load("LoadingPanel")) as GameObject).GetComponent<LoadingPanel>();
        }

        Instance.gameObject.SetActive(true);
    }

    public static void Hide()
    {
        if(Instance)
        {
            Instance.gameObject.SetActive(false);
        }
    }
}
