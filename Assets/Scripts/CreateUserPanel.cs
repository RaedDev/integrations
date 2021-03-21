using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Extensions;


public class CreateUserPanel : MonoBehaviour
{
    public InputField username;
    public InputField bio;
    public Image image;
    string img = null;

    public ProfilePanel profilePanel;

    void Start()
    {
        var fbUser = FirebaseManager.auth.CurrentUser;
        username.text = fbUser.DisplayName;

        if(fbUser.PhotoUrl != null)
        {
            img = fbUser.PhotoUrl.AbsoluteUri;
            NetworkHelper.DownloadSprite(this, fbUser.PhotoUrl.AbsoluteUri, sprite =>
            {
                if (img != null && img != "")
                {
                    // Upload the image 
                    var imgRef = Firebase.Storage.FirebaseStorage.DefaultInstance.RootReference.Child(FirebaseManager.auth.CurrentUser.UserId + ".jpg");
                    imgRef.PutBytesAsync(sprite.texture.EncodeToJPG()).ContinueWithOnMainThread(task =>
                    {
                        if (task.IsFaulted || task.IsCanceled)
                        {
                            ErrorHandler.Show("Error uploading picture");
                        }
                    });
                }

                image.sprite = sprite;
            });
        }
    }

    public void ChangePicture()
    {
        FirebaseManager.ChangePicture((imgPath, texture) =>
        {
            img = imgPath;
            image.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        });
    }

    public void Submit()
    {
        User u = new User() { bio = bio.text, username = username.text, uid = FirebaseManager.auth.CurrentUser.UserId, img = img };
        FirebaseManager.SetUser(u);

        profilePanel.Setup(u);
    }
}
