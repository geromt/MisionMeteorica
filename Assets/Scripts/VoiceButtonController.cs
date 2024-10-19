using UnityEngine;
using UnityEngine.UI;

public class VoiceButtonController : MonoBehaviour
{
    public delegate void VoiceButtonEvent();
    public static event VoiceButtonEvent VoiceOn;
    public static event VoiceButtonEvent VoiceOff;

    [SerializeField] private GameObject blockImage;
    [SerializeField] private Button voiceButton;


    private void Awake()
    {
        voiceButton.onClick.RemoveAllListeners();
        voiceButton.onClick.AddListener(ChangeVoiceState);

        if (GameMaster.IsVoiceActive)
            blockImage.SetActive(false);
        else
            blockImage.SetActive(true);
    }

    private void ChangeVoiceState()
    {
        if (GameMaster.IsVoiceActive)
        {
            VoiceOff?.Invoke();
            blockImage.SetActive(true);
            GameMaster.IsVoiceActive = false;
        }
        else
        {
            VoiceOn?.Invoke();
            blockImage.SetActive(false);
            GameMaster.IsVoiceActive = true;
        }
    }
}
