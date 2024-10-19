using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConnectSensorsController : MonoBehaviour
{
    public delegate void ConnectControllerEvent();
    public static event ConnectControllerEvent OnTryingToConnectRight;
    public static event ConnectControllerEvent OnTryingToConnectLeft;
    public static event ConnectControllerEvent OnSuccesfulyConnectBoth;
    public static event ConnectControllerEvent OnFailToConnect;
    public static event ConnectControllerEvent OnVibrateLeft;
    public static event ConnectControllerEvent OnVibrateRight;

    public delegate void LookForPortEvent(Mano mano);
    public static event LookForPortEvent OnLookForPort;

    public delegate void MasInfoEvent(string info);
    public static event MasInfoEvent OnUpdateMasInfo;

    [SerializeField] private GameObject conectaSensorPanel;
    [SerializeField] private TMP_Text instruccionesText;
    [SerializeField] private GameObject esperaPanel;
    [SerializeField] private Button conectarButton;
    [SerializeField] private Button testLeftButton;
    [SerializeField] private Button testRightButton;
    [SerializeField] private Button regresarButton;
    [SerializeField] private Color deactivateButtonColor;

    private bool _isRunningThread = false;
    private bool _foundPort = false;
    private Thread _connectThread;
    private Color _activateButtonColor;
    private Color _activateRegresarButtonColor;
    private bool _waitingForLookForPort = false;
    private bool _reconnect = false;
    private Queue<string> _masInfoQueue = new Queue<string>();


    private void OnEnable()
    {
        MenuController.OnShowConectaSensorPanel += ConfigurePanel;
        HapticSensorController.OnTimeoutExceptionLeft += ReConnectLeftSensor;
        HapticSensorController.OnTimeoutExceptionRight += ReConnectRightSensor;
        HapticSensorController.OnPortFound += GetPortName;
        HapticSensorController.OnPortNotFound += EndWaitingForPort;
    }

    private void OnDisable()
    {
        MenuController.OnShowConectaSensorPanel -= ConfigurePanel;
        HapticSensorController.OnTimeoutExceptionLeft -= ReConnectLeftSensor;
        HapticSensorController.OnTimeoutExceptionRight -= ReConnectRightSensor;
        HapticSensorController.OnPortFound -= GetPortName;
        HapticSensorController.OnPortNotFound -= EndWaitingForPort;
    }

    private void Awake()
    {
        _activateButtonColor = conectarButton.GetComponent<Image>().color;
        _activateRegresarButtonColor = regresarButton.GetComponent<Image>().color;

        esperaPanel.SetActive(false);
        instruccionesText.gameObject.SetActive(true);

        testLeftButton.onClick.RemoveAllListeners();
        testLeftButton.onClick.AddListener(delegate { OnVibrateLeft?.Invoke(); });
        testRightButton.onClick.RemoveAllListeners();
        testRightButton.onClick.AddListener(delegate { OnVibrateRight?.Invoke(); });

        DeactivateButton(testLeftButton);
        DeactivateButton(testRightButton);
    }

    private void Update() 
    {
        if (_masInfoQueue.Count > 0)
            OnUpdateMasInfo?.Invoke(_masInfoQueue.Dequeue());
    }

    private void ConfigurePanel()
    {
        if (!GameMaster.IsRightDeviceConnected)
        {
            conectarButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Conectar\nDerecho";
            conectarButton.onClick.RemoveAllListeners();
            conectarButton.onClick.AddListener(PreConnectRight);
            instruccionesText.text = "Intentaremos conectar el sensor DERECHO.\n\nAsegúrate que el sensor está encendido y conectado al Bluetooth.\n\nDe preferencia, APAGA el sensor izquierdo.";
            OnTryingToConnectRight?.Invoke();
        }
        else if (!GameMaster.IsLeftDeviceConnected)
        {
            instruccionesText.text = "Intentaremos conectar el sensor IZQUIERDO.\n\nAsegúrate que el sensor está encendido y conectado al Bluetooth.";
            conectarButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Conectar\nIzquierdo";
            ActivateButton(testRightButton);
            conectarButton.onClick.RemoveAllListeners();
            conectarButton.onClick.AddListener(PreConnectLeft);
            OnTryingToConnectLeft?.Invoke();
        }
        else if (GameMaster.IsRightDeviceConnected && GameMaster.IsLeftDeviceConnected)
        {
            instruccionesText.text = "Los dos sensores están listos.\n\nPuedes probarlos si quieres.";
            conectarButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "(^_^)";
            ActivateButton(testRightButton);
            ActivateButton(testLeftButton);
            conectarButton.interactable = false;
            conectarButton.onClick.RemoveAllListeners();
            OnSuccesfulyConnectBoth?.Invoke();
        }
    }

    private void ReConnectLeftSensor()
    {
        if (GameMaster.TimeoutLeftCounter < 3) return;

        instruccionesText.text = "Ha ocurrido un error con el sensor IZQUIERDO \n\n¿Desea volver a conectarlo?";
        OnUpdateMasInfo?.Invoke("Fallo en sensor izquierdo");
        GameMaster.IsLeftDeviceConnected = false;
        GameMaster.ResetSerialPortLeft();
        GameMaster.TimeoutLeftCounter = 0;
        DeactivateButton(testLeftButton);
        conectarButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Conectar Izquierdo";
        conectarButton.onClick.RemoveAllListeners();
        conectarButton.onClick.AddListener(PreConnectLeft);
    }

    private void ReConnectRightSensor()
    {
        if (GameMaster.TimeoutRightCounter < 3) return;

        instruccionesText.text = "Ha ocurrido un error con el sensor DERECHO \n\n¿Desea volver a conectarlo?";
        OnUpdateMasInfo?.Invoke("Fallo en sensor derecho");
        GameMaster.IsRightDeviceConnected = false;
        GameMaster.ResetSerialPortRight();
        DeactivateButton(testRightButton);
        conectarButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Conectar Derecho";
        conectarButton.onClick.RemoveAllListeners();
        conectarButton.onClick.AddListener(PreConnectRight);
    }

    private void PreConnectLeft()
    {
        if (GameMaster.IsLeftDeviceConnected) return;
        if (_isRunningThread) return;

        StartCoroutine(ConnectLeft());
    }

    private void PreConnectRight()
    {
        if (GameMaster.IsRightDeviceConnected) return;
        if (_isRunningThread) return;

        StartCoroutine(ConnectRight());
    }

    private IEnumerator ConnectLeft()
    {
        instruccionesText.gameObject.SetActive(false);
        esperaPanel.SetActive(true);
        DeactivateButton(conectarButton);
        DeactivateButton(regresarButton);

        if (string.IsNullOrEmpty(GameMaster.SerialPortLeftName))
        {
            OnLookForPort?.Invoke(Mano.Izquierda);
            _waitingForLookForPort = true;
            _foundPort = false;
            _reconnect = false;
        }
        else
        {
            _connectThread = new Thread(new ThreadStart(() => ReConnectToPort(GameMaster.SerialPortLeftName)));
            _connectThread.Start();
            _reconnect = true;
        }
        
        yield return new WaitForSeconds(0.5f);

        while (_waitingForLookForPort)
            yield return null;
        while (_isRunningThread)
            yield return null;

        if (_foundPort)
        {
            if (_reconnect)
            {
                GameMaster.IsLeftDeviceConnected = true;
                GameMaster.OpenSerialPortLeft(GameMaster.SerialPortLeftName);
            }
            #if UNITY_EDITOR
            Debug.Log("Se ha conectado dispositivo izquierdo");
            #endif
            OnUpdateMasInfo?.Invoke("Se ha conectado  dispositivo izquierdo");

            ActivateButton(testLeftButton);

            if (GameMaster.IsRightDeviceConnected)
            {
                instruccionesText.text = "¡¡Se ha conectado correctamente!!\n\nLos dos sensores están listos.\n\nPuedes probarlos si quieres.";
                conectarButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "(^_^)";
                conectarButton.interactable = false;
                conectarButton.onClick.RemoveAllListeners();
                OnSuccesfulyConnectBoth?.Invoke();
            }
            else
            {
                instruccionesText.text = "¡¡Se ha conectado correctamente!!\n\nIntentaremos conectar el sensor DERECHO.\n\nAsegúrate que el sensor está encendido y conectado al Bluetooth.";
                conectarButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Conectar\nDerecho";
                conectarButton.onClick.RemoveAllListeners();
                conectarButton.onClick.AddListener(PreConnectRight);
                OnTryingToConnectRight?.Invoke();
            }
        }
        else
        {
            #if UNITY_EDITOR
            Debug.Log("No se pudo conectar dispositivo");
            #endif
            OnUpdateMasInfo?.Invoke("No se pudo conectar dispositivo izquierdo");
            instruccionesText.text = "No pudimos conectar el sensor\n\nAsegúrate que el sensor está encendido y conectado al Bluetooth, e inténtalo de nuevo.";
            OnFailToConnect?.Invoke();
        }

        ActivateButton(conectarButton);
        ActivateButton(regresarButton);
        esperaPanel.SetActive(false);
        instruccionesText.gameObject.SetActive(true);
    }

    private IEnumerator ConnectRight()
    {
        instruccionesText.gameObject.SetActive(false);
        esperaPanel.SetActive(true);
        DeactivateButton(conectarButton);
        DeactivateButton(regresarButton);

        if (string.IsNullOrEmpty(GameMaster.SerialPortRightName))
        {
            OnLookForPort?.Invoke(Mano.Derecha);
            _waitingForLookForPort = true;
            _foundPort = false;
            _reconnect = false;
        }
        else
        {
            _connectThread = new Thread(new ThreadStart(() => ReConnectToPort(GameMaster.SerialPortRightName)));
            _connectThread.Start();
            _reconnect = true;
        }

        // Algunas veces _isRunningThread no cambia a true lo suficientemente rapido por lo que se espera 0.5 seg 
        yield return new WaitForSeconds(0.5f);
        while (_waitingForLookForPort)
            yield return null;
        while (_isRunningThread)
            yield return null;

        if (_foundPort)
        {
            if (_reconnect)
            {
                GameMaster.IsRightDeviceConnected = true;
                GameMaster.OpenSerialPortRight(GameMaster.SerialPortRightName);
            }
            #if UNITY_EDITOR
            Debug.Log("Se ha conectado dispositivo derecho");
            #endif
            OnUpdateMasInfo?.Invoke("Se ha  conectado dispositivo derecho");

            ActivateButton(testRightButton);
            if (!GameMaster.IsLeftDeviceConnected)
            {
                instruccionesText.text = "¡¡Se ha conectado correctamente!!\n\nIntentaremos conectar el sensor IZQUIERDO.\n\nAsegúrate que el sensor está encendido y conectado al Bluetooth.";
                conectarButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Conectar\nIzquierdo";
                conectarButton.onClick.RemoveAllListeners();
                conectarButton.onClick.AddListener(PreConnectLeft);
                OnTryingToConnectLeft?.Invoke();
            }
            else
            {
                instruccionesText.text = "¡¡Se ha conectado correctamente!!\n\nLos dos sensores están listos.\n\nPuedes probarlos si quieres.";
                conectarButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "(^_^)";
                conectarButton.interactable = false;
                conectarButton.onClick.RemoveAllListeners();
                OnSuccesfulyConnectBoth?.Invoke();
            }
        }
        else
        {
            #if UNITY_EDITOR
            Debug.Log("No se pudo conectar dispositivo");
            #endif
            OnUpdateMasInfo?.Invoke("No se pudo conectar dispositivo derecho");
            instruccionesText.text = "No pudimos conectar el sensor\n\nAsegúrate que el sensor está encendido y conectado al Bluetooth, e inténtalo de nuevo.";
            OnFailToConnect?.Invoke();
        }

        ActivateButton(conectarButton);
        ActivateButton(regresarButton);
        esperaPanel.SetActive(false);
        instruccionesText.gameObject.SetActive(true);
    }

    private void GetPortName()
    {
        _foundPort = true;
        _waitingForLookForPort = false;
    }

    private void EndWaitingForPort()
    {
        _foundPort = false;
        _waitingForLookForPort = false;
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
            sp.DtrEnable = true;
            if (!sp.IsOpen) sp.Open();
            Debug.Log($"Trying {sp.PortName}");
            _masInfoQueue.Enqueue($"Puerto {sp.PortName} abierto. Intentando conectar");

            try
            {
                sp.DiscardInBuffer();
                sp.DiscardOutBuffer();
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
                _masInfoQueue.Enqueue($"Time out con puerto  {sp.PortName}");

            }
            catch (InvalidOperationException)
            {
                Debug.Log($"Port {sp.PortName} is not open");
                _masInfoQueue.Enqueue($"Puerto {sp.PortName} no esta abierto");
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                _masInfoQueue.Enqueue($"Error al conectar a puerto {sp.PortName}: {e.Message}");
            }
        }
        catch (Exception e)
        {
            Debug.Log("El puerto esta ocupado");
            _masInfoQueue.Enqueue($"Error al intentar abrir puerto {sp.PortName}:  {e.Message}");

        }
        Thread.Sleep(2000);
        _isRunningThread = false;
    }

    private void DeactivateButton(Button b)
    {
        b.gameObject.SetActive(false);
        b.GetComponent<Image>().color = deactivateButtonColor;
        b.GetComponent<ButtonTween>().enabled = false;
        b.interactable = false;
    }

    private void ActivateButton(Button b)
    {
        b.GetComponent<Image>().color = (b == regresarButton) ? _activateRegresarButtonColor : _activateButtonColor;
        b.GetComponent<ButtonTween>().enabled = true;
        b.interactable = true;
        b.gameObject.SetActive(true);
    }
}
