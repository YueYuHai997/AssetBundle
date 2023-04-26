using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class TaskStop : MonoBehaviour
{
    private static CancellationTokenSource cts;

    public static async Task DoWork(CancellationToken token)
    {
        int count = 0;

        while (!token.IsCancellationRequested)
        {
            count++;
            Debug.Log($"执行任务 {count}");
            await Task.Delay(500);
        }

        Debug.Log("任务已停止");
    }
    public static async Task DoWork2(CancellationToken token)
    {
        int count = 0;
        while (!token.IsCancellationRequested)
        {
            count++;
            Debug.Log($"执行任务2 {count}");
            await Task.Delay(500);
        }
        Debug.Log("任务已停止");
    }



    private async void Awake()
    {
        cts = new CancellationTokenSource();

        var task = Task.Run(() => DoWork(cts.Token));
        var task2 = Task.Run(() => DoWork2(cts.Token));
        await Task.Delay(5000);

        Debug.Log("停止任务");
        cts.Cancel();
    }


}