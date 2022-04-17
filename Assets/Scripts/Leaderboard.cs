using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/**
 * Class representing a leaderboard file
 */
public class Leaderboard
{
    const string Version = "1"; // keeps track of the current version

    /**
     * Contains a single score
     */
    public struct HighScore
    {
        public string username;             // stores the players username
        public uint score;                   // keeps track of the players score
        public ushort correct, sloppy, miss; // keeps track of the players kanji performance
        public ushort combo;                 // stores the highest combo
        public float accuracy;              // stores the player's kanji accuracy
        public GameMods mods;               // stores active mods of this score
        public sbyte rank;                   // stores the performance rank
        public ulong timestamp;              // timestamp of when the score was done
    }

    // contains all the scores in this leaderboard
    List<HighScore> highScores;

    /**
     * Creates a leaderboard data structure
     */
    public Leaderboard()
    {
        highScores = new List<HighScore>();
    }

    /**
     * Loads a leaderboard score file
     */
    public static Leaderboard LoadLeaderboardByName(string levelName)
    {
        Leaderboard leaderboard = new Leaderboard();
        // if the score file exists load
        if(File.Exists("Scores/" + levelName + ".nys"))
        {
            try
            {
                using (BinaryReader binaryReader = new BinaryReader(File.Open("Scores/" + levelName + ".nys", FileMode.Open)))
                {
                    // read file tag
                    string fileTag = "";
                    char[] tagChars = binaryReader.ReadChars(3);
                    fileTag += tagChars[0];
                    fileTag += tagChars[1];
                    fileTag += tagChars[2];
                    if (fileTag != "NYS")
                    {
                        // not a nihonyo score file or corrupted
                        return null;
                    }
                    string version = binaryReader.ReadString();
                    if (version.Substring(1) != Leaderboard.Version)
                    {
                        // version mismatch
                        // in this case use the old loading format and save the leaderboard using the new format
                    }
                    while (binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
                    {
                        HighScore highScore = new HighScore();
                        highScore.score = binaryReader.ReadUInt32();
                        highScore.correct = binaryReader.ReadUInt16();
                        highScore.sloppy = binaryReader.ReadUInt16();
                        highScore.miss = binaryReader.ReadUInt16();
                        highScore.combo = binaryReader.ReadUInt16();
                        highScore.accuracy = binaryReader.ReadInt16() / 100.0f;
                        highScore.mods = (GameMods)binaryReader.ReadByte();
                        highScore.rank = binaryReader.ReadSByte();
                        highScore.username = binaryReader.ReadString();
                        highScore.timestamp = binaryReader.ReadUInt64();
                        leaderboard.Add(highScore);
                    }
                }
            }
            catch(EndOfStreamException)
            {
                Debug.LogWarning("Unable to load highscore!");
            }
        }
        return leaderboard;
    }

    /**
     * Saves a leaderboard to a score file
     */
    public void Save(string levelName)
    {
        using (BinaryWriter binaryWriter = new BinaryWriter(File.Open("Scores/" + levelName + ".nys", FileMode.Create)))
        {
            binaryWriter.Write(new char[]{'N', 'Y', 'S' });
            binaryWriter.Write("v" + Version);
            foreach (HighScore highScore in highScores)
            {
                binaryWriter.Write(highScore.score);
                binaryWriter.Write(highScore.correct);
                binaryWriter.Write(highScore.sloppy);
                binaryWriter.Write(highScore.miss);
                binaryWriter.Write(highScore.combo);
                binaryWriter.Write((ushort)(highScore.accuracy*100));
                binaryWriter.Write((sbyte)highScore.mods);
                binaryWriter.Write(highScore.rank);
                binaryWriter.Write(highScore.username);
                binaryWriter.Write(highScore.timestamp);
            }
        }
    }

    /**
     * Adds a highscore to this leaderboards
     */
    public void Add(HighScore highScore)
    {
        // if there are 10 scores only add if it's worth adding
        if (highScores.Count >= 10 && highScores[highScores.Count - 1].score > highScore.score)
        {
            return;
        }
        // add new highscore
        highScores.Add(highScore);
        // sort
        highScores.Sort((s1, s2) => { return s1.score.CompareTo(s2.score); });
        // remove lowest score
        if(highScores.Count > 10)
        {
            highScores.RemoveAt(highScores.Count - 1);
        }
    }

    /**
     * Returns amounts of highscores the leaderboard has
     */
    public int Count()
    {
        return highScores.Count;
    }

    /**
     * Gets the highscore at index from leaderboard
     */
    public HighScore this[int idx]
    {
        get
        {
            return highScores[idx];
        }
    }
}
