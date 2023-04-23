using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetLoad : MonoBehaviour
{
    private async void Awake()
    {
        //AssetBundle ab  = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/mcx");
        //GameObject T = ab.LoadAsset<GameObject>("MCX");
        //Instantiate(T);

        StartCoroutine(LoadABAysnc("mcx", "MCx"));
    }

    AssetBundleCreateRequest abcr;
    /// <summary>
    /// 异步加载AB包
    /// </summary>
    /// <param name="Abname"></param>
    /// <param name="ResName"></param>
    /// <returns></returns>
    IEnumerator LoadABAysnc(string Abname, string ResName)
    {
        //第一步加载ab包
        abcr = AssetBundle.LoadFromFileAsync($"{Application.streamingAssetsPath}/{Abname}");
        yield return abcr;
        //加载资源
        AssetBundleRequest abr = abcr.assetBundle.LoadAssetAsync<GameObject>(Abname);
        yield return abr;

        RelyOn();
        RelyOn();
        Instantiate(abr.asset as GameObject);
    }


    public void RelyOn()
    {
        //加载主包
        AssetBundle abMain = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + "StandaloneWindows");

        //加载主包内的固定文件
        AssetBundleManifest abm = abMain.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

        //从固定文件中得到mcx所有依赖
        string[] strs = abm.GetAllDependencies("mcx");

        //加载所有依赖项
        for (int i = 0; i < strs.Length; i++)
        {
            //AssetBundle ab = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + strs[i]);
//Debug.Log(ab.name);
        }

    }


    private void OnGUI()
    {
        if (GUI.Button(new Rect(25,25,100,100),"卸载ab包(全)"))
        {
            abcr.assetBundle.Unload(true);
        }

        if (GUI.Button(new Rect(25, 125, 100, 100), "卸载ab包(保留)"))
        {
            abcr.assetBundle.Unload(false);
        }
    }
}
