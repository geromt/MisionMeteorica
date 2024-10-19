using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeQualityController : MonoBehaviour
{
    [SerializeField] private MaterialGroup[] groups;
    [SerializeField] private Component[] components;

    private static bool _hasReadQualityPref = false;

    private void OnEnable()
    {
        SelectGameQuality.ChangeQuality += ChangeQuality;
    }

    private void OnDisable()
    {
        SelectGameQuality.ChangeQuality -= ChangeQuality;
    }

    private void Awake()
    {
        if (!_hasReadQualityPref)
        {
            if (PlayerPrefs.HasKey("Quality"))
            {
                GameMaster.CalidadGraficos = (Graficos)PlayerPrefs.GetInt("Quality");
                _hasReadQualityPref = true;
            }
            else
            {
                PlayerPrefs.SetInt("Quality", (int)GameMaster.CalidadGraficos);
                PlayerPrefs.Save();
            }
        }
        ChangeQuality();
    }

    private void ChangeQuality()
    {
        if (GameMaster.CalidadGraficos == Graficos.Bajos)
            ChangeQualityToLow();
        else if (GameMaster.CalidadGraficos == Graficos.Altos)
            ChangeQualityToHigh();
    }

    private void ChangeQualityToHigh()
    {
        QualitySettings.SetQualityLevel(1, true);
        foreach (MaterialGroup mg in groups)
        {
            foreach (Image image in mg.images)
                image.material = mg.materialHighQuality;
            foreach (MeshRenderer mesh in mg.meshes)
                mesh.material = mg.materialHighQuality;
        }
        ActivateComponents();
    }

    private void ChangeQualityToLow()
    {
        QualitySettings.SetQualityLevel(0, true);
        foreach (MaterialGroup mg in groups)
        {
            foreach (Image image in mg.images)
                image.material = mg.materialLowQuality;
            foreach (MeshRenderer mesh in mg.meshes)
                mesh.material = mg.materialLowQuality;
        }
        DeactivateComponents();
    }

    private void ActivateComponents()
    {
        foreach (MonoBehaviour component in components)
            component.enabled = true;
    }

    private void DeactivateComponents()
    {
        foreach (MonoBehaviour script in components)
            script.enabled = false;
    }

    [System.Serializable]
    public struct MaterialGroup
    {
        public Material materialHighQuality;
        public Material materialLowQuality;
        public Image[] images;
        public MeshRenderer[] meshes;
    }
}
