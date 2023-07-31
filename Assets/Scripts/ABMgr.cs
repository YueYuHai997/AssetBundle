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


    //�����ܴ�С
    public float TotalLoad;
    //���ص�ǰ������
    public float CurrentLoad;
    //ab����Դ�첽���ؽ���
    public float ProcessValue;
    //��Դ�첽���ؽ���
    public float ResVale;
    /// <summary>    ������    </summary>
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

    /// <summary>    �ļ�����·��     </summary>
    private string LoadSavePath
    {
        get
        {
            string basepath = Application.persistentDataPath + $"/AssetBundle/{ABMainName}/";
#if UNITY_EDITOR
            return basepath;
#elif UNITY_STANDALONE
            return basepath;
#elif UNITY_IOS
                return  basepath;
#elif UNITY_ANDROID
                return  basepath;
#endif
        }
    }

    /// <summary>     Ĭ����Դ·��     </summary>
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

    /// <summary>     ��ʱ����·��     </summary>
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

    /// <summary>     ������·��     </summary>
    private string ServerPath
    {
        get
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return $"http://192.168.10.104:5000/AssetBundle/{ABMainName}/";
#endif
        }
    }


    //�����ļ�������
    private string CompareFile = "ABCompareInfo.txt";

    /// <summary>  Զ��AB��   </summary>
    private Dictionary<string, ABInfo> remoteABInfo = new Dictionary<string, ABInfo>();

    /// <summary>  ����AB��   </summary>
    private Dictionary<string, ABInfo> localABInfo = new Dictionary<string, ABInfo>();

    // <summary>  ��Ҫ���ص�AB��   </summary>
    private List<string> NeedLoadAB = new List<string>();

    /// <summary>  ����Ab��·��   </summary>
    private Dictionary<string, string> ABPath = new Dictionary<string, string>();

    UnityAction LoadFinish;


    private void Awake()
    {
        DemoLoad();

        Debug.Log("LoadSavePath:" + LoadSavePath);
        Debug.Log("DefaultPath:" + DefaultPath);
        Debug.Log("TempPath:" + TempPath);

        LoadFinish += () =>
        {
            Debug.Log("��ʼ��Դ����");
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


    #region ����AssetBundle


    public void DemoLoad()
    {
        //await System.Threading.Tasks.Task.Delay(1000);
        ////�첽����
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

        //ͬ�����ط���
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
    /// <summary> AssetBundle����ű� ���ڼ���Ƿ����ظ�����AssetBundle����ű�   </summary>
    private Dictionary<string, AssetBundle> abDic = new Dictionary<string, AssetBundle>();

    /// <summary> �������ݼ�¼�����ظ�����Ч��   </summary>
    private Dictionary<string, dynamic> LoadedAss = new Dictionary<string, dynamic>();

    #region ͬ������

    /// <summary>
    /// ͬ������AB��
    /// </summary>
    /// <param name="abName"></param>
    private void LoadAB(string abName)
    {
        //��ȡ����
        if (!abDic.ContainsKey(ABMainName))
        {
            AssetBundle abmain = AssetBundle.LoadFromFile(ABPath[ABMainName] + ABMainName);
            abDic.Add(ABMainName, abmain);

            //��ȡ��������
            abmf = abDic["StandaloneWindows"].LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
        //��ȡ��������
        string[] request = abmf.GetAllDependencies(abName);
        //������������
        for (int i = 0; i < request.Length; i++)
        {
            if (!abDic.ContainsKey(request[i]))
            {
                var Depent = AssetBundle.LoadFromFile(ABPath[request[i]] + request[i]);
                abDic.Add(request[i], Depent);
            }
        }
        //����Ŀ���
        if (!abDic.ContainsKey(abName))
        {
            AssetBundle ab = AssetBundle.LoadFromFile(ABPath[abName] + abName);
            abDic.Add(abName, ab);
        }
    }

    /// <summary>
    /// ͬ�����ط���
    /// </summary>
    /// <param name="abName">ab����</param>
    /// <param name="ResName">��Դ����</param>
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


    #region �첽����

    List<IEnumerator> ieList = new List<IEnumerator>();
    //private AssetBundle mainPage;
    private AssetBundleManifest abManifest;
    private AssetBundleCreateRequest CueentProcess;
    private AssetBundleRequest CueentRes;

    /// <summary>
    /// �첽����AB��
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
    /// �첽����AB���Լ�����������������
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
        //��ȡ����
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

        string[] pagesName = abManifest.GetAllDependencies(abName);//�õ�����������������

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

        //����Ŀ���
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
    /// ����첽�����Ƿ���ɣ������ɣ�IE_LoadABPageЭ�̼���ִ��
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
    /// �첽����
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


    #region ж��AssetBundle

    /// <summary>
    /// ������ж��
    /// </summary>
    /// <param name="ab"></param>
    /// <param name="WithLoad">�Ƿ������Ѽ�������</param>
    public void UnAB(string abName, bool WithLoad)
    {
        if (abDic.ContainsKey(abName))
        {
            abDic[abName].Unload(WithLoad);
            abDic.Remove(abName);
        }
    }

    /// <summary>
    /// ���а�ж��
    /// </summary>
    /// <param name="WithLoad">�Ƿ������Ѽ�������</param>
    public void UnAllAB(bool WithLoad)
    {
        AssetBundle.UnloadAllAssetBundles(WithLoad);
        abDic.Clear();
    }

    #endregion

    /// <summary>
    /// ���ؼ��أ�����Ҫ�����Ϳ��Լ��ء��������
    /// </summary>
    public void LocalLoad()
    {
#if UNITY_EDITOR
        var t = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName("mcx", "mcx");
        var gob = AssetDatabase.LoadAssetAtPath<GameObject>(t[0]);
        Instantiate(gob);
#endif 
    }

    #region ����AssetBundle

    /// <summary>
    /// ����������AssetBundle
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

        //���ضԱ��ļ�����ʱ·��
        LoadWebRequest(ServerPath, CompareFile, TempPath, async () =>
        {
            Debug.Log("�����ļ��������");

            if (!File.Exists(LoadSavePath + CompareFile) || GetMD5(TempPath + CompareFile) != GetMD5(LoadSavePath + CompareFile))
            {
                //�Ƚ���ʱ�ļ��ͱ��������ļ����� �����ز����ļ�
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
                    //Զ��AB����Ϣ
                    if (File.Exists(LoadSavePath + infos[0]))
                        ABPath.Add(infos[0], LoadSavePath);
                    else if (ABPath.ContainsKey(infos[0]))
                        ABPath[infos[0]] = DefaultPath;
                    else
                        Debug.LogError($"�ļ�δ�ҵ�{infos[0]}");
                }

                Debug.Log("��ǰ��ԴΪ���£��������");
            }

            //������ɽ���ʱ�����ļ�д������·�� ��ɾ����ʱ·��
            if (File.Exists(LoadSavePath + CompareFile))
            {
                File.Delete(LoadSavePath + CompareFile);
            }
            File.Move(TempPath + CompareFile, LoadSavePath + CompareFile);

            Debug.Log("�����ļ�д����ɣ���ɸ���");

            LoadFinish?.Invoke();
        });
        //LoadIISCompareFile(path, filename, callBack);

    }

    private async Task DoCompareFile()
    {
        //���ݱȽ��ļ�����Ab�ļ�
        string info = File.ReadAllText(TempPath + CompareFile);
        string[] abs = info.Split("\n");
        string[] infos = null;
        foreach (var item in abs)
        {
            infos = item.Split("||");
            //Զ��AB����Ϣ
            remoteABInfo.Add(infos[0], new ABInfo(infos[0], infos[1], infos[2]));

            if (!ABPath.ContainsKey(infos[0]))
                ABPath.Add(infos[0], LoadSavePath);
        }
        foreach (var item in remoteABInfo.Keys)//����Զ��AB�ļ���Ϊ�ο�
        {
            //����û���ļ����߱����ļ����������ͬ ���Ҳ���Ĭ���ļ�
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
            else if (localABInfo[item] == remoteABInfo[item]) //��Դ��ͬ
            {
                localABInfo.Remove(item);
            }
        }

        UnityAction Dele = null;
        foreach (var item in localABInfo.Keys)
        {
            File.Delete(LoadSavePath + item);
            Debug.Log($"ɾ�������ļ�{ LoadSavePath + item }");
        }

        Dele?.Invoke();

        if (NeedLoadAB.Count == 0)
        {
            Debug.Log("��ǰ��ԴΪ���°汾��������");
        }
        else
        {
            for (int i = 0; i < NeedLoadAB.Count; i++)
            {
                string loadname = NeedLoadAB[i];
                Debug.Log($"��Ҫ���صİ���{loadname}�����ش�СΪ{ remoteABInfo[loadname].size / 1024f / 1024f} MB");
                TotalLoad += remoteABInfo[loadname].size / 1024f / 1024f;
                LoadWebRequest(ServerPath, loadname, callBack: () =>
                {
                    Debug.Log("Load Success");
                    NeedLoadAB.Remove(loadname);  //���سɹ�֮���NeedLoadAB �Ƴ�
                });
            }
            Debug.Log($"�����ܴ�СΪ{ TotalLoad } MB");
        }

        while (!cts.IsCancellationRequested && NeedLoadAB.Count != 0)
        {
            await Task.Yield();
        }

        Debug.Log("��Դȫ���������");
    }


    /// <summary>
    /// ���ص�AssetBundle�ļ�
    /// </summary>
    private void ReadLoaclAsset()
    {
        DirectoryInfo directory = new DirectoryInfo(LoadSavePath);
        //��Ŀ¼�µ������ļ���Ϣ
        FileInfo[] fileInfos = directory.GetFiles();
        foreach (var item in fileInfos)
        {
            if (item.Extension == "") //ab��û�к�׺
            {
                localABInfo.Add(item.Name, new ABInfo(item.Name, item.Length, GetMD5(item.FullName)));
            }
        }
        try
        {
            DirectoryInfo directory_D = new DirectoryInfo(DefaultPath);
            //��Ŀ¼�µ������ļ���Ϣ
            FileInfo[] fileInfos_D = directory_D.GetFiles();
            foreach (var item in fileInfos_D)
            {
                if (item.Extension == "") //ab��û�к�׺
                {
                    //localABInfo.Add(item.Name, new ABInfo(item.Name, item.Length, GetMD5(item.FullName)));
                    ABPath[item.Name] = DefaultPath;
                }
            }
        }
        catch (System.Exception)
        {
            Debug.Log("û��Ĭ��AB��");
        }
    }


    private static string GetMD5(string filepath)
    {
        //�����ļ�·������ȡ�ļ�����Ϣ
        //���ļ�������ʽ��
        using (FileStream file = new FileStream(filepath, FileMode.Open))
        {
            //����һ��MD5���� ����MD5��
            MD5 mD5 = new MD5CryptoServiceProvider();
            var md5Info = mD5.ComputeHash(file);

            //�ر��ļ���
            file.Close();

            //��16�ֽ�ת����16����ƴ���ַ���������MD5�볤��
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
    /// ��IIS���������ضԱ��ļ�
    /// </summary>
    public void LoadIISCompareFile(string serverPath, string filenName, UnityAction callBack = null)
    {
        WebClient _webClient = new WebClient();
        //ʹ��Ĭ�ϵ�ƾ�ݡ�����ȡ��ʱ��ֻ��Ĭ��ƾ�ݾͿ���
        _webClient.Credentials = CredentialCache.DefaultCredentials;
        string uri = serverPath + filenName;
        //���ص����ӵ�ַ���ļ���������
        System.Uri _uri = new System.Uri(uri);
        //ע�����ؽ����¼�֪ͨ
        //_webClient.DownloadProgressChanged += _webClient_DownloadProgressChanged;
        //ע����������¼�֪ͨ
        _webClient.DownloadFileCompleted += (_, Y) =>
        {
            Debug.Log($"{filenName}||{Y.Error}");
            callBack?.Invoke();
        };
        //�첽����
        _webClient.DownloadFileAsync(_uri, LoadSavePath + filenName);
    }

    /// <summary>
    /// ʹ��WebRequest�ӷ�����������Դ
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
                Debug.Log($"��������[{fileName}],��[{attemptCount}]��");
                System.Action<int> progressCallback = (value => Debug.Log($"{fileName}���ؽ��ȣ�{value}%"));
                //var progress = new System.Progress<int>(value => Debug.Log($"���ؽ��ȣ�{value}%"));
                await DownloadFileAsync(serverPath, fileName, localPath, progressCallback);
                downloaded = true;
            }
            catch (System.Exception ex)
            {
                await Task.Delay(2000);  //�ȴ�2s�����³���
                Debug.Log($"����ʧ�ܣ�{ex.Message}");
                attemptCount++;
            }
        }
        if (downloaded)
        {
            Debug.Log($"{fileName}���سɹ�");
            callBack?.Invoke();
        }
        else
            Debug.Log($"{fileName}����ʧ��,�������粢����");
    }

    /// <summary>  Task������־      /// </summary>
    private CancellationTokenSource cts = new CancellationTokenSource();

    /// <summary>
    /// �첽�����ļ�������
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

            if (File.Exists(localpath + fileName)) //����Ѿ��б����ļ���
            {
                // ��ȡ�Ѿ����ص��ļ��Ĵ�С
                existingFileSize = new FileInfo(localpath + fileName).Length;

                // ���Range����ͷ��ָ����Ҫ���ص��ļ���Χ
                request.AddRange((int)existingFileSize);

                Debug.Log($"��������{fileName}�� {existingFileSize} bytes");
            }
            else
            {
                Debug.Log($"��ʼ������{fileName}");
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
                Debug.Log($"{fileName}������ɣ�");
            }
        }
        catch (WebException ex) when (ex.Status == WebExceptionStatus.ProtocolError)
        {
            //Debug.Log($"����ʧ�ܣ�Զ�̷�����������Ч http ״̬�� '{((HttpWebResponse)ex.Response).StatusCode}'.");
            if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.RequestedRangeNotSatisfiable)
            {
                Debug.Log($"{fileName}��������");
            }
            else
            {
                throw new System.Exception($"Զ�̷�����������Ч http ״̬�� '{((HttpWebResponse)ex.Response).StatusCode}'.");
            }
        }
        catch (WebException ex) when (ex.Status == WebExceptionStatus.ConnectFailure)
        {
            //Debug.Log($"����ʧ�ܣ��ͻ���δ�����ӵ�Զ�̷����� '{url}'.");
            throw new System.Exception($"�ͻ���δ�����ӵ�Զ�̷�����{url}");
        }

        catch (WebException ex)
        {
            //Debug.Log($"����ʧ�ܣ�{ex.Message}");
            throw new System.Exception($"{ex.Message}");
        }
    }

    private void OnApplicationQuit()
    {
        cts.Cancel();
        Debug.Log("����ֹͣ");
    }

    #endregion

}
