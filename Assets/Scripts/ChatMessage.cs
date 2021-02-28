using System;
using System.Collections.Generic;
using Firebase.Firestore;

public class ChatMessage
{
    public int timestamp { get; set; }
    public string sender { get; set; }
    public string message { get; set; }
}

[FirestoreData()]
public class Conversation
{
    public string id;
    [FirestoreProperty()] public List<string> users { get; set; }
}
