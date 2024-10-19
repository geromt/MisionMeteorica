using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class AduanaCITAN : MonoBehaviour
{
    public delegate void descargaRutinaAction(string nombreRutina);
    public static event descargaRutinaAction rutinaDescargadaConExito;
    public static event descargaRutinaAction rutinaNoEncontrada;

    public delegate void descargaPartidasAction(string[] partidas);
    public static event descargaPartidasAction partidasDescargadasDeCITAN;

    public delegate void descargaRutinasDocAction();
    public static event descargaRutinasDocAction rutinasDescargadasExito;

    public delegate void subePartidaA_CitanAction();
    public static event subePartidaA_CitanAction partidaSubidaConExito;
    public static event subePartidaA_CitanAction partidaNoSePudoSubir;

    public delegate void subeRutinaA_CitanAction();
    public static event subeRutinaA_CitanAction rutinaSubidaConExito;

    public static IEnumerator SubePartidasA_CITAN(Partida p, string idPaciente)
    {
        Debug.Log("Vamos a subir los datos de una partida directamente a la Base de Datos");

        StringBuilder urlBuilder = new StringBuilder();
        urlBuilder.Append(DireccionesURL.InsertaPartidasEnBD);
        urlBuilder.Append("?id=");
        urlBuilder.Append(idPaciente);
        urlBuilder.Append("&fecha=%27");
        urlBuilder.Append(p.fecha.Replace(" ", "%20"));
        urlBuilder.Append("%27&ronda=");
        urlBuilder.Append(p.ronda.ToString());
        urlBuilder.Append("&tiempo=");
        urlBuilder.Append(p.tiempo.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
        urlBuilder.Append("&aciertos=");
        urlBuilder.Append(p.aciertos.ToString());
        urlBuilder.Append("&fallos=");
        urlBuilder.Append(p.fallos.ToString());

        urlBuilder.Append("&lanzamientosDer=");
        urlBuilder.Append(p.lanzamientosDer.ToString());
        urlBuilder.Append("&lanzamientosIzq=");
        urlBuilder.Append(p.lanzamientosIzq.ToString());
        urlBuilder.Append("&aciertosDer=");
        urlBuilder.Append(p.aciertosDer.ToString());
        urlBuilder.Append("&aciertosIzq=");
        urlBuilder.Append(p.aciertosIzq.ToString());
        urlBuilder.Append("&aciertosDerCruzados=");
        urlBuilder.Append(p.aciertosDerCruzados.ToString());
        urlBuilder.Append("&aciertosIzqCruzados=");
        urlBuilder.Append(p.aciertosIzqCruzados.ToString());
        urlBuilder.Append("&fallosDer=");
        urlBuilder.Append(p.fallosDer.ToString());
        urlBuilder.Append("&fallosIzq=");
        urlBuilder.Append(p.fallosIzq.ToString());

        urlBuilder.Append("&usaGuia=");
        urlBuilder.Append(p.usaGuia.ToString());
        urlBuilder.Append("&usaADD=");
        urlBuilder.Append(p.usaADD.ToString());
        urlBuilder.Append("&dificultad=");
        urlBuilder.Append(p.dificultad.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
        urlBuilder.Append("&velocidad=");
        urlBuilder.Append(p.velocidad.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
        urlBuilder.Append("&intervalo=");
        urlBuilder.Append(p.intervalo.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
        urlBuilder.Append("&rango=");
        urlBuilder.Append(p.rango.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
        urlBuilder.Append("&tamano=");
        urlBuilder.Append(p.tamano.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
        urlBuilder.Append("&desempeno=");
        urlBuilder.Append(p.desempeno.ToString());
        urlBuilder.Append("&IC=");
        urlBuilder.Append(p.IC.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));

        string urlString = urlBuilder.ToString();
        Debug.Log("Esto es lo que vamos a subir:" + urlString);

        UnityWebRequest postName = new UnityWebRequest(urlString);
        postName.downloadHandler = new DownloadHandlerBuffer();
        yield return postName.SendWebRequest();
        Debug.Log("El servidor regresa:" + postName.downloadHandler.text);

        if (!postName.downloadHandler.text.Contains("Exito al subir datos Gerardo"))
        {
            Debug.Log("No pudimos subir la partida");
            if (partidaNoSePudoSubir != null)
                partidaNoSePudoSubir();
        }
        else
        {
            Debug.Log("La partida se subió con éxito");
            if (partidaSubidaConExito != null)
                partidaSubidaConExito();
        }
    }

    public static IEnumerator DescargaPartidasDeCITAN(string idPaciente)
    {
        string[] Datos_S;
        string urlString = string.Format("{0}?id={1}", DireccionesURL.ObtenResultadosJuego, UnityWebRequest.EscapeURL(idPaciente));
        UnityWebRequest postName = new UnityWebRequest(urlString);
        postName.downloadHandler = new DownloadHandlerBuffer();
        yield return postName.SendWebRequest();

        if (!string.IsNullOrEmpty(postName.downloadHandler.text)) //si no entramos en esta condición significa que no hay partidas en la BD
        {
#if UNITY_EDITOR
            Debug.Log("Tenemos partidas almacenadas en la BD");
            Debug.Log(postName.downloadHandler.text);
#endif
            Datos_S = postName.downloadHandler.text.Split(';');
            partidasDescargadasDeCITAN?.Invoke(Datos_S);
        }
    }


    public static IEnumerator DescargaRutina(string nombreXML, string rutaDeArchivos, string idPaciente)
    {
        Debug.Log("Descargando rutina personalizada...");
        string[] idTerapeuta = nombreXML.Split('_');
        string url = DireccionesURL.DirectorioRutinasJuego + nombreXML;

        Debug.Log("Esto es lo que tenemos que descargar" + url);
        UnityWebRequest rutina = new UnityWebRequest(url);
        rutina.downloadHandler = new DownloadHandlerBuffer();
        yield return rutina.SendWebRequest();
        if (rutina.error == null)
        {
            ManejadorXMLs miXml = new ManejadorXMLs();
            miXml.BorraRutinasPrevias(idPaciente, rutaDeArchivos);
            string fullPath = rutaDeArchivos + "\\" + nombreXML;
            string nuevoNombre = idPaciente + "_MMRutina.xml";
            File.WriteAllBytes(fullPath, rutina.downloadHandler.data);
            Debug.Log("Rutina descargada con Exito");

            Debug.Log("Renombramos la rutina descargada para que en vez de ser idTerapeuta_NombreRutina_SandwichRutina.xml se llame -idPaciente_NombreRutina_SandwichRutina.xml");
            File.Move(fullPath, rutaDeArchivos + "\\" + nuevoNombre);

            if (rutinaDescargadaConExito != null)
                rutinaDescargadaConExito(nuevoNombre);
        }
        else
        {
            Debug.Log("No tenemos rutina en CITAN.");
            if (rutinaNoEncontrada != null)
                rutinaNoEncontrada(" ");
        }
    }

    public static IEnumerator RevisaSiTieneRutinaAsignada(string id_paciente, MonoBehaviour instanciaMono, string rutaDeArchivos)
    {
        Debug.Log("Vamos a ver si el paciente tiene una rutina asignada en CITAN");

        string url = DireccionesURL.IdPacienteIdJuego_RevisaSiTieneRutina + "?id=" + UnityWebRequest.EscapeURL(id_paciente) + "&game_id=" + UnityWebRequest.EscapeURL("18");  //<------Cambiar el 4 por el Id del juego
        UnityWebRequest request = new UnityWebRequest(url);
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();
        Debug.Log(request.downloadHandler.text);

        if (request.error is null && !request.downloadHandler.text.Contains("Ninguna"))
        {
            Debug.Log("Existe una rutina para el paciente. Procedemos a descargarla...");
            instanciaMono.StartCoroutine(DescargaRutina(request.downloadHandler.text, rutaDeArchivos, id_paciente));
        }
        else
        {
            Debug.Log("No tenemos rutina en CITAN.");
            rutinaNoEncontrada?.Invoke(" ");
        }
    }

    public static IEnumerator RevisaSiTieneRutinaAsignada(string id_paciente, MonoBehaviour instanciaMono, string rutaDeArchivos, int idJuego)
    {
#if UNITY_EDITOR
        Debug.Log("Vamos a ver si el paciente tiene una rutina asignada en CITAN");
#endif
        string url = string.Format("{0}?id={1}&game_id={2}", DireccionesURL.IdPacienteIdJuego_RevisaSiTieneRutina, UnityWebRequest.EscapeURL(id_paciente), UnityWebRequest.EscapeURL(idJuego.ToString()));
        UnityWebRequest request = new UnityWebRequest(url);
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();
        Debug.Log(request.downloadHandler.text);

        if (request.error is null && !request.downloadHandler.text.Equals("Ninguna"))
        {
            string _nombreRutina = request.downloadHandler.text;
            Debug.Log("Existe una rutina para el paciente. Procedemos a descargarla...");
            instanciaMono.StartCoroutine(DescargaRutina(_nombreRutina, rutaDeArchivos, id_paciente));
        }
        else
        {
            Debug.Log("No tenemos rutina en CITAN.");
            if (rutinaNoEncontrada != null)
                rutinaNoEncontrada(" ");
        }
    }

    //NOTA:Hay algunos juego en los que este método no es necesario
    public static IEnumerator DescargaRutinasTerapeuta(string doc_id)
    {
        string url = DireccionesURL.IdTerapeutaNombreJuego_CreaZipRutinas + doc_id + "&juego='sandwich'";  //<-------CAMBIAR EL NOMBRE DEL JUEGO
        UnityWebRequest postName = new UnityWebRequest(url);
        postName.downloadHandler = new DownloadHandlerBuffer();

        yield return postName.SendWebRequest();      //creamos el ZIP de las rutinas que ha creado el terapeuta se llama id_doc.zip ej. 13.zip
#if UNITY_EDITOR
        Debug.Log("Vamos a intentar descargar del servidor todas las rutinas que ha creado el terapeuta");
        Debug.Log("Esto es lo que recibimos  " + postName.downloadHandler.text);
#endif

        if (postName.downloadHandler.text.Contains("ZIPcreated"))
        { //si existen rutinas creadas por el terapeuta descargamos el zip de esas rutinas
            url = "http://lanr.ifc.unam.mx/unity/" + doc_id + ".zip";
            UnityWebRequest ww = new UnityWebRequest(url);
            ww.downloadHandler = new DownloadHandlerBuffer();
            yield return ww.SendWebRequest(); //aqui ya descargamos el zip
            if (ww.error == null)
            {
#if UNITY_EDITOR
                Debug.Log("Hemos descargado el ZIP de rutinas del servidor");
#endif
                string fullPath = GameMaster.rutaDeArchivos + "\\" + doc_id + ".zip"; //ruta donde guardamos el ZIP que descargamos
                File.WriteAllBytes(fullPath, ww.downloadHandler.data);
                string exportLocation = GameMaster.rutaDeArchivos + "\\";   //ruta donde extraemos el contenido del ZIP ej. "C:/Users/Yoás/Desktop//"	
                ZipUtil.Unzip(fullPath, exportLocation);
#if UNITY_EDITOR
                Debug.Log("Hemos descomprimido el ZIP de rutinas y estan ahora en la PC");
#endif
                File.Delete(fullPath); //borramos el ZIP
                                       //borramos el ZIP del servidor
                url = DireccionesURL.IdTerapeuta_BorraZip + doc_id;
                UnityWebRequest postName2 = new UnityWebRequest(url);
                yield return postName2.SendWebRequest();
                if (rutinasDescargadasExito != null)
                    rutinasDescargadasExito();
#if UNITY_EDITOR
                Debug.Log("Hemos borrado el ZIP de rutinas que se creó en el servidor");
#endif
            }
        }
    }

    public static IEnumerator SubeRutinaAlServidor(string idPaciente, string DireccionRutina)
    {
        yield return new WaitForSeconds(5f);
        string filePath = DireccionRutina + "\\" + idPaciente + "_MMRutina.xml";
        Debug.Log("Vamos a intentar subir la rutina al servidor");
        Debug.Log(filePath);

        WWWForm form = new WWWForm();
        if (File.Exists(filePath))
        {
            StreamReader r = File.OpenText(filePath);
            string _info = r.ReadToEnd();
            r.Close();
            Debug.Log("File Read");
            byte[] levelData = Encoding.UTF8.GetBytes(_info);
            string fileName = idPaciente + "_MMRutina.xml";
            form.AddField("file", "file");
            form.AddBinaryData("file", levelData, fileName, "text/xml");
            Debug.Log("Se creo el form de la rutina, sin errores hasta ahora");

            UnityWebRequest w = UnityWebRequest.Post(DireccionesURL.SubeRutinaACITAN, form);
            w.downloadHandler = new DownloadHandlerBuffer();

            yield return w.SendWebRequest();
            yield return new WaitForSeconds(5f);

            Debug.Log("La rutina se subió");
            Debug.Log("Me responde" + w.downloadHandler.text);
        }
        else
        {
            Debug.Log("La rutina no existe");
        }

        if (rutinaSubidaConExito != null)
            rutinaSubidaConExito();
    }
}
