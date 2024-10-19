using UnityEngine;

public class ParticlesController : MonoBehaviour
{
    [SerializeField] private ParticleSystem particleSys;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float nearDistance = 8;
    [SerializeField] private float velocity;

    private int _numberOfParticles;
    private ParticleSystem.Particle[] _particles;
    private Vector3 _mouseWorldPos;

    private void Awake()
    {
        _particles = new ParticleSystem.Particle[particleSys.main.maxParticles];
    }

    private void Update()
    {
        var mousePos = Input.mousePosition;
        mousePos.z = 8.5f;
        _mouseWorldPos = mainCamera.ScreenToWorldPoint(mousePos);

        _numberOfParticles = particleSys.particleCount;
        particleSys.GetParticles(_particles, _numberOfParticles);

        for (int i = 0; i < _numberOfParticles; i++)
        {
            if (Vector3.Distance(_mouseWorldPos, _particles[i].position) < nearDistance)
                _particles[i].position = ((_mouseWorldPos - _particles[i].position).normalized * Time.deltaTime * velocity) + _particles[i].position;
        }

        particleSys.SetParticles(_particles);
    }
}
