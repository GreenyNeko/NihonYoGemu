using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * <summary>EditorAction are used to keep track, undo and redo action.</summary>
 */
public class EditorAction
{
    public int index;
    public int page;
    public EditorAction(int index, int page)
    {
        this.index = index;
        this.page = page;
    }

    /**
     * <summary>Performs the action</summary>
     */
    public virtual void Perform(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings=null)
    {
        
    }

    /**
     * <summary>Undoes the action</summary>
     */
    public virtual void Undo(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings=null)
    {
        
    }
}

/**
 * <summary>Sentence edited action</summary>
 */
public class EditorActionEditSentence : EditorAction
{
    public string redo, undo;
    public string[] redoFurigana, undoFurigana;
    public EditorActionEditSentence(int index, int page, string redo, string undo, string[] redoFurigana, string[] undoFurigana) : base(index, page)
    {
        this.redo = redo;
        this.undo = undo;
        this.redoFurigana = redoFurigana;
        this.undoFurigana = undoFurigana;
    }

    public override void Perform(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings = null)
    {
        levelData.pages[page].sentenceObjects[index].text = redo;
        levelData.pages[page].sentenceObjects[index].furigana = redoFurigana;
        sentenceObjects[index].GetComponent<LevelSentenceObject>().DisplayedText.text = redo;
        sentenceObjects[index].GetComponent<LevelSentenceObject>().inputField.text = redo;
    }

    public override void Undo(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings = null)
    {
        levelData.pages[page].sentenceObjects[index].text = undo;
        levelData.pages[page].sentenceObjects[index].furigana = undoFurigana;
        sentenceObjects[index].GetComponent<LevelSentenceObject>().DisplayedText.text = undo;
        sentenceObjects[index].GetComponent<LevelSentenceObject>().inputField.text = undo;
    }
}

/**
 * <summary>Text outline change action</summary>
 */
public class EditorActionEditTextOutline : EditorAction
{
    public float redo, undo;
    public EditorActionEditTextOutline(int index, int page, float redo, float undo) : base(index, page)
    {
        this.redo = redo;
        this.undo = undo;
    }

    public override void Perform(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings = null)
    {
        levelData.pages[page].sentenceObjects[index].outlineSize = redo;
        sentenceObjects[index].GetComponent<LevelSentenceObject>().DisplayedText.outlineWidth = redo;
        sentenceObjects[index].GetComponent<LevelSentenceObject>().inputField.textComponent.outlineWidth = redo;
        // update furigana
        Transform furiganaParent = sentenceObjects[index].transform.GetChild(0).GetComponent<OrientedText>().FuriganaHandler.transform;
        for (int i = 0; i < furiganaParent.childCount; i++)
        {
            furiganaParent.GetChild(i).GetComponent<TMPro.TMP_Text>().outlineWidth = redo;
        }
    }

    public override void Undo(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings = null)
    {
        levelData.pages[page].sentenceObjects[index].outlineSize = undo;
        sentenceObjects[index].GetComponent<LevelSentenceObject>().DisplayedText.outlineWidth = undo;
        sentenceObjects[index].GetComponent<LevelSentenceObject>().inputField.textComponent.outlineWidth = undo;
        // update furigana
        Transform furiganaParent = sentenceObjects[index].transform.GetChild(0).GetComponent<OrientedText>().FuriganaHandler.transform;
        for (int i = 0; i < furiganaParent.childCount; i++)
        {
            furiganaParent.GetChild(i).GetComponent<TMPro.TMP_Text>().outlineWidth = undo;
        }
    }
}

/**
 * <summary>Text size edited action</summary>
 */
public class EditorActionEditTextSize : EditorAction
{
    public float redo, undo;
    public EditorActionEditTextSize(int index, int page, float redo, float undo) : base(index, page)
    {
        this.redo = redo;
        this.undo = undo;
    }

    public override void Perform(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings = null)
    {
        levelData.pages[page].sentenceObjects[index].textSize = redo;
        sentenceObjects[index].GetComponent<LevelSentenceObject>().DisplayedText.fontSize = redo;
        sentenceObjects[index].GetComponent<LevelSentenceObject>().inputField.textComponent.fontSize = redo;
    }

    public override void Undo(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings = null)
    {
        levelData.pages[page].sentenceObjects[index].textSize = undo;
        sentenceObjects[index].GetComponent<LevelSentenceObject>().DisplayedText.fontSize = undo;
        sentenceObjects[index].GetComponent<LevelSentenceObject>().inputField.textComponent.fontSize = undo;
    }
}

