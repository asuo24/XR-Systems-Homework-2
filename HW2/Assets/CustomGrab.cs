using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CustomGrab : MonoBehaviour
{
    CustomGrab otherHand;

    public List<Transform> nearObjects = new List<Transform>();
    public Transform grabbedObject;

    public InputActionReference action; // grip (Select)
    public InputActionReference toggleDoubleRotationAction; // Button A (primaryButton)

    [SerializeField] bool doubleRotation = false;
    [SerializeField] float rotationMultiplier = 2f;

    Vector3 prevPos;
    Quaternion prevRot;

    void OnEnable()
    {
        if (action && action.action != null)
            action.action.Enable();

        if (toggleDoubleRotationAction && toggleDoubleRotationAction.action != null)
        {
            toggleDoubleRotationAction.action.Enable();
            toggleDoubleRotationAction.action.performed += ToggleDoubleRotation;
        }
    }

    void OnDisable()
    {
        if (toggleDoubleRotationAction && toggleDoubleRotationAction.action != null)
            toggleDoubleRotationAction.action.performed -= ToggleDoubleRotation;
    }

    void Start()
    {
        if (transform.parent)
        {
            var hands = transform.parent.GetComponentsInChildren<CustomGrab>();
            for (int i = 0; i < hands.Length; i++)
            {
                if (hands[i] != this)
                    otherHand = hands[i];
            }
        }

        prevPos = transform.position;
        prevRot = transform.rotation;
    }

    void Update()
    {
        bool isGrabbing = action != null && action.action != null && action.action.IsPressed();

        if (isGrabbing)
        {
            if (!grabbedObject)
            {
                grabbedObject = GetGrabTarget();
                if (grabbedObject)
                {
                    LockPhysics(grabbedObject);

                    // avoid huge jump on first frame
                    prevPos = transform.position;
                    prevRot = transform.rotation;
                }
            }

            if (grabbedObject)
                ApplyDelta(grabbedObject);
        }
        else
        {
            if (grabbedObject)
            {
                UnlockPhysics(grabbedObject);
                grabbedObject = null;
            }
        }

        prevPos = transform.position;
        prevRot = transform.rotation;
    }

    Transform GetGrabTarget()
    {
        // pick closest
        Transform best = null;
        float bestDist = float.PositiveInfinity;

        for (int i = nearObjects.Count - 1; i >= 0; i--)
        {
            var t = nearObjects[i];
            if (!t)
            {
                nearObjects.RemoveAt(i);
                continue;
            }

            float d = (t.position - transform.position).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = t;
            }
        }

        if (best) return best;

        // grab what the other hand is holding (for 2-hand)
        if (otherHand && otherHand.grabbedObject)
            return otherHand.grabbedObject;

        return null;
    }

    void ApplyDelta(Transform obj)
    {
        Quaternion deltaRot = transform.rotation * Quaternion.Inverse(prevRot);

        if (doubleRotation && Mathf.Abs(rotationMultiplier - 1f) > 0.0001f)
            deltaRot = ScaleRotation(deltaRot, rotationMultiplier);

        Vector3 newPos = transform.position + (deltaRot * (obj.position - prevPos));
        obj.position = newPos;
        obj.rotation = deltaRot * obj.rotation;
    }

    void ToggleDoubleRotation(InputAction.CallbackContext ctx)
    {
        doubleRotation = !doubleRotation;
    }

    static Quaternion ScaleRotation(Quaternion q, float mult)
    {
        q.ToAngleAxis(out float angle, out Vector3 axis);

        if (axis.sqrMagnitude < 1e-8f || Mathf.Abs(angle) < 1e-5f)
            return q;

        axis.Normalize();
        return Quaternion.AngleAxis(angle * mult, axis);
    }

    void LockPhysics(Transform t)
    {
        var rb = t.GetComponent<Rigidbody>();
        if (!rb) return;

        var lk = t.GetComponent<GrabPhysicsLock>();
        if (!lk) lk = t.gameObject.AddComponent<GrabPhysicsLock>();

        if (lk.holders == 0)
        {
            lk.savedUseGravity = rb.useGravity;
            lk.savedKinematic = rb.isKinematic;
        }

        lk.holders++;

        rb.useGravity = false;
        rb.isKinematic = true;
    }

    void UnlockPhysics(Transform t)
    {
        var rb = t.GetComponent<Rigidbody>();
        var lk = t.GetComponent<GrabPhysicsLock>();
        if (!rb || !lk) return;

        lk.holders = Mathf.Max(0, lk.holders - 1);

        if (lk.holders == 0)
        {
            rb.useGravity = lk.savedUseGravity;
            rb.isKinematic = lk.savedKinematic;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        var t = other.transform;
        if (t && t.CompareTag("grabbable"))
        {
            if (!nearObjects.Contains(t))
                nearObjects.Add(t);
        }
    }

    void OnTriggerExit(Collider other)
    {
        var t = other.transform;
        if (t && t.CompareTag("grabbable"))
            nearObjects.Remove(t);
    }
}

public class GrabPhysicsLock : MonoBehaviour
{
    public int holders;
    public bool savedUseGravity;
    public bool savedKinematic;
}
