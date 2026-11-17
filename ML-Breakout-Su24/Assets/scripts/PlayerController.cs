using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.SceneManagement;
using System;


public class PlayerController : Agent
{
    public int environmentIndex;
    public int playerIndex;
    public Transform ballTransform;
    public Transform leftWall;
    public Transform rightWall;
    public BrickManager brickManager;

    public float leftBoundary;
    public float rightBoundary;
    private Rigidbody2D ballRigidbody;
    public ballStart ball;
    private KeepScore keepScore;

    public float speed = 5;
    public float startingY = -4.52f;
    public float startingX = 4.28f;
    public float deathLineY = -5.5f;
    private int lastScore = 0;
    private int lastKnownLevel = 0;
    const int maxBrickObservations = 50;
    const int maxItemObservations = 10;

    public int paddleSizeItemCount = 0;
    public int ballSpeedItemCount = 0;

    private Vector3 originalPaddleScale;
    private float originalBallSpeed;

    public float Lwall = .88f;
    public float Rwall = 8f;

    public float sTime;

    void Start()
    {
        sTime = 0;
        ballRigidbody = ballTransform.GetComponent<Rigidbody2D>();
        ball = ballTransform.GetComponent<ballStart>();
        keepScore = GameObject.Find("Canvas").GetComponent<KeepScore>();
        lastScore = playerIndex == 1 ? keepScore.score1 : keepScore.score2;

        // if playerIndex is 1, set the left boundary to 0 and the right boundary to 8.56
        if (playerIndex == 1)
        {
            leftBoundary = -9;
            rightBoundary = 8.56f;
        }
        // if playerIndex is 2, set the left boundary to -8.56 and the right boundary to 0
        else if (playerIndex == 2)
        {
            leftBoundary = -8.56f;
            rightBoundary = 9;
        }

        startingX = playerIndex == 1 ? 4.28f : -4.28f;
        // Debug.Log("startinX: " + startingX + "playerindex " + playerIndex);

        originalPaddleScale = new Vector3(1.0f, 1.0f, 1);
        originalBallSpeed = ballRigidbody.velocity.magnitude;
    }

    public override void OnEpisodeBegin()
    {
        // Reset paddle position and size
        transform.position = new Vector3(startingX, startingY, 0);
        ResetPaddleSize();

        // Reset ball position and velocity
        ball.ResetPositionAndVelocity();
        // ballTransform.position = new Vector3(startingX, startingY + 1.0f, 0);     
        // ballRigidbody.velocity = Vector2.zero;

        // Add initial velocity to the ball
        // ballRigidbody.AddForce(new Vector2(UnityEngine.Random.Range(-1.0f, 1.0f), 1).normalized * -1.0f, ForceMode2D.Impulse);

        // Reset score
        keepScore.score1 = 0;
        keepScore.score2 = 0;

        // Reset levels
        brickManager.player1Level = 0;
        brickManager.player2Level = 0;

        // Reset lives
        keepScore.lives1 = 3;
        keepScore.lives2 = 3;

        // reset bricks in brick manager
        brickManager.ResetGame();

        paddleSizeItemCount = 0;
        ballSpeedItemCount = 0;

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Position and size of the paddle
        sensor.AddObservation(transform.position.x);

        // Position and velocity of the ball
        sensor.AddObservation(ballTransform.position);
        sensor.AddObservation(ballRigidbody.velocity);

        // Distances to walls
        sensor.AddObservation(transform.position.x - leftWall.position.x);
        sensor.AddObservation(rightWall.position.x - transform.position.x);

        sensor.AddObservation(transform.localScale.x);

        sensor.AddObservation(playerIndex == 1 ? keepScore.lives1 : keepScore.lives2);



        //Brick Observations
        int gridRows = brickManager.rows;
        int gridColumns = brickManager.columns;
        float brickWidth = brickManager.brickWidth;
        float brickHeight = brickManager.brickHeight;
        float yAdjust = brickManager.yAdjust;
        float roof = brickManager.roof - yAdjust;
        List<GameObject> playerBricks = playerIndex == 1 ? brickManager.player1Bricks : brickManager.player2Bricks;

        float[,] brickGrid = new float[gridRows, gridColumns];
        foreach (var brick in playerBricks)
        {
            int row = Mathf.FloorToInt((roof - brick.transform.position.y) / brickHeight);
            int column = Mathf.FloorToInt((brick.transform.position.x - leftBoundary) / brickWidth);
            // Debug.Log("playerIndex: " + playerIndex + " Row: " + row + " Column: " + column + "brick position: " + brick.transform.position.y + " " + roof  + " " + brickHeight);
            if (row >= 0 && row < gridRows && column >= 0 && column < gridColumns)
            {
                // Debug.Log("Row: " + row + " Column: " + column);
                var brickComponent = brick.GetComponent<brick>();
                brickGrid[row, column] = brickComponent.health == 2 ? 2f : 1f;
            }
        }

        // Debug.Log("Grid rows: " + gridRows + " Grid columns: " + gridColumns);
        // Add brick grid observations
        for (int i = 0; i < gridRows; i++)
        {
            for (int j = 0; j < gridColumns; j++)
            {
                sensor.AddObservation(brickGrid[i, j]);
                // Debug.Log(playerIndex + "Brick grid: " + brickGrid[i, j]);
            }
        }
        // Debug.Log("Brick grid observations end");


    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float move = Math.Clamp(actions.ContinuousActions[0], -1f, 1f);

        Vector3 newPosition = transform.position + Vector3.right * move * Time.deltaTime * speed;
        newPosition.x = Mathf.Clamp(newPosition.x, leftBoundary, rightBoundary);

        transform.position = newPosition;

        // add small negative reward for each step to penalize time
        AddReward(-0.001f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = 0;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            continuousActionsOut[0] = -1;
            // Debug.Log("left");
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            continuousActionsOut[0] = 1;
            // Debug.Log("right");
        }
    }

