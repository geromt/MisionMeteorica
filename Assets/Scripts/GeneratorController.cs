using System.Collections;
using UnityEngine;

public class GeneratorController : MonoBehaviour
{
    public delegate void ParametersEvent(float difficulty, float interval, float speed, float range);
    public static event ParametersEvent OnParametersSend;

    public delegate void GeneratorEvent(Mano side);
    public static event GeneratorEvent OnAsteroidSpawn;

    public delegate void GraphicEvent(float x, float y);
    public static event GraphicEvent OnAsteroidDestinationSelect;


    [SerializeField] private Camera cameraBot;
    [SerializeField] private float asteroidScale;
    [SerializeField] private GameObject[] asteroidPrefabs;
    [SerializeField] private GameObject radarParticlesPrefab;

    [Header("Valores de pruebas")]
    [SerializeField] private bool isTest;
    [SerializeField] private float intervalTest;
    [SerializeField] private float speedTest;
    [SerializeField] private float rangeTest;

    private Transform _thisTransform;

    private float _xDest, _yDest;
    private Vector3 _destination;
    private Vector3 _direction;

    private float _currentDiff, _interval, _speed, _range;
    private float _realInterval, _realSpeed, _realRange;

    private bool _isPlayingRutine = false;
    private bool _isWaitingForLaunching = false;

    // VALORES DE REGION DE DESTINO PARA LOS ASTEROIDES (OCTAGONO)
    private const float OctaMaxX = 0.94f;
    private const float OctaMinX = -0.94f;
    private const float OctaMaxY = 2.11f;
    private const float OctaMinY = 1f; // Se aumento este valor despues de quitar lineas guia porque no se alcanzaban a ver algunos asteroides
    private const float OctaDiff = 0.46f;
    private float _octaZ;
    private Vector2[] _octaVertices = new Vector2[8];
    

    private void OnEnable()
    {
        AsteroidController.OnCollideHand += CallLaunchBall;
        AsteroidController.OnAvoidHand += CallLaunchBall;
        AdminController.OnCalcNewRutineVals += ChangeParameters;
        AdminController.OnRutineStart += StartRutine;
        AdminController.OnRutineEnd += EndRutine;
    }

    private void OnDisable()
    {
        AsteroidController.OnCollideHand -= CallLaunchBall;
        AsteroidController.OnAvoidHand -= CallLaunchBall;
        AdminController.OnCalcNewRutineVals -= ChangeParameters;
        AdminController.OnRutineStart -= StartRutine;
        AdminController.OnRutineEnd -= EndRutine;
    }

