using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour {

    public Text scoreRecordText;
    public string mainGameSceneName;

    private void Start(){
        scoreRecordText.text = PlayerPrefs.GetInt("scoreRecord").ToString();
    }

    //Method called by button in scene
    public void StartGame() {
        SceneManager.LoadScene(mainGameSceneName);
    }
}