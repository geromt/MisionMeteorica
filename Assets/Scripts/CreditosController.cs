using UnityEngine;
using UnityEngine.UI;

public class CreditosController : MonoBehaviour
{
    [SerializeField] private Scrollbar scrollbar;

    void Start()
    {
        scrollbar.value = 1;
    }
}
