using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

public class ManejadorXMLs
{
    public void NoGuardesLasPartidasPendientes(string rutaDelHistorialDePartidasDelPaciente)
    {
        XmlDocument myXmlDoc = new XmlDocument();
        myXmlDoc.Load(rutaDelHistorialDePartidasDelPaciente);
        XmlNode node;
        node = myXmlDoc.DocumentElement;
        bool _partidaMarcada = false;
        XmlNode node1 = node.LastChild; //El primer nodo es el de las partidas, el segundo es el de las rutinas
        for (int i = node1.ChildNodes.Count; i > 0; i--)
        {
            XmlNode node2 = node1.ChildNodes.Item(i - 1); //obtenemos una partida especícifa
            if (node2.SelectSingleNode("guardado").InnerText.Equals("0"))
            { //subimos la partida
                node2.SelectSingleNode("guardado").InnerText = "1";
                //node2.SelectSingleNode ("clave").InnerText = "";
                _partidaMarcada = true;
            }
            else
            {
                //terminamos de revisar la lista de partidas porque una vez que encontramos la primer partida guardada en el servidor las demás por default también ya están en el servidor, por lo que es trivial continuar
                if (_partidaMarcada)
                {
                    Debug.Log(i);
                    break;
                }
            }
        }
        myXmlDoc.Save(rutaDelHistorialDePartidasDelPaciente);
    }

    public void GuardaLasPartidasPendientes(MonoBehaviour mono, string rutaDelHistorialDePartidasDelPaciente, string idPaciente)
    {

        PlayerData _myData = CargaHistorialPartidas(rutaDelHistorialDePartidasDelPaciente);

        XmlDocument myXmlDoc = new XmlDocument();
        myXmlDoc.Load(rutaDelHistorialDePartidasDelPaciente);
        XmlNode node;
        node = myXmlDoc.DocumentElement;
        bool _partidaMarcada = false;
        XmlNode node1 = node.LastChild; //El primer nodo es el de las partidas, el segundo es el de las rutinas
        for (int i = node1.ChildNodes.Count; i > 0; i--)
        {
            XmlNode node2 = node1.ChildNodes.Item(i - 1); //obtenemos una partida especícifa
            if (node2.SelectSingleNode("guardado").InnerText.Equals("0"))
            { //subimos la partida				
                mono.StartCoroutine(AduanaCITAN.SubePartidasA_CITAN(_myData.HistorialPartidas[i - 1], idPaciente));
                node2.SelectSingleNode("guardado").InnerText = "1";
                //node2.SelectSingleNode ("clave").InnerText = "";
                _partidaMarcada = true;
            }
            else
            {
                //terminamos de revisar la lista de partidas porque una vez que encontramos la primer partida guardada en el servidor las demás por default también ya están en el servidor, por lo que es trivial continuar
                if (_partidaMarcada)
                {
                    Debug.Log(i);
                    break;
                }
            }
        }
        myXmlDoc.Save(rutaDelHistorialDePartidasDelPaciente);
    }

    public void BorraHistarialPartidasXML(string rutaDelHistorialDePartidasDelPaciente)
    {
        if (File.Exists(rutaDelHistorialDePartidasDelPaciente))
        {
            FileInfo t = new FileInfo(rutaDelHistorialDePartidasDelPaciente);
            t.Delete();
        }
    }

