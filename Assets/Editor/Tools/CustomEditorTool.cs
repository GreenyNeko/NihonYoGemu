#if(UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(ConvertKanjiDic))]
public class CustomEditorTool : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(GUILayout.Button("Convert"))
        {
            ConvertKanjiDic convertKanjiDicScript = (ConvertKanjiDic)target;
            convertKanjiDicScript.Convert();
        }
    }
}

#endif