using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraDamp : MonoBehaviour
{
    public float upMult = 0.5f;
    public float backMult = 1f;
    public float dampTime = 0.15f;
    public GameObject Target;
    //----
    private Vector3 velocity = Vector3.zero;
    private Vector3 targetVector;

    void Update()
    {
        if (Target != null)
        {
            targetVector = Target.transform.position + Vector3.back * Target.transform.position.y * backMult + Vector3.up * Target.transform.position.y * upMult;
            transform.position = Vector3.SmoothDamp(transform.position, targetVector, ref velocity, dampTime);
        }
    }
}
