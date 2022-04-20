using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonAddSentence : MonoBehaviour
{
    public SentenceLister ScriptSentenceLister;
    public void OnClick()
    {
        ScriptSentenceLister.AddNewSentence();
    }
}
