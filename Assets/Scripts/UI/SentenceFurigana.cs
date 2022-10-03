using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

/**
 * <summary>Handles the furigana for a editor sentence</summary>
 */
public class SentenceFurigana : MonoBehaviour
{
    public FuriganaData furiganaData;
    public GameObject furiganaPrefab;
    public GameObject furiganaParent;

    private List<GameObject> furiganaChildren;

    void Awake()
    {
        furiganaChildren = new List<GameObject>();    
    }

    /**
     * <summary>Updates the furigana for this object given the sentence and readings</summary>
     */
    public void UpdateFurigana(TMP_Text TextSentence, List<(int, string)> readings)
    {
        // needed for getting mesh information
        TextSentence.ForceMeshUpdate();
        int childCount = furiganaParent.transform.childCount;
        // instantiate prefab for each missing furigana prefab
        for(int i = 0; i + childCount < readings.Count; i++)
        {
            furiganaChildren.Add(Instantiate(furiganaPrefab, furiganaParent.transform));
            furiganaChildren[i].GetComponent<TMP_Text>().color = Color.black;
        }
        // if there are readings
        if(readings.Count > 0)
        {
            // we want the defaultsize here
            int generalSize = furiganaData.defaultSize;
            // update furiganas
            for (int i = 0; i < readings.Count; i++)
            {
                var childText = furiganaChildren[i].GetComponent<TMP_Text>();
                // set the size
                childText.fontSize = generalSize;

                // set the furigana
                childText.SetText(readings[i].Item2);
                // get the kanji position
                int charIdx = readings[i].Item1;
                TMP_CharacterInfo charInfo = TextSentence.textInfo.characterInfo[charIdx];
                int vertexIndex = charInfo.vertexIndex;
                Vector3[] vertexPositions = TextSentence.mesh.vertices;
                // manually fix the offset that happens on the second line
                int xOffset = 0;
                // significantly below so probably on the lower line
                if(vertexPositions[vertexIndex].y < vertexPositions[0].y - 10)
                {
                    xOffset = -18;
                }
                // place the furigana above the kanji
                furiganaChildren[i].GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, vertexPositions[vertexIndex + 2].x - vertexPositions[vertexIndex + 1].x);
                furiganaChildren[i].transform.localPosition = new Vector3((vertexPositions[vertexIndex + 1].x + vertexPositions[vertexIndex + 2].x) / 2 - 10 + xOffset, vertexPositions[vertexIndex + 1].y + 13, furiganaChildren[i].transform.position.z);
            }
        }

    }
}
