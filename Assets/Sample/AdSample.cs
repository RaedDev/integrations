using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AdSample : MonoBehaviour
{
    public GameObject adPrefab;
    public GameObject line;
    public Transform content;

    private void Awake()
    {
        FirebaseManager.InitializeFirebase();
    }

    public void ShowAd()
    {
        var campaign = FirebaseManager.GetAd();

        var adPanel = Instantiate(adPrefab, content);
        
        var id = Instantiate(line, adPanel.transform);
        id.GetComponent<Text>().text = "id: " + campaign.id;

        var points = Instantiate(line, adPanel.transform);
        points.GetComponent<Text>().text = "points: " + campaign.points;

        var maxCoverage = Instantiate(line, adPanel.transform);
        maxCoverage.GetComponent<Text>().text = "maxCoverage: " + campaign.maxCoverage;

        var img = Instantiate(line, adPanel.transform);
        img.GetComponent<Text>().text = "img: " + campaign.img;

        var video = Instantiate(line, adPanel.transform);
        video.GetComponent<Text>().text = "video: " + campaign.video;

        var link = Instantiate(line, adPanel.transform);
        link.GetComponent<Text>().text = "link: " + campaign.link;

        var impressions = Instantiate(line, adPanel.transform);
        impressions.GetComponent<Text>().text = "impressions: " + campaign.impressions.ToString();

        NetworkHelper.DownloadSprite(this, campaign.img, sprite =>
        {
            adPanel.GetComponent<Image>().sprite = sprite;
        });
    }

    public void movescene()
    {
        SceneManager.LoadScene("SampleScene");
    }

}
