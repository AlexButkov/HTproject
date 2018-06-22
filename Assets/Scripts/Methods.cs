using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Methods : MonoBehaviour
{
    #region Initialization
    public GameObject[] Spires = new GameObject[spiresQuant];
    [Space(8)]
    public GameObject BaseSample;
    public GameObject SpireSample;
    public GameObject RingSample;
    [Space(8)]
    public GameObject UiSpeed;
    public InputField UiInput;
    public GameObject UiOutput;
    public GameObject UiStart;
    public GameObject UiEnd;

    public float Speed
    {
        get
        {
            return speed;
        }
        set
        {
            SpText.text = Math.Round(value).ToString();
            speed = value * RingsQuant;
        }
    }
    public int RingsQuant
    {
        get
        {
            return ringsQuant;
        }
        set
        {
            if (ringsQuant != value)
            {
                if (value >= 0)
                {
                    ringsQuant = value;
                    StartCoroutine(Restart());
                }
            }
        }
    }


    //----
    [SerializeField, Space(8)]
    private float scaleMult = 0.5f;
    private const int spiresQuant = 3;
    private GameObject[] bases = new GameObject[spiresQuant];
    private string warning = "Длина этого массива должна быть равна трём!";
    private string message = "преобразование не удалось,\n попробуйте снова...";
    private readonly List<GameObject> Rings = new List<GameObject>();
    private readonly float waitDivider = 5;
    private Vector3 scaleVector = new Vector3(1,0,1);
    private int ringsQuant = 0 ;
    private float speed = 20;
    private Text SpText;
    private Text OutText;
    #endregion
    
    public void Restarter()
    {
        StartCoroutine(Restart());
    }

    public void Starter()
    {
        StartCoroutine(MovingHandler());
    }

    public void CheckString()
    {
        int buffer;
        try
        {
            buffer = Convert.ToInt32(UiInput.text);
            if (buffer < 0)
            {
                throw new Exception();
            }
            else
            {
                RingsQuant = buffer;
            }
        }
        catch (Exception)
        {
            UiInput.text = "";
            OutText.text = message;
        }
    }
    //---
    // Use this for initialization
    void Start()
    {
        SpText = UiSpeed.GetComponent<Text>();
        OutText = UiOutput.GetComponent<Text>();
        UiStart.SetActive(true);
        UiEnd.SetActive(false);
        //---
        for (int i = 0; i < bases.Length; i++)
        {
            bases[i] = Instantiate(BaseSample,Vector3.down*2,Quaternion.Euler(Vector3.zero));
        }
    }

    private void OnValidate()
    {
        if (Spires.Length != spiresQuant)
        {
            Debug.LogWarning(warning);
            Array.Resize(ref Spires, spiresQuant);
        }
    }

    private IEnumerator AddRings()
    {
        for (int i = RingsQuant - 1 ; i >= 0; i--)
        {
            GameObject ring = Instantiate(RingSample);
            Transform parent = Spires[0].transform;
            ring.name += string.Format(" ({0})", i);
            ring.transform.parent = parent;
            ring.transform.position = parent.position + Vector3.down * parent.localScale.y + Vector3.up * (RingsQuant - i);
            ring.GetComponentInChildren<MeshRenderer>().material.color = UnityEngine.Random.ColorHSV();
            ring.transform.localScale += scaleVector * i * scaleMult ;
            Rings.Add(ring);
            yield return new WaitForSeconds(Time.deltaTime / waitDivider);
        }
    }

    private IEnumerator Restart()
    {
        for (int i = Rings.Count - 1 ; i >= 0; i--)
        {
            Destroy(Rings[i]);
            yield return new WaitForSeconds(Time.deltaTime / waitDivider);
        }
        Rings.Clear();
        yield return new WaitForSeconds(Time.deltaTime / waitDivider);
        yield return SetScene();
        yield return StartCoroutine(AddRings());

    }

    private IEnumerator SetScene()
    {
        Vector3 tempVector = Vector3.up * Math.Max(1, RingsQuant);
        Vector3 tempScale = scaleVector * RingsQuant * scaleMult ;
        float tempMult = tempScale.x + 3;
        for (int i = 0; i < spiresQuant; i++)
        {
            Spires[i].transform.localScale = SpireSample.transform.localScale + tempVector;
            bases[i].transform.localScale = BaseSample.transform.localScale + tempScale / 2;
        }
        Vector3 tempPosition = Spires[1].transform.parent.position + Vector3.up * Spires[1].transform.localScale.y;
        Vector3 basePosition = Spires[1].transform.parent.position;
        for (int i = 0; i < spiresQuant; i++)
        {
            if (RingsQuant >= 1)
            {
                switch (i)
                {
                    case 0:
                        Spires[i].transform.position = tempPosition + Vector3.left * tempMult; 
                        bases[i].transform.position = basePosition + Vector3.left * tempMult; break;
                    case 1:
                        Spires[i].transform.position = tempPosition; 
                        bases[i].transform.position = basePosition; break;
                    case 2:
                        Spires[i].transform.position = tempPosition + Vector3.right * tempMult;
                        bases[i].transform.position = basePosition + Vector3.right * tempMult; break;
                    default:
                        break;
                }
            }
        }
        yield break;
    }

    private IEnumerator MovingHandler()
    {
        yield return StartCoroutine(SetMoving(0, Spires[0], Spires[2], Spires[1]));
        UiEnd.SetActive(true);
    }

    private IEnumerator SetMoving(int number, GameObject source, GameObject target, GameObject buffer)
    {
        if (number < Rings.Count)
        {
            yield return StartCoroutine(SetMoving(number + 1, source, buffer, target));
            yield return StartCoroutine(DoMoving(Rings[number].transform, target.transform));
            yield return StartCoroutine(SetMoving(number + 1, buffer, target, source));
        }
    }
    
    private IEnumerator DoMoving(Transform ring, Transform target)
    {
        yield return StartCoroutine(MovingTowards(ring, ring.parent.position));
        yield return StartCoroutine(MovingTowards(ring, target.position));
        Vector3 targetPosition = target.position + Vector3.down * target.localScale.y + Vector3.up * Mathf.Max(0, target.childCount);
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
