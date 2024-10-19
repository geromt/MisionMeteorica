using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EncuestaController : MonoBehaviour
{
    [Header("Panel de opciones")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private Toggle isTherapistToggle;
    [SerializeField] private Toggle didUseSensorsToggle;
    [SerializeField] private Button continuarButton;

    [Header("Panel encuesta")]
    [SerializeField] private GameObject encuestaPanel;
    [SerializeField] private Transform sliderContent;
    [SerializeField] private GameObject sectionPrefab;
    [SerializeField] private GameObject preguntaLikertPrefab;
    [SerializeField] private GameObject preguntaAbiertaPrefab;
    [SerializeField] private GameObject buttonsPrefab;
    [SerializeField] private GameObject contestePreguntasPanel;
    [SerializeField] private GameObject confirmarSalirPanel;
    [SerializeField] private GameObject confirmarTerminarPanel;


    private List<SectionData> _sections;
    private List<LikertController> _preguntasLikertPaneles;
    private List<GameObject> _preguntasAbiertasPaneles;

    private bool _didUseSensor = false;
    private bool _isTherapist = true;

    private const string JSONPath = "D:\\Gerardo\\PoseTrackerTest\\Assets\\Encuesta\\datos-preguntas.json";
    private string _CSVPath = Path.Combine(GameMaster.rutaDeArchivos, "encuesta_data.txt");

    private void Awake()
    {
        encuestaPanel.SetActive(false);
        optionsPanel.SetActive(true);
        contestePreguntasPanel.SetActive(false);
        continuarButton.onClick.AddListener(ConfigureEncuesta);

        GetSectionDatas();
    }


    /// <summary>
    /// Deserializa el JSON con los datos de las preguntas y lo guarda en _sections
    /// </summary>
    private void GetSectionDatas()
    {
        using (FileStream r = File.OpenRead(JSONPath))
        {
            DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(List<SectionData>));
            _sections = (List<SectionData>)deserializer.ReadObject(r);
        }
    }

    /// <summary>
    /// Crea los paneles de las secciones y preguntas de acuerdo a las opciones del panel optionsPanel
    /// </summary>
    private void ConfigureEncuesta()
    {
        _isTherapist = isTherapistToggle.isOn;
        _didUseSensor = didUseSensorsToggle.isOn;

        _preguntasLikertPaneles = new List<LikertController>();
        _preguntasAbiertasPaneles = new List<GameObject>();

        foreach (SectionData section in _sections)
        {
            var sectionPanel = Instantiate(sectionPrefab, sliderContent);
            sectionPanel.transform.GetChild(0).GetComponent<Text>().text = section.Title;

            foreach (QuestionData pregunta in section.Questions)
            {
                if ((!_isTherapist && pregunta.IsTherapistQuestion) || (!_didUseSensor && pregunta.IsSensorQuestion))
                {
                    if (pregunta.Type == QuestionType.Likert)
                        _preguntasLikertPaneles.Add(null);
                    else
                        _preguntasAbiertasPaneles.Add(null);

                    continue;
                }

                if (pregunta.Type == QuestionType.Open)
                {
                    var panel = Instantiate(preguntaAbiertaPrefab, sectionPanel.transform);
                    _preguntasAbiertasPaneles.Add(panel);
                    panel.transform.GetChild(0).GetComponent<Text>().text = pregunta.Text;
                }
                else if (pregunta.Type == QuestionType.Likert)
                {
                    var likertPanel = Instantiate(preguntaLikertPrefab, sectionPanel.transform);
                    _preguntasLikertPaneles.Add(likertPanel.GetComponent<LikertController>());
                    likertPanel.transform.GetChild(0).GetComponent<Text>().text = pregunta.Text;
                }
            }
        }

        var buttonsPanel = Instantiate(buttonsPrefab, sliderContent);
        buttonsPanel.transform.GetChild(0).GetChild(0).GetComponent<Button>().onClick.AddListener(delegate { confirmarSalirPanel.SetActive(true); });
        buttonsPanel.transform.GetChild(0).GetChild(1).GetComponent<Button>().onClick.AddListener(CheckAnswers);
        optionsPanel.SetActive(false);
        encuestaPanel.SetActive(true);
    }

    /// <summary>
    /// Obtiene los valores de las preguntas y los guarda en un archivo CSV separado por comas
    /// </summary>
    public void RetriveValues()
    {
        StringBuilder sb = new StringBuilder();

        foreach (LikertController l in _preguntasLikertPaneles)
        {
            if (l == null)
            {
                sb.Append("-1");
                sb.Append(',');
                continue;
            }

            var val = l.Value;
            sb.Append(val);
            sb.Append(',');
        }

        foreach (GameObject p in _preguntasAbiertasPaneles)
        {
            if (p == null)
            {
                sb.Append("*");
                sb.Append(',');
                continue;
            }

            var val = p.transform.GetChild(1).GetComponent<InputField>().text;
            val = val.Replace(',', ' ');

            // Reemplazamos los saltos de linea, todos los posibles
            string lineSeparator = ((char)0x2028).ToString();
            string paragraphSeparator = ((char)0x2029).ToString();

            val = val.Replace("\r\n", " ")
                     .Replace("\n", " ")
                     .Replace("\r", " ")
                     .Replace(lineSeparator, " ")
                     .Replace(paragraphSeparator, " ");

            if (string.IsNullOrEmpty(val))
            {
                contestePreguntasPanel.SetActive(true);
                return;
            }
            sb.Append(val);
            sb.Append(',');
        }

        using (var encuestaWriter = File.AppendText(_CSVPath))
        {
            encuestaWriter.WriteLine(sb.ToString());
        }
        Return();
    }

    /// <summary>
    /// Revisa que todas las preguntas se hayan contestado
    /// </summary>
    public void CheckAnswers()
    {
        foreach (LikertController l in _preguntasLikertPaneles)
        {
            if (l == null) continue;

            var val = l.Value;
            if (val == -1)
            {
                contestePreguntasPanel.SetActive(true);
                return;
            }
        }

        foreach (GameObject p in _preguntasAbiertasPaneles)
        {
            if (p == null) continue;

            var val = p.transform.GetChild(1).GetComponent<InputField>().text;

            if (string.IsNullOrEmpty(val))
            {
                contestePreguntasPanel.SetActive(true);
                return;
            }
        }

        confirmarTerminarPanel.SetActive(true);
    }

    public void Return()
    {
        SceneManager.LoadScene("InitScene");
    }
}

public enum QuestionType { Likert, Open }

public class QuestionData
{
    public QuestionType Type { get; set; }
    public string Text { get; set; }
    public bool IsSensorQuestion { get; set; }
    public bool IsTherapistQuestion { get; set; }
}

public class SectionData
{
    public string Title { get; set; }
    public List<QuestionData> Questions { get; set; }
}