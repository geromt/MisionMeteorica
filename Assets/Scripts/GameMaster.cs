using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Net.Sockets;
using UnityEngine;

public static class GameMaster
{
    #region "Datos de paciente"
    public static string rutaDeArchivos = "C:\\LANR\\MisionMeteorica";

    /// <summary>
    /// Crea el directorio donde se guardaran los resultados del juego 
    /// </summary>
    public static void CreaDirectorioDelJuego()
    {
        if (!Directory.Exists(rutaDeArchivos))
        {
            Directory.CreateDirectory(rutaDeArchivos);
        }
    }

    // Datos paciente
    private static string nombrePaciente = "";
    public static string NombrePaciente { get { return nombrePaciente; } }

    private static string idPaciente = "";
    public static string IdPaciente { get { return idPaciente; } }

    private static string fileName = "";
    public static string FileName { get { return fileName; } }

    private static string rutaHistorialPartidasPaciente = "";
    public static string RutaHistorialPartidasPaciente { get { return rutaHistorialPartidasPaciente; } }

    public static void SetNombrePaciente(string nombre)
    {
        nombrePaciente = nombre;
    }

    public static void SetIdPaciente(string id)
    {
        idPaciente = id;
        fileName = id + "_Data.xml";
        rutaHistorialPartidasPaciente = rutaDeArchivos + "\\" + fileName;
    }

    public static void AsignaNombreyIdPaciente(string nombre, string id)
    {
        nombrePaciente = nombre;
        idPaciente = id;
        fileName = id + "_Data.xml";
        rutaHistorialPartidasPaciente = rutaDeArchivos + "\\" + fileName;
    }

    public static void AsignaNombreyIdPaciente(string nombre, int id)
    {
        nombrePaciente = nombre;
        idPaciente = id.ToString();
        fileName = id + "_Data.xml";
        rutaHistorialPartidasPaciente = rutaDeArchivos + "\\" + fileName;
    }

    public static void LimpiaNombreyIdPaciente()
    {
        nombrePaciente = "";
        idPaciente = "";
    }

    // Datos Terapeuta
    private static string nombreTerapeuta;
    public static string NombreTerapeuta { get { return nombreTerapeuta; } }

    private static string idTerapeuta;
    public static string IdTerapeuta { get { return idTerapeuta; } }

    public static void AsignaNombreyIdTerapeuta(string nombre, string id)
    {
        nombreTerapeuta = nombre;
        idTerapeuta = id;
    }

    public static void LimpiaNombreyIdTerapeuta()
    {
        nombreTerapeuta = "";
        idTerapeuta = "";
    }

    // Datos de conexion con internet
    private static bool modoSinConexion;
    public static bool ModoSinConexion { get { return modoSinConexion; } }

    private static string terapeutaCuandoNoHayConexion = "clinica";
    public static string TerapeutaCuandoNoHayConexion { get { return terapeutaCuandoNoHayConexion; } }

    private static string contraseniaDeTerapeutaCuandoNoHayConexion = "clinica";
    public static string ContraseniaDeTerapeutaCuandoNoHayConexion { get { return contraseniaDeTerapeutaCuandoNoHayConexion; } }

    public static void RegistraQueEstoyEnModoConConexion()
    {
        modoSinConexion = false;
    }

    public static void RegistraQueEstoyEnModoSINConexion()
    {
        modoSinConexion = true;
    }

    // Datos de XML
    private static bool noHayXMLdelPacienteEnPC;
    public static bool NoHayXMLdelPacienteEnPC { get { return noHayXMLdelPacienteEnPC; } }

    public static void RegistraQueNoHayXMLdelPacienteEnPC()
    {
        noHayXMLdelPacienteEnPC = true;
    }

    public static void RegistraQueSiHayXMLdelPacienteEnPC()
    {
        noHayXMLdelPacienteEnPC = false;
    }

