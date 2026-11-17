using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum ItemType { IncreasePaddleSize, DecreasePaddleSize, IncreaseBallSpeed, DecreaseBallSpeed }
    public ItemType itemType;
    public float dropSpeed = 2f;
    public float deathLineY = -5.5f;

    void Update()
    {
        transform.Translate(Vector2.down * dropSpeed * Time.deltaTime);
        if (transform.position.y < deathLineY)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Power-up collided with: " + collision.tag);
        if (collision.CompareTag("Player"))
        {
            ApplyPowerUp(collision.gameObject);
            Debug.Log("Power-up applied" + itemType);
            Destroy(gameObject);
        }
    }

    private void ApplyPowerUp(GameObject paddle)
    {
        // ballStart ball = GameObject.FindObjectOfType<ballStart>();
        PlayerController playerController = paddle.GetComponent<PlayerController>();
        // if playerController's playerIndex == 1 then get the ball with the x position greater than 0, else get the ball with the x position less than 0
        

        // Implement the logic to apply the power-up effect to the paddle or game
        if (itemType == ItemType.IncreasePaddleSize)
        {
            if (playerController.paddleSizeItemCount <= 3)
            {
                playerController.paddleSizeItemCount += 1;
                int multiplier = playerController.paddleSizeItemCount;
                // itemcount * 0.5f = scale increase
                playerController.transform.localScale = new Vector3(1 + (0.5f * multiplier),1 + (0.5f * multiplier), 1);
                playerController.AddReward(0.5f);
            }
        }
        else if (itemType == ItemType.DecreasePaddleSize)
        {
            if (playerController.paddleSizeItemCount > 0)
            {
                playerController.paddleSizeItemCount -= 1;
                int multiplier = playerController.paddleSizeItemCount;
                playerController.transform.localScale = new Vector3(1 - (0.5f * multiplier),1 - (0.5f * multiplier), 1);
                // playerController.AddReward(-0.1f);
            }
        }
        else if (itemType == ItemType.IncreaseBallSpeed)
        {
            if (playerController.ballSpeedItemCount <= 3)
            {
                // if playerIndex == 1, increases the speed of all balls with the x position greater than 0, else increase the speed of all balls with the x position less than 0
                GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");
                if (playerController.playerIndex == 1)
                {
                    foreach (GameObject ball in balls)
                    {
                        if (ball.transform.position.x > 0)
                        {
                            ball.GetComponent<ballStart>().IncreaseSpeed();
                            
                        }
                    }
                }
                else if (playerController.playerIndex == 2)
                {
                    foreach (GameObject ball in balls)
                    {
                        if (ball.transform.position.x < 0)
                        {
                            ball.GetComponent<ballStart>().IncreaseSpeed();
                        }
                    }
                }
                playerController.ballSpeedItemCount += 1;
                playerController.AddReward(0.5f);
            }
        }
        else if (itemType == ItemType.DecreaseBallSpeed)
        {
            if (playerController.ballSpeedItemCount > 0)
            {
                GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");
                if (playerController.playerIndex == 1)
                {
                    foreach (GameObject ball in balls)
                    {
                        if (ball.transform.position.x > 0)
                        {
                            ball.GetComponent<ballStart>().DecreaseSpeed();
                        }
                    }
                }
                else if (playerController.playerIndex == 2)
                {
                    foreach (GameObject ball in balls)
                    {
                        if (ball.transform.position.x < 0)
                        {
                            ball.GetComponent<ballStart>().DecreaseSpeed();
                        }
                    }
                }
                playerController.ballSpeedItemCount -= 1;
                // playerController.AddReward(-0.1f);
            }
        }

    }
}