    public void CreaHistorialPartidasXML(PlayerData myData, string rutaDelHistorialDePartidasDelPaciente, bool addNuevaPartida = false)
    {
        Debug.Log(rutaDelHistorialDePartidasDelPaciente);
        GameMaster.CreaDirectorioDelJuego();
        StreamWriter writer;
        FileInfo t = new FileInfo(rutaDelHistorialDePartidasDelPaciente);
        string _data = SerializeObject(myData);
        PlayerData myFinalData = new PlayerData();
        PlayerData myNewData = new PlayerData();
        if (!t.Exists)
        {
            myNewData = (PlayerData)DeserializeObject(_data);
        }
        else   //agregua la info a la ya existente, no la borramos
        {
            //primero cargamos la informacion que ya existia en el archivo
            StreamReader r = File.OpenText(rutaDelHistorialDePartidasDelPaciente);
            string _oldData = r.ReadToEnd();
            r.Close();
            t.Delete();
            myFinalData = (PlayerData)DeserializeObject(_oldData);
            myNewData = (PlayerData)DeserializeObject(_data);
        }
        //agregar todas las partidas de la sesion
        //

        if (addNuevaPartida)
            myFinalData.HistorialPartidas.Insert(0, myNewData.HistorialPartidas[myNewData.HistorialPartidas.Count - 1]);
        else
            for (int i = 0; i < myNewData.HistorialPartidas.Count; i++)
            {
                myFinalData.HistorialPartidas.Add(myNewData.HistorialPartidas[i]);
            }

        _data = "";
        myFinalData.nombre = myData.nombre;
        myFinalData.id = myData.id;
        _data = SerializeObject(myFinalData);
        writer = t.CreateText();
        writer.Write(_data);
        writer.Close();
        Debug.Log("File written and data cleared.");
    }


    public bool VerificaPartidasNoSubidas(string rutaDelHistorialDePartidasDelPaciente)
    {
        if (File.Exists(rutaDelHistorialDePartidasDelPaciente))
        {
            XmlDocument myXmlDoc = new XmlDocument();
            myXmlDoc.Load(rutaDelHistorialDePartidasDelPaciente);
            XmlNode node;
            node = myXmlDoc.DocumentElement;
            //XmlNode node1 = node.ChildNodes.Item(2); //El primer nodo es el de las partidas, el segundo es el de las rutinas
            XmlNode node1 = node.LastChild;
            //recorremos la lista de partidas de atras hacia adelante porque al final es donde se encuentran las partidas sin guardar, no al inicio
            for (int i = node1.ChildNodes.Count; i > 0; i--)
            {
                XmlNode node2 = node1.ChildNodes.Item(i - 1); //obtenemos una partida especícifa
                if (node2.SelectSingleNode("guardado").InnerText.Equals("0"))
                {
                    //Debug.Log(node2.SelectSingleNode("_IC").InnerText);
                    return true;
                }
            }
            myXmlDoc.Save(rutaDelHistorialDePartidasDelPaciente);
        }
        // Cuando no existe en el archivo XML de la persona una partida por subir al servidor
        return false;
    }

    public PlayerData CargaHistorialPartidas(string rutaDelHistorialDePartidasDelPaciente)
    {
        string _data;
        StreamReader r = File.OpenText(rutaDelHistorialDePartidasDelPaciente);
        string _info = r.ReadToEnd();
        r.Close();
        _data = _info;
        PlayerData miHistorial = (PlayerData)DeserializeObject(_data);
        return miHistorial;
    }

    public string BuscaRutinaXML(string rutaDeArchivos, string _nombreRutina)
    {
        string[] files = System.IO.Directory.GetFiles(rutaDeArchivos, _nombreRutina);
        if (files.Length >= 1)
        {
            return files[0].Substring(files[0].LastIndexOf("\\") + 1);
        }
        else
        {
            return "ninguna";
        }
    }

    public bool BuscaArchivoXML(string _nombreArchivo)
    {

        if (File.Exists(_nombreArchivo))
        {
            return true;
        }
        else
            return false;
    }

    public RutinaData CargaRutinaXML(string _rutaDeArchivos, string _rutinaFileName)
    {
        StreamReader r = File.OpenText(_rutaDeArchivos + "\\" + _rutinaFileName);
        string _info = r.ReadToEnd();
        r.Close();
        string _data = _info;
        RutinaData myData = (RutinaData)DeserializeObjectRutina(_data);
        return myData;
    }

