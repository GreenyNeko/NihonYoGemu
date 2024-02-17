using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundLoaderMenu : MonoBehaviour
{
    public Image image;
    public GameObject ErrorMessage;
    public GameObject EditorManagerInstance;
    public GameObject DropdownScaleMode;

    byte[] bytes;
    Texture2D texture;
    public void ToggleMenu()
    {
        if(gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    public void LoadImage(string path)
    {
        try
        {
            bytes = System.IO.File.ReadAllBytes(path);
            DestroyImmediate(texture);
            texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            // prevent memory issues
            DestroyImmediate(image.sprite);
            image.sprite = null;
            // now create new preview
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            ErrorMessage.SetActive(false);
        }
        catch(System.Exception e)
        {
            ErrorMessage.SetActive(true);
        }
    }

    public void Open()
    {
        
    }

    public void SaveImage()
    {
        EditorManagerInstance.GetComponent<EditorManager>().SetScalingModeBackground(DropdownScaleMode.GetComponent<TMPro.TMP_Dropdown>().value);
        EditorManagerInstance.GetComponent<EditorManager>().StoreBackgroundImage(bytes);
        EditorManagerInstance.GetComponent<EditorManager>().NotifyOfChanges();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDisable()
    {
        Destroy(texture);
        Destroy(image.sprite);
    }
}
