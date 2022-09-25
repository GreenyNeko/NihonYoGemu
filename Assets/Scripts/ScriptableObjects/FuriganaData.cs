using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * <summary>ScriptableObject containing data about furigana</summary>
 */
[CreateAssetMenu(fileName = "Data", menuName = "Data/Furigana Data", order =1)]
public class FuriganaData : ScriptableObject
{
    public int defaultSize;     // size for 0 or more than 4 characters
    public int sizeFurigana4;   // size for 4 characters
    public int sizeFurigana3;   // size for 3 characters
    public int sizeFurigana2;   // size for 2 characters
    public int sizeFurigana1;   // size for 1 character

    /**
     * <summary>Given a character count returns a fitting size.</summary>
     */
    public int GetSizeByCount(int charCount)
    {
        switch (charCount)
        {
            case 4:
                return sizeFurigana4;
            case 3:
                return sizeFurigana3;
            case 2:
                return sizeFurigana2;
            case 1:
                return sizeFurigana1;
            default:
                return defaultSize;
        }
    }
}
