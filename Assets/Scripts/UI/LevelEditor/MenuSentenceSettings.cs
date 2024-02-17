using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSentenceSettings : MonoBehaviour
{
    public GameObject inputPositionX, inputPositionY;
    public GameObject inputSizeWidth, inputSizeHeight;
    public GameObject verticalOption;
    public GameObject inputTextSize;
    public GameObject buttonColor;
    public GameObject buttonBold;
    public GameObject sliderOutlineSize;
    public GameObject alignVerticalOption;
    public GameObject alignHorizontalOption;
    [SerializeField]
    GameObject TextOutlineSize;

    public void UpdateOutlineSizeText(float value)
    {
        TextOutlineSize.GetComponent<TMPro.TMP_Text>().SetText((Mathf.Round(value * 100)).ToString() + "%"); 
    }
}
