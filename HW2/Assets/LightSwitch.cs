using UnityEngine;
using UnityEngine.InputSystem;

public class LightSwitch : MonoBehaviour
{
    public Light light;
    public InputActionReference action;

    void Start()
    {
        light = GetComponent<Light>();

        action.action.Enable();
        action.action.performed += (ctx) => {
            light.color = Random.ColorHSV(0f, 1f, 0.8f, 1f, 0.8f, 1f);
        };
    }
}
