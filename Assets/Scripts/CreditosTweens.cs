using UnityEngine;

public class CreditosTweens : MonoBehaviour
{
    [SerializeField] private GameObject creditosPanel;
    [SerializeField] private GameObject regresarPanel;
    [SerializeField] private GameObject marte;

    private void OnEnable()
    {
        TransitionController.EndsInitTranstion += ShowPanels;
    }

    private void OnDisable()
    {
        TransitionController.EndsInitTranstion -= ShowPanels;
    }

    void Start()
    {
        marte.transform.localPosition += new Vector3(0, -1, 0);
        GetComponent<TransitionController>().PlayInitTransition();
    }

    private void ShowPanels()
    {
        LeanTween.moveLocalY(marte, -0.05f, 1f).setEase(LeanTweenType.easeOutCirc);
    }

    public void HidePanels()
    {
        LeanTween.scaleY(creditosPanel, 0, 0.5f).setDelay(0.5f).setEase(LeanTweenType.easeInBounce);
        LeanTween.scaleY(regresarPanel, 0, 0.5f).setDelay(1).setEase(LeanTweenType.easeInBounce);
        LeanTween.moveLocalY(marte, -1.05f, 1f).setEase(LeanTweenType.easeInCirc);
        GetComponent<TransitionController>().PlayFinalTransition("InitScene");
    }
}
