using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecordListItem : MonoBehaviour 
{
    public Image image;
    public Text usernameText;
    public Text scoreText;

    public Text rankText;

    public void Setup(ScoreRecord record, int rank)
    {
        NetworkHelper.DownloadSprite(this, record.img, sprite =>
        {
            image.sprite = sprite;
        });

        usernameText.text = record.username;
        scoreText.text = record.score.ToString();
        rankText.text = rank.ToString();
    }
}
