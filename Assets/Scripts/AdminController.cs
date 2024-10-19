using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdminController : MonoBehaviour
{
    public delegate void EvalRutineEvent(Desempeno desempeno);
    public static event EvalRutineEvent OnCalcNewRutineVals;

    public delegate void AdminEvent();
    public static event AdminEvent OnCalibrationStart;
    public static event AdminEvent OnTutorialStart;
    public static event AdminEvent OnRutineStart;
    public static event AdminEvent OnRutineEnd;
    public static event AdminEvent OnDataUpload;

    public delegate void GraphicsEvent(bool didCatchBall);
    public static event GraphicsEvent OnCatchEnd;

    public delegate void FeedbackEvent(int aciertos);
    public static event FeedbackEvent OnAciertosDePartidaEnviados;

    [Header("Status")]
    [SerializeField] private Text aciertosText;
    [SerializeField] private Text fallosText;
    [SerializeField] private Text rondasText;
    [SerializeField] private GameObject continuarPanel;

    [Header("ADD Info")]
    [SerializeField] private GameObject ADDPanel;
    [SerializeField] private Text dificultadText;
    [SerializeField] private Text intervaloTex;
    [SerializeField] private Text speedText;
    [SerializeField] private Text rangeText;

    [Header("Webcam")]
    [SerializeField] private GameObject esperaWebcamPanel;

    private int _aciertos = 0;
    private int _fallos = 0;
    private int _lanzamientos = 0;
    private int _ronda = 0;

    private int _lanzamientosIzq = 0;
    private int _lanzamientosDer = 0;
    private int _aciertosIzq = 0;
    private int _aciertosIzqCruzados = 0; // Aciertos del lado izquierdo atrapados con la mano derecha
    private int _fallosIzq = 0;
    private int _aciertosDer = 0;
    private int _aciertosDerCruzados = 0; // Aciertos del lado derecho atrapados con la mano izquierda
    private int _fallosDer = 0;
    private Mano _lanzamientoLado = Mano.Izquierda;

    private float _initRondaTime;

    private bool _isReceivingWebcamData = false;

    private void OnEnable()
    {
        Calibration.OnCalibrationEnd += EndCalibration;
        AsteroidController.OnCollideHand += UpdateAciertos;
        AsteroidController.OnAvoidHand += UpdateFallos;
        AsteroidController.SendCollisionHand += CheckHand;
        GeneratorController.OnParametersSend += ShowDifficultyParameters;
        GeneratorController.OnAsteroidSpawn += CheckLaunchSide;
        TransitionController.EndsInitTranstion += CheckCalibration;
        Button3DController.OnContinueGameClick += CalcNewVals;
        Button3DController.OnGameEndClick += Continue;
    }

    private void OnDisable()
    {
        Calibration.OnCalibrationEnd -= EndCalibration;
        AsteroidController.OnCollideHand -= UpdateAciertos;
        AsteroidController.OnAvoidHand -= UpdateFallos;
        AsteroidController.SendCollisionHand -= CheckHand;
        GeneratorController.OnParametersSend -= ShowDifficultyParameters;
        GeneratorController.OnAsteroidSpawn -= CheckLaunchSide;
        TransitionController.EndsInitTranstion -= CheckCalibration;
        Button3DController.OnContinueGameClick -= CalcNewVals;
        Button3DController.OnGameEndClick -= Continue;
    }

    private void Awake()
    {
        ResetVals();
        ADDPanel.SetActive(false);
        continuarPanel.SetActive(false);
        esperaWebcamPanel.SetActive(false);
    }

    private void Start()
    {
        GetComponent<TransitionController>().PlayInitTransition();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
            ADDPanel.SetActive(!ADDPanel.activeSelf);

        if (!_isReceivingWebcamData)
        {
            if (GameMaster.IsReceivingWebcamData)
            {
                esperaWebcamPanel.SetActive(false);
                CheckCalibration();
            }
        }
    }

    private void CheckCalibration()
    {
        if (_isReceivingWebcamData) return;

        if (GameMaster.IsReceivingWebcamData)
        {
            _isReceivingWebcamData = true;

            if (GameMaster.ModoTutorial)
                OnTutorialStart?.Invoke();
            else
                OnCalibrationStart?.Invoke();
        }
        else
            esperaWebcamPanel.SetActive(true);
    }

    private void EndCalibration()
    {
        #if UNITY_EDITOR
        Debug.Log("Termina Calibracion");
        #endif
        GameMaster.IsCalibrated = true;
        OnRutineStart?.Invoke();
        _initRondaTime = Time.time;
    }

    private void UpdateAciertos()
    {
        _aciertos++;
        aciertosText.text = string.Format("Aciertos: {0}", _aciertos.ToString());
        UpdateLanzamientos();
        OnCatchEnd?.Invoke(true);
    }

    private void UpdateFallos()
    {
        _fallos++;
        fallosText.text = string.Format("Fallos: {0}", _fallos.ToString());

        if (_lanzamientoLado == Mano.Derecha)
            _fallosDer++;
        else if (_lanzamientoLado == Mano.Izquierda)
            _fallosIzq++;

        UpdateLanzamientos();
        OnCatchEnd?.Invoke(false);
    }

    private void CheckLaunchSide(Mano mano)
    {
        if (mano == Mano.Izquierda)
            _lanzamientosIzq++;
        else if (mano == Mano.Derecha)
            _lanzamientosDer++;

        _lanzamientoLado = mano;
    }

    private void CheckHand(Mano mano)
    {
        if (_lanzamientoLado == Mano.Derecha)
        {
            if (mano == Mano.Derecha)
                _aciertosDer++;
            else if (mano == Mano.Izquierda)
                _aciertosDerCruzados++;
        }

        if (_lanzamientoLado == Mano.Izquierda)
        {
            if (mano == Mano.Izquierda)
                _aciertosIzq++;
            else if (mano == Mano.Derecha)
                _aciertosIzqCruzados++;
        }
    }

    private void UpdateLanzamientos()
    {
        _lanzamientos++;

        if (GameMaster.ModoTutorial) return;

        if (_lanzamientos == 10)
        {
            continuarPanel.SetActive(true);
            #if UNITY_EDITOR
            Debug.Log("Termina Partida");
            Debug.Log(string.Format("Lanzamientos derecha: {0}, aciertos: {1}, aciertos cruzados: {2}, fallos: {3}", _lanzamientosDer, _aciertosDer, _aciertosDerCruzados, _fallosDer));
            Debug.Log(string.Format("Lanzamientos izquierda: {0}, aciertos: {1}, aciertos cruzados: {2}, fallos: {3}", _lanzamientosIzq, _aciertosIzq, _aciertosIzqCruzados, _fallosIzq));
            #endif

            OnRutineEnd?.Invoke();
            GameMaster.SaveRondaData(_ronda, Time.time - _initRondaTime, _aciertos, _fallos, _lanzamientosDer, _lanzamientosIzq, _aciertosDer, _aciertosIzq, _aciertosDerCruzados, _aciertosIzqCruzados, _fallosDer, _fallosIzq);
            OnDataUpload?.Invoke();
            OnAciertosDePartidaEnviados?.Invoke(_aciertos);
        }
    }

    private void ShowDifficultyParameters(float dificultad, float intervalo, float speed, float rango)
    {
        dificultadText.text = string.Format("Dificultad:\n{0}", dificultad.ToString(System.Globalization.CultureInfo.InvariantCulture));
        intervaloTex.text = string.Format("Intervalo:\n{0}", intervalo.ToString(System.Globalization.CultureInfo.InvariantCulture));
        speedText.text = string.Format("Velocidad:\n{0}", speed.ToString(System.Globalization.CultureInfo.InvariantCulture));
        rangeText.text = string.Format("Rango:\n{0}", rango.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// OnClick "Seguir Jugando" button
    /// </summary>
    public void CalcNewVals()
    {
        if (!GameMaster.IsADDActive)
        {
            #if UNITY_EDITOR
            Debug.Log("ADD no esta activo");
            #endif
            OnCalcNewRutineVals?.Invoke(Desempeno.Mantiene);
        }
        else
        {
            if (_aciertos > 7)
            {
                #if UNITY_EDITOR
                Debug.Log("Aumenta Dificultad");
                #endif
                OnCalcNewRutineVals?.Invoke(Desempeno.Incremento);
            }
            else if (_aciertos < 5)
            {
                #if UNITY_EDITOR
                Debug.Log("Disminuye Dificultad");
                #endif
                OnCalcNewRutineVals?.Invoke(Desempeno.Decremento);
            }
            else
            {
                #if UNITY_EDITOR
                Debug.Log("Mantiene Dificultad");
                #endif
                OnCalcNewRutineVals?.Invoke(Desempeno.Mantiene);
            }
        }
        _initRondaTime = Time.time;
        ResetVals();
    }

    /// <summary>
    /// OnClick Continuar button
    /// </summary>
    public void Continue()
    {
        #if UNITY_EDITOR
        Debug.Log("Termina juego");
        #endif
        SceneManager.LoadScene("ResultsScene");
    }

    private void ResetVals()
    {
        _aciertos = 0;
        _fallos = 0;
        _lanzamientos = 0;
        _lanzamientosIzq = 0;
        _lanzamientosDer = 0;
        _aciertosIzq = 0;
        _aciertosIzqCruzados = 0;
        _fallosIzq = 0;
        _aciertosDer = 0;
        _aciertosDerCruzados = 0;
        _fallosDer = 0;
        _ronda++;
        aciertosText.text = "Aciertos: 0";
        fallosText.text = "Fallos: 0";
        rondasText.text = string.Format("Ronda: {0}", _ronda.ToString());
    }
}
