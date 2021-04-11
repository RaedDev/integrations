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

        // Where the image is taken
        var img = campaign.GetRandomImg();
        var video = campaign.GetRandomVideo();
        
        var id = Instantiate(line, adPanel.transform);
        id.GetComponent<Text>().text = "id: " + campaign.id;

        var points = Instantiate(line, adPanel.transform);
        points.GetComponent<Text>().text = "points: " + campaign.points;

        var maxCoverage = Instantiate(line, adPanel.transform);
        maxCoverage.GetComponent<Text>().text = "maxCoverage: " + campaign.maxCoverage;

        var imgText = Instantiate(line, adPanel.transform);
        imgText.GetComponent<Text>().text = "img: " + img;

        var videoText = Instantiate(line, adPanel.transform);
        videoText.GetComponent<Text>().text = "video: " + video;

        var link = Instantiate(line, adPanel.transform);
        link.GetComponent<Text>().text = "link: " + campaign.link;

        var impressions = Instantiate(line, adPanel.transform);
        impressions.GetComponent<Text>().text = "impressions: " + campaign.impressions.ToString();

        NetworkHelper.DownloadSprite(this, img, sprite =>
        {
            adPanel.GetComponent<Image>().sprite = sprite;
        });
    }

    public void movescene()
    {
        SceneManager.LoadScene("SampleScene");
    }

}
