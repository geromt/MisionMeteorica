using UnityEngine;
using UnityEngine.UI;

public class LikertController : MonoBehaviour
{
    [SerializeField] private Button[] buttons;

    private int value = -1;
    public int Value { get { return value; } }

    private void Awake()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            var j = i;
            buttons[i].onClick.AddListener(() => { SelectButton(j); });
            buttons[i].transform.GetChild(1).gameObject.SetActive(false);
        }
    }

    private void SelectButton(int buttonIndex)
    {
        if (value != -1)
            buttons[value].transform.GetChild(1).gameObject.SetActive(false);

        value = buttonIndex;
        buttons[buttonIndex].transform.GetChild(1).gameObject.SetActive(true);
    }
}
