using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using System;

[FirestoreData()]
public class ScoreRecord
{
    public string uid;
    [FirestoreProperty()] public int score { get; set; }
    [FirestoreProperty()] public string username { get; set; }
    [FirestoreProperty()] public string img { get; set; }
    [FirestoreProperty()] public DateTime timestamp { get; set; }
}

public static class Leaderboard
{
    static FirebaseFirestore db { get { return FirebaseFirestore.DefaultInstance; } }

    public static void GetLeaderboard(string id, Action<List<ScoreRecord>> callback)
    {
        db.Collection(id).Limit(50).OrderByDescending("score").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            var docs = task.Result.Documents.Select(selector =>
            {
                var r = selector.ConvertTo<ScoreRecord>();
                r.uid = selector.Id;
                return r;
            }).ToList();
            callback(docs);
        });
    }

    public static void SubmitScore(string id, int score)
    {
        db.Collection(id).Document(FirebaseManager.user.uid).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (!task.IsCompleted)
            {
                ErrorHandler.Show("Error submitting score to the leaderboard, check your internet connection");
                return;
            }

            var oldRecord = task.Result.ConvertTo<ScoreRecord>();
            if (score > oldRecord.score)
            {
                ScoreRecord record = new ScoreRecord
                {
                    score = score,
                    uid = FirebaseManager.user.uid,
                    img = FirebaseManager.user.img,
                    username = FirebaseManager.user.username,
                    timestamp = DateTime.Now
                };

                db.Collection(id).Document(record.uid).SetAsync(record).ContinueWithOnMainThread(task2 =>
                {
                });
            }
        });

    }
}
