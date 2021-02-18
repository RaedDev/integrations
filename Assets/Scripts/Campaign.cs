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
        new Campaign {points = 1, clicks = 0, link = "rababagames.com", img="", video=null, maxCoverage=1, impressions=0 }
    };

    public string id;

    [FirestoreProperty()] public int points { get; set; }
    [FirestoreProperty()] public int clicks { get; set; }
    [FirestoreProperty()] public string link { get; set; }
    [FirestoreProperty()] public string img { get; set; }
    [FirestoreProperty()] public string video { get; set; }
    [FirestoreProperty()] public float maxCoverage { get; set; }
    [FirestoreProperty()] public int impressions { get; set; }
    [FirestoreProperty()] public DateTime expiryDate { get; set; }
}