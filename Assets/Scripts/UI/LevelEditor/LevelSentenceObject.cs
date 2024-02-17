using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelSentenceObject : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    public KanjiClickHandler kanjiClickHandler;
    public RectTransform ContextMenu;
    public GameObject ScaleHelper;
    public TMPro.TMP_Text DisplayedText;
    public TMPro.TMP_InputField inputField;
    public EditorManager editorManager;
    public bool template;
    public int sentenceDataIndex = -1;
    public bool editable = false;
    bool dragging = false;
    bool selected = false;
    bool editing = false;
    Vector2 offset = -Vector2.one;
    Vector2 dragStart;
   

    public void OnPointerClick(PointerEventData eventData)
    {
        if(!editable)
        {
            return;
        }
        // update selected
        editorManager.SetSelectedSentenceObject(gameObject);
        selected = true;
        if (!editorManager.IsFuriganaMode() && eventData.clickCount > 1)
        {
            // activate sentence editing
            DisplayedText.gameObject.SetActive(false);
            inputField.gameObject.SetActive(true);
            inputField.text = DisplayedText.text;
            inputField.ActivateInputField();
            editing = true;
        }
        if(!editing)
        {
            ScaleHelper.SetActive(true);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!editable)
        {
            return;
        }
        editorManager.SetSelectedSentenceObject(gameObject);
        if(offset.x < 0)
        {
            Vector2 convertedCursor = new Vector2(eventData.position.x - 50, eventData.position.y-Screen.height + 50); 
            offset = convertedCursor - transform.GetComponent<RectTransform>().anchoredPosition;
        }
        dragStart = eventData.position;
        dragging = true;
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (!editable)
        {
            return;
        }
        if (dragging)
        {
            Vector2 convertedCursor = new Vector2(eventData.position.x - 50, eventData.position.y - Screen.height + 50);
            transform.GetComponent<RectTransform>().anchoredPosition = convertedCursor - offset;
            KeepOnScreen();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!editable)
        {
            return;
        }
        bool undoable = false;
        if(Vector2.Distance(dragStart, eventData.position) > 0.01)
        {
            undoable = true;
        }
        // need to convert from editor coords to level coords
        Vector2 coordOffset = (new Vector2(Screen.width, -Screen.height) - new Vector2(1, -1) * editorManager.GetLevelNativeSize()) / 2 - new Vector2(50, -50);
        // gameobject to level coords
        Vector2 convertedPos = GetComponent<RectTransform>().anchoredPosition - coordOffset;
        editorManager.UpdateSentenceObjectRect(new Rect(convertedPos.x, convertedPos.y, GetComponent<RectTransform>().rect.width, GetComponent<RectTransform>().rect.height), undoable);
        dragging = false;
        offset = -Vector2.one;
    }

    /**
     * <summary>Updates the text of the sentence object and passes it to the editormanager</summary>
     */
    public void UpdateText(string text)
    {
        DisplayedText.SetText(text);
        editorManager.UpdateSentenceText(text);
    }

    /**
     * removes selection, called by editormanager when requesting to deselect current object
     */
    public void Deselect()
    {
        selected = false;
        ScaleHelper.SetActive(false);
    }

    public void Start()
    {
        DisplayedText.outlineWidth = 0f;
        if (!template)
        {
            //selected = true;
            //editorManager.SetSelectedSentenceObject(gameObject);
        }
        if(inputField != null)
        {
            inputField.textComponent.color = Color.white;
        }
    }

    public void FixedUpdate()
    {
        bool furiganaMode = false;
        if(editorManager != null)
        {
            furiganaMode = editorManager.IsFuriganaMode();
        }
        kanjiClickHandler.enabled = furiganaMode;
        //ContextMenu.gameObject.SetActive(selected && !editorManager.IsFuriganaMode());
        if (!selected)
        {
            editing = false;
        }
        if(selected && !editing)
        {
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                transform.GetComponent<RectTransform>().anchoredPosition += new Vector2(-0.5f, 0);
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                transform.GetComponent<RectTransform>().anchoredPosition += new Vector2(0.5f, 0);
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                transform.GetComponent<RectTransform>().anchoredPosition += new Vector2(0, 0.5f);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                transform.GetComponent<RectTransform>().anchoredPosition += new Vector2(0, -0.5f);
            }
            KeepOnScreen();
            ContextMenu.GetComponent<RectTransform>().position = editorManager.GetSentenceContextPosition();
            // prevent context menu being off-screen
            if (-ContextMenu.GetComponent<RectTransform>().anchoredPosition.y < 0)
            {
                ContextMenu.GetComponent<RectTransform>().anchoredPosition = new Vector2(ContextMenu.GetComponent<RectTransform>().anchoredPosition.x, 0);
            }
            if (ContextMenu.GetComponent<RectTransform>().anchoredPosition.x > Screen.width - 120)
            {
                ContextMenu.GetComponent<RectTransform>().anchoredPosition = new Vector2(Screen.width - 120, ContextMenu.GetComponent<RectTransform>().anchoredPosition.y);
            }
        }
    }

    /**
     * <summary>Returns if the gameobject is being edited</summary>
     */
    public bool IsEditing()
    {
        return editing;
    }

    /**
     * <summary>Mark the given character</summary>
     */
    public void MarkCharacter(int character)
    {
        if (character >= 0 && character < DisplayedText.text.Length)
        {
            DisplayedText.GetComponent<TMPEffects>().character = character;
            DisplayedText.GetComponent<TMPEffects>().colorCharacter = true;
        }
    }

    /**
     * <summary>Clear all markings from characters</summary>
     */
    public void UnmarkCharacter()
    {
        DisplayedText.GetComponent<TMPEffects>().colorCharacter = false;
    }

    /**
     * <summary>Called when the scaling is ended by mouse up</summary>
     */
    public void ScalingDone()
    {
        if (!editable)
        {
            return;
        }
        // need to convert from editor coords to level coords
        Vector2 coordOffset = (new Vector2(Screen.width, -Screen.height) - new Vector2(1, -1) * editorManager.GetLevelNativeSize()) / 2 - new Vector2(50, -50);
        // gameobject to level coords
        Vector2 convertedPos = GetComponent<RectTransform>().anchoredPosition - coordOffset;
        editorManager.UpdateSentenceObjectRect(new Rect(convertedPos.x, convertedPos.y, GetComponent<RectTransform>().rect.width, GetComponent<RectTransform>().rect.height), true);
    }

    /**
     * <summary>Corrects the position to ensure sentence objects stay on the screen</summary>
     */
    private void KeepOnScreen()
    {
        if(!template)
        {
            if (transform.GetComponent<RectTransform>().anchoredPosition.x < -50)
            {
                transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(-50, transform.GetComponent<RectTransform>().anchoredPosition.y);
            }
            if (transform.GetComponent<RectTransform>().anchoredPosition.y > +50)
            {
                transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(transform.GetComponent<RectTransform>().anchoredPosition.x, 50);
            }
            if (transform.GetComponent<RectTransform>().anchoredPosition.x + transform.GetComponent<RectTransform>().rect.width > Screen.width - 50)
            {
                transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(Screen.width - transform.GetComponent<RectTransform>().rect.width - 50, transform.GetComponent<RectTransform>().anchoredPosition.y);
            }
            if (transform.GetComponent<RectTransform>().anchoredPosition.y - transform.GetComponent<RectTransform>().rect.height < -Screen.height + 50)
            {
                transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(transform.GetComponent<RectTransform>().anchoredPosition.x, -Screen.height + transform.GetComponent<RectTransform>().rect.height + 50);
            }
        }
    }
}
