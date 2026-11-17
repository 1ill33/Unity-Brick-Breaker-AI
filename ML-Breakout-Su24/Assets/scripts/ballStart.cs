using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;



public class ballStart : MonoBehaviour
{
    public Rigidbody2D rb;
    float ang;
    public bool dead = false;
    float dtime;
    public float speed = 5f;

    public float minVelocityY = 4f;
    public float maxVelocityY = 6f;
    public float VelocityY;

    public KeepScore keepScore;
    public PlayerController playerController;

    public int playerIndex = 1;


    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        keepScore = GameObject.Find("Canvas").GetComponent<KeepScore>();

        rb.velocity = new Vector3(0, 0, 0);
        dead = true;
        dtime = Time.time + 2.0f;
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

        // on death
        if (transform.position.y < -5.5f)
        {
            // reset
            rb = GetComponent<Rigidbody2D>();

            if (playerIndex == 1)
            {
                transform.position = new Vector3(4.28f, 0, 0);
                rb.velocity = new Vector3(0, 0, 0);
                dead = true;
                dtime = Time.time + 2.0f;
                keepScore.lives1 -= 1;
                if (keepScore.lives1 < 0)
                {
                    SceneManager.LoadScene("YouWin");
                    dead = false;
                    Debug.Log("player 1 has lost all lives");
                    playerController.AddReward(-10.0f);
                    playerController.EndEpisodeForAllPlayers();
                    // Application.Quit();
                    //UnityEditor.EditorApplication.isPlaying = false;
                }
                
                
            }
            else if (playerIndex == 2)
            {
                transform.position = new Vector3(-4.28f, 0, 0);
                rb.velocity = new Vector3(0, 0, 0);
                dead = true;
                dtime = Time.time + 2.0f;
                keepScore.lives2 -= 1;
                if (keepScore.lives2 < 0)
                {
                    SceneManager.LoadScene("YouLose");
                    dead = false;
                    Debug.Log("player 2 has lost all lives");
                    playerController.AddReward(-10.0f);
                    playerController.EndEpisodeForAllPlayers();
                    // Application.Quit();
                    //UnityEditor.EditorApplication.isPlaying = false;
                }
                
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

    public void ResetBallAndPause()
    {
        if (playerController.playerIndex == 1)
        {
            transform.position = new Vector3(4.28f, 0, 0);

        }
        else if (playerController.playerIndex == 2)
        {
            transform.position = new Vector3(-4.28f, 0, 0);
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
            transform.position = new Vector3(4.28f, 0, 0);

        }
        else if (playerController.playerIndex == 2)
        {
            transform.position = new Vector3(-4.28f, 0, 0);
        }
        transform.rotation = Quaternion.identity;
        rb.velocity = new Vector3(-ang, -speed, 0);
        dead = false;
    }

    public void IncreaseSpeed()
    {
        rb.velocity *= 1.2f;
    }

    public void DecreaseSpeed()
    {
        rb.velocity *= 0.8f;
    }

}

