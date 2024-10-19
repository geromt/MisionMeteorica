using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public delegate void SelectAsteroidEvent(int index);
    public static event SelectAsteroidEvent ClickAsteroid;

    public delegate void MenuEvent();
    public static event MenuEvent OnShowBienvenidaPanel;
    public static event MenuEvent OnShowConectaSensorPanel;

    [Header("Panel de Bienvenida")]
    [Tooltip("Panel de Bienvenida")]
    [SerializeField] private GameObject bienvenidaPanel;
    [Tooltip("Texto para mostrar nombre del paciente")]
    [SerializeField] private TMP_Text nombreText;
    [Tooltip("Texto para indicar que no hay rutina")]
    [SerializeField] private GameObject sinRutinaText;
    [Tooltip("Panel para indicar datos de la rutina, solo se muestra en modo paciente")]
    [SerializeField] private GameObject datosRutinaPanel;
    [Tooltip("Texto para mostrar la velocidad en la rutina")]
    [SerializeField] private TMP_Text velocidadText;
    [Tooltip("Texto para mostrar la intervalo en la rutina")]
    [SerializeField] private TMP_Text intervaloText;
    [Tooltip("Texto para mostrar el rango en la rutina")]
    [SerializeField] private TMP_Text rangoText;
    [Tooltip("Texto para mostrar el tamaño en la rutina")]
    [SerializeField] private TMP_Text tamanoText;
    [Tooltip("Texto para mostrar si se usa ADD en la rutina")]
    [SerializeField] private TMP_Text usaADDText;
    [Tooltip("Texto para mostrar si se usan movimientos verticales en la rutina")]
    [SerializeField] private TMP_Text usaEjeYText;
    [Tooltip("Botón para ir a la siguiente escena o panel, dependiendo del modo")]
    [SerializeField] private Button bienvenidaButton;

    [Header("Panel de Valores Iniciales")]
    [Tooltip("Panel para ajustar los valores iniciales de la rutina, sólo se mostrara en modo clínica")]
    [SerializeField] private GameObject valoresInicialesPanel;
    [Tooltip("Botón para regresar del panel de valores iniciales")]
    [SerializeField] private Button regresarValoresInicialesButton;
    [Tooltip("Slider para ajustar la velocidad inicial")]
    [SerializeField] private SliderController velocidadSlider;
    [Tooltip("Slider para ajustar el intervalo inicial")]
    [SerializeField] private SliderController intervaloSlider;
    [Tooltip("Slider para ajustar el rango inicial")]
    [SerializeField] private SliderController rangoSlider;
    [Tooltip("Slider para ajustar el tamaño inicial")]
    [SerializeField] private SliderController tamanoSlider;

    [Header("Panel Configuracion")]
    [Tooltip("Panel para configurar adicionales de la rutina, sólo se mostrará en modo clínica")]
    [SerializeField] private GameObject configuracionPanel;
    [Tooltip("Botón para continuar del panel de configuración")]
    [SerializeField] private Button continuarConfiguracionButton;
    [Tooltip("Toggle para indicar si se usa guía")]
    [SerializeField] private Toggle guiaToggle;
    [Tooltip("Toggle para indicar si se usa ADD")]
    [SerializeField] private Toggle ADDToggle;
    [Tooltip("Toggle para indicar si se usan movimientos verticales")]
    [SerializeField] private Toggle enabledYAxisToggle;
    [SerializeField] private Button configuracionContinuarButton;

    [Header("Panel Seleccion Asteroide")]
    [SerializeField] private GameObject selectAsteroidPanel;
    [SerializeField] private Button[] asteroideButtonList;
    [SerializeField] private int[] desbloqueoValueList;
    [SerializeField] private Button asteroidSelectionContinuarButton;
    [SerializeField] private Button asteroidSelectionRegresarButton;

    [Header("Panel para conectar sensor")]
    [Tooltip("Panel con las indicaciones para conectar los sensores")]
    [SerializeField] private GameObject conectaSensoresPanel;

    private ManejadorXMLs _manejadorXML;
    private TransitionController _transitionController;
    private int _asteroidIndex = 0;
    private Partida _lastPartidaData;


    private void OnEnable()
    {
        TransitionController.EndsInitTranstion += ShowBienvenidaPanel;
        VerificadorRed.noHayConexionConCITAN += RegistrarQueNoHayInternet;
        VerificadorRed.tenemosConexionConCITAN += RegistrarQueSiHayInternet;
        AduanaCITAN.partidasDescargadasDeCITAN += GuardaXML;
        AduanaCITAN.rutinaDescargadaConExito += CargaDatosRutina;
        AduanaCITAN.rutinaNoEncontrada += BuscaRutinaEnLaPC;
    }

    private void OnDisable()
    {
        TransitionController.EndsInitTranstion -= ShowBienvenidaPanel;
        VerificadorRed.noHayConexionConCITAN -= RegistrarQueNoHayInternet;
        VerificadorRed.tenemosConexionConCITAN -= RegistrarQueSiHayInternet;
        AduanaCITAN.partidasDescargadasDeCITAN -= GuardaXML;
        AduanaCITAN.rutinaDescargadaConExito -= CargaDatosRutina;
        AduanaCITAN.rutinaNoEncontrada -= BuscaRutinaEnLaPC;
    }

    private void Awake()
    {
        _transitionController = GetComponent<TransitionController>();
        Debug.Log("Siempre que regresamos a esta escena verificamos si hay conexión con el servidor Citan");
        StartCoroutine(VerificadorRed.VerificaConexionConCITAN());
        _manejadorXML = new ManejadorXMLs();
        sinRutinaText.SetActive(false);
        datosRutinaPanel.SetActive(false);

        GameMaster.ModoTutorial = false;

        for (int i = 0; i< asteroideButtonList.Length; i++)
        {
            int j = i; //Para pasar valor a lambdas
            asteroideButtonList[i].onClick.RemoveAllListeners();
            asteroideButtonList[i].onClick.AddListener(() => SelectAsteroid(j));
        }
    }

    private void Start()
    {
        ConfigurePanelInit(bienvenidaPanel);
        ConfigurePanelInit(valoresInicialesPanel);
        ConfigurePanelInit(configuracionPanel);
        ConfigurePanelInit(selectAsteroidPanel, 1.2f, 0, 1.2f);
        ConfigurePanelInit(conectaSensoresPanel);

        nombreText.text = GameMaster.NombrePaciente;
        TMP_Text buttonText = bienvenidaButton.transform.GetChild(0).GetComponent<TMP_Text>();

        #if UNITY_EDITOR
        Debug.Log(string.Format("Modo Login: {0}", GameMaster.Modo));
        #endif
        switch (GameMaster.Modo)
        {
            case GameMaster.ModoLogin.Paciente:
                // No se activa boton de jugar hasta que revisemos si hay rutina
                buttonText.text = "Jugar";
                GameMaster.SetMinMaxInterval(intervaloSlider.Min, intervaloSlider.Max);
                GameMaster.SetMinMaxRange(rangoSlider.Min / 100, rangoSlider.Max / 100);
                GameMaster.SetMinMaxSpeed(velocidadSlider.Min, velocidadSlider.Max);
                bienvenidaButton.onClick.RemoveAllListeners();
                bienvenidaButton.onClick.AddListener(delegate { ChangePanel(bienvenidaPanel, selectAsteroidPanel, 1.2f); ClickAsteroid?.Invoke(_asteroidIndex); });
                asteroidSelectionRegresarButton.onClick.RemoveAllListeners();
                asteroidSelectionRegresarButton.onClick.AddListener(delegate { ChangePanel(selectAsteroidPanel, bienvenidaPanel); ClickAsteroid?.Invoke(-1); });
                asteroidSelectionContinuarButton.onClick.RemoveAllListeners();
                asteroidSelectionContinuarButton.onClick.AddListener(delegate
                {
                    GameMaster.SetAsteroidValue(_asteroidIndex);
                    LeanTween.scaleY(configuracionPanel, 0, 0.5f).setEase(LeanTweenType.easeInBounce);
                    _transitionController.PlayFinalTransition("GameScene");
                });
                _transitionController.PlayInitTransition();
                break;
            case GameMaster.ModoLogin.Clinica:
                bienvenidaButton.gameObject.SetActive(true);
                buttonText.text = "Continuar";
                bienvenidaButton.onClick.RemoveAllListeners();
                bienvenidaButton.onClick.AddListener(ContinuarBienvenidaPanel);
                configuracionContinuarButton.onClick.RemoveAllListeners();
                configuracionContinuarButton.onClick.AddListener(delegate { ContinuarConfiguracionPanel(); ClickAsteroid?.Invoke(_asteroidIndex); });
                asteroidSelectionRegresarButton.onClick.RemoveAllListeners();
                asteroidSelectionRegresarButton.onClick.AddListener(delegate { ChangePanel(selectAsteroidPanel, configuracionPanel); ClickAsteroid?.Invoke(-1); });
                asteroidSelectionContinuarButton.onClick.RemoveAllListeners();
                asteroidSelectionContinuarButton.onClick.AddListener(delegate
                {
                    GetValues();
                    LeanTween.scaleY(configuracionPanel, 0, 0.5f).setEase(LeanTweenType.easeInBounce);
                    _transitionController.PlayFinalTransition("GameScene");
                });
                _transitionController.PlayInitTransition();
                break;
            case GameMaster.ModoLogin.Terapeuta:
                GameMaster.ObtuvoDatosRutina = false;
                _transitionController.PlayInitTransition();
                ChangePanel(bienvenidaPanel, valoresInicialesPanel);
                regresarValoresInicialesButton.onClick.RemoveAllListeners();
                regresarValoresInicialesButton.onClick.AddListener(delegate { _transitionController.PlayFinalTransition("TerapeutaScene"); });
                continuarConfiguracionButton.onClick.RemoveAllListeners();
                continuarConfiguracionButton.onClick.AddListener(GetValues);
                continuarConfiguracionButton.onClick.AddListener(delegate { GameMaster.ObtuvoDatosRutina = true; _transitionController.PlayFinalTransition("TerapeutaScene"); });
                break;
        }
        UnlockAsteroidButtons();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.F3) && Input.GetKey(KeyCode.Space))
            UnlockAllButtons();
    }

    private void RegistrarQueNoHayInternet()
    {
        GameMaster.RegistraQueEstoyEnModoSINConexion();

        Debug.Log("No tenemos Internet. Vamos a ver si ya tiene una rutina en la PC para jugar.Llamamos una corutina que busca en el servidor porque esa llamará a la función de buscar una rutina en la PC");
        if (GameMaster.Modo == GameMaster.ModoLogin.Paciente)
            StartCoroutine(AduanaCITAN.RevisaSiTieneRutinaAsignada(GameMaster.IdPaciente, this, GameMaster.rutaDeArchivos));
        RegistraNumeroDePartidasJugadas();
        UnlockAsteroidButtons();
    }

    private void RegistrarQueSiHayInternet()
    {
        GameMaster.RegistraQueEstoyEnModoConConexion();

        Debug.Log("Vamos a subir al servidor partidas que no se hayan guardado en el servidor, si es que existen");
        if (_manejadorXML.VerificaPartidasNoSubidas(GameMaster.RutaHistorialPartidasPaciente))
            _manejadorXML.GuardaLasPartidasPendientes(this, GameMaster.RutaHistorialPartidasPaciente, GameMaster.IdPaciente);

        Debug.Log("En este punto tenemos conexión con CITAN, en caso de que ya existiera un XML en la PC, ya revisamos que todas las partidas están en la BD, pero quizá el paciente jugó en otra PC y ahora en la BD hay más partidas.");
        Debug.Log("Por eso vamos a borrar el XML de la PC y volver a descargar las partidas de la BD para asegurarnos que tenemos los datos más actuales.");
        _manejadorXML.BorraHistarialPartidasXML(GameMaster.RutaHistorialPartidasPaciente);
        Debug.Log("Hemos borrado el Xml del paciente de la PC.");

        StartCoroutine(AduanaCITAN.DescargaPartidasDeCITAN(GameMaster.IdPaciente));

        if (GameMaster.Modo == GameMaster.ModoLogin.Paciente)
            StartCoroutine(AduanaCITAN.RevisaSiTieneRutinaAsignada(GameMaster.IdPaciente, this, GameMaster.rutaDeArchivos));
    }

    private void GuardaXML(string[] partidas)
    {
        GameMaster.RegistraQueSiHayXMLdelPacienteEnPC();
        Debug.Log("GuardaXML: " + GameMaster.IdPaciente + GameMaster.NombrePaciente);
        _manejadorXML.CreaContenedorDelHistorialPartidas(partidas, GameMaster.RutaHistorialPartidasPaciente, GameMaster.IdPaciente, GameMaster.NombrePaciente);
        Debug.Log("Hemos creado un nuevo XML.");
        RegistraNumeroDePartidasJugadas();
        UnlockAsteroidButtons();
        SetLastPartidaValues();
    }

    private void BuscaRutinaEnLaPC(string nada)
    {
        Debug.Log("Buscamos si existe una rutina en la PC.");
        string _rutinaPrevia = GameMaster.IdPaciente + "_MMRutina.xml";  //<-Cambiar esto por el formato correspondiente del juego
        _rutinaPrevia = _manejadorXML.BuscaRutinaXML(GameMaster.rutaDeArchivos, _rutinaPrevia);
        if (_rutinaPrevia.Equals("ninguna"))
        {
            Debug.Log("Sin rutina.");
            MuestraMensajeSinRutina();
        }
        else
        {
            Debug.Log("Rutina encontrada.");
            CargaDatosRutina(_rutinaPrevia);
        }
    }

    private void MuestraMensajeSinRutina()
    {
        sinRutinaText.SetActive(true);
        bienvenidaButton.gameObject.SetActive(false);
    }

    private void CargaDatosRutina(string nombreRutina)
    {
        #if UNITY_EDITOR
        Debug.Log("Cargamos los datos de la rutina.");
        Debug.Log(string.Format("Este es el nombre de la rutina: {0}", nombreRutina));
        #endif

        RutinaData rutinaData = _manejadorXML.CargaRutinaXML(GameMaster.rutaDeArchivos, nombreRutina);
        GameMaster.SetConfigurationVals(rutinaData);
        velocidadText.text = rutinaData.velocidadInicial.ToString() + " m/s";
        intervaloText.text = rutinaData.intervaloInicial.ToString() + " seg";
        rangoText.text = rutinaData.rangoInicial.ToString() + " m";
        tamanoText.text = rutinaData.tamanoInicial.ToString();
        usaADDText.text = rutinaData.usaADD ? "Sí" : "No";
        usaEjeYText.text = rutinaData.usaEjeY ? "Sí" : "No";

        datosRutinaPanel.SetActive(true);
        bienvenidaButton.gameObject.SetActive(true);
    }

    private void RegistraNumeroDePartidasJugadas()
    {
        string archivoXML = $"{GameMaster.rutaDeArchivos}\\{GameMaster.IdPaciente}_Data.xml";
        if (!_manejadorXML.BuscaArchivoXML(archivoXML)) return;

        PlayerData dataForRead = _manejadorXML.CargaHistorialPartidas(archivoXML);
        GameMaster.PartidasJugadas = dataForRead.HistorialPartidas.Count;
        _lastPartidaData = dataForRead.HistorialPartidas[0];
        #if UNITY_EDITOR
        Debug.Log($"El paciente ha jugado {GameMaster.PartidasJugadas} partidas");
        #endif
    }

    private void UnlockAsteroidButtons()
    {
        for (int i = 0; i < asteroideButtonList.Length; i++)
        {
            if (desbloqueoValueList[i] <= GameMaster.PartidasJugadas)
            {
                asteroideButtonList[i].interactable = true;
                asteroideButtonList[i].GetComponent<ButtonTween>().enabled = true;
                asteroideButtonList[i].transform.GetChild(1).gameObject.SetActive(false);
            }
            else
            {
                asteroideButtonList[i].interactable = false;
                asteroideButtonList[i].GetComponent<ButtonTween>().enabled = false;
                asteroideButtonList[i].transform.GetChild(1).gameObject.SetActive(true);
            }
        }
    }

    private void SetLastPartidaValues()
    {
        if (_lastPartidaData == null) return;

        velocidadSlider.Value = _lastPartidaData.velocidad;
        intervaloSlider.Value = _lastPartidaData.intervalo;
        rangoSlider.Value = _lastPartidaData.rango * 100; // De metros a centimetros

        //guiaToggle.isOn = _lastPartidaData.usaGuia;
        ADDToggle.isOn = _lastPartidaData.usaADD;
        // Esta informacion todavia no se guarda en la partida
        //enabledYAxisToggle.isOn = _lastPartidaDat
    }

    // Utilizado para pruebas
    private void UnlockAllButtons()
    {
        foreach (Button asteroideButton in asteroideButtonList)
        {
            asteroideButton.interactable = true;
            asteroideButton.GetComponent<ButtonTween>().enabled = true;
            asteroideButton.transform.GetChild(1).gameObject.SetActive(false);
        }
    }

    private void SelectAsteroid(int index)
    {
        #if UNITY_EDITOR
        Debug.Log("Se selecciono asteriode: " + index);
        #endif
        _asteroidIndex = index;
        ClickAsteroid?.Invoke(index);
    }

    private void ShowBienvenidaPanel()
    {
        bienvenidaPanel.SetActive(true);
        OnShowBienvenidaPanel?.Invoke();
        LeanTween.scaleY(bienvenidaPanel, 1, 0.5f).setDelay(0.5f).setEase(LeanTweenType.easeOutBounce);
    }

    public void ContinuarBienvenidaPanel()
    {
        ChangePanel(bienvenidaPanel, valoresInicialesPanel);
    }

    public void RegresarValoresInicialesPanel()
    {
        ChangePanel(valoresInicialesPanel, bienvenidaPanel);
    }

    public void ContinuarValoresInicialesPanel()
    {
        ChangePanel(valoresInicialesPanel, configuracionPanel);
    }

    public void RegresarConfiguracionPanel()
    {
        ChangePanel(configuracionPanel, valoresInicialesPanel);
    }

    public void ContinuarConfiguracionPanel()
    {
        ChangePanel(configuracionPanel, selectAsteroidPanel, 1.2f);
    }

    public void IrATutorial()
    {
        GameMaster.ModoTutorial = true;
        _transitionController.PlayFinalTransition("GameScene");
    }

    public void IrConectarSersorPanel()
    {
        OnShowConectaSensorPanel?.Invoke();
        ChangePanel(bienvenidaPanel, conectaSensoresPanel);
    }

    public void RegresarConectarSensorPanel()
    {
        ChangePanel(conectaSensoresPanel, bienvenidaPanel);
    }

    private void ChangePanel(GameObject panel1, GameObject panel2, float scaleY = 1)
    {
        if (GameMaster.CalidadGraficos == Graficos.Altos)
        { 
        LeanTween.scaleY(panel1, 0, 0.5f).setEase(LeanTweenType.easeInBounce).setOnComplete(
            () =>
            {
                panel1.SetActive(false);
                panel2.SetActive(true);
                LeanTween.scaleY(panel2, scaleY, 0.5f).setEase(LeanTweenType.easeOutBounce).setDelay(0.5f);
            }
        );
        }
        else if (GameMaster.CalidadGraficos == Graficos.Bajos)
        {
            panel1.transform.localScale = new Vector3(panel1.transform.localScale.x, 0, panel1.transform.localScale.z);
            panel1.SetActive(false);
            panel2.SetActive(true);
            panel2.transform.localScale = new Vector3(panel2.transform.localScale.x, scaleY, panel2.transform.localScale.z);
        }
    }

    /// <summary>
    /// Realiza la configuracion inicial del panel para que los tweens se comporten adecuadamente
    /// </summary>
    /// <param name="panel">Panel para configurar</param>
    private void ConfigurePanelInit(GameObject panel, float initScaleX = 1, float initScaleY = 0, float initScaleZ = 1)
    {
        panel.transform.localScale = new Vector3(initScaleX, initScaleY, initScaleZ);
        panel.SetActive(true);
    }

    /// <summary>
    /// Obtiene valores de los panele del valores iniciales y de configuracion
    /// </summary>
    private void GetValues()
    {
        var velocidad = velocidadSlider.Value;
        var intervalo = intervaloSlider.Value;
        var rango = rangoSlider.Value;
        var tamano = 0.2f; // Tamano se queda como constante. De preferencia borrar de todos lodas

        var usaGuia = false;
        var usaADD = ADDToggle.isOn;
        var usaEjeY = enabledYAxisToggle.isOn;

        #if UNITY_EDITOR
        Debug.Log($"velocidad: {velocidad}, intervalo: {intervalo}, rango: {rango}, tamano {tamano}, usa guia {usaGuia}, usa ADD {usaADD}, usa Eje Y: {usaEjeY}, asteroide {_asteroidIndex}");
        #endif
        rango /= 100; // De centimetros a metros

        GameMaster.SetMinMaxInterval(intervaloSlider.Min, intervaloSlider.Max);
        GameMaster.SetMinMaxRange(rangoSlider.Min / 100, rangoSlider.Max / 100);
        GameMaster.SetMinMaxSpeed(velocidadSlider.Min, velocidadSlider.Max);
        GameMaster.SetConfigurationVals(velocidad, intervalo, rango, tamano, usaGuia, usaADD, usaEjeY, _asteroidIndex);
    }
}
