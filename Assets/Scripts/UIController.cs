using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    #region ==== Fields ====
    /// <summary>(Main GameObject)</summary>
    public GameObject MainObj;
    [Space(8)]
    public GameObject Speed;
    /// <summary>(UI элемент для ввода)</summary>
    public InputField Input;
    /// <summary>(UI элемент для вывода)</summary>
    public GameObject Output;
    /// <summary>(начальный UI элемент)</summary>
    public GameObject OnStart;
    /// <summary>(конечный UI элемент)</summary>
    public GameObject OnEnd;

    //---
    private Text OutText;
    private string message = "преобразование не удалось,\n попробуйте снова...";
    private MainController MC;
    #endregion
    #region ==== Methods ====
    //---
    /// <summary>
    /// (проверяет строку введенную пользователем)
    /// </summary>
    public void CheckString()
    {
        int buffer;
        try
        {
            buffer = Convert.ToInt32(Input.text);
            if (buffer < 0)
            {
                throw new Exception(message);
            }
            else
            {
                MC.RingsQuant = buffer;
            }
        }
        catch (Exception)
        {
            Input.text = "";
            OutText.text = message;
        }
    }
    //----
    // Use this for initialization
    void Start()
    {
        MC = MainObj.GetComponent<MainController>();
        OnStart.SetActive(true);
        OnEnd.SetActive(false);
        OutText = Output.GetComponent<Text>();
    }
    #endregion
}
