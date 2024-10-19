using System.Collections;
using UnityEngine;

public class RadarController : MonoBehaviour
{
    private ParticleSystem _radarParticles;
    private bool _isCollidingAsteroid = false;

    private void Awake()
    {
        _radarParticles = GetComponent<ParticleSystem>();
        _radarParticles.Stop();

        // El tiempo de vida de cada particula depende de la velocidad de la ronda
        var lifeTime = Mathf.Lerp(1, 2, Mathf.InverseLerp(10, 1, GameMaster.Speed));
        var main = _radarParticles.main;
        main.startLifetime = lifeTime;
        if (GameMaster.Speed > 4)
            main.prewarm = true;
    }

    private void Start()
    {
        _radarParticles.Play();
        StartCoroutine(StopRadarParticlesCoroutine());
    }

    private void Update()
    {
        // Cuando ya no hay mas particulas vivas destruimos el objeto
        if (!_radarParticles.IsAlive())
            Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Asteroid"))
            _isCollidingAsteroid = true;
    }

    private IEnumerator StopRadarParticlesCoroutine()
    {
        // Esperamos hasta que el asteroide colisione con las particulas y las detenemos
        yield return new WaitUntil(() => _isCollidingAsteroid);
        _radarParticles.Stop();
    }
}
