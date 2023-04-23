using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ABMgr : MonoBehaviour
{

    /// <summary>     主包名    </summary>
    public string ABMainName
    {
        get
        {
#if UNITY_DEITOR || UNITY_STANDALONE
            return "StandaloneWindows";
#elif UNITY_IOS
                return "Ios";
#elif UNITY_ANDROID
                return "Android"
#endif
        }
    }

    /// <summary>    路径     </summary>
    public string Path
    {
        get
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return Application.streamingAssetsPath + "/";
#endif
        }
    }

    private void Awake()
    {
        for (int i = 0; i < 10; i++)
        {
            LoadResAsync<GameObject>("gun", "MCX", (x) => { Instantiate(x); });
        }
    }

    AssetBundleManifest abmf;
    private Dictionary<string, AssetBundle> abDic = new Dictionary<string, AssetBundle>();

    /// <summary>
    /// 同步加载AB包
    /// </summary>
    /// <param name="abName"></param>
    private void LoadAB(string abName)
    {
        //获取主包
        if (!abDic.ContainsKey("StandaloneWindows"))
        {
            AssetBundle abmain = AssetBundle.LoadFromFile(Path + ABMainName);
            abDic.Add("StandaloneWindows", abmain);

            //获取主包依赖
            abmf = abDic["StandaloneWindows"].LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
        //获取所有依赖
        string[] request = abmf.GetAllDependencies(abName);
        //加载所有依赖
        for (int i = 0; i < request.Length; i++)
        {
            if (!abDic.ContainsKey(request[i]))
            {
                var Depent = AssetBundle.LoadFromFile(Path + request[i]);
                abDic.Add(request[i], Depent);
            }
        }
        //加载目标包
        if (!abDic.ContainsKey(abName))
        {
            AssetBundle ab = AssetBundle.LoadFromFile(Path + abName);
            abDic.Add(abName, ab);
        }
    }

    /// <summary>
    /// 同步加载方法
    /// </summary>
    /// <param name="abName">ab包名</param>
    /// <param name="ResName">资源名字</param>
    /// <returns></returns>
    public object LoadRes(string abName, string ResName)
    {
        LoadAB(abName);
        return abDic[abName].LoadAsset(ResName);
    }

    public T LoadRes<T>(string abName, string ResName) where T : Object
    {
        LoadAB(abName);
        return abDic[abName].LoadAsset<T>(ResName);
    }

    public Object LoadRes(string abName, string ResName, System.Type types)
    {
        LoadAB(abName);
        return abDic[abName].LoadAsset(ResName, types);
    }

    #region 异步加载

    public void LoadABAsync(string abName, bool isMainPage = false)
    {
        StartCoroutine(IE_LoadABAsync(abName, isMainPage));
    }
    private AssetBundle mainPage;

    IEnumerator IE_LoadABAsync(string abName, bool isMainPage = false)
    {
        if (isMainPage)
        {
            abDic.Add(ABMainName, null);
            AssetBundleCreateRequest abcr = AssetBundle.LoadFromFileAsync(Path + abName);
            yield return abcr;

            abDic[ABMainName] = abcr.assetBundle;
            abmf = abcr.assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
        else
        {
            abDic.Add(abName, null);
            AssetBundleCreateRequest abcr = AssetBundle.LoadFromFileAsync(Path + abName);
            yield return abcr;
            abDic[abName] = abcr.assetBundle;
        }
    }



    IEnumerator Ie_LoadABPage(string abName,UnityAction callback)
    {
        //获取主包
        if (!abDic.ContainsKey("StandaloneWindows"))
        {
            LoadABAsync(abName, true);
            //yield return abName;
            yield return abDic[ABMainName];
        }
        else
        {
            yield return abDic[ABMainName];
        }

        string[] pagesName = abmf.GetAllDependencies(abName);//得到所有依赖包的名称

        for (int i = 0; i < pagesName.Length; i++)
        {
            if (!abDic.ContainsKey(pagesName[i]))
            {
                LoadABAsync(pagesName[i]);
                yield return pagesName[i];
            }
            else
            {
                yield return pagesName[i];
            }
        }

        //加载目标包
        if (!abDic.ContainsKey(abName))
        {
            LoadABAsync(abName);
            yield return abName;
        }
        else
        {
            yield return abName;
        }

        callback?.Invoke();
    }



    /// <summary>
    /// 异步加载
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="ResName"></param>
    /// <param name="CallBack"></param>
    public void LoadResAsync(string abName, string ResName, UnityAction<Object> CallBack)
    {
        StartCoroutine(ResAsync(abName, ResName, CallBack));
    }

    public void LoadResAsync(string abName, string ResName, System.Type type, UnityAction<Object> CallBack)
    {
        StartCoroutine(ResAsync(abName, ResName, type, CallBack));
    }

    public void LoadResAsync<T>(string abName, string ResName, UnityAction<T> CallBack) where T : Object
    {
        StartCoroutine(ResAsync<T>(abName, ResName, CallBack));
    }

    private IEnumerator ResAsync(string abName, string ResName, UnityAction<Object> CallBack)
    {
        LoadAB(abName);
        AssetBundleRequest abr = abDic[abName].LoadAssetAsync(ResName);
        yield return abr;
        CallBack?.Invoke(abr.asset);
    }

    private IEnumerator ResAsync(string abName, string ResName, System.Type type, UnityAction<Object> CallBack)
    {
        LoadAB(abName);
        AssetBundleRequest abr = abDic[abName].LoadAssetAsync(ResName, type);
        yield return abr;
        CallBack?.Invoke(abr.asset);
    }
    private IEnumerator ResAsync<T>(string abName, string ResName, UnityAction<T> CallBack) where T : Object
    {
        LoadAB(abName);
        AssetBundleRequest abr = abDic[abName].LoadAssetAsync<T>(ResName);
        yield return abr;
        CallBack?.Invoke(abr.asset as T);
    }

    #endregion

    /// <summary>
    /// 单个包卸载
    /// </summary>
    /// <param name="ab"></param>
    /// <param name="WithLoad">是否销毁已加载物体</param>
    public void UnAB(string abName, bool WithLoad)
    {
        if (abDic.ContainsKey(abName))
        {
            abDic[abName].Unload(WithLoad);
            abDic.Remove(abName);
        }
    }

    /// <summary>
    /// 所有包卸载
    /// </summary>
    /// <param name="WithLoad">是否销毁已加载物体</param>
    public void UnAllAB(bool WithLoad)
    {
        AssetBundle.UnloadAllAssetBundles(WithLoad);
        abDic.Clear();

    }

}
