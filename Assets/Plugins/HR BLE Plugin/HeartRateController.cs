using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class HeartRateController : MonoBehaviour
{

    public HrData HrData { get; } = new();


    #region Native calls 

    [DllImport("HeartRatePlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr GetData();

    [DllImport("HeartRatePlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern void Disconnect();


    #endregion

    bool communicationFailure;
    float nextUpdate;
    public float pollingTime = 0.25f;
    // Update is called once per frame
    void Update()
    {
        float currentTime = Time.realtimeSinceStartup;
        //if (Time.frameCount % interval == 0)
        if (currentTime >= nextUpdate)
        {
            nextUpdate = currentTime + pollingTime;
            try
            {
                HrData.SetData(GetData());
            }
            catch (Exception e)
            {
                communicationFailure = true;
                Debug.Log("Failed to communicate with Heart Rate Interface");
            }
        }
    }

    void OnDestroy()
    {
        Disconnect();
    }
}
