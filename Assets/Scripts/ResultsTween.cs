using UnityEngine;

public class ResultsTween : MonoBehaviour
{
    [SerializeField] private GameObject graphPanel;
    [SerializeField] private GameObject continuarPanel;

    private void OnEnable()
    {
        TransitionController.EndsInitTranstion += ShowPanels;
    }

    private void OnDisable()
    {
        TransitionController.EndsInitTranstion -= ShowPanels;
    }

    private void Awake()
    {
        if (GameMaster.Modo != GameMaster.ModoLogin.Terapeuta)
        {
            graphPanel.transform.localScale = new Vector3(1, 0, 1);
            continuarPanel.transform.localScale = new Vector3(1, 0, 1);
        }
        else
        {
            graphPanel.transform.localScale = new Vector3(1, 1, 1);
            continuarPanel.transform.localScale = new Vector3(1, 1, 1);
        }

    }

    private void ShowPanels()
    {
        LeanTween.scaleY(continuarPanel, 1, 0.5f).setDelay(0.5f).setEase(LeanTweenType.easeOutBounce);
        LeanTween.scaleY(graphPanel, 1, 0.5f).setEase(LeanTweenType.easeOutBounce);
    }
}
