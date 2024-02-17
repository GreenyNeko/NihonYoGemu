using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TMPEffects : MonoBehaviour
{
    public bool vertical;
    public float degrees;
    public bool colorCharacter;
    public int character;
    public List<char> exceptions;
    TMP_Text textComponent;
    // Start is called before the first frame update
    void Start()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        textComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = textComponent.textInfo;
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
                    if (exceptions.Contains(charInfo.character))
                    {
                        continue;
                    }
                    vertices[charInfo.vertexIndex + j] = (Quaternion.Euler(0, 0, degrees) * (vertices[charInfo.vertexIndex + j] - pivot) + pivot);
                }
            }
        }
        if(colorCharacter)
        {
            // get the x-th kanji given currKanji
            TMP_CharacterInfo charInfo = textInfo.characterInfo[character];
            int vertexIndex = charInfo.vertexIndex;
            int meshIndex = charInfo.materialReferenceIndex;
            Color32[] vertexColors = textInfo.meshInfo[meshIndex].colors32;
            Color32 color = new Color(1, 0, 0);
            vertexColors[vertexIndex + 0] = color;
            vertexColors[vertexIndex + 1] = color;
            vertexColors[vertexIndex + 2] = color;
            vertexColors[vertexIndex + 3] = color;

            textInfo.textComponent.UpdateVertexData(TMPro.TMP_VertexDataUpdateFlags.All);
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
