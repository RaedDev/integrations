using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Auth;
using System;
using System.Linq;
using Firebase.Extensions;

public enum LoginResult
{
    NewUser,
    Successful,
    Failure,
}

public static class FirebaseManager
{
    public static bool ready = false;
    public static FirebaseAuth auth { get => FirebaseAuth.DefaultInstance; }
    
    static FirebaseUser fbUser;
    static User user;

    public static FirebaseFirestore db { get => FirebaseFirestore.DefaultInstance; } 
    public static CollectionReference campaignsCollection { get => db.Collection("campaigns"); }
    public static CollectionReference usersCollection { get => db.Collection("users"); }

    static List<Campaign> campaigns = null;
    static List<Campaign> loadedCampaigns = new List<Campaign>();

    public static void InitializeFirebase()
    {
        if (ready) return;

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                auth.StateChanged += AuthStateChanged;
                AuthStateChanged(null, null);

                Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
                Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;

                GetCampaigns();

                ready = true;
            }
            else
            {
                Debug.LogError(String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

    static void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
    {
        Debug.Log("Received Registration Token: " + token.Token);
    }

    static void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
    {
        Debug.Log("Received a new message from: " + e.Message.From);
    }


    static void GetCampaigns()
    {
        campaignsCollection.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if(task.IsFaulted)
            {
                Debug.LogError("Firebase firestore connection error can't get campaigns");
                return;
            }
            Debug.Log("campaigns are ready");
            campaigns = task.Result.Documents.Select(doc => { var c = doc.ConvertTo<Campaign>(); c.id = doc.Id; return c; })
                .Where(campaign => campaign.expiryDate > DateTime.Now || campaign.expiryDate.Ticks == 0).ToList();
        });
    }

    public static Campaign GetAd(int capacity = -1)
    {
        if(campaigns == null)
        {
            GetCampaigns();
            return _GetAd(Campaign.defaultCampaigns, capacity);
        }

        return _GetAd(campaigns, capacity);
    }

    public static void RemoveAd(string id)
    {
        var i = loadedCampaigns.FindIndex(x => x.id == id);
        if (i == -1) return;

        loadedCampaigns.RemoveAt(i);
    }

    public static void RemoveAllAds()
    {
        loadedCampaigns = new List<Campaign>();
    }

    static Campaign _GetAd(List<Campaign> options, int capacity)
    {
        if (options.Count == 0)
        {
            return _GetAd(Campaign.defaultCampaigns, capacity);
        }

        int points = 0;
        List<int> aggregatedPoints = options.Select(selector => {
            points = selector.points + points;
            return points;
        }).ToList();

        int rand = UnityEngine.Random.Range(0, points);
        int index = aggregatedPoints.FindIndex(x => x > rand);
        Campaign selectedCampaign = options[index];

        int c = 0;
        while (capacity != -1 && selectedCampaign.maxCoverage != 1 && (float)loadedCampaigns.Where(x => x == selectedCampaign).Count() / (float)capacity >= selectedCampaign.maxCoverage)
        {
            selectedCampaign = options[++index % options.Count];
            c++;
            if(c > options.Count)
            {
                return _GetAd(Campaign.defaultCampaigns, capacity);
            }
        }

        loadedCampaigns.Add(selectedCampaign);
        selectedCampaign.impressions++;
        if(selectedCampaign.id != null && selectedCampaign.id != "")
        {
            campaignsCollection.Document(selectedCampaign.id).SetAsync(selectedCampaign);
        }
        return selectedCampaign;
    }

    static void AuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (auth.CurrentUser != fbUser)
        {
            bool signedIn = fbUser != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && fbUser != null)
            {
                Debug.Log("Signed out " + fbUser.UserId);
            }
            fbUser = auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("Signed in " + fbUser.UserId);
                //displayName = user.DisplayName ?? "";
                //emailAddress = user.Email ?? "";
                //photoUrl = user.PhotoUrl ?? "";
            }
        }
    }

    public static void SigninWithEmail(string email, string password, Action<LoginResult> onLoggedIn)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            // Firebase user has been created.
            fbUser = task.Result;

            OnLoggedIn(onLoggedIn);
        });
    }

    public static void SignupWithEmail(string email, string password, Action<LoginResult> onLoggedIn)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            // Firebase user has been created.
            fbUser = task.Result;
            OnLoggedIn(onLoggedIn);
        });
    }

    static void OnLoggedIn(Action<LoginResult> onLoggedIn)
    {
        usersCollection.Document(fbUser.UserId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                if (!task.Result.Exists)
                {
                    onLoggedIn(LoginResult.NewUser);
                }
                else
                {
                    onLoggedIn(LoginResult.Successful);
                }
            }
            else
            {
                onLoggedIn(LoginResult.Failure);
                return;
            }
        });
    }

    public static void SetUser(User user)
    {
        usersCollection.Document(user.uid).SetAsync(user).ContinueWithOnMainThread(task =>
        {
            if(task.IsCompleted)
            {
                return;
            }

            Debug.LogError("Can't set user");
        });
    }

    public static void SignInWithGoogle()
    {
        Credential credential = GoogleAuthProvider.GetCredential("", null);
        auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
        {
            if(task.IsFaulted)
            {
                Debug.LogError("Something went wrong");
                return;
            }
        });
    }

    public static void AddUser()
    {
        User u = new User();
        u.username = "Ahmad";
        u.bio = "Mohammad";
        u.products = new List<Product>() { new Product { product = "Lite", expiryDate = DateTime.Now }, };
        u.img = "haha";

        db.Collection("users").Document().SetAsync(u).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Something went wrong while adding user data");
                return;
            }

            Debug.Log("Done!");
        });
    }

    public static void GetUser(string uid, Func<User, User> callback)
    {
        usersCollection.Document(uid).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if(task.IsFaulted)
            {
                Debug.LogError("Something went wrong");
                return;
            }
            callback(task.Result.ConvertTo<User>());
        });
    }

    public static void Test()
    {
        db.Collection("users").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if(task.IsFaulted)
            {
                Debug.LogError("Something went wrong");
                return;
            }

            List<User> u = task.Result.Documents.Select(x => x.ConvertTo<User>()).ToList();
            Debug.Log(u);
        });
    }
}