    // Modo de LogIn
    public enum ModoLogin { Ninguno, Paciente, Terapeuta, Clinica }
    private static ModoLogin modo;
    public static ModoLogin Modo { get { return modo; } }

    public static void RegistraQueEstoyEnModoClinica()
    {
        modo = ModoLogin.Clinica;
    }

    public static void RegistraQueEstoyEnModoPaciente()
    {
        modo = ModoLogin.Paciente;
    }

    public static void RegistraQueEstoyEnModoTerapeuta()
    {
        modo = ModoLogin.Terapeuta;
    }

    public static void RegistraQueNoEstoyLogeado()
    {
        modo = ModoLogin.Ninguno;
    }

    public static Desempeno ParseDesempeno(string str)
    {
        if (str.Contains("Incremento"))
            return Desempeno.Incremento;
        else if (str.Contains("Decremento"))
            return Desempeno.Decremento;
        else if (str.Contains("Mantiene"))
            return Desempeno.Mantiene;
        else
            return Desempeno.Mantiene;
    }
    #endregion

    #region "Conexion con webcam"
    private static int port = 5052;
    public static int Port { get { return port; } }
    public static bool IsReceivingWebcamData { get; set; }

    private static UdpClient udpClient;
    public static UdpClient UdpClient
    {
        get
        {
            if (udpClient == null)
                udpClient = new UdpClient(Port);
            return udpClient;
        }
    }

    public static void CloseUDPClient()
    {
        if (udpClient != null)
        {
            udpClient.Dispose();
            udpClient.Close();
        }

    }
    #endregion

    public static bool IsCalibrated { get; set; }

    #region "Ajuste Dinamico de Dificultad"
    // VALORES PARA AJUSTE DE DIFICULTAD
    private static float difficulty;
    public static float Difficulty { get { return difficulty; } }

    private static float interval;
    public static float Interval { get { return interval; } }

    private static float minInterval;
    public static float MinInterval { get { return minInterval; } }

    private static float maxInterval;
    public static float MaxInterval { get { return maxInterval; } }

    private static float speed;
    public static float Speed { get { return speed; } }

    private static float minSpeed;
    public static float MinSpeed { get { return minSpeed; } }

    private static float maxSpeed;
    public static float MaxSpeed { get { return maxSpeed; } }

    private static float range;
    public static float Range { get { return range; } }

    private static float minRange;
    public static float MinRange { get { return minRange; } }

    private static float maxRange;
    public static float MaxRange { get { return maxRange; } }

    private static bool usePatientPoly = true;
    public static bool UsePatientPoly { get { return usePatientPoly; } }

    public static void SetDifficultyVals(float difficulty, float interval, float speed, float range)
    {
        GameMaster.difficulty = difficulty;
        GameMaster.interval = interval;
        GameMaster.speed = speed;
        GameMaster.range = range;
    }

    public static void SetMinMaxInterval(float minInterval, float maxInterval)
    {
        GameMaster.minInterval = minInterval;
        GameMaster.maxInterval = maxInterval;
    }

    public static void SetMinMaxRange(float minRange, float maxRange)
    {
        GameMaster.minRange = minRange;
        GameMaster.maxRange = maxRange;
    }

    public static void SetMinMaxSpeed(float minSpeed, float maxSpeed)
    {
        GameMaster.minSpeed = minSpeed;
        GameMaster.maxSpeed = maxSpeed;
    }
    #endregion

    #region "Configuracion"
    public static string EscenaDeDondeVengo { get; set; }

    public static bool ModoTutorial { get; set; }

    public static bool ObtuvoDatosRutina { get; set; }

    private static int partidasJugadas = 0;
    public static int PartidasJugadas { get { return partidasJugadas; } set { partidasJugadas = value; } }

    private static float size;
    public static float Size { get { return size; } }

    private static bool areGuidesActive;
    public static bool AreGuidesActive { get { return areGuidesActive; } set { areGuidesActive = value; } }