/**
 * <summary>Sentence position and size change action</summary>
 */
public class EditorActionTextRect : EditorAction
{
    public Rect redo, undo;
    public Vector2 offset;
    public EditorActionTextRect(int index, int page, Rect redo, Rect undo, Vector2 offset) : base(index, page)
    {
        this.redo = redo;
        this.undo = undo;
        this.offset = offset;
    }

    public override void Perform(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings = null)
    {
        levelData.pages[page].sentenceObjects[index].rect = redo;
        // level coords to gameobject coords
        sentenceObjects[index].GetComponent<RectTransform>().anchoredPosition = redo.position + offset;
        sentenceObjects[index].GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, redo.width);
        sentenceObjects[index].GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, redo.height);
    }

    public override void Undo(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings = null)
    {
        levelData.pages[page].sentenceObjects[index].rect = undo;
        // level coords to gameobject coords
        sentenceObjects[index].GetComponent<RectTransform>().anchoredPosition = undo.position + offset;
        sentenceObjects[index].GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, undo.width);
        sentenceObjects[index].GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, undo.height);
    }
}

/**
 * <summary>Sentence edited action</summary>
 */
public class EditorActionTextColor : EditorAction
{
    public bool redo, undo;
    public EditorActionTextColor(int index, int page, bool redo, bool undo) : base(index, page)
    {
        this.redo = redo;
        this.undo = undo;
    }

    public override void Perform(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings = null)
    {
        Color newColor;
        Color outlineColor;
        levelData.pages[page].sentenceObjects[index].color = redo;
        if (redo)
        {
            newColor = Color.black;
            outlineColor = Color.white;
        }
        else
        {
            newColor = Color.white;
            outlineColor = Color.black;
        }
        TMPro.TMP_Text displayedText = sentenceObjects[index].GetComponent<LevelSentenceObject>().DisplayedText;
        TMPro.TMP_Text inputText = sentenceObjects[index].GetComponent<LevelSentenceObject>().inputField.textComponent;
        displayedText.color = newColor;
        displayedText.outlineColor = outlineColor;
        inputText.color = newColor;
        inputText.outlineColor = outlineColor;
        sentenceSettings.GetComponent<MenuSentenceSettings>().buttonColor.GetComponent<Image>().color = newColor;
        Transform furiganaParent = sentenceObjects[index].transform.GetChild(0).GetComponent<OrientedText>().FuriganaHandler.transform;
        for (int i = 0; i < furiganaParent.childCount; i++)
        {
            furiganaParent.GetChild(i).GetComponent<TMPro.TMP_Text>().color = newColor;
            furiganaParent.GetChild(i).GetComponent<TMPro.TMP_Text>().outlineColor = outlineColor;
        }
    }

    public override void Undo(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings = null)
    {
        Color newColor;
        Color outlineColor;
        levelData.pages[page].sentenceObjects[index].color = undo;
        if (undo)
        {
            newColor = Color.black;
            outlineColor = Color.white;
        }
        else
        {
            newColor = Color.white;
            outlineColor = Color.black;
        }
        TMPro.TMP_Text displayedText = sentenceObjects[index].GetComponent<LevelSentenceObject>().DisplayedText;
        TMPro.TMP_Text inputText = sentenceObjects[index].GetComponent<LevelSentenceObject>().inputField.textComponent;
        displayedText.color = newColor;
        displayedText.outlineColor = outlineColor;
        inputText.color = newColor;
        inputText.outlineColor = outlineColor;
        sentenceSettings.GetComponent<MenuSentenceSettings>().buttonColor.GetComponent<Image>().color = newColor;
        Transform furiganaParent = sentenceObjects[index].transform.GetChild(0).GetComponent<OrientedText>().FuriganaHandler.transform;
        for (int i = 0; i < furiganaParent.childCount; i++)
        {
            furiganaParent.GetChild(i).GetComponent<TMPro.TMP_Text>().color = newColor;
            furiganaParent.GetChild(i).GetComponent<TMPro.TMP_Text>().outlineColor = outlineColor;
        }
    }
}

public class EditorActionTextBold : EditorAction
{
    public bool redo, undo;
    public EditorActionTextBold(int index, int page, bool redo, bool undo) : base(index, page)
    {
        this.redo = redo;
        this.undo = undo;
    }

