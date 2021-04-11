using System;
using System.Collections.Generic;
using UnityEngine;

using Firebase.Firestore;

[FirestoreData()]
public class User
{
    public bool isVIP;
    public bool isLite;
    public bool removeAds { get { return isVIP || isLite; } }
    
    public string uid;

    [FirestoreProperty()] public string username { get; set; }
    [FirestoreProperty()] public string id { get; set; }
    [FirestoreProperty()] public string email { get; set; }
    [FirestoreProperty()] public string bio { get; set; }
    [FirestoreProperty()] public string img { get; set; }
    [FirestoreProperty()] public List<Gift> gifts { get; set; } = new List<Gift>();
    [FirestoreProperty()] public List<string> friends { get; set; } = new List<string>();
    [FirestoreProperty()] public List<string> requests { get; set; } = new List<string>();
    [FirestoreProperty()] public List<string> sentRequests { get; set; } = new List<string>();

    public bool isMe { get { return uid == FirebaseManager.user.uid; } }
}

public enum ProductIDs
{
    lite, vip, vipGift
    //public static string lite = "lite";
    //public static string vip = "vip";
    //public static string vipGift = "vip_gift";
}

[FirestoreData()]
public class Gift
{
    [FirestoreProperty()] public DateTime expiryDate { get; set; } = new DateTime();
    [FirestoreProperty()] public string product { get; set; }
    [FirestoreProperty()] public string sender { get; set; }
}
