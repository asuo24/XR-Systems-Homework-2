using UnityEngine;

public class Orbit : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(0f, 30f * Time.deltaTime, 0f, Space.Self);
    }
}