    public override void Perform(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings = null)
    {
        levelData.pages[page].sentenceObjects[index].bold = redo;
        TMPro.TMP_Text displayedText = sentenceObjects[index].GetComponent<LevelSentenceObject>().DisplayedText;
        TMPro.TMP_Text inputText = sentenceObjects[index].GetComponent<LevelSentenceObject>().inputField.textComponent;
        TMPro.FontStyles fontStyle = TMPro.FontStyles.Normal;
        if (redo)
        {
            fontStyle = TMPro.FontStyles.Bold;
        }
        displayedText.fontStyle = fontStyle;
        Transform furiganaParent = sentenceObjects[index].transform.GetChild(0).GetComponent<OrientedText>().FuriganaHandler.transform;
        for (int i = 0; i < furiganaParent.childCount; i++)
        {
            furiganaParent.GetChild(i).GetComponent<TMPro.TMP_Text>().fontStyle = fontStyle;
        }
    }

    public override void Undo(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings = null)
    {
        levelData.pages[page].sentenceObjects[index].bold = undo;
        TMPro.TMP_Text displayedText = sentenceObjects[index].GetComponent<LevelSentenceObject>().DisplayedText;
        TMPro.TMP_Text inputText = sentenceObjects[index].GetComponent<LevelSentenceObject>().inputField.textComponent;
        TMPro.FontStyles fontStyle = TMPro.FontStyles.Normal;
        if (undo)
        {
            fontStyle = TMPro.FontStyles.Bold;
        }
        displayedText.fontStyle = fontStyle;
        Transform furiganaParent = sentenceObjects[index].transform.GetChild(0).GetComponent<OrientedText>().FuriganaHandler.transform;
        for (int i = 0; i < furiganaParent.childCount; i++)
        {
            furiganaParent.GetChild(i).GetComponent<TMPro.TMP_Text>().fontStyle = fontStyle;
        }
    }
}


public class EditorActionSetFurigana : EditorAction
{
    public string redo, undo;
    int furiganaIndex;
    public EditorActionSetFurigana(int index, int page, int furiganaIndex, string redo, string undo) : base(index, page)
    {
        this.redo = redo;
        this.undo = undo;
        this.furiganaIndex = furiganaIndex;
    }

    public override void Perform(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings = null)
    {
        Level.SentenceData sentenceData = levelData.pages[page].sentenceObjects[index];
        sentenceData.furigana[furiganaIndex] = redo;
    }

    public override void Undo(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings = null)
    {

        Level.SentenceData sentenceData = levelData.pages[page].sentenceObjects[index];
        sentenceData.furigana[furiganaIndex] = undo;
    }
}

public class EditorActionTextOrientation : EditorAction
{
    public bool redo, undo;
    public EditorActionTextOrientation(int index, int page, bool redo, bool undo) : base(index, page)
    {
        this.redo = redo;
        this.undo = undo;
    }
    public override void Perform(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings = null)
    {
        sentenceObjects[index].GetComponentInChildren<OrientedText>().SetOrientation(redo);
        levelData.pages[page].sentenceObjects[index].vertical = redo;
    }

    public override void Undo(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings = null)
    {
        sentenceObjects[index].GetComponentInChildren<OrientedText>().SetOrientation(undo);
        levelData.pages[page].sentenceObjects[index].vertical = undo;
    }
}

public class EditorActionTextAlignmentVertical : EditorAction
{
    public int redo, undo;
    TMPro.TextAlignmentOptions[,] alignmentMapping = { { TMPro.TextAlignmentOptions.TopLeft, TMPro.TextAlignmentOptions.Top, TMPro.TextAlignmentOptions.TopRight },
        { TMPro.TextAlignmentOptions.Left, TMPro.TextAlignmentOptions.Center, TMPro.TextAlignmentOptions.Right },
        { TMPro.TextAlignmentOptions.BottomLeft, TMPro.TextAlignmentOptions.Bottom, TMPro.TextAlignmentOptions.BottomRight },
    };
    public EditorActionTextAlignmentVertical(int index, int page, int redo, int undo) : base(index, page)
    {
        this.redo = redo;
        this.undo = undo;
    }
    public override void Perform(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings = null)
    {
        byte currAlign = levelData.pages[page].sentenceObjects[index].alignment;
        if(currAlign > 9)
        {
            Debug.LogError("Unsupported text alignment");
        }
        else
        {
            // iterates through hor then increment vertical, going from left/top to right/bottom
            // we can use this to calculate
            byte currHorAlign = (byte)(currAlign % 2);
            levelData.pages[page].sentenceObjects[index].alignment = (byte)(3 * this.redo+currHorAlign);
            sentenceObjects[index].GetComponent<TMPro.TMP_Text>().alignment = alignmentMapping[currHorAlign, redo];
        }
    }

