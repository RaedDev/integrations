using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Rababa
{
    public delegate void BillBoardCallback(BillBoardData data);

    public struct BillBoardData
    {
        public bool success;
        public Texture2D img;
    }

    public class BillBoardLoader : MonoBehaviour
    {

        [System.Serializable]
        class adData
        {
            public string url;
        }

        private static string url = "https://rababagames.com/rababaads/large.php";
        private string defaultTexturePath = "Jakaro";
        private Texture2D defaultTexture;

        private void Awake()
        {
            defaultTexture = Resources.Load<Texture2D>(defaultTexturePath);
        }

        internal void Load(BillBoardCallback callback)
        {
            StartCoroutine(GetBillBoardFromWeb(callback));
        }

        IEnumerator GetBillBoardFromWeb(BillBoardCallback callback)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();
            string jsonString = string.Empty;
            BillBoardData callbackData = new BillBoardData();
            callbackData.success = false;
            callbackData.img = defaultTexture;
            if (request.isNetworkError || request.error != null)
            {
                callback(callbackData);
                yield break;
            }
            else
            {
                jsonString = request.downloadHandler.text;
            }
            //jsonString = "{\"imgUrl\": \"https://avatars.githubusercontent.com/u/619673?s=64&v=4\",\"linkUrl\": \"http://Google.com\"}";

            adData data = JsonUtility.FromJson<adData>(jsonString);
            if (data.url == null)
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
            callbackData.img = DownloadHandlerTexture.GetContent(request2);
            callback(callbackData);
        }
    }
}