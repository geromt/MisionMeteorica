using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxController : MonoBehaviour
{
    [SerializeField] private Material[] skyBoxes;

    private Material _currentSkyBox;
    private float _currentRotation;
    private float degreesPerSecond = -1.5f;

    private void Awake()
    {
        _currentSkyBox = new Material(skyBoxes[Random.Range(0, skyBoxes.Length)]);
        _currentRotation = Random.Range(0, 361);

        RenderSettings.skybox = _currentSkyBox;
        RenderSettings.skybox.SetFloat("_Rotation", _currentRotation);
    }

    private void Update()
    {
        _currentRotation += degreesPerSecond * Time.deltaTime;
        RenderSettings.skybox.SetFloat("_Rotation", _currentRotation);
    }
}
