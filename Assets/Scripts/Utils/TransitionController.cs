using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TransitionController : MonoBehaviour
{
    public delegate void TransitionEvent();
    public static event TransitionEvent EndsInitTranstion;

    [SerializeField] private GameObject transitionPanel;
    [SerializeField] private float time = 1.5f;

    private Image _transitionImage;
    private Color _startTransitionColor;
    private Color _finalTransitionColor;

    private void Awake()
    {
        _finalTransitionColor = Color.black;
        _startTransitionColor = new Color(0, 0, 0, 0);
        _transitionImage = transitionPanel.GetComponent<Image>();
        transitionPanel.SetActive(false);
        _transitionImage.color = _startTransitionColor;
    }

    public void PlayFinalTransition(string nextScene)
    {
        transitionPanel.SetActive(true);
        LeanTween.value(gameObject, AssingColor, _startTransitionColor, _finalTransitionColor, time).setEase(LeanTweenType.easeInCubic).setOnComplete(
            () =>
            {
                if (string.IsNullOrEmpty(nextScene))
                    nextScene = GameMaster.EscenaDeDondeVengo;
                GameMaster.EscenaDeDondeVengo = SceneManager.GetActiveScene().name;
                SceneManager.LoadScene(nextScene);
            }
        );
    }

    public void PlayInitTransition()
    {
        transitionPanel.SetActive(true);
        _transitionImage.color = _finalTransitionColor;

        LeanTween.value(gameObject, AssingColor, _finalTransitionColor, _startTransitionColor, time).setEase(LeanTweenType.easeOutCubic).setOnComplete(
            () =>
            {
                transitionPanel.SetActive(false);
                if (GameMaster.Modo != GameMaster.ModoLogin.Terapeuta)
                    EndsInitTranstion?.Invoke();
            }
        );
    }

    private void AssingColor(Color c)
    {
        _transitionImage.color = c;
    }
}
