using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using Unity.MLAgents.Policies;
using UnityEngine.SceneManagement;

public class FunPlayerController : Agent
{
    public int environmentIndex;
    public int playerIndex;
    public bool isPlayer = false;
    public Transform ballTransform;
    public Transform leftWall;
    public Transform rightWall;
    public FunBrickManager brickManager;

    public float leftBoundary;
    public float rightBoundary;
    private Rigidbody2D ballRigidbody;
    public FunBallStart ball;
    private FunKeepScore keepScore;

    public float speed = 5;
    public float startingY;
    public float startingX;
    public float deathLineY;
    private int lastScore = 0;
    private int lastKnownLevel = 0;
    const int maxBrickObservations = 50;
    const int maxItemObservations = 10;
    
    public int paddleSizeItemCount = 0;
    public int ballSpeedItemCount = 0;
    public int lastKnownPaddleSizeItemCount = 0;
    public float maxBounceAngle = 80.0f;

    private Vector3 originalPaddleScale;
    private float originalBallSpeed;
    public float sTime;

    void Start()
    {
        sTime = 0;
        ballRigidbody = ballTransform.GetComponent<Rigidbody2D>();
        ball = ballTransform.GetComponent<FunBallStart>();
        keepScore = GameObject.Find("Canvas").GetComponent<FunKeepScore>();
        lastScore = playerIndex == 1 ? keepScore.score1 : keepScore.score2;

        // NEW: Calculate boundaries relative to screen or parent object
        leftBoundary = leftWall.GetComponent<BoxCollider2D>().bounds.max.x; // NEW
        rightBoundary = rightWall.GetComponent<BoxCollider2D>().bounds.min.x; // NEW

        // NEW: Calculate starting positions based on boundaries
        startingX = playerIndex == 1 ? (leftBoundary + rightBoundary) / 2 : (leftBoundary + rightBoundary) / 2; // NEW
        startingY = transform.position.y; // NEW

        deathLineY = brickManager.deathLineY; // NEW

        originalPaddleScale = new Vector3(1.0f, 1.0f, 1);
        originalBallSpeed = ballRigidbody.velocity.magnitude;

        // check if player has a nn model
        if (GetComponent<BehaviorParameters>().Model == null)
        {
            isPlayer = true;
        }

    }

    public override void OnEpisodeBegin()
    {
        // Reset paddle position and size
        if (playerIndex == 1)
        {
            transform.position = new Vector3(startingX, startingY, 0);
        }
        else if (playerIndex == 2)
        {
            transform.position = new Vector3(startingX, startingY, 0);
        }
        ResetPaddleSize();

        // Reset ball position and velocity
        ball.ResetPositionAndVelocity();

        // Reset score
        keepScore.score1 = 0;
        keepScore.score2 = 0;

        // Reset levels
        brickManager.player1Level = 0;
        brickManager.player2Level = 0;

        // Reset lives
        keepScore.lives1 = 3;
        keepScore.lives2 = 3;

        // Reset bricks in brick manager
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

        sensor.AddObservation(paddleSizeItemCount);
        sensor.AddObservation(playerIndex == 1 ? keepScore.lives1 : keepScore.lives2);

        // Find the closest item to the deathline
        FunItem closestItem = null;
        float closestDistance = float.MaxValue;
        List<FunItem> playerItems = playerIndex == 1 ? brickManager.player1ActiveItems : brickManager.player2ActiveItems;
        foreach (var item in playerItems)
        {
            if (item == null)
            {
                continue;
            }
            float distance = Mathf.Abs(item.transform.position.y - deathLineY);
            if (distance < closestDistance)
            {
                closestItem = item;
                closestDistance = distance;
            }
        }

        // Add item observations (position, velocity, and type)
        if (closestItem != null)
        {
            Rigidbody2D itemRb = closestItem.GetComponent<Rigidbody2D>();
            sensor.AddObservation(closestItem.transform.position);
            sensor.AddObservation(itemRb != null ? itemRb.velocity : Vector2.zero);
            sensor.AddObservation((int)closestItem.itemType);
        }
        else
        {
            // Add default observations if no item is found
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector2.zero);
            sensor.AddObservation(-1); // No item type
        }

