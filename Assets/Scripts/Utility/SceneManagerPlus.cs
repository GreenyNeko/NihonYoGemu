using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneManagerPlus
{
    static byte[] sharedData;

    public static byte[] GetCurrentData()
    {
        return sharedData;
    }

    public static void LoadScene(int sceneBuildIndex, byte[] data)
    {
        sharedData = data;
        SceneManager.LoadScene(sceneBuildIndex);
    }

    public static void LoadScene(string sceneName, byte[] data)
    {
        sharedData = data;
        SceneManager.LoadScene(sceneName);
    }

    public static void LoadScene(int sceneBuildIndex, byte[] data, LoadSceneMode mode)
    {
        sharedData = data;
        SceneManager.LoadScene(sceneBuildIndex, mode);
    }

    public static void LoadScene(string sceneName, byte[] data, LoadSceneMode mode)
    {
        sharedData = data;
        SceneManager.LoadScene(sceneName, mode);
    }

    public static void LoadScene(int sceneBuildIndex, byte[] data, LoadSceneParameters parameters)
    {
        sharedData = data;
        SceneManager.LoadScene(sceneBuildIndex, parameters);
    }

    public static void LoadScene(string sceneName, byte[] data, LoadSceneParameters parameters)
    {
        sharedData = data;
        SceneManager.LoadScene(sceneName, parameters);
    }
}
