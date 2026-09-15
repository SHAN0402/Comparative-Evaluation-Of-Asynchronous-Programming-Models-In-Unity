using UnityEngine;


//Coroutines are tightly coupled with the engine lifecycle and can only be started and managed by components inheriting from MonoBehaviour
public class EmptyBehaviour : MonoBehaviour
{
    void Awake()
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        cube.transform.position = Vector3.zero;

        cube.transform.SetParent(transform);
    }
}