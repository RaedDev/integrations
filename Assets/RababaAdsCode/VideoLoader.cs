using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

namespace Rababa
{
    public delegate void VideoCallback(VideoData data);

    public struct VideoData
    {
        public bool success;
        public RenderTexture texture;
        public string link;
        public VideoPlayer videoPlayer;
        public bool skip;
        public bool audio;
        public bool reset;
    }

    public class VideoLoader : MonoBehaviour
    {

        [System.Serializable]
        class adData
        {
            public string url;
            public string link;
            public string audio;
            public string skip;
            public string reset;
        }

        private static string url = "https://rababagames.com/rababaads/medium.php";
        private RenderTexture videoTexture;
        private VideoPlayer videoPlayer;

        private GameObject VideoPlayerContainer;
        private string VideoPlayerContainerPath = "VideoPlayerContainer";

        private void Awake()
        {

            VideoPlayerContainer = Instantiate(Resources.Load<GameObject>(VideoPlayerContainerPath), transform);

            videoTexture = new RenderTexture(256, 256, 24);

            videoPlayer = VideoPlayerContainer.GetComponent<VideoPlayer>();
            videoPlayer.targetTexture = videoTexture;
        }

        internal void Load(VideoCallback callback)
        {
            StartCoroutine(GetBillBoardFromWeb(callback));
        }

        IEnumerator GetBillBoardFromWeb(VideoCallback callback)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            VideoData callBackData = new VideoData();
            callBackData.success = false;
            callBackData.link = null;
            callBackData.texture = null;
            callBackData.videoPlayer = null;

            string jsonString = string.Empty;
            if (request.isNetworkError || request.error != null)
            {
                callback(callBackData);
                yield break;
            }
            else
            {
                jsonString = request.downloadHandler.text;
            }
            //jsonString = "{\"vidUrl\": \"http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4\",\"linkUrl\": \"http://Google.com\"}";

            adData data = JsonUtility.FromJson<adData>(jsonString);
            if (data.url == null || data.link == null)
            {
                callback(callBackData);
                yield break;
            }

            videoPlayer.url = data.url;
            callBackData.success = true;
            callBackData.link = data.link;
            callBackData.texture = videoTexture;
            callBackData.videoPlayer = videoPlayer;
            callBackData.audio = data.audio != "off";
            callBackData.skip = data.skip != "off";
            callBackData.reset = data.reset != "0";
            callBackData.videoPlayer = videoPlayer;
            callback(callBackData);
        }
    }
}