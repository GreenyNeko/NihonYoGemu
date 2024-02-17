using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/**
 * Handle furigana for the sentence
 **/
public class SentenceFurigana : MonoBehaviour
{
    public TMP_Text sentenceObject;
    public GameObject furiganaPrefab;
    public Transform furiganaParent;

    List<GameObject> furiganaObjects = new List<GameObject>();
    List<int> kanjiIndices = new List<int>();
    bool vertical = false;

    private void Update()
    {
        UpdateFurigana();
        for (int i = 0; i < furiganaObjects.Count; i++)
        {
            furiganaObjects[i].GetComponent<TMP_Text>().ForceMeshUpdate();
            TMP_TextInfo textInfo = furiganaObjects[i].GetComponent<TMP_Text>().textInfo;
            UpdateFuriganaOrientation(ref textInfo);
        }
    }

    /**
     * Called when the sentence has been updated.
     * Counts how many characters need furigana and creates said amount
     **/
    public void UpdatedSentence(string newSentence)
    {
        kanjiIndices.Clear();
        // sentence analysis
        {
            // O(n)
            // count how many we need and which index they should be attached to
            for (int i = 0; i < newSentence.Length; i++)
            {
                if (JapaneseDictionary.IsKanji(newSentence[i].ToString()))
                {
                    kanjiIndices.Add(i);
                }
            }
        }

        // manage furigana objects
        {
            // O(n)
            // create new ones if there aren't enough
            int hasToCreate = Mathf.Max(0, kanjiIndices.Count - furiganaObjects.Count);
            for (int i = 0; i < hasToCreate; i++)
            {
                GameObject furiganaObject = Instantiate(furiganaPrefab, furiganaParent);
                furiganaObjects.Add(furiganaObject);
            }
            // remove if there are too many
            if(furiganaObjects.Count - kanjiIndices.Count > 0)
            {
                for(int i = 0; i < furiganaObjects.Count - kanjiIndices.Count; i++)
                {
                    Destroy(furiganaObjects[furiganaObjects.Count - i - 1]);
                }
                furiganaObjects.RemoveRange(kanjiIndices.Count, furiganaObjects.Count - kanjiIndices.Count);
            }
        }
        UpdateFurigana();
    }

    public void SetOrientation(bool newOrientation)
    {
        vertical = newOrientation;
        Debug.Log("vertical");
        UpdateFurigana();
    }

    public void UpdateFurigana()
    {
        sentenceObject.ForceMeshUpdate();
        // update furigana positions
        if(sentenceObject.textInfo != null)
        { 
            TMP_TextInfo textInfo = sentenceObject.textInfo;
            TMP_CharacterInfo[] charInfo = textInfo.characterInfo;
            // if vertical
            if(vertical)
            {
                // O(n)
                for (int i = 0; i < furiganaObjects.Count; i++)
                {
                    // get coordinates from index at position i
                    TMPro.TMP_CharacterInfo currChar = charInfo[kanjiIndices[i]];
                    Vector3 topRight = currChar.topRight;
                    Vector3 bottomRight = currChar.bottomRight;
                    float width = sentenceObject.fontSize - 2;
                    float height = sentenceObject.fontSize - 2;
                    // position this furiganaObject from top left to bottom left
                    furiganaObjects[i].GetComponent<RectTransform>().anchoredPosition = currChar.topLeft + new Vector3(0, height * 0.5f, 0);
                    furiganaObjects[i].GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, topRight.y - bottomRight.y);
                    furiganaObjects[i].GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, width * 0.5f);
                }
            }
            else
            {
                // if horizontal
                // O(n)
                for (int i = 0; i < furiganaObjects.Count; i++)
                {
                    TMP_CharacterInfo currChar = charInfo[kanjiIndices[i]];
                    // get coordinates from index at position i
                    Vector3 topLeft = currChar.topLeft;
                    Vector3 topRight = currChar.topRight;
                    float height = sentenceObject.fontSize - 2;
                    // position this furiganaObject  from top left to top right
                    furiganaObjects[i].GetComponent<RectTransform>().anchoredPosition = topLeft + new Vector3(0, height * 0.5f, 0);
                    furiganaObjects[i].GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, topRight.x - topLeft.x);
                    furiganaObjects[i].GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height * 0.5f);
                }
            }
        }
    }

    void UpdateFuriganaOrientation(ref TMP_TextInfo textInfo)
    {
        // Debug.Log("mesh change for " + textInfo.characterCount.ToString() + "? " + vertical.ToString());
        // transform
        if (vertical)
        {
            for (int i = 0; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible)
                {
                    continue;
                }
                Vector3[] vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                //Vector3 next = vertices[charInfo.vertexIndex];
                Vector3 pivot = vertices[charInfo.vertexIndex + 1] + (vertices[charInfo.vertexIndex + 3] - vertices[charInfo.vertexIndex + 1]) / 2;
                for (int j = 0; j < 4; ++j)
                {
                    vertices[charInfo.vertexIndex + j] = (Quaternion.Euler(0, 0, 90) * (vertices[charInfo.vertexIndex + j] - pivot) + pivot);
                }
            }
        }
        if (textInfo != null && textInfo.textComponent.gameObject.activeInHierarchy)
        {
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                TMP_MeshInfo meshInfo = textInfo.meshInfo[i];
                meshInfo.mesh.vertices = meshInfo.vertices;
                textInfo.textComponent.UpdateGeometry(meshInfo.mesh, i);
            }
        }
    }
}