    public override void Undo(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings = null)
    {
        byte currAlign = levelData.pages[page].sentenceObjects[index].alignment;
        if (currAlign > 9)
        {
            Debug.LogError("Unsupported text alignment");
        }
        else
        {
            // iterates through hor then increment vertical, going from left/top to right/bottom
            // we can use this to calculate
            byte currHorAlign = (byte)(currAlign % 2);
            levelData.pages[page].sentenceObjects[index].alignment = (byte)(3 * this.undo + currHorAlign);
            sentenceObjects[index].GetComponent<TMPro.TMP_Text>().alignment = alignmentMapping[currHorAlign, undo];
        }
    }
}

public class EditorActionTextAlignmentHorizontal : EditorAction
{
    public int redo, undo;
    TMPro.TextAlignmentOptions[,] alignmentMapping = { { TMPro.TextAlignmentOptions.TopLeft, TMPro.TextAlignmentOptions.Top, TMPro.TextAlignmentOptions.TopRight }, 
        { TMPro.TextAlignmentOptions.Left, TMPro.TextAlignmentOptions.Center, TMPro.TextAlignmentOptions.Right },
        { TMPro.TextAlignmentOptions.BottomLeft, TMPro.TextAlignmentOptions.Bottom, TMPro.TextAlignmentOptions.BottomRight },
    };
    public EditorActionTextAlignmentHorizontal(int index, int page, int redo, int undo) : base(index, page)
    {
        this.redo = redo;
        this.undo = undo;
    }
    public override void Perform(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings = null)
    {
        byte currAlign = levelData.pages[page].sentenceObjects[index].alignment;
        if (currAlign > 9)
        {
            Debug.LogError("Unsupported text alignment");
        }
        else
        {
            // iterates through hor then increment vertical, going from left/top to right/bottom
            // we can use this to calculate
            byte currVerAlign = (byte)(currAlign / 3);
            byte currHorAlign = (byte)(this.redo % 3);
            levelData.pages[page].sentenceObjects[index].alignment = (byte)(3 * currVerAlign + currHorAlign);
        }
    }

    public override void Undo(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings = null)
    {
        byte currAlign = levelData.pages[page].sentenceObjects[index].alignment;
        if (currAlign > 9)
        {
            Debug.LogError("Unsupported text alignment");
        }
        else
        {
            // iterates through hor then increment vertical, going from left/top to right/bottom
            // we can use this to calculate
            byte currVerAlign = (byte)(currAlign / 3);
            byte currHorAlign = (byte)(this.redo % 3);
            levelData.pages[page].sentenceObjects[index].alignment = (byte)(3 * this.undo + currHorAlign);
        }
    }
}


public class EditorActionDeleteSentence : EditorAction
{
    public GameObject prefab;
    public Transform parent;
    public Level.SentenceData undo;
    public Vector2 offset;
    public EditorActionDeleteSentence(int index, int page, Level.SentenceData undo, GameObject prefab, Transform parent, Vector2 offset) : base(index, page)
    {
        this.undo = undo;
        this.prefab = prefab;
        this.parent = parent;
        this.offset = offset;
    }

    public override void Perform(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings = null)
    {
        // remove from sentence objects
        Object.DestroyImmediate(sentenceObjects[index]);
        sentenceObjects.RemoveAt(index);
        // remove from data
        levelData.pages[page].sentenceObjects.RemoveAt(index);
    }

