using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Script to handle tooltips
 */
public class Tooltip : MonoBehaviour
{
    /**
     * Sets visibility of itself
     */
    public void SetVisibility(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
