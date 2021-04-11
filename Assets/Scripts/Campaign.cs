using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Firebase.Firestore;

[FirestoreData()]
public class Campaign
{
    public static List<Campaign> defaultCampaigns = new List<Campaign>
    {
        new Campaign {points = 1, clicks = 0, link = "rababagames.com", img= new List<string>(), video=new List<string>(), maxCoverage=1, impressions=0 }
    };

    public string id;

    [FirestoreProperty()] public int points { get; set; }
    [FirestoreProperty()] public int clicks { get; set; }
    [FirestoreProperty()] public string link { get; set; }
    [FirestoreProperty()] public List<string> img { get; set; } = new List<string>();
    [FirestoreProperty()] public List<string> video { get; set; } = new List<string>();
    [FirestoreProperty()] public float maxCoverage { get; set; }
    [FirestoreProperty()] public int impressions { get; set; }
    [FirestoreProperty()] public DateTime expiryDate { get; set; }

    public string GetRandomImg()
    {
        if(img != null && img.Count == 0)
        {
            return "";
        }

        return img[UnityEngine.Random.Range(0, img.Count)];
    }

    public string GetRandomVideo()
    {
        if (video != null && video.Count == 0)
        {
            return "";
        }

        return video[UnityEngine.Random.Range(0, video.Count)];
    }
}