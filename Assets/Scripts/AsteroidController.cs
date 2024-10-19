using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AsteroidController : MonoBehaviour
{
    public delegate void BallEvent();
    public static event BallEvent OnCollideHand;
    public static event BallEvent OnAvoidHand;

    public delegate void CollideHandEvent(Mano mano);
    public static event CollideHandEvent SendCollisionHand;

    [SerializeField] private Material lineMaterial;

    private Vector3 _destination = Vector3.zero;
    public Vector3 Destination { set { _destination = value; } }


    private ParticleSystem _rightHandExplosion;
    private ParticleSystem _leftHandExplosion;

    private Transform _cameraTransform;
    private Transform _childTransform;
    private Vector3 _childRotation;
    private bool _haChocadoConMano = false;

    private float _cylinderRadious = 0.01f;
    private int _cylinderResolution = 8;
    private GameObject _cylinder;
    private Vector3 _delta;
    private Vector3 _previousDelta;
    private CapsuleCollider _cylinderCollider;


    private void Awake()
    {
        if (SceneManager.GetActiveScene().name != "GameScene") return;

        _leftHandExplosion = GameObject.Find("mixamorig:LeftHand").GetComponent<ParticleSystem>();
        _rightHandExplosion = GameObject.Find("mixamorig:RightHand").GetComponent<ParticleSystem>();
        _cameraTransform = GameObject.FindGameObjectWithTag("cameraBot").transform;
        _childTransform = transform.GetChild(0);
        _childRotation = Random.onUnitSphere * 2;
    }

    private void Start()
    {
        if (GameMaster.AreGuidesActive)
        {
            CreateCylinder();
            UpdateCylinder();
        }
    }

    private void FixedUpdate()
    {
        if (_haChocadoConMano)
            return;

        if (transform.position.z >= _cameraTransform.position.z)
        {
            OnAvoidHand?.Invoke();
            Debug.Log("Evade mano");
            Destroy(gameObject);
            Destroy(_cylinder);
        }

        _childTransform.Rotate(_childRotation);

        if (GameMaster.CalidadGraficos == Graficos.Bajos)
            return;

        if (GameMaster.AreGuidesActive)
            UpdateCylinder();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_haChocadoConMano)
            return;

        if (other.CompareTag("mano"))
        {
            #if UNITY_EDITOR
            Debug.Log("Choca con mano: " + other.name);
            #endif
            if (other.name.Equals("mixamorig:LeftHand"))
            {
                _leftHandExplosion.Play();
                SendCollisionHand?.Invoke(Mano.Izquierda);
            }
            else if (other.name.Equals("mixamorig:RightHand"))
            {
                _rightHandExplosion.Play();
                SendCollisionHand?.Invoke(Mano.Derecha);
            }

            OnCollideHand?.Invoke();
            _haChocadoConMano = true;
            Destroy(_cylinder);
            StartCoroutine(WaitDestroy());
        }
    }

    private IEnumerator WaitDestroy()
    {
        yield return new WaitForSeconds(0.8f);
        Destroy(gameObject);
    }

    private void CreateCylinder()
    {
        _cylinder = new GameObject("Cylinder", typeof(MeshFilter), typeof(MeshRenderer), typeof(CapsuleCollider));
        _cylinder.tag = "asteroidLine";
        _cylinder.GetComponent<MeshRenderer>().sharedMaterial = lineMaterial;
        _cylinder.transform.parent = transform.parent;

        _cylinder.layer = gameObject.layer;
        _cylinder.hideFlags = HideFlags.DontSave;

        _cylinderCollider = _cylinder.GetComponent<CapsuleCollider>();
        _cylinderCollider.isTrigger = true;
        _cylinderCollider.direction = 2;
        _cylinderCollider.radius = _cylinderRadious;
    }

    private void UpdateCylinder()
    {
        _delta = _destination - transform.position;

        if (Vector3.Distance(_delta, _previousDelta) > 0.05f)

            _cylinder.GetComponent<MeshFilter>().sharedMesh = GenerateCylinderMesh(_delta.magnitude);

        _previousDelta = _delta;
        _cylinder.transform.position = transform.position;

        if (_delta.sqrMagnitude <= Mathf.Epsilon)
            return;

        _cylinder.transform.LookAt(_destination);

        _cylinderCollider.center = new Vector3(0, 0, _delta.magnitude / 2);
        _cylinderCollider.height = _delta.magnitude;
    }

    private Mesh GenerateCylinderMesh(float length)
    {
        Mesh mesh = new Mesh();
        mesh.name = "Cylinder";
        mesh.hideFlags = HideFlags.DontSave;

        List<Vector3> vertices = new List<Vector3>();
        List<Color> colors = new List<Color>();
        List<int> tris = new List<int>();

        Vector3 p0 = Vector3.zero;
        Vector3 p1 = Vector3.forward * length;

        float angle, dx, dy;
        int triStart;
        int triCap = _cylinderResolution * 2;

        // Agrega contorno del cilindro
        for (int i = 0; i < _cylinderResolution; i++)
        {
            angle = (Mathf.PI * 2.0f * i) / _cylinderResolution;
            dx = _cylinderRadious * Mathf.Cos(angle);
            dy = _cylinderRadious * Mathf.Sin(angle);

            Vector3 spoke = new Vector3(dx, dy, 0);

            vertices.Add(p0 + spoke);
            vertices.Add(p1 + spoke);

            colors.Add(Color.white);
            colors.Add(Color.white);

            triStart = vertices.Count;

            tris.Add((triStart + 0) % triCap);
            tris.Add((triStart + 2) % triCap);
            tris.Add((triStart + 1) % triCap);

            tris.Add((triStart + 2) % triCap);
            tris.Add((triStart + 3) % triCap);
            tris.Add((triStart + 1) % triCap);
        }

        vertices.Add(p0);
        vertices.Add(p1);

        // Agrega caras del cilindro
        for (int i = 0; i < _cylinderResolution; i++)
        {
            tris.Add(i * 2);
            tris.Add((i * 2 + 2) % triCap);
            tris.Add(vertices.Count - 2);

            tris.Add(i * 2 + 1);
            tris.Add((i * 2 + 3) % triCap);
            tris.Add(vertices.Count - 1);
        }

        mesh.SetVertices(vertices);
        mesh.SetIndices(tris.ToArray(), MeshTopology.Triangles, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.UploadMeshData(true);

        return mesh;
    }
}
