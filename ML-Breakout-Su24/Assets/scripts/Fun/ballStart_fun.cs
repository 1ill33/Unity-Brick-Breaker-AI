using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.SceneManagement;



public class FunBallStart : MonoBehaviour
{
    Rigidbody2D rb;
    float ang;
    public bool dead = false;
    float dtime;
    public float speed = 6.5f;

    public float minVelocityY = 6.5f;
    public float maxVelocityY = 10f;
    public float VelocityY;
    public float deathLineY = -5.5f;
    public float ballStartPositionX = 4.28f;
    public float ballStartPositionY = 0f;
    public AudioSource bounceSound;
    public AudioSource brickSound;
    public AudioSource playerSound;
    public FunBrickManager brickManager;

    public FunKeepScore keepScore;
    public FunPlayerController playerController;

    // Start is called before the first frame update
    void Start()
    {       
        rb = GetComponent<Rigidbody2D>();
        ang = Random.Range(-3.0f, 3.0f);
        rb.velocity = new Vector3(ang, -speed, 0);

        bounceSound = GetComponent<AudioSource>();
        if (bounceSound != null)
        {
            // bounceSound.volume = 0.55f;
            bounceSound.pitch = Random.Range(0.8f, 1.2f);
        }
        if (brickSound != null)
        {
            // brickSound.volume = 0.25f;
            brickSound.pitch = Random.Range(0.8f, 1.2f);
        }
        if (playerSound != null)
        {
            // playerSound.volume = 0.5f;
            playerSound.pitch = Random.Range(0.8f, 1.2f);
        }
        deathLineY = brickManager.deathLineY;
        ballStartPositionX += brickManager.relativeCenterPositionOfGame.x;
        ballStartPositionY += brickManager.relativeCenterPositionOfGame.y;

        keepScore = GameObject.Find("Canvas").GetComponent<FunKeepScore>();
    }

    // Update is called once per frame
    void Update()
    {
        VelocityY = rb.velocity.y;

        // if (keepScore.score1 >= 10 || keepScore.score2 >= 10)
        // {
            
        //     // Application.Quit();
        //     // UnityEditor.EditorApplication.isPlaying = false;
        // }

        // if ball goes out of bounds of the right or left wall's box collider, reset ball and pause
        float rightWall = playerController.rightWall.gameObject.GetComponent<BoxCollider2D>().bounds.max.x;
        float leftWall = playerController.leftWall.gameObject.GetComponent<BoxCollider2D>().bounds.min.x;        

        if (transform.position.x > rightWall)
        {
            Debug.Log( transform.position.x + " " + rightWall);
            ResetBallAndPause();
        }
        else if (transform.position.x < leftWall)
        {
            Debug.Log( transform.position.x + " " + leftWall);
            ResetBallAndPause();
        }
        

        // on death
        if (transform.position.y < deathLineY)
        {
            // reset
            rb = GetComponent<Rigidbody2D>();

            // ------------------------------------CHANGE -5 back to 0 once ball getting through bar prob is fixed-----------------------------

            if (playerController.playerIndex == 1)
            {
                keepScore.lives1 -= 1;
                playerController.AddReward(-60.0f);
                // playerController.EndEpisodeForAllPlayers();
                if (keepScore.lives1 < 0)
                {
                    SceneManager.LoadScene("YouWin");
                    dead = false;
                    Debug.Log("player 1 has lost all lives");
                    playerController.AddReward(-20.0f);
                    playerController.EndEpisodeForAllPlayers();
                    // Application.Quit();
                    //UnityEditor.EditorApplication.isPlaying = false;
                }
                transform.position = new Vector3(ballStartPositionX, ballStartPositionY, 0);
                
            }
            else if (playerController.playerIndex == 2)
            {
                keepScore.lives2 -= 1;
                playerController.AddReward(-20.0f);
                if (keepScore.lives2 < 0)
                {
                    SceneManager.LoadScene("YouLose");
                    dead = false;
                    Debug.Log("player 2 has lost all lives");
                    playerController.AddReward(-20.0f);
                    playerController.EndEpisodeForAllPlayers();
                    // Application.Quit();
                    //UnityEditor.EditorApplication.isPlaying = false;
                }
                transform.position = new Vector3(-ballStartPositionX, ballStartPositionY, 0);
            }
            // pause
            rb.velocity = new Vector3(0, 0, 0);
            dead = true;
            dtime = Time.time + 2.0f;

        }

        //play
        if (Time.time > dtime && dead == true)
        {
            rb = GetComponent<Rigidbody2D>();
            ang = Random.Range(-3.0f, 3.0f);
            rb.velocity = new Vector3(-ang, -speed, 0); ;
            dead = false;
        }

        if (dead == false && Mathf.Abs(rb.velocity.y) < minVelocityY)
        {
            if (rb.velocity.y < 0)
            {
                rb.velocity = new Vector3(rb.velocity.x, -minVelocityY, 0);
            }
            else
            {
                rb.velocity = new Vector3(rb.velocity.x, minVelocityY, 0);
            }
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (bounceSound!=null) 
        {
            bounceSound.PlayOneShot(bounceSound.clip);
            if (collision.gameObject.tag == "Player")
            {
                if (playerSound != null)
                {
                    playerSound.PlayOneShot(playerSound.clip);
                }
            }
            else if (collision.gameObject.tag == "Brick")
            {
                if (brickSound != null)
                {
                    brickSound.PlayOneShot(brickSound.clip);
                }
            }
        }
    }

    public void ResetBallAndPause()
    {
        if (playerController.playerIndex == 1)
        {
            transform.position = new Vector3(ballStartPositionX, ballStartPositionY, 0);

        }
        else if (playerController.playerIndex == 2)
        {
            transform.position = new Vector3(-ballStartPositionX, ballStartPositionY, 0);
        }
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = new Vector3(0, 0, 0);
        dead = true;
        dtime = Time.time + 2.0f;
    }

    public void ResetPositionAndVelocity()
    {
        if (playerController.playerIndex == 1)
        {
            transform.position = new Vector3(ballStartPositionX, ballStartPositionY, 0);

        }
        else if (playerController.playerIndex == 2)
        {
            transform.position = new Vector3(-ballStartPositionX, ballStartPositionY, 0);
        }
        transform.rotation = Quaternion.identity;
        // if rb exists, reset velocity
        if (rb != null)
        {
            // ang = Random.Range(-3.0f, 3.0f);
            rb.velocity = new Vector3(-ang, -speed, 0);
        }
        // rb.velocity = new Vector3(-ang, -speed, 0);
        dead = false;
    }

    public void IncreaseSpeed()
    {
        rb.velocity *= 1.4f;
    }

    public void DecreaseSpeed()
    {
        rb.velocity *= 0.8f;
    }

}
