using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    public Vector3 speed;

    private void Start()
    {
        transform.localRotation *= Quaternion.Euler(speed * Random.Range(0, 180));
    }

    private void Update()
    {
        if (!GameManager.instance.isRunning())
            return;
        transform.localRotation *= Quaternion.Euler(speed * Time.deltaTime);
    }
}
