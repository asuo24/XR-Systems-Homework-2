using UnityEngine;

public class VRMagnifierSimple : MonoBehaviour
{
    [SerializeField] Transform lensCenter;   // put an empty object at the lens center
    [SerializeField] Transform viewer;       // XR Main Camera
    [SerializeField] float positionOffset = 0.015f; // push camera a tiny bit forward

    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (!cam)
            cam = GetComponentInChildren<Camera>();

        // if this camera is parented under the magnifying glass, rotating the glass can mess with it
        // so just detach it to world space once
        if (cam && cam.transform.parent != null)
            cam.transform.SetParent(null, true);
    }

    void LateUpdate()
    {
        if (!cam || !lensCenter || !viewer) return;

        Vector3 dir = lensCenter.position - viewer.position;

        // just in case something is zero
        if (dir.sqrMagnitude < 0.000001f)
            dir = viewer.forward;

        dir.Normalize();

        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
        cam.transform.rotation = rot;

        // keep camera at the lens, slightly "forward" away from the viewer
        cam.transform.position = lensCenter.position + dir * positionOffset;
    }
}
