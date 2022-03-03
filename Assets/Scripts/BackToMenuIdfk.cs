using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMenuIdfk : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            SceneManager.LoadScene("MenuScene");
        }
    }
}
