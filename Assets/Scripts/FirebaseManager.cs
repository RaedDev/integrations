using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Auth;
using System;
using System.Linq;
using Firebase.Extensions;
using Firebase.Database;
using Google;

public enum LoginResult
{
    NewUser,
    Successful,
    Failure,
}

public static class FirebaseManager
{
    public static bool ready = false;
    public static FirebaseAuth auth { get { return FirebaseAuth.DefaultInstance; } }

    static FirebaseUser fbUser;
    public static User user;

    public static FirebaseFirestore db { get { return FirebaseFirestore.DefaultInstance; } }
    public static CollectionReference campaignsCollection { get { return db.Collection("campaigns"); } }
    public static CollectionReference usersCollection { get { return db.Collection("users"); } }
    public static CollectionReference conversationsCollection { get { return db.Collection("conversations"); } }
    public static CollectionReference notesCollection { get { return db.Collection("notes"); } }

    static List<Campaign> campaigns = null;
    static List<Campaign> loadedCampaigns = new List<Campaign>();

    static Action<LoginResult> defaultSignedInAction;
    static Action defaultOnSignedOut;

    static IAPManager iapManager;

    public static void InitializeFirebase(Action<LoginResult> onSignedIn = null, Action onSignedOut = null)
    {
        if (ready)
        {
            defaultSignedInAction = onSignedIn;
            defaultOnSignedOut = onSignedOut;
            return;
        }

        LoadingPanel.ShowLoading();

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                defaultSignedInAction = onSignedIn;
                defaultOnSignedOut = onSignedOut;

                auth.StateChanged += AuthStateChanged;

                Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
                Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;

                GetCampaigns();
                LoadStartupNotes();
                ConfigureGoogleAuthentication();

                ready = true;

                if (auth.CurrentUser == null)
                {
                    LoadingPanel.Hide();
                }
            }
            else
            {
                ErrorHandler.Show(string.Format("Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            }
        });
    }

    static void ConfigureGoogleAuthentication()
    {
        GoogleSignInConfiguration configuration = new GoogleSignInConfiguration
        {
            WebClientId = "620570136285-jjopvkden8utog1odn07jd135raebdej.apps.googleusercontent.com",
            RequestIdToken = true
        };

        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
    }

