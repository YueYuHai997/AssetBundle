using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UImanager : MonoBehaviour
{
    public Slider NetLoad;
    public Slider sliderAB;
    public Slider sliderRes;
    public Slider sliderScene;

    public ABMgr aBMgr;

    private void Update()
    {

        //sliderAB.value = Mathf.Lerp(sliderAB.value, aBMgr.ProcessValue, Time.deltaTime);


        NetLoad.maxValue = aBMgr.TotalLoad;

        NetLoad.value = aBMgr.CurrentLoad ;

        sliderAB.value = aBMgr.ProcessValue;

        sliderRes.value = aBMgr.ResVale;

        sliderScene.value = Manager_Scene.LoadProcess;
    }
}
