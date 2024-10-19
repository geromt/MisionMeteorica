using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class HapticSensorController : MonoBehaviour
{
    public delegate void HapticSensorEvent();
    public static event HapticSensorEvent OnTimeoutExceptionLeft;
    public static event HapticSensorEvent OnTimeoutExceptionRight;
    public static event HapticSensorEvent OnPortFound;
    public static event HapticSensorEvent OnPortNotFound;

    [Header("Panel de más información")]
    [SerializeField] private GameObject panelMasInformacion;
    [SerializeField] private ScrollRect masInfoScroll;
    [SerializeField] private GameObject masInfoContent;
    [SerializeField] private GameObject masInfoPrefab;


    private Task _lookForPortTask;
    private Task _vibrateTask;
    private bool _wasThereTimeout = false;
    private bool _foundPort = false;
    private Queue<string> _masInfoMessages = new Queue<string>();
    
    private const int Intensity = 3;


    private void OnEnable() 
    {
        ConnectSensorsController.OnVibrateLeft += PreVibrateLeft;
        ConnectSensorsController.OnVibrateRight += PreVibrateRight;
        ConnectSensorsController.OnLookForPort += PreLookForPort;
        ConnectSensorsController.OnUpdateMasInfo += UpdateMasInfo;
        HapticSensorActivator.OnVibrateLeft += PreVibrateLeft;
        HapticSensorActivator.OnVibrateRight += PreVibrateRight;
        HapticSensorActivator.OnUpdateMasInfo += UpdateMasInfo;
    }

    private void OnDisable()
    {
        ConnectSensorsController.OnVibrateLeft -= PreVibrateLeft;
        ConnectSensorsController.OnVibrateRight -= PreVibrateRight;
        ConnectSensorsController.OnLookForPort -= PreLookForPort;
        ConnectSensorsController.OnUpdateMasInfo -= UpdateMasInfo;
        HapticSensorActivator.OnVibrateLeft -= PreVibrateLeft;
        HapticSensorActivator.OnVibrateRight -= PreVibrateRight;
        HapticSensorActivator.OnUpdateMasInfo -= UpdateMasInfo;
    }

    private void Awake() 
    {
        panelMasInformacion.SetActive(false);
    }

    private void Update() 
    {
        if (Input.GetKeyDown(KeyCode.F3))
            panelMasInformacion.SetActive(!panelMasInformacion.activeSelf);

        if (_masInfoMessages.Count > 0)
            UpdateMasInfo(_masInfoMessages.Dequeue());
    }

    /// <summary>
    /// Metodo para que vibre el sensor izquierdo
    /// </summary>
    private void PreVibrateLeft()
    {
        if (_vibrateTask != null && !_vibrateTask.IsCompleted) return;
        StartCoroutine(VibrateSensorCoroutine(GameMaster.SerialPortLeft));
    }

    /// <summary>
    /// Metodo para que vibre el  sensor derecho
    /// </summary>
    private  void PreVibrateRight()
    {
        if (_vibrateTask != null && !_vibrateTask.IsCompleted) return;
        StartCoroutine(VibrateSensorCoroutine(GameMaster.SerialPortRight));
    }

    /// <summary>
    /// Corrutina para que vibre el sensor que se encuertra en el puerto sp
    /// </summary>
    /// <param name="sp">Puerto serial conectado al sensor</param>
    /// <returns></returns>
    private IEnumerator VibrateSensorCoroutine(SerialPort sp)
    {
        _vibrateTask = new Task(() => VibrateSensor(sp, Intensity));
        _vibrateTask.Start();
        yield return new WaitUntil(() => _vibrateTask.IsCompleted);
        
        if (_wasThereTimeout)
        {
            if (sp.Equals(GameMaster.SerialPortLeft))
            {
                GameMaster.TimeoutLeftCounter++;
                OnTimeoutExceptionLeft?.Invoke();
            }
            else if (sp.Equals(GameMaster.SerialPortRight))
            {
                GameMaster.TimeoutRightCounter++;
                OnTimeoutExceptionRight?.Invoke();
            }
            yield break;
        }
        else
        {
            if (sp.Equals(GameMaster.SerialPortLeft) && GameMaster.TimeoutLeftCounter > 0)
                GameMaster.TimeoutLeftCounter = 0;
            else if (sp.Equals(GameMaster.SerialPortRight) && GameMaster.TimeoutRightCounter > 0)
                GameMaster.TimeoutRightCounter = 0;
        }
    }

    /// <summary>
    /// Metodo que se inicializa desde un Task para hacer que el sensor vibre
    /// </summary>
    /// <param name="sp">Puerto por el que se mandara la senal</param>
    /// <param name="intensity">Entero positivo para indicar la intensidad</param>
    private void VibrateSensor(SerialPort sp, int intensity)
    {
        try
        {
            sp.DiscardOutBuffer();
            sp.DiscardInBuffer();
            // Se tiene que mandar a fuerzas de esta manera para controlar la intensidad y que la vibracion sea casi inmediata
            // No sirve con Write ni mandando una cadena con varios 1 (1\n1\n1\n) con WriteLine o Write
            // Si se manda asi, las vibraciones son espaciadas y despues de 1 o 2 segundos
            for (int i = 0; i < intensity; i++)
                sp.WriteLine("1");
        }
        catch (TimeoutException)
        {
            Debug.Log($"Time out writing to port {sp.PortName}");
            _masInfoMessages.Enqueue($"Time out writing to port {sp.PortName}");

            _wasThereTimeout = true;

            return;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            _masInfoMessages.Enqueue($"Excepcion  al escribir en el puerto {sp.PortName}: {e.Message}");

            return;
        }

        _wasThereTimeout = false;

        //Limitamos que solo se pueda mandar a llamar esta funcion cada segundo
        Thread.Sleep(1000);
    }

    /// <summary>
    /// Metodo para buscar el puerto del  sensor
    /// </summary>
    /// <param name="mano"></param>
    private void PreLookForPort(Mano mano)
    {
        if (_lookForPortTask != null && !_lookForPortTask.IsCompleted) return;
        StartCoroutine(PreLookForPortCoroutine(mano));
    }

    private IEnumerator PreLookForPortCoroutine(Mano mano)
    {
        string portName = "";
        string excludedPort = "";
        if (mano == Mano.Derecha)
            excludedPort = GameMaster.IsLeftDeviceConnected ? GameMaster.SerialPortLeft.PortName : "";
        else
            excludedPort = GameMaster.IsRightDeviceConnected ? GameMaster.SerialPortRight.PortName : "";
        
        _lookForPortTask = new Task(() => LookForPort(ref portName, excludedPort));
        _lookForPortTask.Start();
        yield return new WaitUntil(() => _lookForPortTask.IsCompleted);

        if (_foundPort) 
        {
            if (mano == Mano.Izquierda)
            {
                GameMaster.IsLeftDeviceConnected = true;
                GameMaster.OpenSerialPortLeft(portName);
            }
            else if (mano == Mano.Derecha)
            {
                GameMaster.IsRightDeviceConnected = true;
                GameMaster.OpenSerialPortRight(portName);
            }
            OnPortFound?.Invoke();
        }
        else
            OnPortNotFound?.Invoke();
    }

    /// <summary>
    /// Busca el puerto que esta mandando "Vibracion". Si lo encuentra _foundPort sera true y guardara el nombre del puerto en portName
    /// </summary>
    /// <param name="portName">Variable con cadena vacia. Si se encuentra un puerto que este enviando "Vibracion", se guardara aqui</param>
    /// <param name="excludedPort">Nombre de puerto que se excluye en la busqueda</param>
    private void LookForPort(ref string portName, string excludedPort = null)
    {
        _foundPort = false;

        SerialPort sp;

        foreach (string puertoSerial in SerialPort.GetPortNames())
        {
            //Se intentara conectar con cada uno de los puertos disponibles 
            #if UNITY_EDITOR
            Debug.Log($"Intentando conectar puerto {puertoSerial}");
            #endif
            _masInfoMessages.Enqueue($"Intentando  conectar puerto {puertoSerial}");

            if (puertoSerial.Length > 4)
                sp = new SerialPort("\\\\.\\" + puertoSerial, 115200);
            else
                sp = new SerialPort(puertoSerial, 115200);

            if (!string.IsNullOrEmpty(excludedPort) && excludedPort.Equals(sp.PortName))
                continue;

            try
            {
                sp.ReadTimeout = 1000;
                sp.WriteTimeout = 1000;
                sp.DtrEnable = true;
                if (!sp.IsOpen) sp.Open();
                Debug.Log($"Trying {sp.PortName}");
                _masInfoMessages.Enqueue($"Puerto {sp.PortName} abierto. Intentando mandar mensaje.");

                try
                {
                    // Se descartan los buffers de entrada y salida para intentar que salga el error 
                    // "El Periodo de Tiempo de Espera del Semáforo Ha Expirado" cuando se satura el puerto de tantos mensajes 
                    sp.DiscardInBuffer();
                    sp.DiscardOutBuffer();
                    sp.WriteLine("1");
                    Thread.Sleep(500);
                    string test = sp.ReadLine();
                    if (test.Contains("Vibracion"))
                    {
                        #if UNITY_EDITOR
                        Debug.Log("Se encontro puerto");
                        #endif
                        _masInfoMessages.Enqueue($"Se encontro puerto de sensor: {sp.PortName}");

                        _foundPort = true;
                        portName = sp.PortName;
                        sp.Close();
                        return;
                    }
                    sp.Close();
                }
                catch (TimeoutException)
                {
                    Debug.Log($"Time out from port {sp.PortName}");
                    _masInfoMessages.Enqueue($"Time out from port {sp.PortName}");
                }
                catch (InvalidOperationException)
                {
                    Debug.Log($"Port {sp.PortName} is not open");
                    _masInfoMessages.Enqueue($"Port {sp.PortName} is not open");
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                    _masInfoMessages.Enqueue(e.Message);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"El puerto esta ocupado: {e.Message}");
                _masInfoMessages.Enqueue($"Error al intentar abrir puerto {sp.PortName}: {e.Message}");
            }
        }
        Thread.Sleep(2000);
    }

    /// <summary>
    /// Agrega un nuevo objeto a masInfoContent y mueve el scroll hacia abajo
    /// </summary>
    /// <param name="newInfo"></param>
    private void UpdateMasInfo(string newInfo)
    {
        GameObject masInfoText = Instantiate(masInfoPrefab, masInfoContent.transform);
        masInfoText.GetComponent<Text>().text = newInfo;

        Canvas.ForceUpdateCanvases();

        masInfoContent.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
        masInfoContent.GetComponent<ContentSizeFitter>().SetLayoutVertical();

        masInfoScroll.content.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
        masInfoScroll.content.GetComponent<ContentSizeFitter>().SetLayoutVertical();
        masInfoScroll.verticalNormalizedPosition = 0;
    }
}
