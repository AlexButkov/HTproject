using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Methods : MonoBehaviour
{
    #region Initialization
    public GameObject[] Spires = new GameObject[3];
    [Space(10)]
    public GameObject RingSample;
    public int RingQuant = 0;
    public float Speed = 2;
    public float Power = 1;

    //----
    [SerializeField] private float scaleMult = 0.5f;
    readonly List<GameObject> Rings = new List<GameObject>();
    readonly float waitDivider = 5;
    Vector3 scaleVector = new Vector3(1,0,1);
    #endregion

    // Use this for initialization
    void Start()
    {
        StartCoroutine(AddRings(RingQuant));
        Invoke("Starter", 0.01f);
    }

    /*// Update is called once per frame
    void Update()
    {
        
    }*/


    public void Starter()
    {
        StartCoroutine(SetMoving(RingQuant-1, Spires[0], Spires[2], Spires[1]));
    }
    //---
    private IEnumerator AddRings(int ringQuant, int startIndex = 0)
    {
        for (int i = 0; i < ringQuant; i++)
        {
            GameObject ring = Instantiate(RingSample);
            Transform parent = Spires[0].transform;
            ring.name += string.Format(" ({0})", i + startIndex);
            ring.transform.parent = parent;
            ring.transform.position = parent.position + Vector3.down * parent.localScale.y ;
            ring.GetComponentInChildren<MeshRenderer>().material.color = Random.ColorHSV();
            ring.transform.localScale += scaleVector * (i + startIndex) * scaleMult ;
            Rings.Add(ring);
            yield return new WaitForSeconds(Time.deltaTime / waitDivider);
        }
    }
    
    private IEnumerator SetMoving(int number, GameObject source, GameObject target, GameObject buffer)
    {
        if (number >= 0)
        {
            yield return StartCoroutine(SetMoving(number - 1, source, buffer, target));
            yield return StartCoroutine(DoMoving(Rings[number].transform, target.transform));
            yield return StartCoroutine(SetMoving(number - 1, buffer, target, source));
        }
    }

    
    private IEnumerator DoMoving(Transform ring, Transform target)
    {
        yield return StartCoroutine(MovingTowards(ring, ring.parent.position));
        yield return StartCoroutine(MovingTowards(ring, target.position));
        Vector3 targetPosition = target.position + Vector3.down * target.localScale.y + Vector3.up * Mathf.Max(0, target.childCount - 1);
        yield return StartCoroutine(MovingTowards(ring, targetPosition));
        ring.parent = target;
    }

    private IEnumerator MovingTowards(Transform ring, Vector3 targetPosition)
    {
        float step;
        while (ring.position != targetPosition)
        {
            step = Speed * Time.deltaTime;
            ring.position = Vector3.MoveTowards(ring.position, targetPosition, step);
            yield return null;
        }
    }
}
