using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FunBrickManager : MonoBehaviour
{
    public int environmentIndex;
    public GameObject brickPrefab;
    public GameObject brick_SPrefab;
    public int rows = 4;
    public int columns = 4;
    public float brickWidth = 2f;
    public float brickHeight = 0.75f;
    public float xAdjustForBrick = 1.14f;
    public float yAdjustForBrick = 1.4f;
    public float percentMissing = 0.9f;
    public int numberOfLevels;
    public GameObject[] availableItems;
    public float itemDropChance = 0.9f;
    public float strongBrickDropChance = 0.25f;
    public float roof = 4.89416f + 1.4f;  
    public float deathLineY = -5.5f;
    public Vector2 relativeCenterPositionOfGame = new Vector2(0, 0);

    // Background images for each level
    public Sprite[] player1Backgrounds;
    public Sprite[] player2Backgrounds;
    public SpriteRenderer player1BackgroundRenderer;
    public SpriteRenderer player2BackgroundRenderer;

    private List<List<(Vector3, int)>> levelBrickPositions;
    public int player1Level;
    public int player2Level;
    public List<GameObject> player1Bricks;
    public List<GameObject> player2Bricks;
    public List<FunItem> player1ActiveItems = new List<FunItem>();
    public List<FunItem> player2ActiveItems = new List<FunItem>();

    void Start()
    {
        levelBrickPositions = new List<List<(Vector3, int)>>(); // Keeps track of brick positions for each level
        player1Bricks = new List<GameObject>();
        player2Bricks = new List<GameObject>();
        player1Level = 0;
        player2Level = 0;

        // NEW: Find position this game object is relative to the center of the game area
        relativeCenterPositionOfGame = new Vector2(transform.position.x, transform.position.y); // NEW

        // NEW: Adjust the roof relative to the center of the game area
        roof += relativeCenterPositionOfGame.y; // NEW
        deathLineY += relativeCenterPositionOfGame.y; // NEW

        GenerateLevelBrickPositions();
        UpdateBackground(1, player1Level);
        UpdateBackground(2, player2Level);
    }

    public void ResetGame()
    {
        player1Level = 0;
        player2Level = 0;
        player1Bricks = new List<GameObject>();
        player2Bricks = new List<GameObject>();
        levelBrickPositions = new List<List<(Vector3, int)>>();

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        GenerateLevelBrickPositions();
        ResetBricks(1);
        ResetBricks(2);
        UpdateBackground(1, player1Level);
        UpdateBackground(2, player2Level);
    }

    private void GenerateLevelBrickPositions()
    {
        for (int level = 0; level < numberOfLevels; level++)
        {
            List<(Vector3, int)> brickPositions = new List<(Vector3, int)>();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (Random.value > percentMissing)
                    {
                        // NEW: Adjust brick position to be relative to the center of the game
                        Vector3 position = new Vector3(j * brickWidth + xAdjustForBrick - relativeCenterPositionOfGame.x, // NEW
                            roof - (i * brickHeight + yAdjustForBrick), 0); // NEW

                        int health = UnityEngine.Random.Range(1, 100) <= strongBrickDropChance * 100 ? 2 : 1;
                        brickPositions.Add((position, health));
                    }
                }
            }
            levelBrickPositions.Add(brickPositions);
        }
    }

    void GenerateRandomBricksForPlayer(List<GameObject> playerBricks, int level)
    {
        foreach (var brickInstance in levelBrickPositions[level])
        {
            GameObject brick;
            Vector3 position = brickInstance.Item1;

            if (playerBricks == player1Bricks)
            {
                brick = Instantiate(brickPrefab, position, Quaternion.identity, transform);
                FunBrick brickComponent = brick.GetComponent<FunBrick>();
                brickComponent.itemOptions = availableItems;
                brickComponent.health = brickInstance.Item2;

                playerBricks.Add(brick);
            }
            else if (playerBricks == player2Bricks)
            {
                // NEW: Adjust player 2's bricks to be positioned relative to the center
                brick = Instantiate(brickPrefab, new Vector3(-position.x + 2 * relativeCenterPositionOfGame.x, position.y, position.z), // NEW
                    Quaternion.identity, transform);
                FunBrick brickComponent = brick.GetComponent<FunBrick>();
                brickComponent.itemOptions = availableItems;
                brickComponent.health = brickInstance.Item2;

                playerBricks.Add(brick);
            }
        }
    }

    public void ResetBricks(int player)
    {
        int currentLevel = player == 1 ? player1Level : player2Level;
        List<GameObject> playerBricks = player == 1 ? player1Bricks : player2Bricks;

        foreach (var brick in playerBricks)
        {
            if (brick != null)
            {
                Destroy(brick);
            }
        }
        playerBricks.Clear();
        GenerateRandomBricksForPlayer(playerBricks, currentLevel);
    }

    public void RemoveBrickFromPlayerList(int player, GameObject brick)
    {
        List<GameObject> playerBricks = player == 1 ? player1Bricks : player2Bricks;
        if (playerBricks.Contains(brick))
        {
            playerBricks.Remove(brick);
            CheckBricksAndLevelUp(player);
        }
    }

    public void CheckBricksAndLevelUp(int player)
    {
        List<GameObject> playerBricks = player == 1 ? player1Bricks : player2Bricks;
        Debug.Log("Player " + player + " has " + playerBricks.Count + " bricks left.");
        
        if (playerBricks.Count == 0)
        {
            if (player == 1)
            {
                player1Level += 1;
                UpdateBackground(1, player1Level);
            }
            else if (player == 2)
            {
                player2Level += 1;
                UpdateBackground(2, player2Level);
            }
            ResetBricks(player);
        }
    }

    void UpdateBackground(int player, int level)
    {
        int backgroundIndex = level % player1Backgrounds.Length;
        if (player == 1 && level < player1Backgrounds.Length)
        {
            player1BackgroundRenderer.sprite = player1Backgrounds[backgroundIndex];
        }
        else if (player == 2 && level < player2Backgrounds.Length)
        {
            player2BackgroundRenderer.sprite = player2Backgrounds[backgroundIndex];
        }
    }

    void Update()
    {
        player1ActiveItems = new List<FunItem>();
        foreach (FunItem item in FindObjectsOfType<FunItem>())
        {
            if (item.transform.position.x > relativeCenterPositionOfGame.x) // NEW
            {
                player1ActiveItems.Add(item);
            }
        }
        player2ActiveItems = new List<FunItem>();
        foreach (FunItem item in FindObjectsOfType<FunItem>())
        {
            if (item.transform.position.x < relativeCenterPositionOfGame.x) // NEW
            {
                player2ActiveItems.Add(item);
            }
        }
    }
}
