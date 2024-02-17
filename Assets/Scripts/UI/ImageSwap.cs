using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageSwap : MonoBehaviour
{
    [SerializeField]
    Image image;
    [SerializeField]
    Sprite startSprite;
    [SerializeField]
    Sprite swapSprite;
    bool pressed = false;

    // Start is called before the first frame update
    void Start()
    {
        image.sprite = startSprite;   
    }

    public void SwapImage()
    {
        pressed = !pressed;
        if(!pressed)
        {
            image.sprite = startSprite;
        }
        else
        {
            image.sprite = swapSprite;
        }
         
    }
}
