using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class Inicio : MonoBehaviour
{
    public delegate void InicioAction();
    public static event InicioAction SiguienteEscena;


    void Awake()
    {
        //GameMaster.CreaDirectorioSandwichmania();
    }

    /// <summary>
    /// Metodo asociado al boton Jugar
    /// </summary>
    public void Jugar()
    {
        Debug.Log("Iniciando programa de trackeo de mano");
        Process trackerProcess = new Process();
        string trackerPath = System.IO.Directory.GetCurrentDirectory() + "\\Tracker\\Sandwichmania-HandTracker\\Sandwichmania-HandTracker";

        try
        {
            trackerProcess.StartInfo.UseShellExecute = false;
            trackerProcess.StartInfo.FileName = trackerPath;
            trackerProcess.StartInfo.CreateNoWindow = true;

            Debug.Log("Iniciando proceso");
            trackerProcess.Start();
            trackerProcess.PriorityClass = ProcessPriorityClass.AboveNormal;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

        StartCoroutine(CargaSiguienteEscena());
    }

    private IEnumerator CargaSiguienteEscena()
    {
        if (SiguienteEscena != null)
            SiguienteEscena();

        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("Inicia Sesion");
    }
}
