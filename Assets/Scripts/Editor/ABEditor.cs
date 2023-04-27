using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

public class ABEditor
{

    public static string platform = "StandaloneWindows";

    //abBrowser生成路径
    public static string abBrowser = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/") + 1) + $"AssetBundles/{platform}/";

    //本地ab包文件路径
    public static string abTempPath = Application.streamingAssetsPath + $"/AssetBundle/Temp/{platform}/";


    [MenuItem("AB包工具/Temp创建对比文件")]
    public static void CreateABCompareFile()
    {
        //文件夹信息类
        DirectoryInfo directory = Directory.CreateDirectory(abTempPath);
        //该目录下的所有文件信息
        FileInfo[] fileInfos = directory.GetFiles();

        //将ab的文件名、大小、MD5码存入资源对比文件
        string abCompareInfo = null;
        foreach (var item in fileInfos)
        {
            if (item.Extension == "") //ab包没有后缀
            {
                abCompareInfo += item.Name + "||" + item.Length + "||" + GetMD5(item.FullName);
                abCompareInfo += "\n";
            }
        }
        abCompareInfo = abCompareInfo.TrimEnd('\n');
        Debug.Log(abCompareInfo.ToString());
        //存储AB包对比文件
        File.WriteAllText(abTempPath + "ABCompareInfo.txt", abCompareInfo.ToString());
        //编辑器刷新
        AssetDatabase.Refresh();
        Debug.Log("AB包对比文件生成成功");
    }


    [MenuItem("AB包工具/移动资源到Temp")]
    public static void MoveToStreamingAsset()
    {
        if (!Directory.Exists(abTempPath))
        {
            Directory.CreateDirectory(abTempPath);
        }

        //文件夹信息类
        DirectoryInfo directory = Directory.CreateDirectory(abBrowser);
        //该目录下的所有文件信息
        FileInfo[] fileInfos = directory.GetFiles();

        foreach (var item in fileInfos)
        {
            if (item.Extension == "" ||
                item.Extension == ".txt") //ab包没有后缀
            {
                File.Copy(abBrowser + item.Name, abTempPath + item.Name);
            }
        }
        AssetDatabase.Refresh();
    }

    [MenuItem("AB包工具/上传AB文件和对比文件")]
    public static void UpLoadALLAB()
    {
        //文件夹信息类
        DirectoryInfo directory = Directory.CreateDirectory(abTempPath);
        //该目录下的所有文件信息
        FileInfo[] fileInfos = directory.GetFiles();

        foreach (var item in fileInfos)
        {
            if (item.Extension == ".txt" ||
                item.Extension == ".txt") //ab包没有后缀
            {
                //上传
                UpLoad(item.FullName, item.Name);
            }
        }
    }

    private static void UpLoad(string filePath, string fileName)
    {
        //FTPUpLoad(filePath, fileName);
        IISIpLoad(filePath, fileName);
    }

    /// <summary>
    /// FTP上传
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="fileName"></param>
    private async static void FTPUpLoad(string filePath, string fileName)
    {
        await Task.Run(() =>
        {
            try
            {
                //1.创建FTP链接 用于上传
                FtpWebRequest req = FtpWebRequest.Create(new System.Uri("ftp://127.0.0.1/AB/PC" + fileName)) as FtpWebRequest;
                //2. 设置一个通信凭证  这样才能上传
                NetworkCredential nc = new NetworkCredential("usetname", "password");
                req.Credentials = nc;
                //3.其他设置
                //设置代理为空
                req.Proxy = null;
                //请求完毕后是否关闭控制连接
                req.KeepAlive = false;
                //命令-上传
                req.Method = WebRequestMethods.Ftp.UploadFile;
                //指定传输类型 2禁止
                req.UseBinary = true;
                //4.上传文件
                //ftp流文件
                Stream uploadSteam = req.GetRequestStream();
                //读取文件信息 写入流对象
                using (FileStream file = File.OpenRead(filePath))
                {
                    //一点一点的上传内容
                    byte[] bytes = new byte[2048];
                    //返回值代表读取了多少个字节
                    int contentLength = file.Read(bytes, 0, bytes.Length);
                    //循环上传
                    while (contentLength != 0)
                    {
                        //写入到上传流中
                        uploadSteam.Write(bytes, 0, contentLength);
                        //写完再读
                        contentLength = file.Read(bytes, 0, bytes.Length);
                    }

                    //循环完毕后 上传结束
                    file.Close();
                    uploadSteam.Close();
                }
                Debug.Log($"{fileName}上传成功");
            }
            catch (System.Exception e)
            {
                Debug.Log($"{fileName}上传失败：[{ e }]");
            }
        });
    }


    private static void IISIpLoad(string filePath, string fileName)
    {
        Debug.Log(fileName);
        try
        {
            //定义_webClient对象
            WebClient _webClient = new WebClient();
            //使用Windows登录方式
            _webClient.Credentials = new NetworkCredential("IUSR", "");
            //上传的链接地址（文件服务器）
            System.Uri _uri = new System.Uri(@$"http://192.168.10.104:5000/AssetBundle/");
            //上传成功事件注册
            _webClient.UploadFileCompleted += (_, Y) => 
            {
                Debug.Log($"{fileName}||{Y.Result}");
            };
            //异步从D盘上传文件到服务器
            _webClient.UploadFileAsync(_uri, "PUT", filePath);
        }
        catch (System.Exception e)
        {
            Debug.Log($"{fileName}上传失败：[{ e }]");
        }

    }


    /// <summary>
    /// 获取文件MD5码
    /// </summary>
    /// <param name="filepath"></param>
    /// <returns></returns>
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
}
