using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VR;

public static class Level
{
    public static int currentWork = 0;
    public static int currentWorkToDo = 15;
    public static int WarningCount = 0;
}

public class GameController : MonoBehaviour {

    public Transform paperStackResizable;
    public Text main;

    bool isWorking {
        get { return playingTimeCount < 3; } }
    float playingTimeCount = 0f;
    int currentWork;

	void Start ()
    {
        paperStackResizable.localScale = new Vector3(1, Level.currentWorkToDo, 1);
        main.text = "서류들에 앞발 도장을 찍자!\n"+ Level.currentWork + "/" + Level.currentWorkToDo;
        currentWork = Level.currentWork;
    }
	

	void Update ()
    {
        main.text = "서류들에 앞발 도장을 찍자!\n" + Level.currentWork + "/" + Level.currentWorkToDo;
        if (currentWork != Level.currentWork)
        {
            playingTimeCount = 0f;
            Debug.Log("Working");
            currentWork = Level.currentWork;
        }
        else
        {
            playingTimeCount += Time.deltaTime;
        }
    }
}