    private static bool isADDActive;
    public static bool IsADDActive { get { return isADDActive; } }

    public static bool IsYAxisEnabled { get; set; }

    private static int asteroidIndex;
    public static int AsteroidIndex { get { return asteroidIndex; } }

    private static bool isVoiceActive = true;
    public static bool IsVoiceActive { get { return isVoiceActive; } set { isVoiceActive = value; } }

    public static void SetConfigurationVals(float speed, float interval, float range, float size, bool useGuides, bool useADD, bool useYAxis, int asteroidIndex)
    {
        GameMaster.speed = speed;
        GameMaster.interval = interval;
        GameMaster.range = range;
        GameMaster.size = size;
        areGuidesActive = useGuides;
        isADDActive = useADD;
        IsYAxisEnabled = useYAxis;
        GameMaster.asteroidIndex = asteroidIndex;
    }

    public static void SetConfigurationVals(RutinaData rutinaData)
    {
        //var velocidadInicial = Mathf.Lerp(8, 25, Mathf.InverseLerp(1, 12, rutinaData.velocidadInicial));
        //speed = velocidadInicial;
        speed = rutinaData.velocidadInicial;
        interval = rutinaData.intervaloInicial;
        range = rutinaData.rangoInicial;
        size = rutinaData.tamanoInicial;
        areGuidesActive = rutinaData.usaGuia;
        isADDActive = rutinaData.usaADD;
        IsYAxisEnabled = rutinaData.usaEjeY;
    }

    public static void SetAsteroidValue(int index)
    {
        asteroidIndex = index;
    }
    #endregion

    #region "Calidad de Graficos"
    private static Graficos calidadGraficos = Graficos.Bajos;
    public static Graficos CalidadGraficos { get { return calidadGraficos; } set { calidadGraficos = value; } }

    public static ConfiguracionData GetConfiguracionData()
    {
        ConfiguracionData cd = new ConfiguracionData();
        cd.graficos = calidadGraficos;
        return cd;
    }
    #endregion


    #region "Datos Partida"
    private static Partida partida;
    public static Partida Partida { get { return partida; } }

    public static void SaveRondaData(int ronda, float tiempo, int aciertos, int fallos, int lanzamientoDer, int lanzamientosIzq, int aciertosDer, int aciertosIzq, int aciertosDerCruzados, int aciertosIzqCruzados, int fallosDer, int fallosIzq)
    {
        partida = new Partida();
        partida.guardado = 0;
        partida.fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        partida.ronda = ronda;
        partida.tiempo = tiempo;
        partida.aciertos = aciertos;
        partida.fallos = fallos;
        partida.lanzamientosDer = lanzamientoDer;
        partida.lanzamientosIzq = lanzamientosIzq;
        partida.aciertosDer = aciertosDer;
        partida.aciertosIzq = aciertosIzq;
        partida.aciertosDerCruzados = aciertosDerCruzados;
        partida.aciertosIzqCruzados = aciertosIzqCruzados;
        partida.fallosDer = fallosDer;
        partida.fallosIzq = fallosIzq;
        partida.usaGuia = areGuidesActive;
        partida.usaADD = isADDActive;
        partida.dificultad = difficulty;
        partida.velocidad = speed;
        partida.intervalo = interval;
        partida.rango = range;
        partida.tamano = size;
        partida.desempeno = CalcDesempeno(aciertos);
        partida.IC = CalcIC(aciertos);
    }

    private static Desempeno CalcDesempeno(int aciertos)
    {
        if (!isADDActive)
            return Desempeno.Mantiene;

        if (aciertos > 7)
            return Desempeno.Incremento;
        else if (aciertos < 5)
            return Desempeno.Decremento;
        else
            return Desempeno.Mantiene;
    }

