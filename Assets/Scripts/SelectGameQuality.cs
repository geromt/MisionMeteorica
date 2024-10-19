using UnityEngine;
using UnityEngine.UI;

public class SelectGameQuality : MonoBehaviour
{
    public delegate void SelectQualityEvent();
    public static event SelectQualityEvent ChangeQuality;

    [SerializeField] private Button selecCalidadButton;
    [SerializeField] private Button calidadBajaButton;
    [SerializeField] private Button calidadAltaButton;
    [SerializeField] private GameObject seleccionaCalidadPanel;

    private void Awake()
    {
        seleccionaCalidadPanel.SetActive(false);

        selecCalidadButton.onClick.AddListener(delegate { seleccionaCalidadPanel.SetActive(true); });

        calidadBajaButton.onClick.AddListener(delegate
        {
            #if UNITY_EDITOR
            Debug.Log("Calidad de graficos: Baja");
            #endif
            GameMaster.CalidadGraficos = Graficos.Bajos;
            ChangeQuality?.Invoke();
            seleccionaCalidadPanel.SetActive(false);
            PlayerPrefs.SetInt("Quality", (int)Graficos.Bajos);
            PlayerPrefs.Save();
        });

        calidadAltaButton.onClick.AddListener(delegate
        {
            #if UNITY_EDITOR
            Debug.Log("Calidad de graficos: Alta");
            #endif
            GameMaster.CalidadGraficos = Graficos.Altos;
            ChangeQuality?.Invoke();
            seleccionaCalidadPanel.SetActive(false);
            PlayerPrefs.SetInt("Quality", (int)Graficos.Altos);
            PlayerPrefs.Save();
        });
    }
}
