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
    /// ͬ�����س���
    /// </summary>
    public static void LoadSence(int num, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        SceneManager.LoadScene(num, loadSceneMode);
    }

    public static void LoadSence(string scenename, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        SceneManager.LoadScene(scenename, loadSceneMode);
    }


    /// <summary>     �첽���ؽ���     </summary>
    public static float LoadProcess;


    /// <summary>
    /// �첽���س���
    /// </summary>
    /// <param name="scenename"></param>
    /// <param name="loadSceneMode"></param>
    /// <param name="callBack"></param>
    public static void LoadSceneAsync(string scenename, LoadSceneMode loadSceneMode = LoadSceneMode.Single, UnityAction<string> callBack = null)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(scenename, loadSceneMode);
        async.allowSceneActivation = true; //������ɺ��Զ���ת
        AwaitLoad(async, scenename, callBack);
    }
    private async static void AwaitLoad(AsyncOperation async, string scenename, UnityAction<string> callBack)
    {
        while (!async.isDone)
        {
            if (async == null) return;
            LoadProcess = async.progress;
            await Task.Yield();
            //if (async.progress >= 0.9f) //�����ȸ���ֵ����1.0������isDone�������allowSceneActivation����Ϊfalse������Ƚ��� 0.9 ��ֹͣ��ֱ��������Ϊ true������ڴ�����������˵�������á�
            //{
            //    async.allowSceneActivation = true; //������ɺ��Զ���ת
            //}
        }
        LoadProcess = 1.0f;
        callBack?.Invoke(scenename);
    }

    /// <summary>
    /// ж�س���
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
            //if (async.progress >= 0.9f) //�����ȸ���ֵ����1.0������isDone�������allowSceneActivation����Ϊfalse������Ƚ��� 0.9 ��ֹͣ��ֱ��������Ϊ true������ڴ�����������˵�������á�
            //{
            //    async.allowSceneActivation = true; //������ɺ��Զ���ת
            //}
        }
        callBack?.Invoke(scenename);
        Resources.UnloadUnusedAssets();  //�ͷ��ڴ���Դ
    }
}
