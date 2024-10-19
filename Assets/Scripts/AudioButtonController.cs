using UnityEngine;
using UnityEngine.UI;

public class AudioButtonController : MonoBehaviour
{
    public delegate void AudioButtonEvent();
    public static event AudioButtonEvent AudioOn;
    public static event AudioButtonEvent AudioOff;

    [SerializeField] private Sprite audioOnSprite;
    [SerializeField] private Sprite audioOffSprite;

    private Button _audioButton;
    private Image _buttonImage;
    private static bool _isOn = true;

    private void Awake()
    {
        _audioButton = transform.GetChild(0).GetComponent<Button>();
        _audioButton.onClick.RemoveAllListeners();
        _audioButton.onClick.AddListener(OnClickAudioButton);

        _buttonImage = _audioButton.transform.GetChild(0).GetComponent<Image>();
        if (_isOn)
            _buttonImage.sprite = audioOnSprite;
        else
            _buttonImage.sprite = audioOffSprite;
    }

    private void OnClickAudioButton()
    {
        if (_isOn)
        {
            AudioOff?.Invoke();
            _buttonImage.sprite = audioOffSprite;
            _isOn = false;
        }
        else
        {
            AudioOn?.Invoke();
            _buttonImage.sprite = audioOnSprite;
            _isOn = true;
        }
    }
}
