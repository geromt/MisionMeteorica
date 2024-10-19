using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioSource soundsAudioSource;
    [SerializeField] private AudioSource voicesAudioSource;

    [Header("Music")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip secondaryMenuMusic;
    [SerializeField] private AudioClip gameMusic;

    [Header("Effects")]
    [SerializeField] private AudioClip buttonClip;
    [SerializeField] private AudioClip spawnAsteroidClip;
    [SerializeField] private AudioClip collisionHandClip;
    [SerializeField] private AudioClip evadeHandClip;

    [Header("Menu Voices")]
    [SerializeField] private AudioClip preguntaAcompanadoClip;
    [SerializeField] private AudioClip bienvenidoClip;
    [SerializeField] private AudioClip alejateCamaraClip;
    [SerializeField] private AudioClip preguntaSalirClip;

    [Header("Conecta Sensores Voices")]
    [SerializeField] private AudioClip conectarDerechoClip;
    [SerializeField] private AudioClip conectarIzquierdoClip;
    [SerializeField] private AudioClip sensoresListosClip;
    [SerializeField] private AudioClip noPudimosConectarClip;

    [Header("Feedback Voices")]
    [Tooltip("Orden: ¡No te rindas!, ¡Tú puedes!, ¡No te desanimes!")]
    [SerializeField] private AudioClip[] vocesAnimo;
    [Tooltip("Orden: ¡Sigue así!, ¡Continúa así!, ¡Bien hecho!")]
    [SerializeField] private AudioClip[] vocesSeguir;
    [Tooltip("Orden: ¡Lo has hecho muy bien!, ¡Muy bien hecho!, ¡Excelente!")]
    [SerializeField] private AudioClip[] vocesFelicitaciones;

    private bool _isSoundOn = true;

    private static AudioController audioControllerInstance;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnLoadScene;
        ButtonTween.EnterButton += PlayButtonClip;
        GeneratorController.OnAsteroidSpawn += PlaySpawnAsteroidClip;
        AsteroidController.OnCollideHand += PlayCollisionHandClip;
        AsteroidController.OnAvoidHand += PlayEvadeHandClip;
        AudioButtonController.AudioOn += TurnSoundOn;
        AudioButtonController.AudioOff += TurnSoundOff;

        FeedbackController.OnMessageSelected += PlayFeedbackVoice;
        Admin_level0.SuccessUserLogIn += PlayPreguntaAcompanadoVoice;
        MenuController.OnShowBienvenidaPanel += PlayBienvenidoVoice;
        PanelSalirController.OnShowConfirmaSalirPanel += PlayPreguntaSalirVoice;
        AdminController.OnCalibrationStart += PlayAlejateCamaraVoice;

        ConnectSensorsController.OnTryingToConnectRight += PlayConectaDerechoVoice;
        ConnectSensorsController.OnTryingToConnectLeft += PlayConectaIzquierdoVoice;
        ConnectSensorsController.OnSuccesfulyConnectBoth += PlaySensoresListosVoice;
        ConnectSensorsController.OnFailToConnect += PlayNoPudimosConectarVoice;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLoadScene;
        ButtonTween.EnterButton -= PlayButtonClip;
        GeneratorController.OnAsteroidSpawn -= PlaySpawnAsteroidClip;
        AsteroidController.OnCollideHand -= PlayCollisionHandClip;
        AsteroidController.OnAvoidHand -= PlayEvadeHandClip;
        AudioButtonController.AudioOn -= TurnSoundOn;
        AudioButtonController.AudioOff -= TurnSoundOff;

        FeedbackController.OnMessageSelected -= PlayFeedbackVoice;
        Admin_level0.SuccessUserLogIn -= PlayPreguntaAcompanadoVoice;
        MenuController.OnShowBienvenidaPanel -= PlayBienvenidoVoice;
        PanelSalirController.OnShowConfirmaSalirPanel -= PlayPreguntaSalirVoice;
        AdminController.OnCalibrationStart -= PlayAlejateCamaraVoice;

        ConnectSensorsController.OnTryingToConnectRight -= PlayConectaDerechoVoice;
        ConnectSensorsController.OnTryingToConnectLeft -= PlayConectaIzquierdoVoice;
        ConnectSensorsController.OnSuccesfulyConnectBoth -= PlaySensoresListosVoice;
        ConnectSensorsController.OnFailToConnect -= PlayNoPudimosConectarVoice;
    }

    private void Awake()
    {
        DontDestroyOnLoad(transform);

        if (audioControllerInstance == null)
            audioControllerInstance = this;
        else
            Destroy(gameObject);
    }

    private void TurnSoundOn()
    {
        _isSoundOn = true;
        soundsAudioSource.Play();
    }

    private void TurnSoundOff()
    {
        _isSoundOn = false;
        soundsAudioSource.Pause();
    }

    private void OnLoadScene(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "InitScene":
            case "MenuScene":
                PlayMusic(menuMusic);
                break;
            case "GameScene":
                PlayMusic(gameMusic);
                break;
            default:
                PlayMusic(secondaryMenuMusic);
                break;
        }

        soundsAudioSource.pitch = 1f;
        soundsAudioSource.loop = true;
    }

    private void PlayMusic(AudioClip music)
    {
        if (soundsAudioSource.clip == music)
            return;

        soundsAudioSource.Stop();
        soundsAudioSource.clip = music;
        if (_isSoundOn)
            soundsAudioSource.Play();
    }

    private void SwitchAudioState()
    {
        if (_isSoundOn)
        {
            soundsAudioSource.Pause();
            _isSoundOn = false;
        }
        else
        {
            soundsAudioSource.Play();
            _isSoundOn = true;
        }
    }

    private void PlayClip(AudioClip clip)
    {
        if (_isSoundOn)
        {
            soundsAudioSource.PlayOneShot(clip);
        }
    }

    private void PlaySpawnAsteroidClip(Mano side)
    {
        PlayClip(spawnAsteroidClip);
    }

    private void PlayCollisionHandClip()
    {
        PlayClip(collisionHandClip);
    }

    private void PlayEvadeHandClip()
    {
        PlayClip(evadeHandClip);
    }

    private void PlayButtonClip()
    {
        PlayClip(buttonClip);
    }

    private void PlayVoice(AudioClip voice)
    {
        if (GameMaster.IsVoiceActive)
        {
            if (voicesAudioSource.isPlaying)
                voicesAudioSource.Stop();

            voicesAudioSource.clip = voice;
            voicesAudioSource.Play();
        }

    }

    private void PlayFeedbackVoice(int aciertos, int clipIndex)
    {
        if (!GameMaster.IsVoiceActive) return;
        if (aciertos < 5)
            PlayVoice(vocesAnimo[clipIndex]);
        else if (aciertos > 7)
            PlayVoice(vocesFelicitaciones[clipIndex]);
        else
            PlayVoice(vocesSeguir[clipIndex]);
    }

    private void PlayPreguntaAcompanadoVoice()
    {
        PlayVoice(preguntaAcompanadoClip);
    }

    private void PlayBienvenidoVoice()
    {
        PlayVoice(bienvenidoClip);
    }

    private void PlayConectaDerechoVoice()
    {
        PlayVoice(conectarDerechoClip);
    }

    private void PlayConectaIzquierdoVoice()
    {
        PlayVoice(conectarIzquierdoClip);
    }

    private void PlaySensoresListosVoice()
    {
        PlayVoice(sensoresListosClip);
    }

    private void PlayNoPudimosConectarVoice()
    {
        PlayVoice(noPudimosConectarClip);
    }

    private void PlayAlejateCamaraVoice()
    {
        PlayVoice(alejateCamaraClip);
    }

    private void PlayPreguntaSalirVoice()
    {
        PlayVoice(preguntaSalirClip);
    }
}