    public override void Undo(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings = null)
    {
        levelData.pages[page].sentenceObjects.Insert(index, undo);
        // create object for each in data
        GameObject sentenceObject = Object.Instantiate(prefab, parent);
        sentenceObject.GetComponent<LevelSentenceObject>().template = false;
        sentenceObject.GetComponent<RectTransform>().anchoredPosition = undo.rect.position + offset;
        sentenceObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, undo.rect.width);
        sentenceObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, undo.rect.height);
        sentenceObject.GetComponent<LevelSentenceObject>().DisplayedText.text = undo.text;
        // update to create furigana
        sentenceObject.transform.GetChild(0).GetComponent<OrientedText>().FuriganaHandler.GetComponent<SentenceFurigana>().UpdatedSentence(undo.text);
        // set furigana
        for (int i = 0; i < undo.furigana.Length; i++)
        {
            TMPro.TMP_Text furigana = sentenceObject.transform.GetChild(0).GetComponent<OrientedText>().FuriganaHandler.GetComponent<SentenceFurigana>().transform.GetChild(i).GetComponent<TMPro.TMP_Text>();
            furigana.text = undo.furigana[i];
            if (undo.color)
            {
                furigana.color = Color.black;
            }
            else
            {
                furigana.color = Color.white;
            }
        }
        sentenceObject.GetComponent<LevelSentenceObject>().DisplayedText.fontSize = undo.textSize;
        if (undo.color)
        {
            sentenceObject.GetComponent<LevelSentenceObject>().DisplayedText.color = Color.black;
            sentenceObject.GetComponent<LevelSentenceObject>().inputField.textComponent.color = Color.black;
        }
        else
        {
            sentenceObject.GetComponent<LevelSentenceObject>().DisplayedText.color = Color.white;
            sentenceObject.GetComponent<LevelSentenceObject>().inputField.textComponent.color = Color.white;
        }
        sentenceObject.GetComponentInChildren<OrientedText>().vertical = undo.vertical;
        sentenceObject.GetComponent<LevelSentenceObject>().sentenceDataIndex = index;
        sentenceObjects.Insert(index, sentenceObject);
    }
}

public class EditorActionCreateSentence : EditorAction
{
    public GameObject prefab;
    public Transform parent;
    public Rect redo;
    public Level.SentenceData sentenceData;
    public Vector2 offset;
    public EditorActionCreateSentence(int index, int page, Rect redo, GameObject prefab, Transform parent, Vector2 offset) : base(index, page)
    {
        this.prefab = prefab;
        this.redo = redo;
        this.parent = parent;
        this.offset = offset;
    }

    public override void Perform(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings = null)
    {
        // do
        GameObject sentenceObject = Object.Instantiate(prefab, parent);
        sentenceObject.GetComponent<LevelSentenceObject>().template = false;
        sentenceObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, redo.width);
        sentenceObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, redo.height);
        sentenceObject.transform.position = redo.position + new Vector2(0, 0);
        sentenceData = new Level.SentenceData();
        sentenceData.text = sentenceObject.GetComponent<LevelSentenceObject>().DisplayedText.text;
        sentenceData.rect = new Rect(sentenceObject.GetComponent<RectTransform>().anchoredPosition.x, sentenceObject.GetComponent<RectTransform>().anchoredPosition.y, sentenceObject.GetComponent<RectTransform>().rect.width, sentenceObject.GetComponent<RectTransform>().rect.height);
        sentenceData.furigana = new string[0];
        sentenceData.color = false;
        sentenceData.textSize = sentenceObject.GetComponent<LevelSentenceObject>().DisplayedText.fontSize;
        sentenceData.vertical = false;
        levelData.pages[page].sentenceObjects.Add(sentenceData);
        sentenceObject.GetComponent<LevelSentenceObject>().sentenceDataIndex = levelData.pages[page].sentenceObjects.Count - 1;
        sentenceObjects.Add(sentenceObject);
    }

    public override void Undo(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings = null)
    {
        // remove from sentence objects
        Object.DestroyImmediate(sentenceObjects[index]);
        sentenceObjects.RemoveAt(index);
        // remove from data
        levelData.pages[page].sentenceObjects.RemoveAt(index);
    }
}

public class EditorActionPasteSentenceObject : EditorAction
{
    public GameObject prefab;
    public Transform parent;
    public Level.SentenceData redo;
    public Vector2 offset;
    public EditorActionPasteSentenceObject(int index, int page, Level.SentenceData redo, GameObject prefab, Transform parent, Vector2 offset) : base(index, page)
    {
        this.redo = redo;
        this.prefab = prefab;
        this.parent = parent;
        this.offset = offset;
    }

