using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine.EventSystems;
using UnityEngine.UI; 

public class SqlConnection : MonoBehaviour
{
     
    [SerializeField] private QuizManager quizManager;
    [SerializeField] private GameObject gameOverPanel, mainMenu, gamePanel, levelUI, scrollRect; 

    // Start is called before the first frame update
    void Start()
    { 

    }


    // Update is called once per frame
    void Update()
    { 
    }
     
 
    public void CategoryBtn(string level) 
    {  
        quizManager.StartGame(level);
        mainMenu.SetActive(false);              //deactivate mainMenu
        gamePanel.SetActive(true);              //activate game panel
        levelUI.SetActive(false);
        scrollRect.SetActive(false);
    }

}

