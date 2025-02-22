using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

// Referenced: https://forum.unity.com/threads/application-loadlevel-resets-input-getaxis-work-arounds.118511/#post-7003463
public static partial class SceneHelper
{
    public static Action ImbetweenUnloads;
    public static Action FinishedChangeScene;
    public static List<string[]> betweenSceneData = new List<string[]>();
    public static bool changedSceneThisFrame;

    // @kurtdekker - to load next scene without resetting Input axes
    public static void LoadScene(string SceneNameToLoad)
    {
        changedSceneThisFrame = true;
        PendingPreviousScene = SceneManager.GetActiveScene().name;
        SceneManager.sceneLoaded += ActivatorAndUnloader;
        SceneManager.LoadScene(SceneNameToLoad, LoadSceneMode.Additive);
    }

    static string PendingPreviousScene;
    static void ActivatorAndUnloader(Scene scene, LoadSceneMode mode)
    {
        ImbetweenUnloads?.Invoke();
        SceneManager.sceneUnloaded += finishedUnload;
        SceneManager.sceneLoaded -= ActivatorAndUnloader;
        SceneManager.SetActiveScene(scene);
        SceneManager.UnloadSceneAsync(PendingPreviousScene);
        //FinishedChangeScene?.Invoke();
    }

    public static void finishedUnload(Scene scene)
    {
        FinishedChangeScene?.Invoke();
    }
}