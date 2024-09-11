using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public float rotationSpeed = 100f; // Rotation speed in degrees per second

    private void OnEnable()
    {
        // Start rotating the object when it is enabled
        StartCoroutine(SelfRotate());
    }

    private IEnumerator SelfRotate()
    {
        while (true) // Keep rotating indefinitely
        {
            // Rotate the object around the z-axis
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            yield return null; // Wait for the next frame
        }
    }

    private void OnDisable()
    {
        // Stop rotating when the object is disabled
        StopCoroutine(SelfRotate());
    }
}
