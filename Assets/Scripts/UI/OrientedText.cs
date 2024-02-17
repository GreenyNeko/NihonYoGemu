using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OrientedText : MonoBehaviour
{
    public GameObject FuriganaHandler;
    public GameObject InputFieldGameObject;
    public TMP_Text DisplayedText;
    public TMP_Text InputField;
    public bool vertical;
    bool prevVertical;
    static List<char> exceptions = new List<char>() {'Å[', 'Å`', 'Å|'}; 
    // Start is called before the first frame update
    void Start()
    {
        if(vertical)
        {
            prevVertical = !vertical;
        }
        else
        {
            prevVertical = vertical;
        }
             
    }

    // Update is called once per frame
    void Update()
    {
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        DisplayedText.ForceMeshUpdate();
        InputField?.ForceMeshUpdate();
        //TMP_TextInfo displayTextInfo = DisplayedText.textInfo;
        bool prev;
        if(InputFieldGameObject != null)
        {
            prev = InputFieldGameObject.activeInHierarchy;
            InputFieldGameObject.gameObject.SetActive(true);
            //TMP_TextInfo inputTextInfo = InputField.textInfo;
            InputFieldGameObject.gameObject.SetActive(prev);
        }


        if(vertical)
        {
            DisplayedText.characterSpacing = 10;
            if(InputFieldGameObject != null)
            {
                InputFieldGameObject.GetComponent<TMP_InputField>().textComponent.characterSpacing = 10;
            }
            
        }
        else
        {
            DisplayedText.characterSpacing = 0;
            if (InputFieldGameObject != null)
            {
                InputFieldGameObject.GetComponent<TMP_InputField>().textComponent.characterSpacing = 0;
            }
        }

        UpdateOrientation(ref rectTransform);
        DisplayedText.GetComponent<TMPEffects>().vertical = vertical;
        if(InputField != null)
        {
            InputField.GetComponent<TMPEffects>().vertical = vertical;
        }
        for(int i = 0; i < FuriganaHandler.transform.childCount; i++)
        {
            FuriganaHandler.transform.GetChild(i).GetComponent<TMPEffects>().vertical = vertical;
        }

        // update parent rotation to rotate contained text related components
        if (prevVertical != vertical)
        {
            if(!prevVertical)
            {
                rectTransform.offsetMin = new Vector2(0, 0);
                rectTransform.offsetMax = new Vector2(0, 0);
            }
            float temp = rectTransform.rect.width;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rectTransform.rect.height);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, temp);
            if (prevVertical)
            {
                rectTransform.offsetMin = new Vector2(0, 0);
                rectTransform.offsetMax = new Vector2(0, 0);
            }
            FuriganaHandler.GetComponent<SentenceFurigana>().SetOrientation(vertical);
            prevVertical = vertical;
        }
    }

    public void SetOrientation(bool newVertical)
    {
        prevVertical = vertical;
        vertical = newVertical;
    }

    void UpdateOrientation(ref RectTransform rectTransform)
    {
        if(rectTransform != null)
        {
            rectTransform.localRotation = Quaternion.Euler(0, 0, 0);
        }
        if (vertical)
        {
            if (rectTransform != null)
            {
                rectTransform.localRotation = Quaternion.Euler(0, 0, -90);
            }
        }
    }
}
