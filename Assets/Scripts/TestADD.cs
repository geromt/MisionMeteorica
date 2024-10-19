using UnityEngine;
using UnityEngine.UI;

public class TestADD : MonoBehaviour
{

    public Text resultado;
    public bool polinomioPaciente;
    public SliderController velocidad;
    public SliderController rango;
    public SliderController intervalo;

    public InputField numeroInputField;
    public Text escala;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            GradientDescent(0.1f, 100);
    }
    public void UpdateResult()
    {
        Debug.Log(string.Format("velocidad: {0}, intervalo: {1}, rango: {2}, paciente: {3}", velocidad.Value, intervalo.Value, rango.Value, polinomioPaciente));
        //var inter = Mathf.Lerp(3, 0, Mathf.InverseLerp(0, 3, intervalo.Value));
        //var rang = Mathf.Lerp(0.97f, 0.42f, Mathf.InverseLerp(0.42f, 0.97f, rango.Value));
        resultado.text = QuadraticRegresionModel(intervalo.Value, velocidad.Value, rango.Value, _coefficientsPatients).ToString();
    }

    public void CambiaEscala()
    {
        var diff = float.Parse(numeroInputField.text);
        Debug.Log(diff);
        Debug.Log(Mathf.Lerp(0, 100, Mathf.InverseLerp(-0.933f, -0.389f, diff)));
        escala.text = Mathf.Lerp(0, 100, Mathf.InverseLerp(-0.933f, -0.389f, diff)).ToString();
    }

    private static float[] _coefficientsControl = { -0.2412f, 0.1127f, -0.0023f, 0.1545f, -0.0001f, 0.0099f, 0.0007f, -0.0162f, -0.0000f, -0.0239f };
    private static float[] _coefficientsPatients = { -1.0948f, 0.1313f, 0.0212f, 0.4216f, -0.0003f, -0.0004f, 0.0001f, -0.0190f, -0.0001f, -0.470f };


    private static float QuadraticRegresionModel(float interval, float speed, float range, float[] coefficients)
    {
        return coefficients[0] +
            coefficients[1] * interval +
            coefficients[2] * speed +
            coefficients[3] * range +
            coefficients[4] * interval * speed +
            coefficients[5] * interval * range +
            coefficients[6] * speed * range +
            coefficients[7] * interval * interval +
            coefficients[8] * speed * speed +
            coefficients[9] * range * range;
    }

    private float FuncToMin(float objective, float current)
    {
        return Mathf.Pow(objective - current, 2);
    }

    private float MyGradient(float objective, float current)
    {
        return -2 * (objective - current);
    }

    private float GradientVel(float vel, float rang, float intervalo, float[] coefficients)
    {
        return coefficients[2] +
            coefficients[4] * intervalo +
            coefficients[6] * rang +
            2 * coefficients[8] * vel; 
    }

    private float GradientRan(float vel, float rang, float intervalo, float[] coefficients)
    {
        return coefficients[3] +
            coefficients[5] * intervalo +
            coefficients[6] * vel +
            2 * coefficients[9] * rang;
    }

    private float GradientInt(float vel, float rang, float intervalo, float[] coefficients)
    {
        return coefficients[1] +
            coefficients[4] * vel +
            coefficients[5] * rang +
            2 * coefficients[7] * intervalo;
    }

    private (float newDiff, float newVel, float newRan, float newInt) GradientDescent(float rate, float maxIterations)
    {
        float objective = 0;
        float current = QuadraticRegresionModel(intervalo.Value, velocidad.Value, rango.Value, _coefficientsPatients);

        float initVel = velocidad.Value;
        float initRan = rango.Value;
        float initInt = intervalo.Value;

        int numIterations = 0;
        float grad = MyGradient(objective, current);
        float x = FuncToMin(objective, current);

        while (numIterations < maxIterations && Mathf.Abs(objective - current) > 0.01f)
        {
            var tempVel = initVel - (rate * GradientVel(initVel, initRan, initInt, _coefficientsPatients));
            var tempRan = initRan - (rate * GradientRan(initVel, initRan, initInt, _coefficientsPatients));
            var tempInt = initInt - (rate * GradientInt(initVel, initRan, initInt, _coefficientsPatients));

            initVel = tempVel;
            initRan = tempRan;
            initInt = tempInt;

            current = QuadraticRegresionModel(initInt, initVel, initRan, _coefficientsPatients);

            grad = MyGradient(objective, current);
            numIterations++;
        }

        Debug.Log("Num iteraciones: " + numIterations);
        Debug.Log("New diff: " + current);
        Debug.Log("New vel: " + initVel);
        Debug.Log("New rango:" + initRan);
        Debug.Log("New Inter:" + initInt);
        return (current, initVel, initRan, initInt);
    }
}
