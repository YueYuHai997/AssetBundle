using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


public static class Manager_Scene
{

    /// <summary>
    /// 同步加载场景
    /// </summary>
    public static void LoadSence(int num, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        SceneManager.LoadScene(num, loadSceneMode);
    }

    public static void LoadSence(string scenename, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        SceneManager.LoadScene(scenename, loadSceneMode);
    }


    /// <summary>     异步加载进度     </summary>
    public static float LoadProcess;


    /// <summary>
    /// 异步加载场景
    /// </summary>
    /// <param name="scenename"></param>
    /// <param name="loadSceneMode"></param>
    /// <param name="callBack"></param>
    public static void LoadSceneAsync(string scenename, LoadSceneMode loadSceneMode = LoadSceneMode.Single, UnityAction<string> callBack = null)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(scenename, loadSceneMode);
        async.allowSceneActivation = true; //加载完成后自动跳转
        AwaitLoad(async, scenename, callBack);
    }
    private async static void AwaitLoad(AsyncOperation async, string scenename, UnityAction<string> callBack)
    {
        while (!async.isDone)
        {
            if (async == null) return;
            LoadProcess = async.progress;
            await Task.Yield();
            //if (async.progress >= 0.9f) //当进度浮点值到达1.0并调用isDone。如果将allowSceneActivation设置为false，则进度将在 0.9 处停止，直到被设置为 true。这对于创建加载条来说极其有用。
            //{
            //    async.allowSceneActivation = true; //加载完成后自动跳转
            //}
        }
        LoadProcess = 1.0f;
        callBack?.Invoke(scenename);
    }

    /// <summary>
    /// 卸载场景
    /// </summary>
    /// <param name="scenename"></param>
    /// <param name="callBack"></param>
    public static void UnloadScene(string scenename, UnityAction<string> callBack = null)
    {
        AsyncOperation async = SceneManager.UnloadSceneAsync(scenename);
        AwaitUnLoad(async, scenename, callBack);
    }

    private async static void AwaitUnLoad(AsyncOperation async, string scenename, UnityAction<string> callBack)
    {
        while (!async.isDone)
        {
            if (async == null) return;
            LoadProcess = async.progress;
            await Task.Delay(1);
            //if (async.progress >= 0.9f) //当进度浮点值到达1.0并调用isDone。如果将allowSceneActivation设置为false，则进度将在 0.9 处停止，直到被设置为 true。这对于创建加载条来说极其有用。
            //{
            //    async.allowSceneActivation = true; //加载完成后自动跳转
            //}
        }
        callBack?.Invoke(scenename);
        Resources.UnloadUnusedAssets();  //释放内存资源
    }
}