    public RutinaData CargaRutinaXML(string _rutaCompletaDeLaRuta)
    {
        StreamReader r = File.OpenText(_rutaCompletaDeLaRuta);
        string _info = r.ReadToEnd();
        r.Close();
        string _data = _info;
        RutinaData myData = (RutinaData)DeserializeObjectRutina(_data);
        return myData;
    }

    public int ObtenNumeroDePartidas(string _historialPartidasPaciente)
    {

        if (File.Exists(_historialPartidasPaciente))
        {
            PlayerData _myData = CargaHistorialPartidas(_historialPartidasPaciente);
            return _myData.HistorialPartidas.Count;
        }
        else
            return 0;
    }

    public void CreaContenedorDelHistorialPartidas(string[] partidas, string rutaDelHistorialDePartidasDelPaciente, string idPaciente, string nombrePaciente)
    {
        PlayerData myData = new PlayerData();

        //le restamos dos porque cada partida esta dividida por un _ debido al split, pero debido a ese split el último elemento 
        //va a ser una cadena vacía, des esta manera , por ejemplo si el paciente sólo jugo 1 partida al haecer el split tendremos que jugó 2,
        //entonces i=2-2=0 y sólo se guarda la partida 0 porque la 1 está vacia
        for (int i = (partidas.Length - 2); i > -1; i--)
        {
            Partida partida = new Partida();
            string[] datos_partida_x = partidas[i].Split('_');
            //Cambiar esta parte dependiendo de cómo se obtienen los datos de CITAN al descargar las partidas

            partida.fecha = datos_partida_x[0];
            partida.ronda = int.Parse(datos_partida_x[1]);
            partida.tiempo = float.Parse(datos_partida_x[2], System.Globalization.CultureInfo.InvariantCulture);
            partida.aciertos = int.Parse(datos_partida_x[3]);
            partida.fallos = int.Parse(datos_partida_x[4]);
            partida.usaGuia = datos_partida_x[5].Contains("1");
            partida.usaADD = datos_partida_x[6].Contains("1");
            partida.dificultad = float.Parse(datos_partida_x[7], System.Globalization.CultureInfo.InvariantCulture);
            partida.velocidad = float.Parse(datos_partida_x[8], System.Globalization.CultureInfo.InvariantCulture);
            partida.intervalo = float.Parse(datos_partida_x[9], System.Globalization.CultureInfo.InvariantCulture);
            partida.rango = float.Parse(datos_partida_x[10], System.Globalization.CultureInfo.InvariantCulture);
            partida.tamano = float.Parse(datos_partida_x[11], System.Globalization.CultureInfo.InvariantCulture);
            partida.desempeno = GameMaster.ParseDesempeno(datos_partida_x[12]);
            partida.IC = float.Parse(datos_partida_x[13], System.Globalization.CultureInfo.InvariantCulture);

            partida.lanzamientosDer = int.Parse(datos_partida_x[14]);
            partida.lanzamientosIzq = int.Parse(datos_partida_x[15]);
            partida.aciertosDer = int.Parse(datos_partida_x[16]);
            partida.aciertosIzq = int.Parse(datos_partida_x[17]);
            partida.aciertosDerCruzados = int.Parse(datos_partida_x[18]);
            partida.aciertosIzqCruzados = int.Parse(datos_partida_x[19]);
            partida.fallosDer = int.Parse(datos_partida_x[20]);
            partida.fallosIzq = int.Parse(datos_partida_x[21]);

            partida.guardado = 1;
            myData.HistorialPartidas.Add(partida);
        }
        myData.id = idPaciente;
        myData.nombre = nombrePaciente;
        CreaHistorialPartidasXML(myData, rutaDelHistorialDePartidasDelPaciente);
    }


