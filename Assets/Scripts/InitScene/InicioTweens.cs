using TMPro;
using UnityEngine;

public class InicioTweens : MonoBehaviour
{
    [SerializeField] private TransitionController transition;

    [SerializeField] private GameObject inicioButton;
    [SerializeField] private GameObject earth;

    [Header("Panel Login Paciente")]
    [SerializeField] private GameObject autenticaPacientePanel;
    [SerializeField] private TMP_InputField idPacienteInput;

    [Header("Panel Login Terapeuta")]
    [SerializeField] private GameObject autenticaTerapeutaPanel;
    [SerializeField] private TMP_InputField emailTerapeutaInput;
    [SerializeField] private GameObject preguntaAsistidoPanel;

    [Header("Mensajes")]
    [SerializeField] private GameObject sinInternetPanel;
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TMP_Text messageText;


    private void OnEnable()
    {
        Admin_level0.ShowErrorMessage += ShowMessage;
        Admin_level0.AskContinueWithoutInternet += ShowSinInternetPanel;
        Admin_level0.SuccessUserLogIn += ContinuarAutenticaPacientePanel;
        Admin_level0.SuccessTerapeutaLogInClinica += GoToClinica;
        Admin_level0.SuccessTerapeutaLogInTerapeuta += GoToTerapeuta;
    }

    private void OnDisable()
    {
        Admin_level0.ShowErrorMessage -= ShowMessage;
        Admin_level0.AskContinueWithoutInternet -= ShowSinInternetPanel;
        Admin_level0.SuccessUserLogIn -= ContinuarAutenticaPacientePanel;
        Admin_level0.SuccessTerapeutaLogInClinica -= GoToClinica;
        Admin_level0.SuccessTerapeutaLogInTerapeuta -= GoToTerapeuta;
    }

    private void Awake()
    {
        inicioButton.transform.localScale = new Vector3(1, 0, 1);
        ConfigurePanel(inicioButton);
        ConfigurePanel(autenticaPacientePanel, false);
        ConfigurePanel(autenticaTerapeutaPanel, false);
        ConfigurePanel(preguntaAsistidoPanel, false);
        ConfigurePanel(messagePanel, false);
        ConfigurePanel(sinInternetPanel, false);
    }

    private void Start()
    {
        LeanTween.scaleY(inicioButton, 1, 0.5f).setDelay(1f).setEase(LeanTweenType.easeInOutBounce);
    }

    public void HideEarth()
    {
        LeanTween.moveLocalY(earth, earth.transform.position.y - 1, 1f).setEase(LeanTweenType.easeInCirc);
    }

    public void ShowAutenticaPacientePanel()
    {
        ChangePanel(inicioButton, autenticaPacientePanel, idPacienteInput);
    }

    public void RegresarAutenticaPacientePanel()
    {
        ChangePanel(autenticaPacientePanel, inicioButton);
    }

    public void ContinuarAutenticaPacientePanel()
    {
        ChangePanel(autenticaPacientePanel, preguntaAsistidoPanel);
    }

    public void ShowAutenticaTerapeutaPanel()
    {
        ChangePanel(preguntaAsistidoPanel, autenticaTerapeutaPanel, emailTerapeutaInput);
    }

    public void HidePreguntaAsistidoPanel()
    {
        HidePanel(preguntaAsistidoPanel);
    }

    public void ShowSinInternetPanel()
    {
        ChangePanel(autenticaPacientePanel, sinInternetPanel);
    }

    public void ContinuarSinInternetPanel()
    {
        ChangePanel(sinInternetPanel, preguntaAsistidoPanel);
    }

    public void RegresarSinInternetPanel()
    {
        ChangePanel(sinInternetPanel, inicioButton);
    }

    public void GoToClinica()
    {
        GoToScene("MenuScene", autenticaTerapeutaPanel);
    }

    public void GoToTerapeuta()
    {
        GoToScene("TerapeutaScene", autenticaPacientePanel);
    }

    public void GoToScene(string scene, GameObject panelToHide)
    {
        HidePanel(panelToHide);
        HideEarth();
        HidePanel(messagePanel, 0.2f);
        transition.PlayFinalTransition(scene);
    }

    public void RegresarAutenticaTerapeutaPanel()
    {
        ChangePanel(autenticaTerapeutaPanel, preguntaAsistidoPanel);
    }

    private void ConfigurePanel(GameObject panel, bool setActive = true)
    {
        panel.SetActive(setActive);
        panel.transform.localScale = new Vector3(1, 0, 1);
    }

    private void ChangePanel(GameObject panel1, GameObject panel2, TMP_InputField activeInput = null)
    {
        if (GameMaster.CalidadGraficos == Graficos.Altos)
        {
            LeanTween.scaleY(messagePanel, 0, 0.2f).setEase(LeanTweenType.easeInBounce).setOnComplete(
                () => messagePanel.SetActive(false)
            );
            LeanTween.scaleY(panel1, 0, 0.5f).setEase(LeanTweenType.easeInBounce).setOnComplete(
                () =>
                {
                    panel1.SetActive(false);
                    panel2.SetActive(true);
                    LeanTween.scaleY(panel2, 1, 0.5f).setEase(LeanTweenType.easeOutBounce).setDelay(0.5f);
                    if (!(activeInput is null))
                    {
                        activeInput.Select();
                        activeInput.ActivateInputField();
                    }
                }
            );
        }
        else if (GameMaster.CalidadGraficos == Graficos.Bajos)
        {
            var messageScale = messagePanel.transform.localScale;
            messageScale.y = 0;
            messagePanel.transform.localScale = messageScale;
            messagePanel.SetActive(false);

            var panel1Scale = panel1.transform.localScale;
            panel1Scale.y = 0;
            panel1.transform.localScale = panel1Scale;
            panel1.SetActive(false);

            panel2.SetActive(true);
            var panel2Scale = panel2.transform.localScale;
            panel2Scale.y = 1;
            panel2.transform.localScale = panel2Scale;
            if (!(activeInput is null))
            {
                activeInput.Select();
                activeInput.ActivateInputField();
            }
        }
    }

    private void HidePanel(GameObject panel1, float time = 0.5f)
    {
        LeanTween.scaleY(panel1, 0, time).setEase(LeanTweenType.easeInBounce);
    }

    private void ShowMessage(string message)
    {
        if (messagePanel.activeSelf)
        {
            LeanTween.cancel(messagePanel);
            LeanTween.scaleY(messagePanel, 0, 0.2f).setEase(LeanTweenType.easeInBounce).setOnComplete(
                () =>
                {
                    messageText.text = message;
                    LeanTween.scaleY(messagePanel, 1, 0.2f).setEase(LeanTweenType.easeInOutBounce);
                }
            );
        }
        else
        {
            messagePanel.SetActive(true);
            messageText.text = message;
            LeanTween.scaleY(messagePanel, 1, 0.2f).setEase(LeanTweenType.easeInOutBounce);
        }
    }
}
