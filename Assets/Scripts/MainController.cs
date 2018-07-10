using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainController : MonoBehaviour
{
    #region ==== Fields ====
    //----public----
    /// <summary>(массив стержней)</summary>
    public GameObject[] Spires = new GameObject[spiresQuant];
    /// <summary>(экземпляр поверхности)</summary>
    [Space(8)]
    public GameObject Plane;
    /// <summary>(префаб основания)</summary>
    public GameObject BaseSample;
    /// <summary>(префаб стержня)</summary>
    public GameObject SpireSample;
    /// <summary>(префаб диска)</summary>
    public GameObject RingSample;
    [Space(8)]
    /// <summary>(UI GameObject)</summary>
    public GameObject UIObj;
    /// <summary>(скорость движения)</summary>
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
    /// <summary>(длина массива дисков)</summary>
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
                    Restarter();
                }
            }
        }
    }
    //----private----
    /// <summary>(рамерный множитель)</summary>
    [SerializeField, Space(8)]
    private float scaleMult = 0.5f;
    private const int spiresQuant = 3;
    private GameObject[] bases = new GameObject[spiresQuant];
    private readonly string warning = "Длина этого массива должна быть равна трём!";
    private readonly List<GameObject> Rings = new List<GameObject>();
    private Vector3 scaleVector = new Vector3(1,0,1);
    private int ringsQuant = 0 ;
    private float speed;
    private readonly float startSpeed = 10;
    private Text SpText;
    private UIController UC;

    private bool isReady;
    #endregion
    #region ==== Methods ====
    #region ~~~~ Wrap ~~~~
    //----public----
    /// <summary>
    /// (запускает корутину <see cref="Restart"/>)
    /// </summary>
    public void Restarter()
    {
        StartCoroutine(Restart());
    }

    /// <summary>
    /// (запускает корутину <see cref="MovingHandler"/>)
    /// </summary>
    public void Starter()
    {
        StartCoroutine(MovingHandler());
    }
    #endregion
    #region ~~~~ MonoBehaviour ~~~~
    //----private----
    private void Start()
    {
        UC = UIObj.GetComponent<UIController>(); 
        SpText = UC.Speed.GetComponent<Text>();
        //---
        for (int i = 0; i < bases.Length; i++)
        {
            bases[i] = Instantiate(BaseSample);
            bases[i].transform.position = Vector3.right * Spires[i].transform.position.x ;
            bases[i].transform.parent = transform;
            bases[i].name += string.Format(" ({0})", i);
        }
        Plane.transform.parent = bases[1].transform;
    }

    private void OnValidate()
    {
        if (Spires.Length != spiresQuant)
        {
            Debug.LogWarning(warning);
            Array.Resize(ref Spires, spiresQuant);
        }
    }
    #endregion
    #region ~~~~ Resize Objects ~~~~
    /// <summary>
    /// (создает заданное количество дисков)
    /// </summary>
    public IEnumerator AddRings()
    {
        for (int i = RingsQuant - 1 ; i >= 0; i--)
        {
            GameObject ring = Instantiate(RingSample);
            Transform parent = Spires[0].transform;
            ring.name += string.Format(" ({0})", (RingsQuant - 1 - i));
            ring.transform.parent = parent;
            ring.transform.position = parent.position + Vector3.down * parent.localScale.y + Vector3.up * (RingsQuant - i);
            ring.GetComponentInChildren<MeshRenderer>().material.color = UnityEngine.Random.ColorHSV();
            ring.transform.localScale += scaleVector * i * scaleMult ;
            Rings.Add(ring);
        }
        yield break;
    }

    /// <summary>
    /// (возвращает сцену в начальное состояние)
    /// </summary>
    private IEnumerator Restart()
    {
        isReady = false;
        for (int i = Rings.Count - 1 ; i >= 0; i--)
        {
            Destroy(Rings[i]);
        }
        Rings.Clear();
        yield return StartCoroutine(SetScene());
        yield return StartCoroutine(AddRings());
        isReady = true;
    }

    /// <summary>
    /// (модифицирует объекты на сцене)
    /// </summary>
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
        for (int i = 0; i < spiresQuant; i++)
        {
            if (RingsQuant >= 0)
            {
                switch (i)
                {
                    case 0:
                        Spires[i].transform.position = tempPosition + Vector3.left * tempMult; break;
                    case 1:
                        Spires[i].transform.position = tempPosition; break;
                    case 2:
                        Spires[i].transform.position = tempPosition + Vector3.right * tempMult; break; 
                    default:
                        break;
                }
                bases[i].transform.position = Vector3.right * Spires[i].transform.position.x;
            }
        }
        yield break;
    }
    #endregion
    #region ~~~~ Move Objects ~~~~
    /// <summary>
    /// (при возможности запускает корутину <see cref="SetMoving"/> и по завершению активирует <see cref="UIController.OnEnd"/>)
    /// </summary>
    private IEnumerator MovingHandler()
    {
        while (!isReady)
        {
            yield return null;
        }
        speed = RingsQuant * startSpeed;
        yield return StartCoroutine(SetMoving(Spires[0], Spires[2], Spires[1]));
        UC.OnEnd.SetActive(true);
    }

    /// <summary>
    /// (определяет порядок перемещений и запускает корутину <see cref="DoMoving"/> необходимое количество раз)
    /// </summary>
    /// <param name="source">(начальный стержень)</param>
    /// <param name="target">(конечный стержень)</param>
    /// <param name="buffer">(вспомогательный стержень)</param>
    /// <param name="endIndex">(индекс нижнего диска)</param>
    private IEnumerator SetMoving(GameObject source, GameObject target, GameObject buffer, uint endIndex = 0)
    {
        if (endIndex < RingsQuant)
        {
            for (int i = RingsQuant - 1 ; i >= (int)endIndex; i--)
            {
                if (i % 2 == endIndex % 2)
                {
                    yield return StartCoroutine(DoMoving(Rings[i].transform, target.transform));
                    yield return StartCoroutine(SetMoving(buffer, target, source, (uint)i + 1));
                }
                else
                {
                    yield return StartCoroutine(DoMoving(Rings[i].transform, buffer.transform));
                    yield return StartCoroutine(SetMoving(target, buffer, source, (uint)i + 1));
                }
            }
        }
    }

    /// <summary>
    /// (реализует перемещение на всех отрезках)
    /// </summary>
    /// <param name="ring">(Transform диска)</param>
    /// <param name="target">(Transform конечного стержня)</param>
    private IEnumerator DoMoving(Transform ring, Transform target)
    {
        yield return StartCoroutine(MovingTowards(ring, ring.parent.position));
        yield return StartCoroutine(MovingTowards(ring, target.position));
        Vector3 targetPosition = target.position + Vector3.down * target.localScale.y + Vector3.up * Mathf.Max(0, target.childCount);
        yield return StartCoroutine(MovingTowards(ring, targetPosition));
        ring.parent = target;
    }

    /// <summary>
    /// (реализует перемещение на текущем отрезке)
    /// </summary>
    /// <param name="ring">(Transform диска)</param>
    /// <param name="targetPosition">(Position конечного стержня)</param>
    /// <returns></returns>
    private IEnumerator MovingTowards(Transform ring, Vector3 targetPosition)
    {
        float step;
        while ((targetPosition - ring.position).magnitude > 0.001)
        {
            step = Speed * Time.deltaTime;
            ring.position = Vector3.MoveTowards(ring.position, targetPosition, step);
            yield return null;
        }
    }
    #endregion
    #endregion
}
