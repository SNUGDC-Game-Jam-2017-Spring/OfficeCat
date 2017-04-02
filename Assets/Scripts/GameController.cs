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
    public Transform paperStackAir;

    public int currentWork = 0;
    public int currentWorkToDo = 15;
    public int totalWorkToDo = 15;
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

    public bool isWorking {
        get { return playingTimeCount < 3; } }
    float playingTimeCount = 0f;
    int workDone;
    bool completed = false;

	void Start ()
    {
        paperStackResizable.localScale = new Vector3(1, Level.currentWorkToDo, 1);
        main.text = "서류들에 앞발 도장을 찍자!\n"+ currentWork + "/" + totalWorkToDo;
        workDone = currentWork;
    }
	

	void Update ()
    {
        currentTime += Time.deltaTime;
        time.text = ""+ currentTime;
        main.text = "서류들에 앞발 도장을 찍자!\n" + currentWork + "/" + totalWorkToDo;
        if (workDone != currentWork)
        {
            playingTimeCount = 0f;
            workDone = currentWork;
        }
        else
        {
            playingTimeCount += Time.deltaTime;
        }
        if(currentTime <= 120 && currentTime >= 110 && !completed)
        {
            if (currentWork == currentWorkToDo)
            {
                workDoneCanvas.gameObject.SetActive(true);
                completed = true;
            }
        }
        else if(currentTime >= 120f)
        {
            currentTime -= 120f;
        }
        if(WarningCount == 4 && !completed)
        {
            gameOverCanvas.gameObject.SetActive(true);
            completed = true;
        }
    }
    void addWarningCount()
    {
        WarningCount++;
    }
    public void AddWork()
    {
        if(!completed && paperStackResizable.gameObject.activeInHierarchy == false)
        {
            paperStackResizable.gameObject.SetActive(true);
            currentWorkToDo += 5;
            paperStackResizable.localScale = new Vector3(1, currentWorkToDo, 1);
            paperStackAir.localPosition = new Vector3(0, paperStackAir.localPosition.y + currentWorkToDo * 0.01f, 0);
            totalWorkToDo += currentWorkToDo;
        }
    }
}
