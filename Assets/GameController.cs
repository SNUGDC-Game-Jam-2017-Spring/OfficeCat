using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class Level
{
    public static int currentWork = 0;
    public static int currentWorkToDo = 15;
    public static int WarningCount = 0;
}

public class GameController : MonoBehaviour {

    public Transform paperStackResizable;
    public Text main;

	void Start ()
    {
        paperStackResizable.localScale = new Vector3(1, Level.currentWorkToDo, 1);
        main.text = "서류들에 앞발 도장을 찍자!\n"+ Level.currentWork + "/" + Level.currentWorkToDo;
    }
	

	void Update ()
    {
        main.text = "서류들에 앞발 도장을 찍자!\n" + Level.currentWork + "/" + Level.currentWorkToDo;
    }
}
