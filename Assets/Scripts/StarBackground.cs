using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarBackground : MonoBehaviour
{
    private Vector3 _rotation;

    private void Awake()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] normals = mesh.normals;

        for (int i =0; i < normals.Length; i++)
            normals[i] = normals[i] * -1;

        mesh.normals = normals;

        _rotation = Random.onUnitSphere * 0.1f;
    }
 
    void FixedUpdate()
    {
        if (GameMaster.CalidadGraficos == Graficos.Bajos)
            return;

        transform.Rotate(_rotation);
    }
}
