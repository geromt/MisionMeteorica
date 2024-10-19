using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ModeloPsicometrico
{
    private static float[] _coefficientsControl = { -0.2412f, 0.1127f, -0.0023f, 0.1545f, -0.0001f, 0.0099f, 0.0007f, -0.0162f, -0.0000f, -0.0239f };
    private static float[] _coefficientsPatients = { -1.0948f, 0.1313f, 0.0212f, 0.4216f, -0.0003f, -0.0004f, 0.0001f, -0.0190f, -0.0001f, -0.470f };
    private static float _initTemperature = 1000f;
    private static float _coolingRate = 0.97f;


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

    public static float QRM(float interval, float speed, float range, bool isPareticHand)
    {
        if (!isPareticHand)
            return QuadraticRegresionModel(interval, speed, range, _coefficientsControl);
        else
            return QuadraticRegresionModel(interval, speed, range, _coefficientsPatients);
    }

    private static float CalculateObjectiveFunction(float target, float interval, float speed, float range)
    {
        return Mathf.Abs(target - QRM(interval, speed, range, true));
    }

    public static (float interval, float speed, float range) RunSimulatedAnnealing(float targetDiff, float initInterval, float initSpeed, float initRange)
    {
        float currentInterval = initInterval;
        float currentSpeed = initSpeed;
        float currentRange = initRange;
        float currentObj = CalculateObjectiveFunction(targetDiff, currentInterval, currentSpeed, currentRange);
        float temperature = _initTemperature;
        float newInterval, newSpeed, newRange, newObj, deltaObj;

        Debug.Log("SimulatedAnnealing Init Diff: " + QRM(currentInterval, currentSpeed, currentRange, true));

        while (temperature > 1.0)
        {
            newInterval = currentInterval + Random.Range(-0.25f, 0.25f);
            newSpeed = currentSpeed + Random.Range(-0.5f, 0.5f);
            newRange = currentRange + Random.Range(-0.5f, 0.5f);

            newObj = CalculateObjectiveFunction(targetDiff, newInterval, newSpeed, newRange);

            deltaObj = (newObj - currentObj) * _initTemperature; // Se multiplica por 1000 para poder trabajar con valores mayores a 1
            if (newObj < currentObj || Random.value < Mathf.Exp(-deltaObj / temperature))
            {
                currentInterval = newInterval;
                currentSpeed = newSpeed;
                currentRange = newRange;
                currentObj = newObj;
            }
            temperature *= _coolingRate;
        }

        Debug.Log("SimulatedAnnealing Final Diff: " + QRM(currentInterval, currentSpeed, currentRange, true));
        return (currentInterval, currentSpeed, currentRange);
    }

    public static (float interval, float speed, float range) TargetNewDifficulty(float targetDiff, float initInterval, float initSpeed, float initRange)
    {
        float currentInterval = initInterval;
        float currentSpeed = initSpeed;
        float currentRange = initRange;
        float currentObj = CalculateObjectiveFunction(targetDiff, currentInterval, currentSpeed, currentRange);
        float newInterval, newSpeed, newRange, newObj;
        int iterations = 100;

        Debug.Log("Target Init Diff: " + QRM(currentInterval, currentSpeed, currentRange, GameMaster.UsePatientPoly));

        while (currentObj > 0.01 || iterations > 0)
        {
            newInterval = currentInterval + Random.Range(-0.25f, 0.25f);
            newSpeed = currentSpeed + Random.Range(-0.5f, 0.5f);
            newRange = currentRange + Random.Range(-0.2f, 0.2f);

            newObj = CalculateObjectiveFunction(targetDiff, newInterval, newSpeed, newRange);

            if (newObj < currentObj || Random.value < iterations / 100)
            {
                currentInterval = newInterval;
                currentSpeed = newSpeed;
                currentRange = newRange;
                currentObj = newObj;
            }
            iterations--;
        }

        Debug.Log("Target Final Diff: " + QRM(currentInterval, currentSpeed, currentRange, GameMaster.UsePatientPoly));
        return (currentInterval, currentSpeed, currentRange);
    }
}
