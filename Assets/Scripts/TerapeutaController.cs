using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TerapeutaController : MonoBehaviour
{
    [Header("Panel de Selecccion")]
    [SerializeField] private GameObject panelSeleccion;
    [SerializeField] private GameObject panelSinInternet;
    [SerializeField] private Text nombreTerapeuta;
    [SerializeField] private Dropdown pacientesDropdown;
    [SerializeField] private Text advertenciaText;

    [Header("Panel de Asignar Rutina")]
    [SerializeField] private GameObject panelAsignarRutina;
    [SerializeField] private Text indicacionesText;
    [SerializeField] private Text velocidadText;
    [SerializeField] private Text intervaloText;
    [SerializeField] private Text rangoText;
    [SerializeField] private Text tamanoText;
    [SerializeField] private Text usaADDText;
    [SerializeField] private Text usaEjeYText;
    [SerializeField] private InputField mensajePaciente;
    [SerializeField] private GameObject esperaPanel;
    [SerializeField] private GameObject mensajeExitoPanel;

    private ManejadorXMLs _manejadorXML;
    private List<OptionData> _optionList;
    private string _mensajePaciente;
    private RutinaData _rutinaData;


    private void OnEnable()
    {
        AduanaCITAN.rutinaSubidaConExito += ShowExitoAlSubirRutina;
    }

    private void OnDisable()
    {
        AduanaCITAN.rutinaSubidaConExito -= ShowExitoAlSubirRutina;
    }

    private void Awake()
    {
        StartCoroutine(VerificadorRed.VerificaConexionConCITAN());
        nombreTerapeuta.text = GameMaster.NombreTerapeuta;
        panelSinInternet.SetActive(false);
        _manejadorXML = new ManejadorXMLs();
        _optionList = new List<OptionData>();

        esperaPanel.SetActive(false);

        StartCoroutine(GetPatientsFromDoc(GameMaster.IdTerapeuta));
    }

    private void Start()
    {
        panelSeleccion.SetActive(true);
        panelAsignarRutina.SetActive(false);

        if (GameMaster.EscenaDeDondeVengo == "MenuScene")
        {
            if (GameMaster.ObtuvoDatosRutina)
            {
                panelAsignarRutina.SetActive(true);
                panelSeleccion.SetActive(false);
                ShowDatosRutina();
            }
        }
    }

    public void VeResultadosDelPaciente()
    {
        var datos = _optionList[pacientesDropdown.value];
        GameMaster.AsignaNombreyIdPaciente(datos.Name, datos.Id);
        Debug.Log(string.Format("Nombre paciente: {0} {1}", GameMaster.NombrePaciente, GameMaster.IdPaciente));
        StartCoroutine(AduanaCITAN.DescargaPartidasDeCITAN(GameMaster.IdPaciente));

        GameMaster.EscenaDeDondeVengo = "TerapeutaScene";
        SceneManager.LoadScene("ResultsScene");
    }

    public void AsignarRutina()
    {
        var datos = _optionList[pacientesDropdown.value];
        GameMaster.AsignaNombreyIdPaciente(datos.Name, datos.Id);
        Debug.Log(string.Format("Nombre paciente: {0} {1}", GameMaster.NombrePaciente, GameMaster.IdPaciente));

        GameMaster.EscenaDeDondeVengo = "TerapeutaScene";
        SceneManager.LoadScene("MenuScene");
    }

    public void CerrarSesion()
    {
        GameMaster.EscenaDeDondeVengo = "TerapeutaScene";
        SceneManager.LoadScene("InitScene");
    }

    public void CancelarRutina()
    {
        panelSeleccion.SetActive(true);
        panelAsignarRutina.SetActive(false);
    }

    private void ShowDatosRutina()
    {
        GetDatosRutina();
        indicacionesText.text = string.Format("Se ha creado la siguiente rutina para {0}", GameMaster.NombrePaciente);
        velocidadText.text = _rutinaData.velocidadInicial.ToString() + " m/s";
        intervaloText.text = _rutinaData.intervaloInicial.ToString() + " seg";
        rangoText.text = _rutinaData.rangoInicial.ToString() + " m";
        tamanoText.text = _rutinaData.tamanoInicial.ToString();
        usaADDText.text = _rutinaData.usaADD ? "Sí" : "No";
        usaEjeYText.text = _rutinaData.usaEjeY ? "Sí" : "No";
    }

    public void GuardarRutina()
    {
        _rutinaData.mensajeParaPacientes = mensajePaciente.text;
        _manejadorXML.CreaXMLRutina(_rutinaData, GameMaster.IdPaciente, GameMaster.rutaDeArchivos);
        StartCoroutine(AduanaCITAN.SubeRutinaAlServidor(GameMaster.IdPaciente, GameMaster.rutaDeArchivos));
        StartCoroutine(ActualizaRutina());
    }

    private void GetDatosRutina()
    {
        _rutinaData = new RutinaData();
        _rutinaData.velocidadInicial = Mathf.Lerp(1, 12, Mathf.InverseLerp(8, 25, GameMaster.Speed));
        _rutinaData.intervaloInicial = GameMaster.Interval;
        _rutinaData.rangoInicial = GameMaster.Range;
        _rutinaData.tamanoInicial = GameMaster.Size;
        _rutinaData.usaGuia = GameMaster.AreGuidesActive;
        _rutinaData.usaADD = GameMaster.IsADDActive;
        _rutinaData.usaEjeY = GameMaster.IsYAxisEnabled;
    }

    private void ShowExitoAlSubirRutina()
    {
        esperaPanel.SetActive(false);
        mensajeExitoPanel.SetActive(true);
    }

    public void ContinuarMensajeExito()
    {
        mensajeExitoPanel.SetActive(false);
        panelSeleccion.SetActive(true);
        panelAsignarRutina.SetActive(false);
    }


    private IEnumerator GetPatientsFromDoc(string id)
    {
        Debug.Log("Obteniendo pacientes del Doc.");
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();

        string urlString = string.Format("{0}{1}", DireccionesURL.IdTerapeuta_ListaPacientes, UnityWebRequest.EscapeURL(id));
        UnityWebRequest postName = new UnityWebRequest(urlString);
        postName.downloadHandler = new DownloadHandlerBuffer();
        yield return postName.SendWebRequest();

#if UNITY_EDITOR
        Debug.Log("Pacientes:" + postName.downloadHandler.text + "FIN");
#endif

        string[] patients = postName.downloadHandler.text.Split(';');
        foreach (string p in patients)
        {
            if (p.Length > 2)
            {
                var info = p.Split('_');
                int patientId;
                if (int.TryParse(info[1], out patientId))
                {
                    var optionData = new OptionData(patientId, info[0]);
                    _optionList.Add(optionData);
                    Dropdown.OptionData option = new Dropdown.OptionData(optionData.ToString());
                    options.Add(option);
                }
            }
        }
        pacientesDropdown.options = options;
    }

    IEnumerator ActualizaRutina()
    {
        string urlString = string.Format("{0}?id={1}&rutina={2}",
            DireccionesURL.IdPacienteIdJuego_ActualizaRutina,
            UnityWebRequest.EscapeURL(GameMaster.IdPaciente),
            UnityWebRequest.EscapeURL(GameMaster.IdPaciente + "_MMRutina.xml") + "&id_game=18"); //<----actualizar el ID del juego

        Debug.Log("Vamos a actualizar la base de datos");
        Debug.Log("Estoy mandando" + urlString);

        UnityWebRequest postName = new UnityWebRequest(urlString);
        postName.downloadHandler = new DownloadHandlerBuffer();
        yield return postName.SendWebRequest();

        Debug.Log("La asignacion de rutina retorno" + postName.downloadHandler.text);
    }
}

public readonly struct OptionData
{
    public OptionData(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; }
    public string Name { get; }

    public override string ToString() => $"{Id}: {Name}";
}
