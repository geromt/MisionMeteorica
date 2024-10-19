using System;
using System.Collections;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class Admin_level0 : MonoBehaviour
{
    public delegate void LoginEvent();
    public static event LoginEvent AskContinueWithoutInternet;
    public static event LoginEvent SuccessUserLogIn;
    public static event LoginEvent SuccessTerapeutaLogInClinica;
    public static event LoginEvent SuccessTerapeutaLogInTerapeuta;

    public delegate void MessageEvent(string message);
    public static event MessageEvent ShowErrorMessage;

    [Header("Paneles de Autenticacion")]
    [Tooltip("Aquí va el panel donde el paciente ingresa el ID y su contraseña")]
    [SerializeField] private GameObject autenticaPacientePanel;
    [Tooltip("Input con el nombre del paciente.")]
    [SerializeField] private TMP_InputField nombrePacienteInput;
    [Tooltip("Input con el password del pacienet.")]
    [SerializeField] private TMP_InputField passwordPacienteInput;
    [Tooltip("Aquí va el panel donde el terapeuta tiene que ingresar su correo y su contraseña")]
    [SerializeField] private GameObject panelAutenticaTerapeuta;
    [Tooltip("Input con el nombre del terapeuta.")]
    [SerializeField] private TMP_InputField nombreTerapeutaInput;
    [Tooltip("Input con el password del terapeuta.")]
    [SerializeField] private TMP_InputField passwordTerapeutaInput;

    private ManejadorXMLs _manejadorXML;

    private void OnEnable()
    {
        VerificadorRed.noHayConexionConCITAN += RegistrarQueNoHayInternet;
        VerificadorRed.tenemosConexionConCITAN += RegistrarQueSIHayInternet;
    }

    private void OnDisable()
    {
        VerificadorRed.noHayConexionConCITAN -= RegistrarQueNoHayInternet;
        VerificadorRed.tenemosConexionConCITAN -= RegistrarQueSIHayInternet;
    }

    private void Awake()
    {
        _manejadorXML = new ManejadorXMLs();

        GameMaster.CreaDirectorioDelJuego();
        GameMaster.LimpiaNombreyIdPaciente();
        GameMaster.LimpiaNombreyIdTerapeuta();
        GameMaster.RegistraQueNoEstoyLogeado();

        StartCoroutine(VerificadorRed.VerificaConexionConCITAN());

        #if UNITY_EDITOR
        Debug.Log("Iniciando programa de trackeo de mano");
        #endif

        // Revisamos si ya hay un proceso corriendo con este nombre
        // Esto solo es para cuando se esta corriendo el proyecto en el editor,
        // una vez creado el ejecutable no deberia de pasar que haya mas de uno
        var handTrackerProcesses = Process.GetProcessesByName("LANRBodyTracker");
        if (handTrackerProcesses.Length > 0) return;

        Process trackerProcess = new Process();
        string trackerPath = System.IO.Directory.GetCurrentDirectory() + "\\LANRBodyTracker\\LANRBodyTracker";
        string configPath = "\"" + System.IO.Directory.GetCurrentDirectory() + "\\LANRBodyTracker\\config.yaml\"";
        #if UNITY_EDITOR
        Debug.Log("Archivo de configuracion: " + configPath);
        #endif

        try
        {
            trackerProcess.StartInfo.UseShellExecute = false;
            trackerProcess.StartInfo.FileName = trackerPath;
            trackerProcess.StartInfo.Arguments = configPath;
            trackerProcess.StartInfo.CreateNoWindow = true;

            #if UNITY_EDITOR
            Debug.Log("Iniciando proceso");
            #endif
            trackerProcess.Start();
            trackerProcess.PriorityClass = ProcessPriorityClass.AboveNormal;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (panelAutenticaTerapeuta.activeSelf)
            {
                if (!nombreTerapeutaInput.isFocused)
                    nombreTerapeutaInput.Select();
                else
                    passwordTerapeutaInput.Select();
            }
            else if (autenticaPacientePanel.activeSelf)
            {
                if (!nombrePacienteInput.isFocused)
                    nombrePacienteInput.Select();
                else
                    passwordPacienteInput.Select();
            }
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (panelAutenticaTerapeuta.activeSelf)
                RevisaDatosTerapeuta();
            else if (autenticaPacientePanel.activeSelf)
                CompruebaUsuario();
        }
    }

    /// <summary>
    /// Método suscrito a evento VerificadorRed.noHayConexionConCITAN
    /// </summary>
    private void RegistrarQueNoHayInternet()
    {
        Debug.Log("No hay conexión con el servidor. Registramos que estamos en modo sin conexión");
        GameMaster.RegistraQueEstoyEnModoSINConexion();
    }

    /// <summary>
    /// Método suscrito a evento VerificadorRed.tenemosConexionConCITAN
    /// </summary>
    void RegistrarQueSIHayInternet()
    {
        Debug.Log("Registramos que estamos en modo con conexión.");
        GameMaster.RegistraQueEstoyEnModoConConexion();
    }

    /// <summary>
    /// Método llamado cuando se presiona el botón aceptar en el panel autenticaPacientePanel. Comprueba que los datos en el 
    /// panel son correctos
    /// </summary>
    public void CompruebaUsuario()
    {
        string usuario = nombrePacienteInput.text.Trim();
        if (string.IsNullOrEmpty(usuario))
        {
            ShowErrorMessage?.Invoke("Debe ingresar un ID de usuario.");
            nombrePacienteInput.Select();
            nombrePacienteInput.ActivateInputField();
            return;
        }

        int idUsuario = 0;
        if (int.TryParse(usuario, out idUsuario))
        {
            RevisaDatosPaciente(usuario);
        }
        else // Si intenta entrar a modo terapeuta
        {
            if (GameMaster.ModoSinConexion)
                ShowErrorMessage?.Invoke("No hay conexión con el servidor. Revise su conexión e intente más tarde.");
            else
                StartCoroutine(VerifyTherapist(nombrePacienteInput.text, passwordPacienteInput.text, false));
        }
    }

    private void RevisaDatosPaciente(string idPaciente)
    {
        GameMaster.SetIdPaciente(idPaciente);

        if (GameMaster.ModoSinConexion)
            AskContinueWithoutInternet?.Invoke();
        else
            StartCoroutine(LoadName());
    }

    public void VeACreditos()
    {
        GameMaster.EscenaDeDondeVengo = "InitScene";
        SceneManager.LoadScene("CreditosScene");
    }

    public void Guardar()
    {
        Debug.Log("El usuario ha elegido subir a CITAN las partidas que se jugaron sin Internet. Se pedirá la clave.");
        Debug.Log("Se ha ingresado una clave válida, se subirán las partidas a CITAN. Preguntamos si el paciente esta asistido por un terapeuta.");

        _manejadorXML.GuardaLasPartidasPendientes(this, GameMaster.RutaHistorialPartidasPaciente, GameMaster.IdPaciente);
    }

    private IEnumerator LoadName()
    {
        Debug.Log("Tenemos Internet, verificando que el ID que se ingresó corresponda a un paciente.");

        string nombrePacienteUrlRequest = DireccionesURL.Id_NombrePaciente + UnityWebRequest.EscapeURL(GameMaster.IdPaciente);
        UnityWebRequest postName1 = new UnityWebRequest(nombrePacienteUrlRequest);
        postName1.downloadHandler = new DownloadHandlerBuffer();
        yield return postName1.SendWebRequest();

        if (postName1.downloadHandler.text.Contains("Inexistente"))
        {
            Debug.Log("El ID que se ingresó no existe en la BD");
            ShowErrorMessage?.Invoke("ID de usuario inválido.");
            yield break;
        }
        GameMaster.SetNombrePaciente(postName1.downloadHandler.text);


        Debug.Log("Hemos obtenido el nombre completo del paciente: " + GameMaster.NombrePaciente);

        if (string.IsNullOrEmpty(passwordPacienteInput.text))
        {
            Debug.Log("No ingreso contrasena");
            ShowErrorMessage("Ingrese su contraseña.");
            passwordPacienteInput.Select();
            passwordPacienteInput.ActivateInputField();
            yield break;
        }

        Debug.Log("Vamos a obtener la contraseña del paciente...");

        string passwordPacienteUrlRequest = string.Format("{0}?id={1}", DireccionesURL.Id_ContraseniaPaciente, UnityWebRequest.EscapeURL(GameMaster.IdPaciente));
        UnityWebRequest postName2 = new UnityWebRequest(passwordPacienteUrlRequest);
        postName2.downloadHandler = new DownloadHandlerBuffer();
        yield return postName2.SendWebRequest();
        string password = postName2.downloadHandler.text;

        Debug.Log("Contraseña: " + password);

        if (!passwordPacienteInput.text.Equals(password))
        {
            ShowErrorMessage("Contraseña incorrecta.");
            passwordPacienteInput.Select();
            passwordPacienteInput.ActivateInputField();
            yield break;
        }

        Debug.Log("Vemos si hay un archivo XML del paciente en la PC, si lo hay vemos si hay partidas sin subir, si no lo hay en la sig. escena intentaremos descargarlo de CITAN");

        if (_manejadorXML.BuscaArchivoXML(GameMaster.RutaHistorialPartidasPaciente))
        {
            Debug.Log("El paciente cuenta con partidas en esta PC. Vamos a ver si hay partidas sin guardar.");
            GameMaster.RegistraQueSiHayXMLdelPacienteEnPC();

            if (_manejadorXML.VerificaPartidasNoSubidas(GameMaster.RutaHistorialPartidasPaciente))
            {
                Debug.Log("Hay partidas que no están guardadas en CITAN");
                Guardar();
            }
            SuccessUserLogIn?.Invoke();
        }
        else // Cuando no existe el archivo XML de la persona hay que tomar datos del servidor
        {
            Debug.Log("No se encontró un archivo XML. Se intentarán descargar el historial de partidas del servidor.");
            GameMaster.RegistraQueNoHayXMLdelPacienteEnPC();
            SuccessUserLogIn?.Invoke();
        }
    }

    public void RegistraModoPaciente()
    {
        Debug.Log("Entramos al juego como Paciente únicamente. Registramos que NO estamos en modo clínica");
        GameMaster.RegistraQueEstoyEnModoPaciente();
    }

    public void RevisaDatosTerapeuta()
    {
        if (string.IsNullOrEmpty(nombreTerapeutaInput.text))
        {
            ShowErrorMessage("Debe ingresar un email");
            nombreTerapeutaInput.Select();
            nombreTerapeutaInput.ActivateInputField();
            return;
        }

        if (string.IsNullOrEmpty(passwordTerapeutaInput.text))
        {
            ShowErrorMessage("Debe ingresar una contraseña");
            passwordTerapeutaInput.Select();
            passwordTerapeutaInput.ActivateInputField();
            return;
        }

        if (GameMaster.ModoSinConexion)
        {
            //No hay conexion con el servidor
            Debug.Log("No hay conexión con el servidor, se deben ingresar las credenciales clinica/clinica. Registramos que estamos en modo clinica");
            if (nombreTerapeutaInput.text.Contains(GameMaster.TerapeutaCuandoNoHayConexion) && passwordTerapeutaInput.text.Contains(GameMaster.ContraseniaDeTerapeutaCuandoNoHayConexion))
            {
                //Iniciamos sesion en clinica sin conexion
                GameMaster.RegistraQueEstoyEnModoClinica();
                SuccessTerapeutaLogInClinica?.Invoke();
            }
            else
            {
                ShowErrorMessage("Error. Por favor ingrese el nombre de usuario y contraseña correspondientes cuando no hay conexión.");
                nombreTerapeutaInput.Select();
                nombreTerapeutaInput.ActivateInputField();
            }
        }
        else
        {
            //Hay conexión con el servidor
            Debug.Log("Hay conexión con el servidor, comprobaremos que el correo existe en CITAN");
            StartCoroutine(VerifyTherapist(nombreTerapeutaInput.text, passwordTerapeutaInput.text, true));
        }
    }

    private IEnumerator VerifyTherapist(string email, string password, bool quieroEntrarAModoClinica)
    {
        string urlStringDoctor = DireccionesURL.Email_NombreTerapeutaID + UnityWebRequest.EscapeURL(email);
        UnityWebRequest postNameDoctor = new UnityWebRequest(urlStringDoctor);
        postNameDoctor.downloadHandler = new DownloadHandlerBuffer();
        yield return postNameDoctor.SendWebRequest();

        if (postNameDoctor.downloadHandler.text.Contains("Inexistente"))
        {
            ShowErrorMessage("Correo inválido");
            if (autenticaPacientePanel.activeSelf)
            {
                nombrePacienteInput.Select();
                nombrePacienteInput.ActivateInputField();
            }

            if (panelAutenticaTerapeuta.activeSelf)
            {
                nombreTerapeutaInput.Select();
                nombreTerapeutaInput.ActivateInputField();
            }
            yield break;
        }

        string nombreTerapeuta = postNameDoctor.downloadHandler.text.Substring(0, postNameDoctor.downloadHandler.text.LastIndexOf(" "));
        string idTerapeuta = postNameDoctor.downloadHandler.text.Substring(postNameDoctor.downloadHandler.text.LastIndexOf(" ") + 1);
        GameMaster.AsignaNombreyIdTerapeuta(nombreTerapeuta, idTerapeuta);

        string urlString1 = string.Format("{0}{1}/{2}", DireccionesURL.EmailPassword_TrueFalse, UnityWebRequest.EscapeURL(email), UnityWebRequest.EscapeURL(password));
        UnityWebRequest postName1 = new UnityWebRequest(urlString1);
        postName1.downloadHandler = new DownloadHandlerBuffer();
        yield return postName1.SendWebRequest();

        if (postName1.downloadHandler.text.Contains("true"))
        {
            GameMaster.EscenaDeDondeVengo = "InitScene";
            if (quieroEntrarAModoClinica)
            {
                Debug.Log("Las credenciales coinciden. Registramos que estamos en modo clinica");
                GameMaster.RegistraQueEstoyEnModoClinica();
                SuccessTerapeutaLogInClinica?.Invoke();
            }
            else
            {
                Debug.Log("Las credenciales coinciden. Registramos que estamos en modo terapeuta");
                GameMaster.RegistraQueEstoyEnModoTerapeuta();
                SuccessTerapeutaLogInTerapeuta?.Invoke();
            }
        }
        else
        {
            ShowErrorMessage("Contraseña incorrecta. Por favor verifique e intente nuevamente.");
            passwordTerapeutaInput.Select();
            passwordTerapeutaInput.ActivateInputField();
            yield break;
        }
    }
}