        int gridRows = brickManager.rows;
        int gridColumns = brickManager.columns;
        float brickWidth = brickManager.brickWidth;
        float brickHeight = brickManager.brickHeight;
        float yAdjust = brickManager.yAdjustForBrick;
        float roof = brickManager.roof - yAdjust;
        List<GameObject> playerBricks = playerIndex == 1 ? brickManager.player1Bricks : brickManager.player2Bricks;

        float[,] brickGrid = new float[gridRows, gridColumns];
        foreach (var brick in playerBricks)
        {
            int row = Mathf.FloorToInt((roof - brick.transform.position.y) / brickHeight);
            int column = Mathf.FloorToInt((brick.transform.position.x - leftBoundary) / brickWidth);
            if (row >= 0 && row < gridRows && column >= 0 && column < gridColumns)
            {
                var brickComponent = brick.GetComponent<FunBrick>();
                brickGrid[row, column] = brickComponent.health == 2 ? 2f : 1f;
            }
        }

        // Add brick grid observations
        for (int i = 0; i < gridRows; i++)
        {
            for (int j = 0; j < gridColumns; j++)
            {
                sensor.AddObservation(brickGrid[i, j]);
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float move = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);

        Vector3 newPosition = transform.position + Vector3.right * move * Time.deltaTime * speed;
        newPosition.x = Mathf.Clamp(newPosition.x, leftBoundary, rightBoundary); // NEW

        transform.position = newPosition;

        // Add small negative reward for each step to penalize time
        AddReward(-0.001f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = 0;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            continuousActionsOut[0] = -1;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            continuousActionsOut[0] = 1;
        }
    }

    private void Update()
    {
        // if (isPlayer)
        // {
        //     if (Input.GetKey(KeyCode.Q))
        //     {
        //         transform.Translate(Vector3.left * Time.deltaTime * speed);
        //     }

        //     if (Input.GetKey(KeyCode.E))
        //     {
        //         transform.Translate(Vector3.right * Time.deltaTime * speed);
        //     }
        // }
        

        if (playerIndex == 1)
        {
            // bounds for player 1, if leftmost side of paddle is less than 0, set it to 0
            if (transform.position.x - transform.localScale.x*3/4 < leftBoundary)
            {
                transform.position = new Vector3(leftBoundary + transform.localScale.x*3/4, transform.position.y, transform.position.z);
            }
            // if rightmost side of paddle is greater than 8.56, set it to rightBoundary
            else if (transform.position.x + transform.localScale.x / 2 > rightBoundary)
            {
                transform.position = new Vector3(rightBoundary - transform.localScale.x / 2, transform.position.y, transform.position.z);
            }
        }
        else if (playerIndex == 2)
        {
            // bounds for player 2
            if (transform.position.x + transform.localScale.x*3/4 > rightBoundary)
            {
                transform.position = new Vector3(rightBoundary - transform.localScale.x*3/4, transform.position.y, transform.position.z);
            }
            else if (transform.position.x - transform.localScale.x / 2 < leftBoundary)
            {
                transform.position = new Vector3(leftBoundary + transform.localScale.x / 2, transform.position.y, transform.position.z);
            }

        }
        // if (Time.time - sTime > 100) //TEMP 300
        // {
        //     EndEpisodeForAllPlayers();
        // }

    }

