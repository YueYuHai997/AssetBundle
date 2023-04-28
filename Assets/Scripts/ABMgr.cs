using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class ABMgr : MonoBehaviour
{


    //下载总大小
    public float TotalLoad;
    //下载当前下载量
    public float CurrentLoad;

    //ab包资源异步加载进度
    public float ProcessValue;

    //资源异步加载进度
    public float ResVale;



    /// <summary>    主包名    </summary>
    private string ABMainName
    {
        get
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return "StandaloneWindows";
#elif UNITY_IOS
                return "IOS";
#elif UNITY_ANDROID
                return "Android"
#endif
        }
    }

    /// <summary>    文件下载路径     </summary>
    private string LoadSavePath
    {
        get
        {
            string basepath = Application.persistentDataPath + $"/AssetBundle/{ABMainName}/";
#if UNITY_EDITOR
            return basepath;
#elif          UNITY_STANDALONE
            return basepath;
#elif UNITY_IOS
                return  basepath;
#elif UNITY_ANDROID
                return  basepath;
#endif
        }
    }

    /// <summary>     默认资源路径     </summary>
    private string DefaultPath
    {
        get
        {
            string basepath = Application.streamingAssetsPath + $"/AssetBundle/{ABMainName}/";

#if UNITY_EDITOR
            return basepath;
#elif UNITY_STANDALONE
            return  basepath;
#elif UNITY_IOS
                return  basepath;
#elif UNITY_ANDROID
                return  basepath;
#endif
        }
    }

    /// <summary>     默认资源路径     </summary>
    private string TempPath
    {
        get
        {
            string basepath = Application.temporaryCachePath + $"/AssetBundle/{ABMainName}/";

#if UNITY_EDITOR
            return basepath;
# elif  UNITY_STANDALONE
            
            return  basepath;
#elif UNITY_IOS
                return  basepath;
#elif UNITY_ANDROID
                return   basepath;  
#endif
        }
    }

    /// <summary>     服务器路径     </summary>
    private string ServerPath
    {
        get
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return $"http://192.168.10.104:5000/AssetBundle/{ABMainName}/";
#endif
        }
    }


    //配置文件包名字
    private string CompareFile = "ABCompareInfo.txt";

    /// <summary>  远端AB包   </summary>
    private Dictionary<string, ABInfo> remoteABInfo = new Dictionary<string, ABInfo>();

    /// <summary>  本地AB包   </summary>
    private Dictionary<string, ABInfo> localABInfo = new Dictionary<string, ABInfo>();

    // <summary>  需要下载的AB包   </summary>
    private List<string> NeedLoadAB = new List<string>();

    /// <summary>  最终Ab包路径   </summary>
    private Dictionary<string, string> ABPath = new Dictionary<string, string>();


    UnityAction LoadFinish;


    private void Awake()
    {
        Load();


        Debug.Log("LoadSavePath:" + LoadSavePath);
        Debug.Log("DefaultPath:" + DefaultPath);
        Debug.Log("TempPath:" + TempPath);

        LoadFinish += () =>
        {
            Debug.Log("开始资源加载");
            LoadSceneAsync("scene 1", (_) =>
            {
                Manager_Scene.LoadSceneAsync(_, LoadSceneMode.Additive, (x) => 
                {
                    for (int i = 0; i < 10; i++)
                    {
                        LoadResAsync<GameObject>("zombie1", "zombie1", (p) => { Instantiate(p); });
                    }
                });

            });
        };
    }


    #region 加载AssetBundle


    async public void Load()
    {
        await System.Threading.Tasks.Task.Delay(1000);

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
        if (!abDic.ContainsKey(ABMainName))
        {
            AssetBundle abmain = AssetBundle.LoadFromFile(ABPath[ABMainName] + ABMainName);
            abDic.Add(ABMainName, abmain);

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
                var Depent = AssetBundle.LoadFromFile(ABPath[request[i]] + request[i]);
                abDic.Add(request[i], Depent);
            }
        }
        //加载目标包
        if (!abDic.ContainsKey(abName))
        {
            AssetBundle ab = AssetBundle.LoadFromFile(ABPath[abName] + abName);
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
        AssetBundleCreateRequest abcr = AssetBundle.LoadFromFileAsync(ABPath[abName] + abName);
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
    public async void NetLoadAsset()
    {
        if (!Directory.Exists(LoadSavePath))
            Directory.CreateDirectory(LoadSavePath);
        if (!Directory.Exists(TempPath))
            Directory.CreateDirectory(TempPath);

        remoteABInfo.Clear();
        localABInfo.Clear();
        NeedLoadAB.Clear();

        ReadLoaclAsset();

        //下载对比文件到临时路径
        LoadWebRequest(ServerPath, CompareFile, TempPath, async () =>
        {
            Debug.Log("配置文件下载完成");

            if (!File.Exists(LoadSavePath + CompareFile) || GetMD5(TempPath + CompareFile) != GetMD5(LoadSavePath + CompareFile))
            {
                //比较临时文件和本地配置文件差异 并下载差异文件
                await DoCompareFile();
            }
            else
            {
                string info = File.ReadAllText(TempPath + CompareFile);
                string[] abs = info.Split("\n");
                string[] infos = null;
                foreach (var item in abs)
                {
                    infos = item.Split("||");
                    //远端AB包信息
                    if (File.Exists(LoadSavePath + infos[0]))
                        ABPath.Add(infos[0], LoadSavePath);
                    else if (ABPath.ContainsKey(infos[0]))
                        ABPath[infos[0]] = DefaultPath;
                    else
                        Debug.LogError($"文件未找到{infos[0]}");
                }

                Debug.Log("当前资源为最新，无需更新");
            }

            //下载完成将临时配置文件写入下载路径 并删除临时路径
            if (File.Exists(LoadSavePath + CompareFile))
            {
                File.Delete(LoadSavePath + CompareFile);
            }
            File.Move(TempPath + CompareFile, LoadSavePath + CompareFile);

            Debug.Log("配置文件写入完成，完成更新");

            LoadFinish?.Invoke();
        });
        //LoadIISCompareFile(path, filename, callBack);

    }

    private async Task DoCompareFile()
    {
        //根据比较文件下载Ab文件
        string info = File.ReadAllText(TempPath + CompareFile);
        string[] abs = info.Split("\n");
        string[] infos = null;
        foreach (var item in abs)
        {
            infos = item.Split("||");
            //远端AB包信息
            remoteABInfo.Add(infos[0], new ABInfo(infos[0], infos[1], infos[2]));

            if (!ABPath.ContainsKey(infos[0]))
                ABPath.Add(infos[0], LoadSavePath);
        }
        foreach (var item in remoteABInfo.Keys)//根据远端AB文件作为参考
        {
            //本地没有文件或者本地文件与服务器不同 并且不是默认文件
            if (!localABInfo.ContainsKey(item) || localABInfo[item] != remoteABInfo[item])
            {
                if ((ABPath.ContainsKey(item) && ABPath[item] == DefaultPath))
                {
                }
                else
                {
                    NeedLoadAB.Add(item);
                }
            }
            else if (localABInfo[item] == remoteABInfo[item]) //资源相同
            {
                localABInfo.Remove(item);
            }
        }

        UnityAction Dele = null;
        foreach (var item in localABInfo.Keys)
        {
            File.Delete(LoadSavePath + item);
            Debug.Log($"删除多余文件{ LoadSavePath + item }");
        }

        Dele?.Invoke();

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
                TotalLoad += remoteABInfo[loadname].size / 1024f / 1024f;
                LoadWebRequest(ServerPath, loadname, callBack: () =>
                {
                    Debug.Log("Load Success");
                    NeedLoadAB.Remove(loadname);  //下载成功之后从NeedLoadAB 移除
                });
            }
            Debug.Log($"下载总大小为{ TotalLoad } MB");
        }

        while (!cts.IsCancellationRequested && NeedLoadAB.Count != 0)
        {
            await Task.Yield();
        }

        Debug.Log("资源全部下载完成");
    }


    /// <summary>
    /// 本地的AssetBundle文件
    /// </summary>
    private void ReadLoaclAsset()
    {
        DirectoryInfo directory = new DirectoryInfo(LoadSavePath);
        //该目录下的所有文件信息
        FileInfo[] fileInfos = directory.GetFiles();
        foreach (var item in fileInfos)
        {
            if (item.Extension == "") //ab包没有后缀
            {
                localABInfo.Add(item.Name, new ABInfo(item.Name, item.Length, GetMD5(item.FullName)));
            }
        }

        try
        {
            DirectoryInfo directory_D = new DirectoryInfo(DefaultPath);
            //该目录下的所有文件信息
            FileInfo[] fileInfos_D = directory_D.GetFiles();
            foreach (var item in fileInfos_D)
            {
                if (item.Extension == "") //ab包没有后缀
                {
                    //localABInfo.Add(item.Name, new ABInfo(item.Name, item.Length, GetMD5(item.FullName)));
                    ABPath[item.Name] = DefaultPath;
                }
            }
        }
        catch (System.Exception)
        {
            Debug.Log("没有默认AB包");
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
            callBack?.Invoke();
        };
        //异步下载
        _webClient.DownloadFileAsync(_uri, LoadSavePath + filenName);
    }

    /// <summary>
    /// 使用WebRequest从服务器下载资源
    /// </summary>
    /// <param name="serverPath"></param>
    /// <param name="fileName"></param>
    /// <param name="callBack"></param>
    async void LoadWebRequest(string serverPath, string fileName, string localPath = null, UnityAction callBack = null)
    {
        int attemptCount = 1;
        bool downloaded = false;
        int maxAttempts = 4;

        while (!cts.IsCancellationRequested && !downloaded && attemptCount < maxAttempts)
        {
            try
            {
                Debug.Log($"尝试下载[{fileName}],第[{attemptCount}]次");
                System.Action<int> progressCallback = (value => Debug.Log($"{fileName}下载进度：{value}%"));
                //var progress = new System.Progress<int>(value => Debug.Log($"下载进度：{value}%"));
                await DownloadFileAsync(serverPath, fileName, localPath, progressCallback);
                downloaded = true;
            }
            catch (System.Exception ex)
            {
                await Task.Delay(2000);  //等待2s再重新尝试
                Debug.Log($"下载失败：{ex.Message}");
                attemptCount++;
            }
        }
        if (downloaded)
        {
            Debug.Log($"{fileName}下载成功");
            callBack?.Invoke();
        }
        else
            Debug.Log($"{fileName}下载失败,请检查网络并重试");
    }

    /// <summary>  Task结束标志      /// </summary>
    private CancellationTokenSource cts = new CancellationTokenSource();

    /// <summary>
    /// 异步加载文件并保存
    /// </summary>
    /// <param name="url"></param>
    /// <param name="fileName"></param>
    /// <param name="progressCallback"></param>
    /// <returns></returns>
    public async Task DownloadFileAsync(string url, string fileName, string localpath = null, System.Action<int> progressCallback = null)
    {
        try
        {
            var request = (HttpWebRequest)WebRequest.Create(url + fileName);
            request.UseDefaultCredentials = true;
            request.Proxy.Credentials = request.Credentials;
            long existingFileSize = 0;
            if (string.IsNullOrEmpty(localpath))
            {
                localpath = LoadSavePath ;
            }

            if (File.Exists(localpath + fileName)) //如果已经有本地文件了
            {
                // 获取已经下载的文件的大小
                existingFileSize = new FileInfo(localpath + fileName).Length;

                // 添加Range请求头，指定需要下载的文件范围
                request.AddRange((int)existingFileSize);

                Debug.Log($"继续下载{fileName}从 {existingFileSize} bytes");
            }
            else
            {
                Debug.Log($"开始新下载{fileName}");
            }

            using (var response = (HttpWebResponse)await request.GetResponseAsync())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    using (var fileStream = new FileStream(localpath + fileName, FileMode.Append))
                    {
                        byte[] buffer = new byte[1024 * 1024];
                        int bytesRead;
                        long totalBytesRead = existingFileSize;
                        long totalBytes = response.ContentLength + existingFileSize;
                        while (!cts.IsCancellationRequested && (bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            totalBytesRead += bytesRead;
                            CurrentLoad = totalBytesRead;
                            TotalLoad = totalBytes;
                            progressCallback?.Invoke((int)((double)totalBytesRead / totalBytes * 100));
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                        }
                    }
                }
                Debug.Log($"{fileName}下载完成！");
            }
        }
        catch (WebException ex) when (ex.Status == WebExceptionStatus.ProtocolError)
        {
            //Debug.Log($"下载失败，远程服务器返回无效 http 状态码 '{((HttpWebResponse)ex.Response).StatusCode}'.");
            if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.RequestedRangeNotSatisfiable)
            {
                Debug.Log($"{fileName}无需下载");
            }
            else
            {
                throw new System.Exception($"远程服务器返回无效 http 状态码 '{((HttpWebResponse)ex.Response).StatusCode}'.");
            }
        }
        catch (WebException ex) when (ex.Status == WebExceptionStatus.ConnectFailure)
        {
            //Debug.Log($"下载失败，客户端未能连接到远程服务器 '{url}'.");
            throw new System.Exception($"客户端未能连接到远程服务器{url}");
        }

        catch (WebException ex)
        {
            //Debug.Log($"下载失败：{ex.Message}");
            throw new System.Exception($"{ex.Message}");
        }
    }

    private void OnApplicationQuit()
    {
        cts.Cancel();
        Debug.Log("程序停止");
    }

    #endregion

}