    private void Update()
    {
        //Bounds
        if (gameObject.transform.position.x < Lwall)
        {
            gameObject.transform.position = new Vector3(Lwall, gameObject.transform.position.y, 0);
        }

        if (gameObject.transform.position.x > Rwall)
        {
            gameObject.transform.position = new Vector3(Rwall, gameObject.transform.position.y, 0);
        }

        if (Time.time - sTime > 300)
        {
            EndEpisodeForAllPlayers();
        }
    }

    void FixedUpdate()
    {
        int playerLevel = playerIndex == 1 ? brickManager.player1Level : brickManager.player2Level;
        List<Item> playerItems = playerIndex == 1 ? brickManager.player1ActiveItems : brickManager.player2ActiveItems;
        // if player level has changed, reset ball and paddle
        if (playerLevel != lastKnownLevel)
        {
            lastKnownLevel = playerLevel;
            ball.ResetBallAndPause();
            // ballTransform.position = new Vector3(startingX, startingY + 1.0f, 0);
            // Debug.Log("levelchange - startinX: " + startingX + "playerindex " + playerIndex);
            // ballRigidbody.velocity = Vector2.zero;
            // ballRigidbody.AddForce(new Vector2(UnityEngine.Random.Range(-1.0f, 1.0f), 1).normalized * 10.0f, ForceMode2D.Impulse);
            transform.position = new Vector3(startingX, startingY, 0);
            foreach (Item powerUp in playerItems)
            {
                Destroy(powerUp.gameObject);
            }
        }

        if (playerLevel > brickManager.numberOfLevels)
        {
            Debug.Log("Player has completed all levels");
            AddReward(10.0f);
            if (playerIndex == 1)
            {
                SceneManager.LoadScene("YouLose");
                EndEpisodeForAllPlayers();
            }
            else
            {
                SceneManager.LoadScene("YouWin");
                EndEpisodeForAllPlayers();
            }
            EndEpisodeForAllPlayers();
        }

        // if (ballTransform.position.y <= deathLineY)
        // {
        //     Debug.Log("Ball fell off");
        //     AddReward(-3.0f);

        //     if (playerIndex == 1)
        //     {
        //         keepScore.lives1 -= 1;
        //         if (keepScore.lives1 < 0)
        //         {
        //             Debug.Log("Player 1 has lost all lives" + keepScore.lives1);
        //             AddReward(-10.0f);
        //             EndEpisode();
        //         }
        //     }
        //     else if (playerIndex == 2)
        //     {
        //         keepScore.lives2 -= 1;
        //         if (keepScore.lives2 < 0)
        //         {
        //             Debug.Log("Player 2 has lost all lives" + keepScore.lives2);    
        //             AddReward(-10.0f);
        //             EndEpisode();
        //         }
        //     }        
        // }
        CheckBrickCollisions();
    }

    void CheckBrickCollisions()
    {
        // check if brickManager or its bricks list is null
        if (brickManager == null || brickManager.player1Bricks == null || brickManager.player2Bricks == null)
        {
            Debug.LogError("brickManager or bricks list is null");
            return;
        }

        // Debug.Log("Bricks count: " + brickManager.player1Bricks.Count);

        // check if the bricks list in brickManager is empty
        List<GameObject> playerBricks = playerIndex == 1 ? brickManager.player1Bricks : brickManager.player2Bricks;
        if (playerBricks.Count == 0)
        {
            AddReward(1.0f);
            brickManager.CheckBricksAndLevelUp(playerIndex);
            Debug.Log("All bricks destroyed");
        }
        else
        {
            // make sure ballTransform is not null before proceeding
            if (ballTransform == null)
            {
                Debug.LogError("ballTransform is null");
                return;
            }


            // check if score went up and reward if it does
            int currentScore = playerIndex == 1 ? keepScore.score1 : keepScore.score2;
            if (currentScore > lastScore)
            {
                float reward = currentScore - lastScore;
                AddReward(reward);
                lastScore = currentScore;
            }


        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Debug.Log("collided");
        if (collision.gameObject.CompareTag("Ball"))
        {
            AddReward(10f); // Reward for hitting the ball
        }
    }

    public void ResetPaddleSize()
    {
        // Debug.Log("Resetting paddle size");
        // transform.localScale = originalPaddleScale;
        transform.localScale = new Vector3(1.0f, 1.0f, 1);
        paddleSizeItemCount = 0;
    }

    public void EndEpisodeForAllPlayers()
    {
        sTime = Time.time;
        Debug.Log("Ending episode for all players");
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            // if player is current player, skip
            if (player == gameObject)
            {
                continue;
            }
            PlayerController playerController = player.GetComponent<PlayerController>();
            playerController.EndEpisode();
        }
        // find all balls and reset them
        GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");
        foreach (GameObject ball in balls)
        {
            ball.GetComponent<ballStart>().ResetBallAndPause();
        }
        EndEpisode();
    }

}



