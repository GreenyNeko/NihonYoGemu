using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class KanjiClickHandler : MonoBehaviour, IPointerClickHandler
{
    public InputHandler inputHandler;
    public Transform FuriganaParent;
    public GameObject ContextMenu;
    public float firstInputDelay;
    public EditorManager editorManager;
    public UnityEvent<int, string> OnFuriganaUpdated;
    TMP_Text currentFurigana;
    int clickedCharIdx = -1;
    UnityAction callback;
    int furiganaIdx;

    void Start()
    {
        callback = new UnityAction(DeselectFurigana);
    }

    void Update()
    {
        // if a furigana is selected
        if(currentFurigana != null)
        {
            // if user clicked
            if (Input.GetMouseButtonDown(0))
            {
                // if the click was on the kanji or the furigana
                if (!RectTransformUtility.RectangleContainsScreenPoint(currentFurigana.GetComponent<RectTransform>(), Input.mousePosition)
                    && TMP_TextUtilities.FindIntersectingCharacter(GetComponent<TMP_Text>(), Input.mousePosition, null, true) == -1)
                {
                    DeselectFurigana();
                }
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        TMP_Text text = GetComponent<TMP_Text>();
        TMP_CharacterInfo[] charInfos = text.textInfo.characterInfo;
        int clickedCharIdx = TMP_TextUtilities.FindIntersectingCharacter(text, eventData.position, null, true);
        if(clickedCharIdx >= 0)
        {
            if(JapaneseDictionary.IsKanji(charInfos[clickedCharIdx].character.ToString()))
            {
                int furiganaIndex = 0;
                foreach(char c in text.text.Substring(0, clickedCharIdx))
                {
                    furiganaIndex = JapaneseDictionary.IsKanji(c.ToString()) ? furiganaIndex + 1 : furiganaIndex;
                }
                furiganaIdx = furiganaIndex;
                // set current furigana
                currentFurigana = FuriganaParent.GetChild(furiganaIndex).GetComponent<TMP_Text>();
                // show caret
                currentFurigana.transform.GetChild(0).gameObject.SetActive(true);
                // enable input
                inputHandler.enabled = true;
                inputHandler.targetText = currentFurigana;
                inputHandler.SetCurrentInput(currentFurigana.text);
                inputHandler.OnEnterButton.AddListener(callback);
                editorManager.editingFurigana = true;
                editorManager.SetSelectedSentenceObject(transform.parent.parent.gameObject);
            }
        }
    }

    public void DeselectFurigana()
    {
        editorManager.SetSentenceFurigana(furiganaIdx, currentFurigana.text);
        inputHandler.enabled = false;
        currentFurigana.transform.GetChild(0).gameObject.SetActive(false);
        ContextMenu.SetActive(true);
        currentFurigana = null;
        inputHandler.OnEnterButton.RemoveListener(callback);
        editorManager.editingFurigana = false;
        editorManager.DeselectSentenceObject();
    }
}