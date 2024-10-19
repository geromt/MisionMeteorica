using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private string format;
    [SerializeField] private string suffix;
    [SerializeField] private float minValue;
    [SerializeField] private float maxValue;
    [SerializeField] private float step;

    private Slider _thisSlider;

    private float value;
    public float Value 
    { 
        get 
        { 
            return value; 
        }
        set
        {
            this.value = value;
            ConvertToSlider();
            UpdateValueText();
        }
    }
    public float Min { get { return minValue; } }
    public float Max { get { return maxValue; } }


    private void Awake()
    {
        if (_thisSlider == null)
            ConfigureSlider();
    }

    private void Start()
    {
        UpdateValueText();
    }

    public void UpdateValueText()
    {
        value = ConvertValue();
        if (string.IsNullOrEmpty(format))
            valueText.text = $"{value:0.0} {suffix}";
        else
            valueText.text = string.Format(format, value, suffix);
    }

    private float ConvertValue()
    {
        return (_thisSlider.value * step) + minValue;
    }

    private void ConvertToSlider()
    {
        // Puede pasar que este metodo se llame antes de Awake dependiendo de las corutinas en MenuController 
        if (_thisSlider == null)
            ConfigureSlider();
        _thisSlider.value = (value - minValue) / step;
    }

    private void ConfigureSlider()
    {
        _thisSlider = GetComponent<Slider>();
        _thisSlider.minValue = 0;
        _thisSlider.maxValue = Mathf.Ceil((maxValue - minValue) / step);
        _thisSlider.wholeNumbers = true;
    }
}