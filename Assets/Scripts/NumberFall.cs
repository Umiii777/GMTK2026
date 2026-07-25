using TMPro;
using UnityEngine;

public class NumberFall : MonoBehaviour
{
    void Start()
    {
        GetComponent<MeshCollider>().sharedMesh = GetComponent<TextMeshPro>().mesh;
    }
}