    private static float CalcIC(int aciertos)
    {
        var minDiff = ModeloPsicometrico.QRM(minInterval, minSpeed, maxRange, usePatientPoly);
        var maxDiff = ModeloPsicometrico.QRM(maxInterval, maxSpeed, minRange, usePatientPoly);
        var tempDiff = partida.dificultad;

        if (aciertos > 7)
            tempDiff += 0.05f;
        else if (aciertos < 5)
            tempDiff -= 0.05f;

        return Mathf.Lerp(0, 100, Mathf.InverseLerp(minDiff, maxDiff, tempDiff));
    }
    #endregion

    #region "Datos de Sensores"
    public static bool IsLeftDeviceConnected { get; set; }
    public static bool IsRightDeviceConnected { get; set; }
    public static int TimeoutRightCounter { get; set;}
    public static int TimeoutLeftCounter { get; set; }

    private static string serialPortLeftName;
    public static string SerialPortLeftName { get => serialPortLeftName; }

    private static SerialPort serialPortLeft;
    public static SerialPort SerialPortLeft { get => serialPortLeft; }

    private static string serialPortRightName;
    public static string SerialPortRightName { get => serialPortRightName; }

    private static SerialPort serialPortRight;
    public static SerialPort SerialPortRight { get => serialPortRight; }

    public static void OpenSerialPortLeft(string name)
    {
        serialPortLeftName = name;
        serialPortLeft = new SerialPort(name, 115200);
        serialPortLeft.WriteTimeout = 1000;
        serialPortLeft.ReadTimeout = 1000;
        serialPortLeft.DtrEnable = true;
        if (!serialPortLeft.IsOpen)
            serialPortLeft.Open();
    }

    public static void ResetSerialPortLeft()
    {
        if (serialPortLeft == null) return;
        if (serialPortLeft.IsOpen)
            serialPortLeft.Close();
        serialPortLeft.Dispose();
        serialPortLeft = null;
    }

    public static void OpenSerialPortRight(string name)
    {
        serialPortRightName = name;
        serialPortRight = new SerialPort(name, 115200);
        serialPortRight.WriteTimeout = 1000;
        serialPortRight.ReadTimeout = 1000;
        serialPortRight.DtrEnable = true;
        if (!serialPortRight.IsOpen)
            serialPortRight.Open();
    }

    public static void ResetSerialPortRight()
    {
        if (serialPortRight == null) return;

        if (serialPortRight.IsOpen)
            serialPortRight.Close();
        serialPortRight.Dispose();
        serialPortRight = null;
    }
    #endregion
}

public enum Desempeno
{
    Mantiene,
    Incremento,
    Decremento
}

public enum Mano
{
    Izquierda,
    Derecha
}

public enum Graficos
{
    Bajos,
    Altos
}

#region CLASES
public class PlayerData
{
    public string nombre;
    public string id;
    public List<Partida> HistorialPartidas = new List<Partida>();
}

public class TerapeutaData
{
    public string Nombre;
    public int Id;
}

public class Partida
{
    public int guardado;
    public string fecha;
    public int ronda;
    public float tiempo;
    public int aciertos;
    public int fallos;
    public int lanzamientosDer;
    public int lanzamientosIzq;
    public int aciertosDer;
    public int aciertosIzq;
    public int aciertosDerCruzados;
    public int aciertosIzqCruzados;
    public int fallosDer;
    public int fallosIzq;
    public bool usaGuia;
    public bool usaADD;
    public float dificultad;
    public float velocidad;
    public float intervalo;
    public float rango;
    public float tamano;
    public Desempeno desempeno;
    public float IC;
}

public class RutinaData
{
    public bool usaGuia;
    public bool usaADD;
    public bool usaEjeY;
    public float velocidadInicial;
    public float intervaloInicial;
    public float rangoInicial;
    public float tamanoInicial;
    public string mensajeParaPacientes;
}

public class ConfiguracionData
{
    public Graficos graficos;
}
#endregion
