using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public static class VerificadorRed
{
    public delegate void verificaConexionAction();
    public static event verificaConexionAction noHayConexionConCITAN;
    public static event verificaConexionAction tenemosConexionConCITAN;


    public static IEnumerator VerificaConexionConCITAN()
    {
        Debug.Log("Verificando si hay conexion con Internet...");
        UnityWebRequest www = new UnityWebRequest(DireccionesURL.LigaParaProbarSiHayInternet);

        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();

        if (www.downloadHandler.text.Contains("consulta"))
        {
            Debug.Log("Si hay conexión con el servidor. Debemos registrar que estamos en modo CON conexión.");
            tenemosConexionConCITAN?.Invoke();
        }
        else
        {
            Debug.Log("No hay conexión con el servidor. Debemos registrar que estamos en modo SIN conexión");
            noHayConexionConCITAN?.Invoke();
        }
    }
}
