﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskManager : MonoBehaviour {

    public class Task
    {
        public Texture image;
        public float timer;
        public System.Action<Vector3> function;
        public Vector3 position;

        public Task(Texture image, float timer, System.Action<Vector3> function, Vector3 position)
        {
            this.image = image;
            this.timer = timer;
            this.function = function;
            this.position = position;
        }
    }

    //TIMED TASKS THE PLAYER MUST DO

    //UI
    private GameObject taskBox;
    private Animator iconAnimator;
    public Texture foodImage;
    public Texture waterImage;
    public Texture barkCatImage;
    private GameObject currentImageObject;
    private RawImage currentImage;
    private RectTransform timerBar;
    private Vector3 startWidth = new Vector3(1, 1, 1);
    private Vector3 endWidth = new Vector3(0, 1, 1);
    private bool timerOn = true;
    private Image gameOverScreen;
    private bool gameOver = false;
    private bool isPaused = false;

    //Positions
    private Vector3 catPos;
    private Vector3 waterBowlPos;
    private Vector3 foodBowlPos;
    public static string taskLocation = "";

    //Tasks
    private Task currentTask;
    private List<Task> tasks = new List<Task>();
    private int task;

    void Start () {
        EventBus.AddListener<PauseEvent>(HandleEvent);

        //set-up transforms
        catPos = GameObject.FindGameObjectWithTag("Cat").GetComponent<Transform>().position;
        waterBowlPos = GameObject.FindGameObjectWithTag("WaterBowl").GetComponent<Transform>().position;
        foodBowlPos = GameObject.FindGameObjectWithTag("FoodBowl").GetComponent<Transform>().position;

        //Get UI:
        currentImageObject = GameObject.FindGameObjectWithTag("TaskImage");
        currentImage = currentImageObject.GetComponent<RawImage>();
        taskBox = GameObject.FindGameObjectWithTag("TaskBox");
        timerBar = GameObject.FindGameObjectWithTag("TimerBar").GetComponent<RectTransform>();
        iconAnimator = taskBox.GetComponentInChildren<Animator>();
        gameOverScreen = GameObject.FindGameObjectWithTag("GameOverScreen").GetComponent<Image>();

        //Fill tasks arraylist:
        tasks.Add(new Task(barkCatImage, 15f, BarkAt, catPos));
        tasks.Add(new Task(foodImage, 20f, EatAt, foodBowlPos));
        tasks.Add(new Task(waterImage, 20f, DrinkAt, waterBowlPos));

        GetNewTask(); //tasks all have their own text, times, and RULES
    }

    public void GetNewTask()
    {
        currentTask = TaskRandomizer();
        print("New Task");
        currentImage.texture = currentTask.image;
        StartCoroutine(LerpTimer());
        currentTask.function(currentTask.position); //set current task rules
    }

    public void CheckTask(string action)
    {
        if (currentTask != null)
        {
            if (string.Compare(action, taskLocation) == 0)
            {
                TaskComplete();
            }
        }
    }

    //Randomly picks a task from list
    private Task TaskRandomizer()
    {
        task = Random.Range(0, tasks.Count);
        return tasks[task];
    }

    private void TaskComplete()
    {
        timerOn = false;
        taskBox.SetActive(false);
        currentImageObject.transform.rotation = Quaternion.Euler(0, 0, 0);
        iconAnimator.SetBool("Urgent", false);
        StartCoroutine(TaskDelay());
    }

    private IEnumerator TaskDelay()
    {
        yield return new WaitForSeconds(1.8f);
        timerOn = true;
        GetNewTask();
        yield return new WaitForSeconds(0.1f);
        taskBox.SetActive(true); //show task to player
    }

    //TASK RULES

    private void BarkAt(Vector3 location)
    {
        taskLocation = "cat";
    }

    private void DrinkAt(Vector3 location)
    {
        taskLocation = "water";
    }

    private void EatAt(Vector3 location)
    {
        taskLocation = "food";
    }

    IEnumerator LerpTimer()
    {
        float progress = 0;
        float speed = currentTask.timer / 500;

        while (progress <= 1)
        {
            while (isPaused)
            {
                yield return null;
            }

            timerBar.localScale = Vector3.Lerp(startWidth, endWidth, progress);
            progress += Time.deltaTime * speed;
            yield return null;

            if (timerOn == false)
            {
                yield break;
            }

            if (progress >= 0.5)
            {
                iconAnimator.SetBool("Urgent", true);
            }

        }
        timerBar.localScale = endWidth;
        GameOver();
    }

    public void GameOver()
    {
        timerOn = false;
        gameOver = true;
        EventBus.Emit<GameOverEvent>(new GameOverEvent()); //game over
    }

    private void Update()
    {
        if (gameOver)
        {
            //GAME OVER
            Color color = gameOverScreen.color;
            color.a = Mathf.MoveTowards(color.a, 255, Time.deltaTime);
            gameOverScreen.color = color;
        }
    }

    private void HandleEvent(PauseEvent msg)
    {
        isPaused = !isPaused;
    }

}
