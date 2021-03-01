using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Rababa
{

    public class AdsManager : MonoBehaviour
    {
        private static AdsManager _instance = null;
        private static AdsManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new GameObject().AddComponent<AdsManager>();
                _instance.billBoardLoader = _instance.gameObject.AddComponent<BillBoardLoader>();
                _instance.imageLoader = _instance.gameObject.AddComponent<ImageLoader>();
                _instance.videoLoader = _instance.gameObject.AddComponent<VideoLoader>();
                _instance.gameObject.name = "Ads Manager Dehayat";
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
        private BillBoardLoader billBoardLoader;
        private ImageLoader imageLoader;
        private VideoLoader videoLoader;


        public static void GetBillboard(BillBoardCallback callback)
        {
            GetInstance().billBoardLoader.Load(callback);
        }
        public static void GetImage(ImageCallback callback)
        {
            GetInstance().imageLoader.Load(callback);
        }
        public static void GetVideo(VideoCallback callback)
        {
            GetInstance().videoLoader.Load(callback);
        }
    }
}