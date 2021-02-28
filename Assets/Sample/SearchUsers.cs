using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Extensions;
using System.Linq;

public class SearchUsers : MonoBehaviour
{
    public static SearchUsers Instance;

    public GameObject scrollView;
    public Transform content;
    public InputField seachField;
    public GameObject searchPrefab;
    public GameObject bgPanel;

    void Awake()
    {
        Instance = this;

        seachField.onEndEdit.AddListener(val =>
        {
            FirebaseManager.usersCollection.WhereEqualTo("username", val).GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                foreach (Transform child in content)
                {
                    Destroy(child.gameObject);
                }
                List<User> users = task.Result.Documents.Select(s =>
                {
                    User u = s.ConvertTo<User>();
                    u.uid = s.Id;
                    return u;
                }).ToList();
                foreach (var u in users)
                {
                    var o = Instantiate(searchPrefab, content);
                    o.GetComponent<SearchItem>().Setup(u);
                }

                bgPanel.SetActive(true);
                scrollView.SetActive(true);
            });
        });
    }

    public void CloseSearch()
    {
        scrollView.SetActive(false);
        bgPanel.SetActive(false);
    }

    public static void Close()
    {
        Instance.CloseSearch();
    }
}
