using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardPanel : MonoBehaviour {
    public Transform content;
    public GameObject leaderboardRecordPrefeb;

    public void Setup(List<ScoreRecord> records)
    {
        gameObject.SetActive(true);

        foreach(Transform t in content.transform)
        {
            Destroy(t.gameObject);
        }

        for(int i = 0; i < records.Count; i++)
        {
            var o = Instantiate(leaderboardRecordPrefeb, content);
            o.GetComponent<RecordListItem>().Setup(records[i], i + 1);
        }
    }
}
