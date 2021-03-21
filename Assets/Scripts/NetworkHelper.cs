using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class NetworkHelper {
    public static void DownloadSprite(MonoBehaviour sender, string img, System.Action<Sprite> onSuccess, System.Action onError = null)
    {
        sender.StartCoroutine(_DownloadSprite(img, onSuccess, onError));
    }

    static IEnumerator _DownloadSprite(string img, System.Action<Sprite> onSuccess, System.Action onError = null)
    {
        if(string.IsNullOrEmpty(img))
        {
            onError?.Invoke();
            yield break;
        }
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(img);
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error + " " + img);
            onError?.Invoke();
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            Rect rec = new Rect(0, 0, texture.width, texture.height);

            Sprite userSprite = Sprite.Create(texture, rec, new Vector2(0.5f, 0.5f), 100);
            onSuccess(userSprite);
        }
    }
}
