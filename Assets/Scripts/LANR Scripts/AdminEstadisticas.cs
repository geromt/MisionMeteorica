using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class AdminEstadisticas : MonoBehaviour
{
    [SerializeField] private GameObject generalGraphPanel;
    [SerializeField] private Button graphButton; //este boton se instanciara sobre la grafica por cada partida, al darle click se mostraran mas detalles de la partida
    [SerializeField] private GameObject dataWindow; //La ventana donde se mostraran los detalles especificos de cada partida
    [SerializeField] private Transform gridImage; // es la imagen con la cuadricula, debe cambiar de tamaño tamaño en proporcion al workSpace
    [SerializeField] private UILineTextureRenderer lineRenderer; //el line renderer que dibuja la grafica
    [SerializeField] private TMP_Text nombrePaciente;
    [SerializeField] private GameObject sinDatosText;

    private PlayerData _dataForRead;
    private ManejadorXMLs _manejadorXML;
    int _noPartidasJugadas;

    private List<Button> _botonesxNivel;
    private List<GameObject> _cuadrosDeInfo;

    private void Start()
    {
        GetComponent<TransitionController>().PlayInitTransition();
        _dataForRead = new PlayerData();
        _manejadorXML = new ManejadorXMLs();
        _botonesxNivel = new List<Button>();
        _cuadrosDeInfo = new List<GameObject>();

        nombrePaciente.text = "Avance general de " + GameMaster.NombrePaciente;
        sinDatosText.SetActive(false);
        var archivoXML = string.Format("{0}\\{1}_Data.xml", GameMaster.rutaDeArchivos, GameMaster.IdPaciente);

        if (!_manejadorXML.BuscaArchivoXML(archivoXML))
        {
            generalGraphPanel.SetActive(false);
            sinDatosText.SetActive(true);
            return;
        }

        //en el objeto dataForRead se almacenara la informacion que se lea del XML del paciente     
        _dataForRead = _manejadorXML.CargaHistorialPartidas(archivoXML);
        _noPartidasJugadas = _dataForRead.HistorialPartidas.Count;

        DibujaGraficaGeneral();
    }


    /*El punto de origen de la grafica es en 0,0 y va aumentando cada 27.5 tanto X como Y,
	* por lo tanto el maximo valor en Y para un punto del line renderer es 275
	*/
    private void DibujaGraficaGeneral()
    {
        if (_noPartidasJugadas == 0) return; //no se dibuja grafica//no se dibuja grafica

        int NoPuntos = (_noPartidasJugadas > 15) ? 15 : _noPartidasJugadas;
        int indexListPartidasPorNivel = _noPartidasJugadas - 1;
        List<string> indexList = new List<string>();

        lineRenderer.Points = new Vector2[NoPuntos];

        for (int i = 0; i < NoPuntos; i++)
        {
            //indexList.Add(indexListPartidasPorNivel + "n"); //n indica que la partida pertenece a la lista de partidas por nivel
            //indexListPartidasPorNivel--;
            indexList.Add(i + "n");
        }

        //Graficando las ultimas 15 partidas(maximo) que jugó el paciente
        for (int i = 0; i < NoPuntos; i++)
        {
            float _indiceComplejidad = _dataForRead.HistorialPartidas[i].IC;
            Debug.Log("INDICE DE COMPLEJIDAD" + _indiceComplejidad);

            if (NoPuntos > 1)
                lineRenderer.Points[i].Set(27.5f * (NoPuntos - i - 1), 2.75f * _indiceComplejidad);
            else
                lineRenderer.gameObject.SetActive(false);

            //Se crea un boton por cada partida jugada
            Button partidaButton = Instantiate(graphButton, gridImage);
            partidaButton.transform.localScale = Vector3.one;
            partidaButton.transform.localPosition = Vector3.zero;
            Vector2 buttonPos = new Vector2(27.5f * (NoPuntos - i - 1), 2.75f * _indiceComplejidad);
            partidaButton.GetComponent<RectTransform>().anchoredPosition = buttonPos;
            partidaButton.name = i.ToString();

            //A cada boton que se ha creado se le agrega un listener para poder llamar a la funcion MoreInfoPartida tomando como argumento
            //el nombre del boton para saber el indice de la partida y mostrar sus respectiva info.
            partidaButton.onClick.AddListener(() => MoreInfoPartidapersonalizada(int.Parse(partidaButton.name), "GeneralGraphPanel", buttonPos, partidaButton.transform));
            _botonesxNivel.Add(partidaButton);
        }
    }

    public void MoreInfoPartidapersonalizada(int index, string nameOfParentPanel, Vector2 button_position, Transform partidaButton)
    {
        //Se instancia la pequeña ventana de datos y se agrega a la lista de ventanas
        //Se crea una ventana cada vez que se presiona el boton corresopndiente a la partida jugada
        if (dataWindow)
        {
            GameObject window = Instantiate(dataWindow, generalGraphPanel.transform);
            window.transform.position = partidaButton.transform.TransformPoint(Vector3.zero);
            window.transform.localPosition += new Vector3(75, 90, 0);
            window.transform.localScale = Vector3.one;
            _cuadrosDeInfo.Add(window);

            Partida partida = _dataForRead.HistorialPartidas[index];

            StringBuilder datosBuilder = new StringBuilder();
            datosBuilder.Append(partida.fecha);
            datosBuilder.Append("\nRonda: ");
            datosBuilder.Append(partida.ronda);
            datosBuilder.Append("\nTiempo: ");
            datosBuilder.AppendFormat("{0:0.00}", partida.tiempo);
            datosBuilder.Append("\nAciertos: ");
            datosBuilder.Append(partida.aciertos);
            datosBuilder.Append("\nFallos: ");
            datosBuilder.Append(partida.fallos);

            datosBuilder.Append("\nLanzamientos Derecha: ");
            datosBuilder.Append(partida.lanzamientosDer);
            datosBuilder.Append("\nAciertos Derecha: ");
            datosBuilder.Append(partida.aciertosDer);
            datosBuilder.Append("\nAciertos Derecha Cruzados: ");
            datosBuilder.Append(partida.aciertosDerCruzados);
            datosBuilder.Append("\nFallos Derecha: ");
            datosBuilder.Append(partida.fallosDer);

            datosBuilder.Append("\nLanzamientos Izquierda: ");
            datosBuilder.Append(partida.lanzamientosIzq);
            datosBuilder.Append("\nAciertos Izquierda: ");
            datosBuilder.Append(partida.aciertosIzq);
            datosBuilder.Append("\nAciertos Izquierda Cruzados: ");
            datosBuilder.Append(partida.aciertosIzqCruzados);
            datosBuilder.Append("\nFallos Izquierda: ");
            datosBuilder.Append(partida.fallosIzq);

            datosBuilder.Append("\nUsa Guía: ");
            datosBuilder.Append(partida.usaGuia ? "Sí" : "No");
            datosBuilder.Append("\nUsa ADD: ");
            datosBuilder.Append(partida.usaADD ? "Sí" : "No");
            datosBuilder.Append("\nDificultad: ");
            datosBuilder.AppendFormat("{0:0.00}", partida.dificultad);
            datosBuilder.Append("\nVelocidad: ");
            datosBuilder.AppendFormat("{0:0.00}", partida.velocidad);
            datosBuilder.Append("\nIntervalo: ");
            datosBuilder.AppendFormat("{0:0.00}", partida.intervalo);
            datosBuilder.Append("\nRango: ");
            datosBuilder.AppendFormat("{0:0.00}", partida.rango);
            datosBuilder.Append("\nTamaño: ");
            datosBuilder.AppendFormat("{0:0.00}", partida.tamano);
            datosBuilder.Append("\nIC: ");
            datosBuilder.AppendFormat("{0:0.00}", partida.IC);

            window.transform.Find("Base/DetailedDescription").GetComponent<Text>().text = datosBuilder.ToString();
        }
    }

    public void Continuar()
    {
        GameMaster.EscenaDeDondeVengo = "ResultsScene";
        if (GameMaster.Modo == GameMaster.ModoLogin.Paciente)
            SceneManager.LoadScene("MenuScene");
        else if (GameMaster.Modo == GameMaster.ModoLogin.Clinica)
            SceneManager.LoadScene("MenuScene");
        else if (GameMaster.Modo == GameMaster.ModoLogin.Terapeuta)
            SceneManager.LoadScene("TerapeutaScene");
    }
}