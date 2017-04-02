using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VR;
using UnityEngine.SceneManagement;

public static class Level
{
    public static int currentWork
    {
        get
        {
            return GameController.instance.currentWork;
        }
        set
        {
            GameController.instance.currentWork = value;
        }
    }
    public static int currentWorkToDo
    {
        get
        {
            return GameController.instance.currentWorkToDo;
        }
    }
    public static int WarningCount
    {
        get
        {
            return GameController.instance.WarningCount;
        }
        set
        {
            GameController.instance.WarningCount = value;
        }
    }
    public static float currentTime
    {
        get
        {
            return GameController.instance.currentTime;
        }
    }
}

public class GameController : MonoBehaviour {

    public Transform paperStackResizable;
    public Text main;
    public Text time;
    public Canvas workDoneCanvas;
    public Canvas gameOverCanvas;

    public int currentWork = 0;
    public int currentWorkToDo = 15;
    public int WarningCount = 0;
    public float currentTime = 0;

    private static GameController _instance;
    public static GameController instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<GameController>();
            }
            return _instance;
        }
    }

    bool isWorking {
        get { return playingTimeCount < 3; } }
    float playingTimeCount = 0f;
    int workDone;

	void Start ()
    {
        paperStackResizable.localScale = new Vector3(1, Level.currentWorkToDo, 1);
        main.text = "서류들에 앞발 도장을 찍자!\n"+ Level.currentWork + "/" + Level.currentWorkToDo;
        workDone = Level.currentWork;
    }
	

	void Update ()
    {
        currentTime += Time.deltaTime;
        time.text = ""+ Level.currentTime;
        main.text = "서류들에 앞발 도장을 찍자!\n" + Level.currentWork + "/" + Level.currentWorkToDo;
        if (workDone != Level.currentWork)
        {
            playingTimeCount = 0f;
            workDone = Level.currentWork;
        }
        else
        {
            playingTimeCount += Time.deltaTime;
        }
        if(Level.currentWork == Level.currentWorkToDo && Level.currentTime >= 120)
        {
            workDoneCanvas.gameObject.SetActive(true);
            currentTime -= 120;
        }
        else if(Level.WarningCount == 3)
        {
            gameOverCanvas.gameObject.SetActive(true);
        }
    }
    void addWarningCount()
    {
        WarningCount++;
    }
}
