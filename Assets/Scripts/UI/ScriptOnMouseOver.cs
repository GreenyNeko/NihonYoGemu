using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/**
 * This script allows you to define unity events that are called when the mouse enters or leaves the object.
 * Useful for tooltips for example
 */
public class ScriptOnMouseOver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public UnityEvent<bool> callOnMouseOver = new UnityEvent<bool>(); // shows a unity event part in the inspector
   
    /**
     * Invokes the event with the attribute true for "entered"
     */
    public void OnPointerEnter(PointerEventData eventData)
    {
        callOnMouseOver.Invoke(true);
    }

    /**
     * Invokes the event with the attribute false for "left"
     */
    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.fullyExited)
        {
            callOnMouseOver.Invoke(false);
        }
    }
}
