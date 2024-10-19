using UnityEngine;
using UnityEngine.UI;

public class PreguntasAdmin : MonoBehaviour
{
    [SerializeField] private GameObject questionsPanel;
    [SerializeField] private Button[] questionButtons;
    [SerializeField] private GameObject[] answerPanels;
    [SerializeField] private Button returnButton;

    private void Awake()
    {
        returnButton.gameObject.SetActive(false);
        for (int i = 0; i < questionButtons.Length; i++)
        {
            var i1 = i;

            answerPanels[i].SetActive(false);
            questionButtons[i].onClick.RemoveAllListeners();
            questionButtons[i].onClick.AddListener(() => ShowAnwser(answerPanels[i1]));
        }
    }

    private void Start()
    {
        GetComponent<TransitionController>().PlayInitTransition();
        questionsPanel.SetActive(true);
    }

    private void ShowAnwser(GameObject answerPanel)
    {
        questionsPanel.SetActive(false);
        answerPanel.SetActive(true);

        answerPanel.transform.GetChild(0).GetChild(1).GetComponent<Scrollbar>().value = 1;

        returnButton.onClick.RemoveAllListeners();
        returnButton.onClick.AddListener(delegate { ReturnToQuestionsPanel(answerPanel); });
        returnButton.gameObject.SetActive(true);
    }

    public void ReturnToQuestionsPanel(GameObject answerPanel)
    {
        answerPanel.SetActive(false);
        questionsPanel.SetActive(true);
        returnButton.gameObject.SetActive(false);
    }

    public void ReturnToMenu()
    {
        GetComponent<TransitionController>().PlayFinalTransition("MenuScene");
    }
}
