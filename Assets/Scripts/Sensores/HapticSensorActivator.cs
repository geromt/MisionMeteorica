using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HapticSensorActivator : MonoBehaviour
{
    public delegate void HapticActivatorEvent();
    public static event HapticActivatorEvent OnVibrateLeft;
    public static event HapticActivatorEvent OnVibrateRight;

    public delegate void MasInfoEvent(string info);
    public static event MasInfoEvent OnUpdateMasInfo;

    [SerializeField] private GameObject fallaSensorPanel;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private GameObject siButton;
    [SerializeField] private GameObject noButton;
    [SerializeField] private GameObject continuarButton;
    [SerializeField] private GameObject esperaImage;

    private Thread _connectThread;
    private bool _isRunningThread;
    private bool _foundPort;
    private Queue<string> _masInfoQueue = new Queue<string>();


    private void OnEnable()
    {
        AsteroidController.SendCollisionHand += CheckHand;
        HapticSensorController.OnTimeoutExceptionLeft += ReConnectLeftSensor;
        HapticSensorController.OnTimeoutExceptionRight += ReConnectRightSensor;
    }

    private void OnDisable()
    {
        AsteroidController.SendCollisionHand -= CheckHand;
        HapticSensorController.OnTimeoutExceptionLeft -= ReConnectLeftSensor;
        HapticSensorController.OnTimeoutExceptionRight -= ReConnectRightSensor;
    }

    private void Awake()
    {
        fallaSensorPanel.SetActive(false);
        noButton.GetComponent<Button>().onClick.AddListener(Continuar);
        continuarButton.GetComponent<Button>().onClick.AddListener(Continuar);
    }

    private void Update()
    {
        if (_masInfoQueue.Count > 0)
            OnUpdateMasInfo?.Invoke(_masInfoQueue.Dequeue());
    }

    private void CheckHand(Mano mano)
    {
        if (mano == Mano.Derecha)
        {
            if (GameMaster.IsRightDeviceConnected)
                OnVibrateRight?.Invoke();
        }
        else if (mano == Mano.Izquierda)
        {
            if (GameMaster.IsLeftDeviceConnected)
                OnVibrateLeft?.Invoke();
        }
    }

    private void ReConnectLeftSensor()
    {
        if (GameMaster.TimeoutLeftCounter < 3) return;

        messageText.text = "Ha ocurrido un error con el sensor IZQUIERDO \n\n¿Desea volver a conectarlo?";
        GameMaster.IsLeftDeviceConnected = false;
        GameMaster.ResetSerialPortLeft();
        siButton.GetComponent<Button>().onClick.RemoveAllListeners();
        siButton.GetComponent<Button>().onClick.AddListener(delegate { PreConnectSensor(Mano.Izquierda); });
        PrepareUIToReConnect();
    }

    private void ReConnectRightSensor()
    {
        if (GameMaster.TimeoutRightCounter < 3) return;
        
        messageText.text = "Ha ocurrido un error con el sensor DERECHO \n\n¿Desea volver a conectarlo?";
        GameMaster.IsRightDeviceConnected = false;
        GameMaster.ResetSerialPortRight();
        siButton.GetComponent<Button>().onClick.RemoveAllListeners();
        siButton.GetComponent<Button>().onClick.AddListener(delegate { PreConnectSensor(Mano.Derecha); });
        PrepareUIToReConnect();
    }

    private void PrepareUIToReConnect()
    {
        siButton.SetActive(true);
        noButton.SetActive(true);
        esperaImage.SetActive(false);
        continuarButton.SetActive(false);
        fallaSensorPanel.SetActive(true);
        Time.timeScale = 0;
    }

    public void PreConnectSensor(Mano mano)
    {
        StartCoroutine(ConnectSensor(mano));
    }

    private IEnumerator ConnectSensor(Mano mano)
    {
        messageText.text = "Espera...";
        siButton.SetActive(false);
        noButton.SetActive(false);
        esperaImage.SetActive(true);

        string portName = "";
        if (mano == Mano.Derecha)
            portName = GameMaster.SerialPortRightName;
        else if (mano == Mano.Izquierda)
            portName = GameMaster.SerialPortLeftName;

        _connectThread = new Thread(new ThreadStart(() => ReConnectToPort(portName)));
        _connectThread.Start();

        yield return new WaitForSecondsRealtime(0.5f);
        while (_isRunningThread)
            yield return null;

        Debug.Log("Se encontro puerto: " + _foundPort);
        if (_foundPort)
        {
            GameMaster.IsRightDeviceConnected = true;
            if (mano == Mano.Derecha)
                GameMaster.OpenSerialPortRight(portName);
            else
                GameMaster.OpenSerialPortLeft(portName);
            Debug.Log("Se ha conectado dispositivo");
            messageText.text = "¡¡Se ha conectado correctamente!!\n\nPuedes volver a jugar.";
            esperaImage.SetActive(false);
            continuarButton.SetActive(true);
        }
        else
        {
            Debug.Log("No se pudo conectar dispositivo");
            messageText.text = "No pudimos conectar el sensor\n\n¿Desea intentarlo otra vez?";
            siButton.SetActive(true);
            noButton.SetActive(true);
            esperaImage.SetActive(false);
        }
    }

    private void ReConnectToPort(string portName)
    {
        _isRunningThread = true;
        _foundPort = false;

        SerialPort sp = new SerialPort(portName, 115200);

        try
        {
            sp.ReadTimeout = 1000;
            sp.WriteTimeout = 1000;
            if (!sp.IsOpen) sp.Open();
            Debug.Log($"Trying {sp.PortName}");
            _masInfoQueue.Enqueue($"Puerto {sp.PortName} abierto. Intentando conectar");

            try
            {
                string test = sp.ReadLine();
                if (test.Contains("Vibracion"))
                {
                    #if UNITY_EDITOR
                    Debug.Log($"Se reconecto con puerto {portName}");
                    #endif
                    _masInfoQueue.Enqueue($"Se reconecto con puerto {portName}");
                    _foundPort = true;
                    sp.Close();
                    _isRunningThread = false;
                    return;
                }
                sp.Close();
            }
            catch (TimeoutException)
            {
                Debug.Log($"Time out from port {sp.PortName}");
                _masInfoQueue.Enqueue($"Time out  from port {sp.PortName}");
            }
            catch (InvalidOperationException)
            {
                Debug.Log($"Port {sp.PortName} is not open");
                _masInfoQueue.Enqueue($"Port {sp.PortName} is not open");
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                _masInfoQueue.Enqueue($"Error intentando comunicar con puerto {sp.PortName}: {e.Message}");
            }
        }
        catch (Exception e)
        {
            Debug.Log("El puerto esta ocupado");
            _masInfoQueue.Enqueue($"Error al intentar abrir puerto {portName}: {e.Message}");
        }
        Thread.Sleep(2000);
        _isRunningThread = false;
    }

    public void Continuar()
    {
        Time.timeScale = 1;
        fallaSensorPanel.SetActive(false);
    }
}
