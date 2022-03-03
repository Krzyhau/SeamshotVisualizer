using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) {
            SceneManager.LoadScene("ShowcaseScene");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) {
            SceneManager.LoadScene("FormulaScene");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) {
            SceneManager.LoadScene("PlaygroundScene");
        }
        if (Input.GetKeyDown(KeyCode.T)) {
            if (Time.timeScale != 1.0f) {
                Time.timeScale = 1.0f;
            } else {
                Time.timeScale = 0.25f;
            }
        }
    }
}
