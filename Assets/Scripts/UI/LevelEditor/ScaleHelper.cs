using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScaleHelper : MonoBehaviour, IPointerDownHandler, IPointerMoveHandler, IPointerUpHandler
{
    public GameObject TextOrienter;
    public GameObject scaledObject;
    public int locationId;
    Vector2 offset = -Vector2.one;
    Vector2 actualPos;
    Vector2 originalPos;
    bool dragging = false;
    static List<int> verticalPoints = new List<int>{ 0, 1, 2, 4, 5, 6 };
    static List<int> horizontalPoints = new List<int> { 0, 2, 3, 4, 6, 7 };
    bool prevVertical = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector2 convertedCursor = new Vector2(eventData.position.x - 50, eventData.position.y - Screen.height + 50);
        if (offset.x < 0)
        {
            // position of this object in canvas coords
            actualPos = scaledObject.GetComponent<RectTransform>().anchoredPosition + transform.GetComponent<RectTransform>().anchoredPosition + new Vector2(scaledObject.GetComponent<RectTransform>().rect.width * transform.GetComponent<RectTransform>().anchorMin.x, -scaledObject.GetComponent<RectTransform>().rect.height * (1 - transform.GetComponent<RectTransform>().anchorMin.y));
            // difference between the cursor position and this scale helper
            offset = convertedCursor - actualPos;
            // actualPos.y wrong
            originalPos = transform.GetComponent<RectTransform>().anchoredPosition;
        }
        prevVertical = TextOrienter.GetComponent<OrientedText>().vertical;
        //TextOrienter.GetComponent<OrientedText>().vertical = false;
        dragging = true;
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        dragging = false;
        offset = -Vector2.one;
        scaledObject.GetComponent<LevelSentenceObject>().ScalingDone();
        // reset it
        transform.GetComponent<RectTransform>().anchoredPosition = originalPos;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (dragging)
        {
            if (TextOrienter.GetComponent<OrientedText>().vertical)
            {
                // fix scaling with vertical text
                RectTransform orientedBoxRectTransform = TextOrienter.transform.GetComponent<RectTransform>();
                //orientedBoxRectTransform.anchoredPosition = new Vector2(0, 0);
                orientedBoxRectTransform.offsetMin = new Vector2(0, 0);
                orientedBoxRectTransform.offsetMax = new Vector2(0, 0);
                float temp = orientedBoxRectTransform.rect.width;
                orientedBoxRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, orientedBoxRectTransform.rect.height);
                orientedBoxRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, temp);
            }
            Vector2 convertedCursor = new Vector2(Input.mousePosition.x - 50, Input.mousePosition.y - Screen.height + 50);
            transform.GetComponent<RectTransform>().anchoredPosition = originalPos;
            Vector2 size = (convertedCursor) - scaledObject.GetComponent<RectTransform>().anchoredPosition;
            // more left than top left
            if(horizontalPoints.Contains(locationId))
            {
                if ((convertedCursor - offset).x < scaledObject.GetComponent<RectTransform>().anchoredPosition.x)
                {
                    float distance = scaledObject.GetComponent<RectTransform>().anchoredPosition.x - convertedCursor.x;
                    scaledObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(convertedCursor.x, scaledObject.GetComponent<RectTransform>().anchoredPosition.y);
                    scaledObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(scaledObject.GetComponent<RectTransform>().rect.width + distance, 80));
                }
                else
                {
                    scaledObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(size.x, 80));
                }
            }
            if (verticalPoints.Contains(locationId))
            {
                // lower than top left
                if ((convertedCursor - offset).y > scaledObject.GetComponent<RectTransform>().anchoredPosition.y)
                {
                    float distance = scaledObject.GetComponent<RectTransform>().anchoredPosition.y - convertedCursor.y;
                    scaledObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(scaledObject.GetComponent<RectTransform>().anchoredPosition.x, convertedCursor.y);
                    scaledObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(scaledObject.GetComponent<RectTransform>().rect.height - distance, 80));

                }
                else
                {
                    scaledObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(-size.y, 80));
                }
            }
        }
    }
}
