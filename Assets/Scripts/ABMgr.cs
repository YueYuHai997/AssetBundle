using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;


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
            return Application.streamingAssetsPath + "/AssetBundle/";
#endif
        }
    }


    /// <summary>    文件下载路径     </summary>
    private string LoadSavePath
    {
        get
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return Application.streamingAssetsPath + "/AssetBundle/";
#endif
        }
    }

    private string ServerPath = "http://192.168.10.104:5000/AssetBundle/";

    private string CompareFile = "ABCompareInfo.txt";


    #region 加载AssetBundle

    private void Awake()
    {
        Load();
    }

    async public void Load()
    {
        await System.Threading.Tasks.Task.Delay(3000);

        ////异步方法
        //LoadSceneAsync("scene 1", (x) =>
        //{
        //    //SceneManager.LoadScene(x, LoadSceneMode.Additive);
        //    Manager_Scene.LoadSceneAsync(x, LoadSceneMode.Additive, (_) => 
        //    {
        //        for (int i = 0; i < 100; i++)
        //        {
        //            //Instantiate(LoadRes<GameObject>("gun", "MCX"));
        //            LoadResAsync<GameObject>("zombie1", "zombie1", (_) => { Instantiate(_); });
        //            //LoadScnen("scene 1", (_) => { Manager_Scene.LoadSceneAsync(_, LoadSceneMode.Additive); });
        //        }
        //    });
        //});

        //同步加载方法
        //LoadScnen("scene 1", (x) =>
        //{
        //    Manager_Scene.LoadSence(x, LoadSceneMode.Additive);

        //    for (int i = 0; i < 10; i++)
        //    {
        //        Instantiate(LoadRes<GameObject>("zombie1", "zombie1"));
        //    }
        //});


        //for (int i = 0; i < 10; i++)
        //{
        //    Instantiate(LoadRes<GameObject>("zombie1", "zombie1"));
        //}

        //LocalLoad();


        NetLoadAsset();
    }

    AssetBundleManifest abmf;
    /// <summary> AssetBundle管理脚本 用于检测是否是重复加载AssetBundle管理脚本   </summary>
    private Dictionary<string, AssetBundle> abDic = new Dictionary<string, AssetBundle>();

    /// <summary> 加载内容记录提升重复加载效率   </summary>
    private Dictionary<string, dynamic> LoadedAss = new Dictionary<string, dynamic>();

    #region 同步加载

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
        if (!LoadedAss.ContainsKey(abName))
        {
            LoadedAss.Add(abName, abDic[abName].LoadAsset(ResName));
        }
        return LoadedAss[abName];
    }

    public T LoadRes<T>(string abName, string ResName) where T : Object
    {
        LoadAB(abName);
        if (!LoadedAss.ContainsKey(abName))
        {
            LoadedAss.Add(abName, abDic[abName].LoadAsset<T>(ResName));
        }
        return LoadedAss[abName];
    }

    public Object LoadRes(string abName, string ResName, System.Type types)
    {
        LoadAB(abName);
        if (!LoadedAss.ContainsKey(abName))
        {
            LoadedAss.Add(abName, abDic[abName].LoadAsset(ResName, types));
        }
        return LoadedAss[abName];
    }

    public void LoadScnen(string abName, UnityAction<string> CallBack)
    {
        LoadAB(abName);
        if (!LoadedAss.ContainsKey(abName))
        {
            LoadedAss.Add(abName, abDic[abName].GetAllScenePaths());
        }
        CallBack?.Invoke(LoadedAss[abName][0]);
    }


    #endregion


    #region 异步加载

    List<IEnumerator> ieList = new List<IEnumerator>();
    //private AssetBundle mainPage;
    private AssetBundleManifest abManifest;
    public float ProcessValue;
    public float ResVale;
    private AssetBundleCreateRequest CueentProcess;
    private AssetBundleRequest CueentRes;

    /// <summary>
    /// 异步加载AB包
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="isMainPage"></param>
    public void LoadABAsync(string abName, bool isMainPage = false)
    {
        StartCoroutine(IE_LoadABAsync(abName, isMainPage));
    }


    IEnumerator IE_LoadABAsync(string abName, bool isMainPage = false)
    {
        abDic.Add(abName, null);
        AssetBundleCreateRequest abcr = AssetBundle.LoadFromFileAsync(Path + abName);
        CueentProcess = abcr;
        yield return abcr;
        abDic[abName] = abcr.assetBundle;
        if (isMainPage)
        {
            abManifest = abDic[abName].LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
    }

    /// <summary>
    /// 异步加载AB包以及其主包和其依赖包
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    private void LoadABPageAsync(string pageName, UnityAction callback)
    {
        IEnumerator ie = Ie_LoadABPage(pageName, callback);
        ie.MoveNext();
        ieList.Add(ie);
    }

    bool isAnsycLoadMainPage;
    IEnumerator Ie_LoadABPage(string abName, UnityAction callback)
    {
        //获取主包
        if ((!abDic.ContainsKey(ABMainName) || abDic[ABMainName] == null) && !isAnsycLoadMainPage)
        {
            LoadABAsync(ABMainName, true);
            isAnsycLoadMainPage = true;
            yield return ABMainName;
        }
        else
        {
            yield return ABMainName;
        }

        string[] pagesName = abManifest.GetAllDependencies(abName);//得到所有依赖包的名称

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
    /// 检测异步加载是否完成，如果完成，IE_LoadABPage协程继续执行
    /// </summary>
    private void DetectionLoadingCompleted()
    {
        for (int i = 0; i < ieList.Count; i++)
        {
            if (abDic.ContainsKey((string)ieList[i].Current)
                && abDic[(string)ieList[i].Current] != null
                || (string)ieList[i].Current == ABMainName && abDic.ContainsKey(ABMainName) && abDic[ABMainName] != null)
            {
                if (!ieList[i].MoveNext())
                {
                    ieList.Remove(ieList[i]);
                }
            }
        }
    }
    private void Update()
    {
        DetectionLoadingCompleted();

        ProcessValue = CueentProcess?.progress ?? 0;

        ResVale = CueentRes?.progress ?? 0;
    }

    /// <summary>
    /// 异步加载
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="ResName"></param>
    /// <param name="CallBack"></param>
    public void LoadResAsync(string abName, string ResName, UnityAction<Object> CallBack)
    {
        LoadABPageAsync(abName, () =>
        {
            StartCoroutine(ResAsync(abName, ResName, CallBack));
        });
    }

    public void LoadResAsync(string abName, string ResName, System.Type type, UnityAction<Object> CallBack)
    {
        LoadABPageAsync(abName, () =>
        {
            StartCoroutine(ResAsync(abName, ResName, type, CallBack));
        });
    }

    public void LoadResAsync<T>(string abName, string ResName, UnityAction<T> CallBack) where T : Object
    {
        LoadABPageAsync(abName, () =>
        {
            StartCoroutine(ResAsync<T>(abName, ResName, CallBack));
        });
    }

    public void LoadSceneAsync(string abName, UnityAction<string> CallBack)
    {
        LoadABPageAsync(abName, () =>
        {
            StartCoroutine(ResAsync(abName, CallBack));
        });
    }



    private IEnumerator ResAsync(string abName, string ResName, UnityAction<Object> CallBack)
    {

        AssetBundleRequest abr = abDic[abName].LoadAssetAsync(ResName);
        CueentRes = abr;
        yield return abr;
        CallBack?.Invoke(abr.asset);
    }

    private IEnumerator ResAsync(string abName, string ResName, System.Type type, UnityAction<Object> CallBack)
    {

        AssetBundleRequest abr = abDic[abName].LoadAssetAsync(ResName, type);
        CueentRes = abr;
        yield return abr;
        CallBack?.Invoke(abr.asset);
    }

    private IEnumerator ResAsync<T>(string abName, string ResName, UnityAction<T> CallBack) where T : Object
    {

        AssetBundleRequest abr = abDic[abName].LoadAssetAsync<T>(ResName);
        CueentRes = abr;
        yield return abr;
        CallBack?.Invoke(abr.asset as T);
    }

    private IEnumerator ResAsync(string abName, UnityAction<string> CallBack)
    {

        string[] scenePaths = abDic[abName].GetAllScenePaths();
        yield return scenePaths;
        CallBack?.Invoke(scenePaths[0]);
    }

    #endregion
    #endregion


    #region 卸载AssetBundle

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

    #endregion

    /// <summary>
    /// 本地加载，不需要发布就可以加载。方便测试
    /// </summary>
    public void LocalLoad()
    {
#if UNITY_EDITOR
        var t = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName("mcx", "mcx");
        var gob = AssetDatabase.LoadAssetAtPath<GameObject>(t[0]);
        Instantiate(gob);
#endif 
    }

    #region 下载AssetBundle


    /// <summary>
    /// 从网络下载AssetBundle
    /// </summary>
    /// <param name="path"></param>
    /// <param name="filename"></param>
    /// <param name="callBack"></param>
    public void NetLoadAsset()
    {

        remoteABInfo.Clear();
        localABInfo.Clear();
        NeedLoadAB.Clear();

        ReadLoaclAsset();


        List<Task> task = new List<Task>();
        //下载比较文件
        StartCoroutine(LoadAssetBundle(ServerPath, CompareFile, () =>
        {
            //TODO-如果远端的配置文件和本地的配置文件MD5码相同则认为没有任何更新


            //根据比较文件下载Ab文件
            string info = File.ReadAllText(LoadSavePath + CompareFile);
            string[] abs = info.Split("\n");
            string[] infos = null;
            foreach (var item in abs)
            {
                infos = item.Split("||");
                //远端AB包信息
                remoteABInfo.Add(infos[0], new ABInfo(infos[0], infos[1], infos[2]));
                Debug.Log(infos[0]);
            }
            Debug.Log("配置文件下载完成");


            foreach (var item in remoteABInfo.Keys)//根据远端AB文件作为参考
            {
                if (!localABInfo.ContainsKey(item) || localABInfo[item] != remoteABInfo[item]) //本地没有文件或者 本地文件与服务器不同
                {
                    NeedLoadAB.Add(item);
                }
                else if (localABInfo[item] == remoteABInfo[item]) //资源相同
                {
                    localABInfo.Remove(item);
                }
                else
                {
                    //  localABInfo 剩下的都是本地多余信息 选择删除
                }
            }

            if (NeedLoadAB.Count == 0)
            {
                Debug.Log("当前资源为最新版本无需下载");
            }
            else
            {
                for (int i = 0; i < NeedLoadAB.Count; i++)
                {
                    string loadname = NeedLoadAB[i];
                    Debug.Log($"需要下载的包名{loadname}，下载大小为{ remoteABInfo[loadname].size / 1024f / 1024f} MB");
                    StartCoroutine(LoadAssetBundle(ServerPath, loadname, () =>
                    {
                        Debug.Log("Load Success");
                        NeedLoadAB.Remove(loadname);//下载成功之后从NeedLoadAB 移除
                    }));
                }
            }
            //ToDo 文件下载失败 再从NeedLoadAB下载没下载完成的部分 设置最大下载次数


        }));

        //LoadIISCompareFile(path, filename, callBack);

    }

    private void ReadLoaclAsset()
    {
        DirectoryInfo directory = Directory.CreateDirectory(LoadSavePath);
        //该目录下的所有文件信息
        FileInfo[] fileInfos = directory.GetFiles();
        foreach (var item in fileInfos)
        {
            if (item.Extension == "") //ab包没有后缀
            {
                localABInfo.Add(item.Name, new ABInfo(item.Name, item.Length, GetMD5(item.FullName)));
            }
        }
    }

    private static string GetMD5(string filepath)
    {
        //根据文件路径，获取文件流信息
        //将文件以流形式打开
        using (FileStream file = new FileStream(filepath, FileMode.Open))
        {
            //声明一个MD5对象 生成MD5码
            MD5 mD5 = new MD5CryptoServiceProvider();
            var md5Info = mD5.ComputeHash(file);

            //关闭文件流
            file.Close();

            //把16字节转换成16进制拼接字符串，减少MD5码长度
            StringBuilder sb = new StringBuilder();
            for (var i = 0; i < md5Info.Length; i++)
            {
                sb.Append(md5Info[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }


    /// <summary>  远端AB包   </summary>
    private Dictionary<string, ABInfo> remoteABInfo = new Dictionary<string, ABInfo>();

    /// <summary>  本地AB包   </summary>
    private Dictionary<string, ABInfo> localABInfo = new Dictionary<string, ABInfo>();

    // <summary>  需要下载的AB包   </summary>
    private List<string> NeedLoadAB = new List<string>();


    private class ABInfo
    {
        public string name;
        public long size;
        public string md5;

        public ABInfo(string name, string size, string md5)
        {
            this.name = name;
            this.size = long.Parse(size);
            this.md5 = md5;
        }
        public ABInfo(string name, long size, string md5)
        {
            this.name = name;
            this.size = size;
            this.md5 = md5;
        }

        public static bool operator ==(ABInfo a, ABInfo b)
        {
            if (!(a is null) && !(b is null) && a.md5 == b.md5) return true;
            return false;
        }

        public static bool operator !=(ABInfo a, ABInfo b)
        {
            if (!(a is null) && !(b is null) && a.md5 == b.md5) return false;
            return true;
        }

    }

    /// <summary>
    /// 从IIS服务器下载对比文件
    /// </summary>
    public void LoadIISCompareFile(string serverPath, string filenName, UnityAction callBack = null)
    {
        WebClient _webClient = new WebClient();
        //使用默认的凭据——读取的时候，只需默认凭据就可以
        _webClient.Credentials = CredentialCache.DefaultCredentials;
        string uri = serverPath + filenName;
        //下载的链接地址（文件服务器）
        System.Uri _uri = new System.Uri(uri);
        //注册下载进度事件通知
        //_webClient.DownloadProgressChanged += _webClient_DownloadProgressChanged;
        //注册下载完成事件通知
        _webClient.DownloadFileCompleted += (_, Y) =>
        {
            Debug.Log($"{filenName}||{Y.Error}");
            AssetDatabase.Refresh();
            callBack?.Invoke();
        };
        //异步下载
        _webClient.DownloadFileAsync(_uri, LoadSavePath + filenName);
    }

    /// <summary>
    /// 使用webRequest下载
    /// </summary>
    /// <param name="Serverpath"></param>
    /// <param name="filename"></param>
    /// <param name="CallBack"></param>
    /// <returns></returns>
    IEnumerator LoadAssetBundle(string serverPath, string fileName, UnityAction callBack = null)
    {
        //服务器上的文件路径
        string uri = serverPath + fileName;

        using (var webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Download Error:" + webRequest.error);
            }
            else
            {
                //下载完成后执行的回调
                if (webRequest.isDone)
                {
                    byte[] results = webRequest.downloadHandler.data;
                    if (!Directory.Exists(LoadSavePath))
                    {
                        Directory.CreateDirectory(LoadSavePath);
                    }
                    using (FileStream fs = File.Create(LoadSavePath + fileName))
                    {
                        fs.Write(results, 0, results.Length);
                        fs.Close();
                    }

                    //using (FileStream fs = File.Create(fileName))  //Task线程不能访问applocation
                    //{
                    //    fs.Write(results, 0, results.Length);
                    //    fs.Close();
                    //}

                    //FileInfo fileInfo = new FileInfo(LoadSavePath + filename);
                    //FileStream fs = fileInfo.Create();
                    ////fs.Write(字节数组, 开始位置, 数据长度);
                    //fs.Write(results, 0, results.Length);
                    //fs.Flush(); //文件写入存储到硬盘
                    //fs.Close(); //关闭文件流对象
                    //fs.Dispose(); //销毁文件对象

                    callBack?.Invoke();
                }
            }
        }
    }

    #endregion

}
