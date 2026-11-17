using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MENU_select : MonoBehaviour
{
   
    public string[] scene = { "Easy", "Medium", "Hard", "Extra" };
    public int gameState = 0;
    public int gameStateMax = 4;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        

        //Right and Left

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                gameState += 1;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                gameState -= 1;
            }

            //Wrap around
            if (gameState == gameStateMax)
            {
                gameState = 0;
            }
            if (gameState == -1)
            {
                gameState = gameStateMax - 1;
            }
    

        //Ready to play

        if (Input.GetKeyDown(KeyCode.Return))
        {
            
            SceneManager.LoadScene(scene[gameState]);

        }
        
    }
}