    static void LoadStartupNotes()
    {
        notesCollection.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if( task.IsFaulted || task.IsCanceled)
            {
                return;
            }

            var docs = task.Result.Documents.ToList();
            foreach (var doc in docs)
            {
                StartupNotice.ShowNote(doc.GetValue<string>("type"), doc.GetValue<string>("img"), null);
            }
        });
    }

    public static DatabaseReference GetConversationRef(string conversationId)
    {
        return FirebaseDatabase.DefaultInstance.GetReference("conversations/" + conversationId);
    }

    public static void GetConversationWith(User other, Action<string> callback)
    {
        if(other.uid == user.uid)
        {
            ErrorHandler.Show("Trying to open conversation with the local user");
            return;
        }

        conversationsCollection.WhereArrayContainsAny("users", new List<object>() { other.uid, user.uid } ).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if ( task.IsFaulted || task.IsCanceled)
            {
                ErrorHandler.Show("Error while opening conversation please check your internet connection");
                return;
            }

            if (task.Result.Documents.Count() == 0)
            {
                Conversation conv = new Conversation()
                {
                    users = new List<string>() {
                    user.uid, other.uid
                }
                };

                conversationsCollection.AddAsync(conv).ContinueWithOnMainThread(addTask =>
                {
                    callback(addTask.Result.Id);
                });
            }
            else
            {
                callback(task.Result.Documents.First().Id);
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
            if (task.IsFaulted || task.IsCanceled)
            {
                ErrorHandler.Show("Firebase firestore connection error can't get campaigns");
                return;
            }

            campaigns = task.Result.Documents.Select(doc => { var c = doc.ConvertTo<Campaign>(); c.id = doc.Id; return c; })
                .Where(campaign => campaign.expiryDate > DateTime.Now || campaign.expiryDate.Ticks == 0).ToList();
        });
    }

    public static Campaign GetAd(int capacity = -1)
    {
        if (campaigns == null)
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
            if (c > options.Count)
            {
                return _GetAd(Campaign.defaultCampaigns, capacity);
            }
        }

        loadedCampaigns.Add(selectedCampaign);
        selectedCampaign.impressions++;
        if (selectedCampaign.id != null && selectedCampaign.id != "")
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
                defaultOnSignedOut?.Invoke();
            }
            fbUser = auth.CurrentUser;
            if (signedIn)
            {
                OnSignedIn(null);
            }
        }
    }

    public static void SigninWithEmail(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                LoadingPanel.Hide();
                return;
            }
            if (task.IsFaulted)
            {
                ErrorHandler.Show("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                LoadingPanel.Hide();
                return;
            }

            // Firebase user has been created.
            fbUser = task.Result;
        });
    }

    public static void SignupWithEmail(string email, string password)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if ( task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                LoadingPanel.Hide();
                return;
            }

            // Firebase user has been created.
            fbUser = task.Result;
        });
    }

    public static void SigninAnonymously()
    {
        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if ( task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Something went wrong check your internet connection");
                LoadingPanel.Hide();
                return;
            }

            // Firebase user has been created.
            fbUser = task.Result;
        });
    }

    public static void SignOut()
    {
        auth.SignOut();
    }

    static void OnSignedIn(Action<LoginResult> onSignedIn)
    {
        if (onSignedIn == null)
        {
            onSignedIn = defaultSignedInAction;
        }
        usersCollection.Document(fbUser.UserId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            LoadingPanel.Hide();
            if (task.IsFaulted || task.IsCanceled)
            {
                onSignedIn?.Invoke(LoginResult.Failure);
                return;
            }

            if (!task.Result.Exists)
            {
                onSignedIn?.Invoke(LoginResult.NewUser);
            }
            else
            {
                user = task.Result.ConvertTo<User>();
                user.uid = task.Result.Id;
                iapManager = new IAPManager();
                iapManager.Initlize();

                onSignedIn?.Invoke(LoginResult.Successful);
            }
        });
    }

    public static void SetUser(User u)
    {
        usersCollection.Document(u.uid).SetAsync(u).ContinueWithOnMainThread(task =>
        {
            if ( task.IsFaulted || task.IsCanceled)
            {
                ErrorHandler.Show("Can't set user");
                return;
            }

            user = u;
        });
    }

    public static void SignInWithGoogle()
    {
        // android 620570136285-tu68fse9qup2replbhsvfsl767r8i3i2.apps.googleusercontent.com
        // ios 620570136285-rnqjkrgqg22j5cc39bsvotei7nq98ks4.apps.googleusercontent.com
        // web 620570136285-jjopvkden8utog1odn07jd135raebdej.apps.googleusercontent.com

        

        GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(token => 
        {
            if(token.IsCanceled || token.IsFaulted)
            {
                ErrorHandler.Show("Sign in with google was not successful");
                return;
            }

            Credential credential = GoogleAuthProvider.GetCredential(token.Result.IdToken, null);
            auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
            {
                if ( task.IsFaulted || task.IsCanceled)
                {
                    ErrorHandler.Show("Something went wrong");
                }
            });
        });
    }

    public static void AddUser()
    {
        User u = new User();
        u.username = "Ahmad";
        u.bio = "Mohammad";

        db.Collection("users").Document().SetAsync(u).ContinueWithOnMainThread(task =>
        {
            if ( task.IsFaulted || task.IsCanceled)
            {
                ErrorHandler.Show("Something went wrong while adding user data");
                return;
            }
        });
    }

    public static void GetUser(string uid, Action<User> callback)
    {
        usersCollection.Document(uid).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if ( task.IsFaulted || task.IsCanceled)
            {
                ErrorHandler.Show("Cannot get user check your internet conncetion");
                return;
            }
            User u = task.Result.ConvertTo<User>();
            u.uid = uid;

            callback(u);
        });
    }

    public static void AddFriend(string uid)
    {
        if (uid == user.uid)
        {
            ErrorHandler.Show("Can't add yourself!, current user: " + uid + ", firebase user: " + user.uid);
            return;
        }

        if (user.sentRequests.Contains(uid))
        {
            ErrorHandler.Show("Friend request is already sent");
            return;
        }
        var update = new Dictionary<string, object>() { { "requests", FieldValue.ArrayUnion(user.uid) } };

        usersCollection.Document(uid).UpdateAsync(update).ContinueWithOnMainThread(task =>
        {
            if ( task.IsFaulted || task.IsCanceled)
            {
                ErrorHandler.Show("Cannot send friend request check your internet connection");
                return;
            }

            var localUpdate = new Dictionary<string, object>() { { "sentRequests", FieldValue.ArrayUnion(uid) } };
            usersCollection.Document(user.uid).UpdateAsync(localUpdate).ContinueWithOnMainThread(task2 =>
            {
                user.sentRequests.Add(uid);
            });
        });
    }

    public static void AcceptFriend(string uid)
    {
        var update = new Dictionary<string, object>() { { "requests", FieldValue.ArrayRemove(uid) }, { "friends", FieldValue.ArrayUnion(uid) } };

        usersCollection.Document(user.uid).UpdateAsync(update).ContinueWithOnMainThread(task =>
        {
            if ( task.IsFaulted || task.IsCanceled)
            {
                ErrorHandler.Show("Cannot accept friend check your internet conncetion");
                return;
            }

            var localUpdate = new Dictionary<string, object>() { { "sentRequests", FieldValue.ArrayRemove(user.uid) }, { "friends", FieldValue.ArrayUnion(user.uid) } };
            usersCollection.Document(uid).UpdateAsync(localUpdate).ContinueWithOnMainThread(task2 => {
                user.sentRequests.Remove(uid);
                user.friends.Add(uid);
            });
        });
    }

    public static void RejectFriend(string uid)
    {
        var update = new Dictionary<string, object>() { { "requests", FieldValue.ArrayRemove(uid) } };

        usersCollection.Document(user.uid).UpdateAsync(update).ContinueWithOnMainThread(task =>
        {
            if ( task.IsFaulted || task.IsCanceled)
            {
                ErrorHandler.Show("Cannot accept friend check your internet conncetion");
                return;
            }

            var localUpdate = new Dictionary<string, object>() { { "sentRequests", FieldValue.ArrayRemove(user.uid) } };
            usersCollection.Document(uid).UpdateAsync(localUpdate).ContinueWithOnMainThread(task2 => {
                user.sentRequests.Remove(uid);
            });
        });
    }

    public static void SendGift(string uid, string product, Action onSuccess, Action onError)
    {
        var update = new Dictionary<string, object>() { { "gifts", FieldValue.ArrayUnion(new Gift() { sender = user.uid, product = product }) } };

        usersCollection.Document(uid).UpdateAsync(update).ContinueWithOnMainThread(task =>
        {
            if ( task.IsFaulted || task.IsCanceled)
            {
                ErrorHandler.Show("Couldn't send gift", "please check your internet connection", onError);
                return;
            }

            onSuccess();
        });
    }

    public static void AcceptGift(Gift gift)
    {
        int i = user.gifts.IndexOf(gift);
        if(i == -1)
        {
            ErrorHandler.Show("Gift not found");
            return;
        }

        gift.expiryDate = DateTime.Now.AddMonths(1);
        var update = new Dictionary<string, object>() { { "gifts", user.gifts } };

        usersCollection.Document(user.uid).UpdateAsync(update).ContinueWithOnMainThread(task =>
        {
            if ( task.IsFaulted || task.IsCanceled)
            {
                ErrorHandler.Show("Couldn't send gift, please check your internet connection");
                return;
            }
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
