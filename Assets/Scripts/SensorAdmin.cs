using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class SensorAdmin : MonoBehaviour
{
    public delegate void SensorAction(List<Landmark> landmarks);
    public static SensorAction Send;

    private Thread _processDataThread;
    private UdpClient _udpClient;
    private WebcamData? _webcamData;
    private bool _sigueEjecutandoHilo = true;


    private void OnDisable()
    {
        #if UNITY_EDITOR
        Debug.Log("Termina Sensor Admin");
        #endif
        _sigueEjecutandoHilo = false;
        StopAllCoroutines();
    }

    private void Awake()
    {
        _udpClient = GameMaster.UdpClient;
    }

    private void Start()
    {
        #if UNITY_EDITOR
        Debug.Log("Comienza sensor");
        #endif
        _processDataThread = new Thread(new ThreadStart(ReadUDPData));
        _processDataThread.IsBackground = true;
        _processDataThread.Start();
    }

    private void Update()
    {
        if (!_webcamData.HasValue) return;

        Send?.Invoke(_webcamData?.Landmarks);
    }

    private void ReadUDPData()
    {
        while (_sigueEjecutandoHilo)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] dataByte = _udpClient.Receive(ref anyIP);
                var data = WebcamUtils.WebcamDataProcessing(Encoding.UTF8.GetString(dataByte), true);
                if (data.success)
                {
                    GameMaster.IsReceivingWebcamData = true;
                    _webcamData = data.data;
                }
                else
                    GameMaster.IsReceivingWebcamData = false;

            }
            catch (Exception err)
            {
                Debug.LogWarning(err);
            }
        }
    }
}
