using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialController : MonoBehaviour
{
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TMP_Text instruccionesText;
    [SerializeField] private GameObject puntuacionCircle;
    [SerializeField] private GameObject nextInstruccion3DButton;
    [SerializeField] private GeneratorController gc;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] instructionsVoices;

    [SerializeField] private Animator botAnimator;
    [SerializeField] private GameObject cuartoTutorial;

    [Header("Cameras")]
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject roomCamera;
    [SerializeField] private Transform cameraPos2;
    [SerializeField] private Transform cameraPos3;

    private int _instruccion = 0;
    private bool _didCatchAsteroid = false;
    private bool _deactivateNextInstruccion = false;

    private void OnEnable()
    {
        AdminController.OnTutorialStart += StartTutorial;
        AsteroidController.OnCollideHand += ProcessCollideHand;
        AsteroidController.OnAvoidHand += GoToNextInstruction;
    }

    private void OnDisable()
    {
        AdminController.OnTutorialStart -= StartTutorial;
        AsteroidController.OnCollideHand -= ProcessCollideHand;
        AsteroidController.OnAvoidHand -= GoToNextInstruction;
    }

    private void Awake()
    {
        nextInstruccion3DButton.GetComponent<Button3DController>().otherMethod = () => GoToNextInstruction();
        roomCamera.SetActive(false);
        mainCamera.SetActive(true);
        cuartoTutorial.SetActive(false);
    }

    void Start()
    {
        tutorialPanel.SetActive(false);
        puntuacionCircle.SetActive(false);
        nextInstruccion3DButton.SetActive(false);
    }

    private void Update()
    {
        if (_deactivateNextInstruccion)
            return;

        if (Input.anyKeyDown)
            GoToNextInstruction();
    }

    private void StartTutorial()
    {
        StartCoroutine(TutorialCoroutine());
    }

    public void GoToNextInstruction()
    {
        _instruccion++;
    }

    private void ProcessCollideHand()
    {
        _didCatchAsteroid = true;
        GoToNextInstruction();
    }

    private IEnumerator TutorialCoroutine()
    {
        tutorialPanel.SetActive(true);
        nextInstruccion3DButton.SetActive(true);
        cuartoTutorial.SetActive(true);

        List<Instruccion> instruccions = new List<Instruccion>
        {
            new Instruccion(
                "Bienvenido al tutorial de Misión Meteórica\n\nPara ir a la siguiente instrucción presiona cualquier tecla, o toca el botón con la flecha",
                instructionsVoices[0],
                1
            ),
            new Instruccion(
                "Aquí te enseñaremos cómo jugar Misión Meteórica",
                instructionsVoices[1],
                1
            ),
            new Instruccion(
                "Primero te mostraremos las partes del juego",
                instructionsVoices[2],
                1
            ),
            new Instruccion(
                "En la parte de arriba podrás ver el número de aciertos y fallos de la ronda actual, así como el número de rondas que has jugado",
                instructionsVoices[3],
                1,
                () =>
                {
                    puntuacionCircle.SetActive(true);
                    LeanTween.scale(puntuacionCircle, new Vector3(1.1f, 1.1f, 1.1f), 0.5f).setLoopPingPong();
                }
            ),
            new Instruccion(
                "Muy bien (^_^)\n\nAhora veamos cómo se juega",
                instructionsVoices[4],
                1,
                () => puntuacionCircle.SetActive(false)
            ),
            new Instruccion(
                "Primero, deberás alejarte un poco de la cámara para que pueda ver todos tus movimientos.\n\nPrueba alejarte entre 1 y 2 metros",
                instructionsVoices[5],
                1,
                () =>
                {
                    mainCamera.SetActive(false);
                    roomCamera.SetActive(true);
                }
            ),
            new Instruccion(
                "Siéntate al centro de la cámara y siempre intenta mantener la espalda recta",
                instructionsVoices[6],
                1,
                () => botAnimator.SetTrigger("SentarTrigger"),
                3
            ),
            new Instruccion(
                "Para jugar debes usar tus brazos, pero intenta mover sólo uno a la vez",
                instructionsVoices[7],
                1,
                () =>
                {
                    botAnimator.SetTrigger("MoverManosTrigger");
                    LeanTween.move(roomCamera, cameraPos2.position, 3);
                    LeanTween.rotate(roomCamera, cameraPos2.eulerAngles, 3);
                },
                6
            ),
            new Instruccion(
                "Cuando no muevas los brazos descánsalos sobre tus piernas",
                instructionsVoices[20],
                1,
                esperaDurante: 6
            ),
            new Instruccion(
                "Y recuerda siempre mantener la espalda recta",
                instructionsVoices[8],
                1,
                () =>
                {
                    LeanTween.move(roomCamera, cameraPos3.position, 3);
                    LeanTween.rotate(roomCamera, cameraPos3.eulerAngles, 3);
                },
                6
            ),
            new Instruccion(
                "Ahora probemos un poco el juego",
                instructionsVoices[9],
                1,
                () =>
                {
                    mainCamera.SetActive(true);
                    roomCamera.SetActive(false);
                }
            ),
            new Instruccion(
                "Seguro ya viste tus brazos en la pantalla\n\nIntenta moverlos lo más que puedas para que te acostumbres",
                instructionsVoices[10],
                1
            ),
            new Instruccion(
                "Deberás usar tus brazos para atrapar los asteroides que vienen hacia ti y evitar que choquen con la Tierra\n\n¡¡Tú puedes!!",
                instructionsVoices[11],
                1,
                () => GameMaster.SetDifficultyVals(0, 0, 2, 1)
            )
        };

        List<Instruccion> instruccions2 = new List<Instruccion>
        {
            new Instruccion(
                "¡¡Buen trabajo!!\n\nCada ronda consta de 10 asteroides, intenta atrapar todos los que puedas",
                instructionsVoices[16],
                1
            ),
            new Instruccion(
                "No te desanimes si no los atrapas todos, puedes seguir intentándolo.",
                instructionsVoices[17],
                1
            ),
            new Instruccion(
                "Después de cada ronda se te preguntará si quieres jugar otra vez o si quieres terminar de jugar.",
                instructionsVoices[18],
                1
            ),
            new Instruccion(
                "Eso es todo lo que necesitas saber para jugar Misión Meteórica\n\nDiviértete completando la misión.",
                instructionsVoices[19],
                1
            )
        };

        yield return IterateInstrutionList(instruccions);

        int waitInst;
        while (true)
        {
            waitInst = _instruccion;
            nextInstruccion3DButton.SetActive(false);
            _deactivateNextInstruccion = true;
            instruccionesText.text = "¡¡Ahí viene un asteroide!!\n\nIntenta detenerlo con tus manos";
            PlayVoice(instructionsVoices[12]);
            GameMaster.AreGuidesActive = false;
            gc.LaunchBall(0, 1.46f);
            yield return new WaitUntil(() => _instruccion >= waitInst + 1);

            nextInstruccion3DButton.SetActive(true);
            _deactivateNextInstruccion = false;

            if (_didCatchAsteroid)
            {
                instruccionesText.text = "¡¡Muy bien!!\n\nLo has conseguido";
                PlayVoice(instructionsVoices[13]);
                yield return new WaitUntil(() => _instruccion >= waitInst + 2);
                break;
            }
            else
            {
                instruccionesText.text = "No te desanimes, inténtalo otra vez";
                PlayVoice(instructionsVoices[14]);
                yield return new WaitUntil(() => _instruccion >= waitInst + 2);
                _instruccion = waitInst;
            }
        }

        instruccionesText.text = "¡¡Ahí vienen más asteroides!!\n\nIntenta detenerlos";
        PlayVoice(instructionsVoices[15]);
        nextInstruccion3DButton.SetActive(false);
        _deactivateNextInstruccion = true;
        waitInst = _instruccion;
        for (int i = 0; i < 3; i++)
        {
            gc.LaunchBall(-0.25f + (0.25f * i), 1.46f);
            yield return new WaitUntil(() => _instruccion >= waitInst + i + 1);
        }
        nextInstruccion3DButton.SetActive(true);
        _deactivateNextInstruccion = false;

        yield return IterateInstrutionList(instruccions2);

        SceneManager.LoadScene("MenuScene");
    }

    private IEnumerator IterateInstrutionList(List<Instruccion> instruccions)
    {
        int waitInst;
        foreach (Instruccion inst in instruccions)
        {
            instruccionesText.text = inst.Text;
            PlayVoice(inst.Voice);
            inst.Accion?.Invoke();
            waitInst = _instruccion;
            if (inst.EsperaDurante > 0)
                yield return new WaitForSeconds(inst.EsperaDurante);
            yield return new WaitUntil(() => _instruccion >= waitInst + inst.EsperaInstrucciones);
            _instruccion = waitInst + inst.EsperaInstrucciones;
        }
    }

    private void PlayVoice(AudioClip voice)
    {
        if (GameMaster.IsVoiceActive)
        {
            if (audioSource.isPlaying)
                audioSource.Stop();

            audioSource.clip = voice;
            audioSource.Play();
        }
    }
}

public class Instruccion 
{
    public Instruccion(string text, AudioClip voice, int espera, Action action=null, int esperaDurante=0)
    {
        Text = text;
        Voice = voice;
        EsperaInstrucciones = espera;
        EsperaDurante = esperaDurante;
        Accion = action;
    }

    public string Text { get; }
    public AudioClip Voice { get; }
    public int EsperaInstrucciones { get; }
    public int EsperaDurante { get; }
    public Action Accion { get; }
}
