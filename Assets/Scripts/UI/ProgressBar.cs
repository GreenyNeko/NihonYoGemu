using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * This script controls the custom made progressbar UI object
 */
[ExecuteInEditMode]
public class ProgressBar : MonoBehaviour
{
    public Sprite Background;               // what does the background look like?
    public Color BackgroundColor;           // used to dye the background [sprite]
    public Sprite Fill;                     // the sprite used to overlay the background given the fill
    public Color FillColor;                 // the color the fill uses
    public Image ImageFill;                 // internal fill part of the progress bar
    public TMPro.TMP_Text ProgressBarText;  // text shown in the progressbar
    public int MaxValue;                    // the maximum value the progress bar can have
    public int MinValue;                    // the minimum value the progressbar can have
    public int Value;                       // the current value of the progressbar
    public bool Percent;                    // whether or not the progressbar shows a percentage of x / y
    Image imageBackground;                  // the internal background image
    // Start is called before the first frame update

    void Start()
    {
    }

    //[ExecuteInEditMode]
    // Update is called once per frame
    void Update()
    {
        // update and pass inspector settings to internal related components & objects
        imageBackground = GetComponent<Image>();
        imageBackground.sprite = Background;
        imageBackground.color = BackgroundColor;
        ImageFill.sprite = Fill;
        ImageFill.color = FillColor;

        // correct value
        Mathf.Clamp(Value, MaxValue, MaxValue);

        // update the text and how it's displayed
        if(Percent)
        {
            // show the progress as ###.##%
            ProgressBarText.SetText((Mathf.Round(((float)(Value) / MaxValue) * 10000) / 100).ToString() + "%");
        }
        else
        {
            ProgressBarText.SetText(Value.ToString() + " / " + MaxValue.ToString());
        }
        // update the fill image
        // if there's no sprite
        if(ImageFill.sprite == null)
        {
            // change the anchor to scale it down
            if(MaxValue > 0)
            {
                ImageFill.transform.GetComponent<RectTransform>().anchorMax = new Vector2(((float)Value / MaxValue), 1);
            }
        }
        else
        {
            // TODO: Implement image scaling, probably needs different modes
        }
    }
}
