#if(UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Xml;

/**
 * This class creates a component which takes kanjiDic2 and converts it to a new asset file
 */
[ExecuteInEditMode]
public class ConvertKanjiDic : MonoBehaviour
{
    // the kanjidic2 text asset
    public TextAsset kanjiDic2;

    /**
     * Converts the given kanji dic 2 from xml to binary
     */
    public void Convert()
    {
        List<Kanji> kanjis = new List<Kanji>();
        // TODO: is there a way to speed this up / make it more performant?
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(kanjiDic2.text);
        XmlNodeList xmlNodeList = xmlDocument.GetElementsByTagName("character");
        foreach (XmlNode xmlNode in xmlNodeList)
        {
            string character = xmlNode.SelectSingleNode("literal").InnerText;
            XmlNode miscNode = xmlNode.SelectSingleNode("misc");
            XmlNode freqNode = miscNode.SelectSingleNode("freq");
            int ranking = 2502;
            if (freqNode != null)
            {
                ranking = int.Parse(freqNode.InnerText);
            }

            XmlNode jlptNode = miscNode.SelectSingleNode("jlpt");
            int jlpt = 6;
            if (jlptNode != null)
            {
                jlpt = int.Parse(jlptNode.InnerText);
            }

            List<string> readings = new List<string>();
            // get onyomi readings
            XmlNodeList readingsNodes = xmlNode.SelectNodes("./reading_meaning/rmgroup/reading[@r_type='ja_on']");
            foreach (XmlNode readingNode in readingsNodes)
            {
                string reading = readingNode.InnerText;
                readings.Add(JapaneseDictionary.ConvertKanaToKana(reading));
            }

            // get kunyomi readings
            readingsNodes = xmlNode.SelectNodes("./reading_meaning/rmgroup/reading[@r_type='ja_kun']");
            foreach (XmlNode readingNode in readingsNodes)
            {

                string rawReading = readingNode.InnerText;
                if (rawReading.Contains("."))
                {
                    rawReading = rawReading.Substring(0, rawReading.IndexOf("."));
                }
                if (rawReading.Contains("-"))
                {
                    rawReading = rawReading.Replace("-", "");
                }
                readings.Add(rawReading);
            }
            kanjis.Add(new Kanji(character, ranking, jlpt).SetReadings(readings.Distinct<string>().ToArray()));
        }
        using(BinaryWriter binaryWriter = new BinaryWriter(File.Create(Application.dataPath + "/Data/kanjidef.txt")))
        {
            foreach(Kanji kanji in kanjis)
            {
                binaryWriter.Write(kanji.GetCharacter());
                binaryWriter.Write(kanji.GetJLPTLevel());
                binaryWriter.Write(kanji.GetRanking());
                binaryWriter.Write(kanji.GetReadings().Length);
                foreach(string reading in kanji.GetReadings())
                {
                    binaryWriter.Write(reading);
                }
            }
        }
    }
}
#endif