using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorHandler : MonoBehaviour
{
    public static void Show(string message, string description = "" , System.Action callback = null)
    {
        Debug.LogError(message);

        if(callback != null)
        {
            ErrorPanel.ShowError(message, description, callback);
        }
    }
}