    public void BorraRutinasPrevias(string idPaciente, string rutaDeArchivos)
    {
        string[] files = System.IO.Directory.GetFiles(rutaDeArchivos, idPaciente + "_MMRutina.xml");   //<---------------Cambiar esta parte
#if UNITY_EDITOR
        Debug.Log("Procedemos a borrar las rutinas que tenga este paciente");
#endif
        for (int i = 0; i < files.Length; i++)
        {
            //File.Delete (rutaDeArchivos+"\\"+files[i]);
            File.Delete(files[i]);
#if UNITY_EDITOR
            Debug.Log("Borrando" + files[i]);
#endif
        }
#if UNITY_EDITOR
        Debug.Log("Rutinas eliminadas");
#endif
    }

    //Esta función se manda llamar cuando tenemos una una interfaz del Terapeuta Tipo 2
    public void CreaXMLRutina(RutinaData rutinaData, string idPaciente, string rutaDondeSeGuardaraRutina, string nombreRutina)
    {
        string _data = SerializeObjectRutina(rutinaData);
        StreamWriter writer;
        FileInfo t = new FileInfo(rutaDondeSeGuardaraRutina + "\\" + idPaciente + "_MMRutina.xml");   //<--------------cambiar esta parte
                                                                                                      //ya verificamos antes que no existe una rutina con el mismo nombre
        if (!t.Exists)
        {
            writer = t.CreateText();
            writer.Write(_data);
            writer.Close();
        }
        Debug.Log("File written.");
    }

    //Esta función se manda llamar cuando tenemos una una interfaz del Terapeuta Tipo 1
    public void CreaXMLRutina(RutinaData rutinaData, string IdPaciente, string rutaDondeSeGuardaraRutina)
    {
        string _data = SerializeObjectRutina(rutinaData);
        StreamWriter writer;
        //--------------------------
        FileInfo t = new FileInfo(rutaDondeSeGuardaraRutina + "/" + IdPaciente + "_MMRutina.xml");  //<---Se tiene que cambiar este nombre
                                                                                                    //En este caso si ya existía una rutina para el paciente entonces la sobreescribimos
        writer = t.CreateText();
        writer.Write(_data);
        writer.Close();
#if UNITY_EDITOR
        Debug.Log("XML de rutina creado.");
#endif
    }

    byte[] StringToUTF8ByteArray(string pXmlString)
    {
        UTF8Encoding encoding = new UTF8Encoding();
        byte[] byteArray = encoding.GetBytes(pXmlString);
        return byteArray;
    }

    string UTF8ByteArrayToString(byte[] characters)
    {
        UTF8Encoding encoding = new UTF8Encoding();
        string constructedString = encoding.GetString(characters);
        return (constructedString);
    }

    object DeserializeObject(string pXmlizedString)
    {
        XmlSerializer xs = new XmlSerializer(typeof(PlayerData));
        MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(pXmlizedString));
        return xs.Deserialize(memoryStream);
    }

    object DeserializeObjectRutina(string pXmlizedString)
    {
        XmlSerializer xs = new XmlSerializer(typeof(RutinaData));
        MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(pXmlizedString));
        return xs.Deserialize(memoryStream);
    }

    // Here we serialize our UserData object of myData 
    string SerializeObject(object pObject)
    {
        string XmlizedString = null;
        MemoryStream memoryStream = new MemoryStream();
        XmlSerializer xs = new XmlSerializer(typeof(PlayerData));
        XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
        xs.Serialize(xmlTextWriter, pObject);
        memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
        XmlizedString = UTF8ByteArrayToString(memoryStream.ToArray());
        return XmlizedString;
    }

    string SerializeObjectRutina(object pObject)
    {
        string XmlizedString = null;
        MemoryStream memoryStream = new MemoryStream();
        XmlSerializer xs = new XmlSerializer(typeof(RutinaData));
        XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
        xs.Serialize(xmlTextWriter, pObject);
        memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
        XmlizedString = UTF8ByteArrayToString(memoryStream.ToArray());
        return XmlizedString;
    }
}
