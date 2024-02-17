using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EditorCanvas : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    public Vector2 offset;
    public GameObject helper;
    public EditorManager editorManager;
    bool isDragging;
    Vector2 start, end;
    public void OnPointerDown(PointerEventData eventData)
    {
        if(!editorManager.editingFurigana)
        {
            editorManager.DeselectSentenceObject();
        }
        start = eventData.position + offset;
        isDragging = true;
        helper.transform.position = start;
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (isDragging)
        {

            if (Vector3.Distance(start, eventData.position) > 100)
            {
                Rect currRect = GetCorrectRect(start, eventData.position);
                helper.GetComponent<RectTransform>().position = currRect.position;
                helper.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currRect.width);
                helper.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, currRect.height);
                helper.SetActive(true);
            }
            else
            {
                helper.SetActive(false);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        end = eventData.position + offset;
        if(Vector3.Distance(start, end) > 100)
        {
            editorManager.GetComponent<EditorManager>().CreateSentenceObject(GetCorrectRect(start, end));
        }
        isDragging = false;
        helper.SetActive(false);
    }

    Rect GetCorrectRect(Vector2 start, Vector2 end)
    {
        Vector2 size = (end - start);
        Vector2 tmpStart = start;
        // need to flip y
        size.y = -size.y;
        if (size.x < 0)
        {
            // update x position to compensate negative width
            tmpStart.x += size.x;
            size.x = -size.x;
        }
        if (size.y < 0)
        {
            // update y position to compensate negative height
            tmpStart.y -= size.y;
            size.y = -size.y;
        }
        return new Rect(tmpStart, size);
    }
}
