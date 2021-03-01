using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Rababa
{
    public delegate void ImageCallback(ImageData data);


    public struct ImageData
    {
        public bool success;
        public Texture2D img;
        public string link;
        public bool reset;
    }

    public class ImageLoader : MonoBehaviour
    {

        [System.Serializable]
        class adData
        {
            public string url;
            public string link;
            public string reset;
        }

        private static string url = "https://rababagames.com/rababaads/large.php";
        private string defaultTexturePath = "ads";
        private string defaultLink = "";
        private Texture2D defaultTexture;

        private void Awake()
        {
            defaultTexture = Resources.Load<Texture2D>(defaultTexturePath);
        }

        internal void Load(ImageCallback callback)
        {
            StartCoroutine(GetBillBoardFromWeb(callback));
        }

        IEnumerator GetBillBoardFromWeb(ImageCallback callback)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();
            string jsonString = string.Empty;

            ImageData callbackData = new ImageData();
            callbackData.success = false;
            callbackData.link = null;
            callbackData.img = null;

            if (request.isNetworkError || request.error != null)
            {
                callback(callbackData);
                yield break;
            }
            else
            {
                jsonString = request.downloadHandler.text;
            }
            //jsonString = "{\"imgUrl\": \"http://www.objgen.com/images/objgen-logo-sm.png\",\"linkUrl\": \"http://Google.com\"}";

            adData data = JsonUtility.FromJson<adData>(jsonString);
            if (data.url == null || data.link == null)
            {
                callback(callbackData);
                yield break;
            }

            var request2 = UnityWebRequest.Get(data.url);
            request2.downloadHandler = new DownloadHandlerTexture();
            yield return request2.SendWebRequest();
            if (request2.isNetworkError || request2.error != null)
            {
                callback(callbackData);
                yield break;
            }
            yield return new WaitUntil(() => request.downloadHandler.isDone);
            callbackData.success = true;
            callbackData.link = data.link;
            callbackData.reset = data.reset != "0";
            callbackData.img = DownloadHandlerTexture.GetContent(request2);
            callback(callbackData);
        }
    }
}