    private void Awake()
    {
        _thisTransform = transform;
        _realInterval = GameMaster.Interval;
        _realSpeed = GameMaster.Speed;
        _realRange = GameMaster.Range;

        if (isTest)
        {
            _realInterval = intervalTest;
            _realSpeed = speedTest;
            _realRange = rangeTest;
        }

        var inter = Mathf.Lerp(GameMaster.MaxInterval, GameMaster.MinInterval, Mathf.InverseLerp(GameMaster.MinInterval, GameMaster.MaxInterval, _realInterval));
        var rang = Mathf.Lerp(GameMaster.MaxRange, GameMaster.MinRange, Mathf.InverseLerp(GameMaster.MinRange, GameMaster.MaxRange, _realRange));
        _currentDiff = ModeloPsicometrico.QRM(inter, _realSpeed, rang, true);
        AdjustParameters();
        OnParametersSend?.Invoke(_currentDiff, _interval, _speed, _range);
        #if UNITY_EDITOR
        Debug.Log(string.Format("velocidad: {0}, intervalo: {1}, rango: {2}", _speed, _realInterval, _realRange));
        Debug.Log("Dificultad inicial: " + _currentDiff);
        #endif

        asteroidScale = GameMaster.Size;

        _octaVertices[0] = new Vector2(OctaMinX,            OctaMinY + OctaDiff);
        _octaVertices[1] = new Vector2(OctaMinX + OctaDiff, OctaMinY);
        _octaVertices[2] = new Vector2(OctaMaxX - OctaDiff, OctaMinY);
        _octaVertices[3] = new Vector2(OctaMaxX,            OctaMinY + OctaDiff);
        _octaVertices[4] = new Vector2(OctaMaxX,            OctaMaxY - OctaDiff);
        _octaVertices[5] = new Vector2(OctaMaxX - OctaDiff, OctaMaxY);
        _octaVertices[6] = new Vector2(OctaMinX + OctaDiff, OctaMaxY);
        _octaVertices[7] = new Vector2(OctaMinX,            OctaMaxY - OctaDiff);

        _octaZ = cameraBot.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, cameraBot.nearClipPlane + 0.3f)).z;
    }

    private void StartRutine()
    {
        _isPlayingRutine = true;
        CallLaunchBall();
    }

    private void EndRutine()
    {
        GameMaster.SetDifficultyVals(_currentDiff, _realInterval, _speed, _realRange);
        _isPlayingRutine = false;
    }

    private void CallLaunchBall()
    {
        if (GameMaster.ModoTutorial) return;

        if (!_isWaitingForLaunching)
            StartCoroutine(LaunchBallCoroutine());
    }

    private IEnumerator LaunchBallCoroutine()
    {
        _isWaitingForLaunching = true;
        yield return new WaitForSeconds(_interval);
        if (_isPlayingRutine)
            LaunchBall();
        _isWaitingForLaunching = false;
    }

    private void LaunchBall()
    {
        // DistributeRangeX y DistributeRangeY regresan un valor al azar entre -1 y 1 dependediende del rango y la funcion definida
        _xDest = DistributeRangeX(_range) * OctaMaxX;

        if (GameMaster.IsYAxisEnabled)
        {
            var center = (OctaMaxY - OctaMinY) / 2;
            _yDest = (DistributeRangeY(_range) * center) + (OctaMinY + center);
        }
        else
            _yDest = OctaMinY + (OctaDiff * 0.8f); // La altura objetivo es el usuario tenga los brazos sobre la mesa con los codos flexinados aprox. 90 grados

        if (IsInsideOctagon(_xDest, _yDest))
        {
            _destination = new Vector3(_xDest, _yDest, _octaZ);
        }
        else
        {
            var p = new Vector2(_xDest, _yDest);
            var nearestLine = GetNearestLine(p);
            var nearestPoint = CalcNearestPointOverLine(nearestLine.p1, nearestLine.p2, p);
            _destination = new Vector3(nearestPoint.x, nearestPoint.y, _octaZ);
        }

        #if UNITY_EDITOR
        Debug.Log("Destination: " + _destination);
        Debug.Log("Destination on camera: " + GetDestinationOnCamera(_destination));
        #endif

        var viewportDest = cameraBot.WorldToViewportPoint(_destination);
        var initPos = cameraBot.ViewportToWorldPoint(new Vector3(viewportDest.x, viewportDest.y, -_thisTransform.position.z));
        var radarPos = cameraBot.ViewportToWorldPoint(new Vector3(viewportDest.x, viewportDest.y, 5));

        _direction = (_destination - initPos).normalized;

        GameObject asteroid = Instantiate(asteroidPrefabs[GameMaster.AsteroidIndex], initPos, Quaternion.identity, _thisTransform);
        Instantiate(radarParticlesPrefab, radarPos, Quaternion.identity, _thisTransform);

        asteroid.GetComponent<AsteroidController>().Destination = _destination;
        asteroid.transform.localScale = new Vector3(asteroidScale, asteroidScale, asteroidScale);
        Rigidbody asteroidRB = asteroid.GetComponent<Rigidbody>();
        asteroidRB.velocity = _direction * _speed;

        var side = (_xDest < 0.5f) ? Mano.Izquierda : Mano.Derecha;

        OnAsteroidSpawn?.Invoke(side);
        OnAsteroidDestinationSelect?.Invoke(_xDest, _yDest);
    }

    // Metodo usado para tutorial
    // Los valores de x y y deben estar dentro del octagono
    public void LaunchBall(float x, float y)
    {
        _destination = new Vector3(x, y, _octaZ);

        var viewportDest = cameraBot.WorldToViewportPoint(_destination);
        var initPos = cameraBot.ViewportToWorldPoint(new Vector3(viewportDest.x, viewportDest.y, -_thisTransform.position.z));
        var radarPos = cameraBot.ViewportToWorldPoint(new Vector3(viewportDest.x, viewportDest.y, 5));

        _direction = (_destination - initPos).normalized;

        var asteroid = Instantiate(asteroidPrefabs[GameMaster.AsteroidIndex], initPos, Quaternion.identity, _thisTransform);
        Instantiate(radarParticlesPrefab, radarPos, Quaternion.identity, _thisTransform);

        asteroid.GetComponent<AsteroidController>().Destination = _destination;
        asteroid.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        Rigidbody asteroidRB = asteroid.GetComponent<Rigidbody>();
        asteroidRB.velocity = _direction * 2;
    }

    /// <summary>
    /// Calcula las coordenadas del punto en la recta que pasa por el generador y el destino con valor z en nearClipPlane
    /// </summary>
    /// <param name="_destination"></param>
    /// <returns></returns>
    private Vector3 GetDestinationOnCamera(Vector3 destination)
    {
        var generatorPos = _thisTransform.position;
        var z = cameraBot.nearClipPlane;
        var c = (z - destination.z) / (generatorPos.z - destination.z);

        var x = c * (generatorPos.x - destination.x) + destination.x;
        var y = c * (generatorPos.y - destination.y) + destination.y;

        return new Vector3(x, y, z);
    }

    private void ChangeParameters(Desempeno desempeno)
    {
        if (desempeno != Desempeno.Mantiene)
        {
            float newDiff;
            if (desempeno == Desempeno.Incremento)
                newDiff = _currentDiff + 0.05f;
            else
                newDiff = _currentDiff - 0.05f;

            var inter = Mathf.Lerp(GameMaster.MaxInterval, GameMaster.MinInterval, Mathf.InverseLerp(GameMaster.MinInterval, GameMaster.MaxInterval, _realInterval));
            var rang = Mathf.Lerp(GameMaster.MaxRange, GameMaster.MinRange, Mathf.InverseLerp(GameMaster.MinRange, GameMaster.MaxRange, _realRange));
            var speed = Mathf.Clamp(_realSpeed, GameMaster.MinSpeed, GameMaster.MaxSpeed);

            (_realInterval, _realSpeed, _realRange) = ModeloPsicometrico.TargetNewDifficulty(newDiff, inter, speed, rang);

            #if UNITY_EDITOR
            Debug.Log("Intervalo: " + _realInterval);
            Debug.Log("Velocidad: " + _realSpeed);
            Debug.Log("Rango: " + _realRange);
            #endif
            _currentDiff = ModeloPsicometrico.QRM(_realInterval, _realSpeed, _realRange, true);
            AdjustParameters();
            _interval = Mathf.Lerp(GameMaster.MaxInterval, GameMaster.MinInterval, Mathf.InverseLerp(GameMaster.MinInterval, GameMaster.MaxInterval, _realInterval));
            _range = Mathf.Lerp(GameMaster.MaxRange, GameMaster.MinRange, Mathf.InverseLerp(GameMaster.MinRange, GameMaster.MaxRange, _realRange));
        }

        OnParametersSend?.Invoke(_currentDiff, _interval, _speed, _range);
        _isPlayingRutine = true;
        CallLaunchBall();
    }

    private void AdjustParameters()
    {
        _speed = Mathf.Clamp(_realSpeed, GameMaster.MinSpeed, GameMaster.MaxSpeed);
        _interval = Mathf.Clamp(_realInterval, GameMaster.MinInterval, GameMaster.MaxInterval);
        _range = Mathf.Clamp(_realRange, GameMaster.MinRange, GameMaster.MaxRange);
    }

    private float DistributeRangeX(float range)
    {
        var x = Random.value;

        if (x == 0)
            return -range;
        else if (x == 1)
            return range;
        else if (x > 0.5f)
            return (1 - Mathf.Pow(2, -15 * x + 7.5f)) * range;
        else
            return (Mathf.Pow(2, 15 * x - 7.5f) - 1) * range;
    }

    private float DistributeRangeY(float range)
    {
        var x = Random.value;

        if (x == 0)
            return -range;
        else if (x == 1)
            return range;
        else if (x > 0.5f)
            return (1 - Mathf.Pow(2, -15 * x + 7.5f + Mathf.Log(1.386f, 2))) * range - (1 - range) * 0.386f;
        else
            return (Mathf.Pow(2, 15 * x - (7.5f - Mathf.Log(0.614f, 2))) - 1) * range - (1 - range) * 0.386f;
    }

    /// <summary>
    /// Revisa si el punto (x,y) se encuentra dentro del octagono definido por _octaVectices
    /// </summary>
    private bool IsInsideOctagon(float x, float y)
    {
        for (int i = 0; i < _octaVertices.Length - 1; i++)
        {
            if (!IsRightOfLine(_octaVertices[i], _octaVertices[i + 1], new Vector2(x, y)))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Revisa si el punto p se encuentra a la derecha de la linea que va de p1 a p2.
    /// La linea de p1 a p2 deben de girar en direccion a las manecillas del reloj
    /// </summary>
    private bool IsRightOfLine(Vector2 p1, Vector2 p2, Vector2 p)
    {
        var dot = (p.x - p1.x) * (p2.y - p1.y) - (p.y - p1.y) * (p2.x - p1.x);
        return dot < 0;
    }

    private (Vector2 p1, Vector2 p2) GetNearestLine(Vector2 p)
    {
        if (p.y <= (OctaMinY + OctaMaxY) / 2.0f)
        {
            if (p.x <= (OctaMinX + OctaMaxX) / 2.0f)
                return (_octaVertices[0], _octaVertices[1]);
            else
                return (_octaVertices[2], _octaVertices[3]);
        }
        else
        {
            if (p.x <= (OctaMinX + OctaMaxX) / 2.0f)
                return (_octaVertices[6], _octaVertices[7]);
            else
                return (_octaVertices[4], _octaVertices[5]);
        }
    }

    /// <summary>
    /// Obtiene el punto sobre la linea que va p1 a p2 mas cercano a p
    /// </summary>
    private Vector2 CalcNearestPointOverLine(Vector2 p1, Vector2 p2, Vector2 p)
    {
        var v1 = (p2 - p1).normalized;
        var v2 = p - p1;
        var dot = Vector2.Dot(v1, v2);

        return (v1 * dot) + p1;
    }
}
