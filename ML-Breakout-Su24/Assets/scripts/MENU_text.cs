using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MENU_text : MonoBehaviour
{
    public MENU_select select;
    public string[] difficulty = { "EASY", "MEDIUM", "HARD", "EXTRA" };

    public Text itemText;
    public Text difText;

    // Start is called before the first frame update
    void Start()
    {
        select = GameObject.Find("Selector").GetComponent<MENU_select>();
    }

    // Update is called once per frame
    void Update()
    {

        //Difficulty
        difText.text = select.scene[select.gameState];
    }
}
