using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour
{

    private float sTime;
    // Start is called before the first frame update
    void Start()
    {
        sTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (sTime + 3f < Time.time)
        {
            SceneManager.LoadScene("Menu");
        }
    }
}