    public override void Perform(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings = null)
    {
        levelData.pages[page].sentenceObjects.Insert(index, redo);
        // create object for each in data
        GameObject sentenceObject = Object.Instantiate(prefab, parent);
        sentenceObject.GetComponent<LevelSentenceObject>().template = false;
        sentenceObject.GetComponent<RectTransform>().anchoredPosition = redo.rect.position + offset;
        sentenceObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, redo.rect.width);
        sentenceObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, redo.rect.height);
        sentenceObject.GetComponent<LevelSentenceObject>().DisplayedText.text = redo.text;
        // update to create furigana
        sentenceObject.transform.GetChild(0).GetComponent<OrientedText>().FuriganaHandler.GetComponent<SentenceFurigana>().UpdatedSentence(redo.text);
        // set furigana
        for (int i = 0; i < redo.furigana.Length; i++)
        {
            TMPro.TMP_Text furigana = sentenceObject.transform.GetChild(0).GetComponent<OrientedText>().FuriganaHandler.GetComponent<SentenceFurigana>().transform.GetChild(i).GetComponent<TMPro.TMP_Text>();
            furigana.text = redo.furigana[i];
            if (redo.color)
            {
                furigana.color = Color.black;
            }
            else
            {
                furigana.color = Color.white;
            }
        }
        sentenceObject.GetComponent<LevelSentenceObject>().DisplayedText.fontSize = redo.textSize;
        if (redo.color)
        {
            sentenceObject.GetComponent<LevelSentenceObject>().DisplayedText.color = Color.black;
            sentenceObject.GetComponent<LevelSentenceObject>().inputField.textComponent.color = Color.black;
        }
        else
        {
            sentenceObject.GetComponent<LevelSentenceObject>().DisplayedText.color = Color.white;
            sentenceObject.GetComponent<LevelSentenceObject>().inputField.textComponent.color = Color.white;
        }
        sentenceObject.GetComponentInChildren<OrientedText>().vertical = redo.vertical;
        sentenceObject.GetComponent<LevelSentenceObject>().sentenceDataIndex = index;
        sentenceObjects.Insert(index, sentenceObject);
    }

    public override void Undo(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings = null)
    {
        // remove from sentence objects
        Object.DestroyImmediate(sentenceObjects[index]);
        sentenceObjects.RemoveAt(index);
        // remove from data
        levelData.pages[page].sentenceObjects.RemoveAt(index);
    }
}

public class EditorActionChangeOrder : EditorAction
{
    GameObject sentenceContextMenu;
    public EditorActionChangeOrder(int index, int page, GameObject sentenceContextMenu) : base(index, page)
    {
        this.sentenceContextMenu = sentenceContextMenu;
    }

    public override void Perform(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings = null)
    {
        int thisOrder = index + 1;
        int elements = levelData.pages[page].sentenceObjects.Count;
        // newOrder = thisOrder + 1 shifted to start at 0 modulo and shift to start at 1
        int newOrder = ((thisOrder + 1) - 1) % elements + 1;
        // update context menu
        // update sentence context menu order button
        sentenceContextMenu.transform.GetChild(0).GetComponentInChildren<TMPro.TMP_Text>().SetText(newOrder.ToString());
        // if there's been a change
        if (newOrder != thisOrder)
        {
            // update other object with this order to swap them
            // find the one that coincides with current
            // order 1....,n, index 0,....,n | order -> index: o-1

            // swap sentenceobject in data
            Level.SentenceData tempData = levelData.pages[page].sentenceObjects[index];
            levelData.pages[page].sentenceObjects[index] = levelData.pages[page].sentenceObjects[newOrder - 1];
            levelData.pages[page].sentenceObjects[newOrder - 1] = tempData;
            // swap gameobjects
            GameObject tempGameObject = sentenceObjects[index];
            sentenceObjects[index] = sentenceObjects[newOrder - 1];
            sentenceObjects[newOrder - 1] = tempGameObject;
            // update 
            sentenceObjects[index].GetComponent<LevelSentenceObject>().sentenceDataIndex = index;
            sentenceObjects[newOrder - 1].GetComponent<LevelSentenceObject>().sentenceDataIndex = newOrder - 1;
        }
    }

    public override void Undo(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings = null)
    {
        Perform(sentenceObjects, levelData, sentenceSettings);
    }
}

public class EditorActionDeletePage : EditorAction
{
    Level.PageData undo;
    public EditorActionDeletePage(int index, int page, Level.PageData undo) : base(index, page)
    {
        this.undo = undo;
    }

    public override void Perform(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings = null)
    {
        foreach(GameObject sentenceObject in sentenceObjects)
        {
            Object.Destroy(sentenceObject);
        }
        sentenceObjects.Clear();
        levelData.pages.RemoveAt(page);
    }

    public override void Undo(List<GameObject> sentenceObjects, Level levelData, GameObject sentenceSettings = null)
    {
        levelData.pages.Insert(page, undo);
    }
}