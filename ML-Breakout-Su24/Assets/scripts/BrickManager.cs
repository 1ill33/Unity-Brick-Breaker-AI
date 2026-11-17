using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BrickManager : MonoBehaviour
{
    public int environmentIndex;
    public GameObject brickPrefab;
    public GameObject brick_SPrefab;
    public int rows = 8;
    public int columns = 4;
    public float brickWidth = 2f;
    public float brickHeight = 0.75f;
    public float xAdjust = 1.14f;
    public float yAdjust = 1.4f;
    public float percentMissing = 0.9f;
    public int numberOfLevels;
    public GameObject[] availableItems;
    public float itemDropChance = 0.9f;
    public float strongBrickDropChance = 0.25f;
    public float roof = 4.89416f + 1.4f;

    private List<List<(Vector3, int)>> levelBrickPositions;
    public int player1Level;
    public int player2Level;
    public List<GameObject> player1Bricks;
    public List<GameObject> player2Bricks;
    public List<Item> player1ActiveItems = new List<Item>();
    public List<Item> player2ActiveItems = new List<Item>();

    void Start()
    {
        levelBrickPositions = new List<List<(Vector3, int)>>();
        player1Bricks = new List<GameObject>();
        player2Bricks = new List<GameObject>();
        player1Level = 0;
        player2Level = 0;

        GenerateLevelBrickPositions();
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
                        Vector3 position = new Vector3(j * brickWidth + xAdjust, roof - (i * brickHeight + yAdjust), 0);
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
        // if player is 1, generate bricks, else generate bricks for player 2 by changing the x position of the bricks
        foreach (var brickInstance in levelBrickPositions[level])
        {
            GameObject brick;
            Vector3 position = brickInstance.Item1;
            
            if (playerBricks == player1Bricks)
            {
                brick = Instantiate(brickPrefab, position, Quaternion.identity, transform);
                brick brickComponent = brick.GetComponent<brick>();
                //brickComponent.itemOptions = availableItems;
                //brickComponent.health = brickInstance.Item2;

                playerBricks.Add(brick);
            }
            else if (playerBricks == player2Bricks)
            {
                brick = Instantiate(brickPrefab, new Vector3(-position.x, position.y, position.z), Quaternion.identity, transform);
                brick brickComponent = brick.GetComponent<brick>();
                //brickComponent.itemOptions = availableItems;
                //brickComponent.health = brickInstance.Item2;
                
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
        if (playerBricks.Count == 0)
        {
            if (player == 1)
            {
                player1Level += 1;
            }
            else if (player == 2)
            {
                player2Level += 1;
            }
            ResetBricks(player);
        }
    }

    void Update()
    {
        player1ActiveItems = new List<Item>();
        foreach (Item item in FindObjectsOfType<Item>())
        {
            if (item.transform.position.x > 0)
            {
                player1ActiveItems.Add(item);
            }
        }
        player2ActiveItems = new List<Item>();
        foreach (Item item in FindObjectsOfType<Item>())
        {
            if (item.transform.position.x < 0)
            {
                player2ActiveItems.Add(item);
            }
        }

    }
}