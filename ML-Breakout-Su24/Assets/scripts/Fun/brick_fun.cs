using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FunBrick : MonoBehaviour
{
    public FunKeepScore keepScore;
    public SpriteRenderer sprite;
    public FunBrickManager brickManager;
    public GameObject brickFab;
    public int health = 1;
    public GameObject[] itemOptions;
    public Sprite[] brickSprites;

    float sTime;

    // Start is called before the first frame update
    void Start()
    {
        sTime = Time.time;
        brickManager = GameObject.Find("BrickGeneration").GetComponent<FunBrickManager>();
        keepScore = GameObject.Find("Canvas").GetComponent<FunKeepScore>();
        sprite = gameObject.GetComponent<SpriteRenderer>();
        // itemDropChance = brickManager.itemDropChance;
        SetBrickColor();
    }

    void SetBrickColor()
    {
        if (health > brickSprites.Length)
        {
            health = brickSprites.Length;
        }

        sprite.sprite = brickSprites[health - 1];
        
        if (transform.position.x > brickManager.relativeCenterPositionOfGame.x)
        {
            sprite.color = Color.green;
        }
        else
        {
            sprite.color = Color.red;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void DropItem()
    {
        Debug.Log("DropItem" + itemOptions.Length + " " + brickManager.itemDropChance ); 
        if (itemOptions.Length > 0 && UnityEngine.Random.value < brickManager.itemDropChance)
        {
            int randomIndex = UnityEngine.Random.Range(0, itemOptions.Length);
            Instantiate(itemOptions[randomIndex], transform.position, Quaternion.identity);
            Debug.Log("aaaaa" + itemOptions[randomIndex]);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ball")
        {
            TakeDamage();
        }
        
    }

    private void TakeDamage()
    {
        health -= 1;

        if (health <= 0)
        {
            DropItem();

            if (transform.position.x < brickManager.relativeCenterPositionOfGame.x)
            {
                keepScore.score2 += 1;
            }
            else
            {
                keepScore.score1 += 1;
            }

            brickManager.RemoveBrickFromPlayerList(1, gameObject);
            brickManager.RemoveBrickFromPlayerList(2, gameObject);

            Destroy(gameObject);
        }
        else
        {
            SetBrickColor();
        }
    }
}
