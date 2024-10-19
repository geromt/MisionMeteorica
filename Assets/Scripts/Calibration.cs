using TMPro;
using UnityEngine;

public class Calibration : MonoBehaviour
{
    public delegate void CalibrationEvent();
    public static event CalibrationEvent OnCalibrationEnd;

    [SerializeField] private TMP_Text instruccionesText;
    [SerializeField] private GameObject comenzarButton;

    private void OnEnable()
    {
        AdminController.OnCalibrationStart += StartCalibration;
        Button3DController.OnGameStartClick += EndCalibration;
    }

    private void OnDisable()
    {
        AdminController.OnCalibrationStart -= StartCalibration;
        Button3DController.OnGameStartClick -= EndCalibration;
    }

    void Awake()
    {
        comenzarButton.SetActive(false);
        instruccionesText.gameObject.SetActive(false);
    }

    private void StartCalibration()
    {
        comenzarButton.SetActive(true);
        instruccionesText.gameObject.SetActive(true);
        instruccionesText.text = "Aléjate de la cámara para que pueda captar todos tus movimientos.\n\nMueve tus brazos para comprobar que no desaparecen.";
    }

    public void EndCalibration()
    {
        comenzarButton.SetActive(false);
        instruccionesText.gameObject.SetActive(false);
        OnCalibrationEnd?.Invoke();
    }
}
