using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Scripts controlling the UI leaderboard elements
 */
public class UILeaderboard : MonoBehaviour
{
    // the prefab used to fill the viewport with
    public GameObject PrefabUIHighScore;

    public GameObject ContentElement;       // the viewport content gameobject

    string currentLevel = "";

    public void UpdateLeaderboard(string levelName)
    {
        // clean up leaderboard
        for(int i = 0; i < ContentElement.transform.childCount; i++)
        {
            Destroy(ContentElement.transform.GetChild(i).gameObject);
        }
        currentLevel = levelName;
        // load leaderboard from file and create the elements
        Leaderboard leaderboard = Leaderboard.LoadLeaderboardByName(levelName);
        for(int i = 0; i < leaderboard.Count(); i++)
        {
            Leaderboard.HighScore highScore = leaderboard[leaderboard.Count() - i - 1];
            GameObject prefabUIHighScore = Instantiate(PrefabUIHighScore, ContentElement.transform);
            prefabUIHighScore.GetComponent<UIHighScore>().SetValues(highScore.username, (int)highScore.score, highScore.accuracy, highScore.mods);
            prefabUIHighScore.transform.localPosition = new Vector3(prefabUIHighScore.transform.localPosition.x, -64 * i, prefabUIHighScore.transform.localPosition.z);
        }
    }
}
