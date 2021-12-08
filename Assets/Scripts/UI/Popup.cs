using UnityEngine;

/**
 * This script is useful to make a popup
 */
public class Popup : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // hide by default
        gameObject.SetActive(false);   
    }

    /**
     * Show the popup
     */
    public void Show()
    {
        gameObject.SetActive(true);
    }

    /**
     * Hide the popup
     */
    public void Close()
    {
        gameObject.SetActive(false);
    }

    /**
     * Toggles whether or not the popup is visisble
     */
    public void ToggleVisibility()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