    void FixedUpdate()
    {
        int playerLevel = playerIndex == 1 ? brickManager.player1Level : brickManager.player2Level;
        List<FunItem> playerItems = playerIndex == 1 ? brickManager.player1ActiveItems : brickManager.player2ActiveItems;

        if (playerLevel != lastKnownLevel)
        {
            lastKnownLevel = playerLevel;
            ball.ResetBallAndPause();
            transform.position = new Vector3(startingX, startingY, 0);
            foreach (FunItem powerUp in playerItems)
            {
                Destroy(powerUp.gameObject);
            }
        }

        if (playerLevel > brickManager.numberOfLevels)
        {
            Debug.Log("Player has completed all levels");
            AddReward(200.0f - (Time.time - sTime) * 3 / 2);
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

        if (lastKnownPaddleSizeItemCount != paddleSizeItemCount)
        {
            int multiplier = paddleSizeItemCount;
            transform.localScale = new Vector3(1 + (0.5f * multiplier), 1 + (0.5f * multiplier), 1);
            lastKnownPaddleSizeItemCount = paddleSizeItemCount;
        }

        // Ensure ball dies after a certain point
        float forgiveness = 1.05f;
        if (ballTransform.position.y < startingY * forgiveness)
        {   
            // Force the ball downward
            Vector2 downwardVelocity = new Vector2(ballRigidbody.velocity.x, -Mathf.Abs(ballRigidbody.velocity.y));
            ballRigidbody.velocity = downwardVelocity;
        }
        CheckBrickCollisions();
    }

    void CheckBrickCollisions()
    {
        if (brickManager == null || brickManager.player1Bricks == null || brickManager.player2Bricks == null)
        {
            Debug.LogError("brickManager or bricks list is null");
            return;
        }

        List<GameObject> playerBricks = playerIndex == 1 ? brickManager.player1Bricks : brickManager.player2Bricks;
        if (playerBricks.Count == 0)
        {
            AddReward(20.0f);
            brickManager.CheckBricksAndLevelUp(playerIndex);
            Debug.Log("All bricks destroyed");
        }
        else
        {
            if (ballTransform == null)
            {
                Debug.LogError("ballTransform is null");
                return;
            }

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
        if (!collision.gameObject.CompareTag("Ball")) {
            return;
        }

        Rigidbody2D ball = collision.rigidbody;
        Collider2D paddle = collision.otherCollider;
        Vector2 ballDirection = ball.velocity.normalized;
        Vector2 contactDistance = ball.transform.position - paddle.bounds.center;
        Vector2 paddleCenter = paddle.bounds.center;
        Vector2 contactPoint = collision.GetContact(0).point;

        bool isSideHit = Mathf.Abs(contactDistance.x) > paddle.bounds.size.x * 0.3f;
        bool isBottomHalf = contactPoint.y < (paddleCenter.y - paddle.bounds.size.y * (1 / 3f));
        bool isBelowThreshold = contactPoint.y < -startingY;
        float bounceAngle = (contactDistance.x / paddle.bounds.size.x) * maxBounceAngle;

        if (isSideHit && ball.velocity.y < 0)
        {
            if (isBottomHalf || isBelowThreshold)
            {
                ballDirection = Quaternion.AngleAxis(bounceAngle, Vector3.forward) * new Vector2(ballDirection.x, -Mathf.Abs(ballDirection.y));
            }
            else
            {
                ballDirection = Quaternion.AngleAxis(bounceAngle, Vector3.forward) * ballDirection;
            }
        }
        else
        {
            ballDirection = new Vector2(ballDirection.x, Mathf.Abs(ballDirection.y));
        }

        ball.velocity = ballDirection * ball.velocity.magnitude;
        AddReward(1.0f); // Reward for hitting the ball
    }

    public void ResetPaddleSize()
    {
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
            if (player == gameObject)
            {
                continue;
            }
            FunPlayerController playerController = player.GetComponent<FunPlayerController>();
            playerController.EndEpisode();
        }

        GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");
        foreach (GameObject ball in balls)
        {
            ball.GetComponent<FunBallStart>().ResetBallAndPause();
        }

        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject item in items)
        {
            Destroy(item);
        }
        EndEpisode();
    }
}
