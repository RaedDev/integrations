using System;
using System.Collections.Generic;
using UnityEngine;

using Firebase.Firestore;

[FirestoreData()]
public class User
{
    public string uid;

    [FirestoreProperty()] public string username { get; set; }
    [FirestoreProperty()] public string bio { get; set; }
    [FirestoreProperty()] public string img { get; set; }
    [FirestoreProperty()] public List<Product> products { get; set; }
    [FirestoreProperty()] public List<string> friends { get; set; }
}

[FirestoreData()]
public class Product
{
    [FirestoreProperty()] public string product { get; set; }
    [FirestoreProperty()] public DateTime expiryDate { get; set; }
}