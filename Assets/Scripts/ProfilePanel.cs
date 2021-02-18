using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ProfilePanel : MonoBehaviour
{
    public Text nameText;
    public Text bioText;
    public Image image;

    public void Setup(User user)
    {
        StartCoroutine(_Setup(user));
    }

    IEnumerator _Setup(User user)
    {
        nameText.text = user.username;
        bioText.text = user.bio;

        if (user.img != null)
        {

            UnityWebRequest www = UnityWebRequestTexture.GetTexture(user.img);
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                Rect rec = new Rect(0, 0, texture.width, texture.height);

                Sprite userSprite = Sprite.Create(texture, rec, new Vector2(0.5f, 0.5f), 100);
                image.sprite = userSprite;
            }
        }
    }
}
