using UnityEngine;
using UnityEngine.UI;

/**
 * This script controls the history part of the UI
 */
public class HistoryHandler : MonoBehaviour
{
    public GameObject[] HitVisualizer;                              // the elements that visualize hits
    public Color ColorDef, ColorCorrect, ColorSloppy, ColorMiss;    // the colors that should be represented
    private int internalCounter;                                    // which element should be affected next

    private void Start()
    {
        ResetHistoryHandler();
    }

    /**
     * resets the ui elements and counter of the history handler to their initial state
     */
    public void ResetHistoryHandler()
    {
        // initalize counter and color for each element
        internalCounter = 0;
        foreach (GameObject elem in HitVisualizer)
        {
            elem.GetComponent<RawImage>().color = ColorDef;
        }
    }

    /**
     * Updates the respective element that should be affected next by setting it's color respectively given the type
     * Then call a function to update previous elements
     */
    public void RegisterHit(int type)
    {
        switch(type)
        {
            case 2:
                // miss
                HitVisualizer[internalCounter++].GetComponent<RawImage>().color = ColorMiss;
                break;
            case 1:
                // sloppy
                HitVisualizer[internalCounter++].GetComponent<RawImage>().color = ColorSloppy;
                break;
            default:
                // correct
                HitVisualizer[internalCounter++].GetComponent<RawImage>().color = ColorCorrect;
                break;
        }
        internalCounter %= HitVisualizer.Length;
        updateOldColors();
    }

    // lerp the color by age where the oldest one has no color and the newer ones are more saturated(?)
    private void updateOldColors()
    {
        for(int i = 0; i < HitVisualizer.Length - 1; i++)
        {
            // iterate backwards
            int pos = internalCounter - 1 - i;
            // correct position from negative to positive index
            pos %= HitVisualizer.Length;
            if(pos < 0)
            {
                pos = HitVisualizer.Length + pos;
            }
            // get the previous color
            Color oldColor = HitVisualizer[pos].GetComponent<RawImage>().color;
            // update color to the lerped value between default and the color it had before
            HitVisualizer[pos].GetComponent<RawImage>().color = Color.Lerp(oldColor, ColorDef, (i + 1.0f) / (float)HitVisualizer.Length);
        }
    }
}
