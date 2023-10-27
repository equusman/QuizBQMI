using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;
using UnityEngine.Networking; 

public class QuizGameUI : MonoBehaviour
{
#pragma warning disable 649
    [SerializeField] private QuizManager quizManager;               //ref to the QuizManager script
    [SerializeField] private CategoryBtnScript categoryBtnPrefab;
    [SerializeField] private GameObject scrollHolder;
    [SerializeField] private Text scoreText, timerText, scoreFinalText; 
    [SerializeField] private List<Image> lifeImageList;  
    [SerializeField] private GameObject gameOverPanel, mainMenu, gamePanel, levelUI, scrollRect;
    [SerializeField] private Color correctCol, wrongCol, normalCol; //color of buttons
    [SerializeField] private Image questionImg;                     //image component to show image
    [SerializeField] private UnityEngine.Video.VideoPlayer questionVideo;   //to show video
    [SerializeField] private AudioSource questionAudio;             //audio source for audio clip
    [SerializeField] private Text questionInfoText;                 //text to show question
    [SerializeField] private List<Button> options;                  //options button reference
    
#pragma warning restore 649

    private float audioLength;          //store audio length
    private master_soal question;          //store current question data
    private bool answered = false;      //bool to keep track if answered or not

    public Text TimerText { get => timerText; }                     //getter
    public Text ScoreText { get => scoreText; }                     //getter
    public Text ScoreFinalText { get => scoreFinalText; }                     //getter
    public GameObject GameOverPanel { get => gameOverPanel; }                     //getter
    public GameObject MainMenu { get => mainMenu; }                     //getter
    public GameObject GamePanel { get => gamePanel; }                     //getter
    
    Sprite sprite;

    private void Start()
    {
        //add the listner to all the buttons
        for (int i = 0; i < options.Count; i++)
        {
            Button localBtn = options[i];
            localBtn.onClick.AddListener(() => OnClick(localBtn));
        }
        
        CreateCategoryButtons();

    }

    IEnumerator LoadImage( ) {
        string path = "file://" + Path.Combine(Application.streamingAssetsPath + "/image", question.vgambar);  
        UnityWebRequest req = UnityWebRequestTexture.GetTexture(path);
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.ConnectionError) {
            Debug.Log("ERROR :  " + req.error);
        }
        else {
            Texture2D myTexture = ((DownloadHandlerTexture)req.downloadHandler).texture;
            Sprite newSprite = Sprite.Create(myTexture, new Rect(0, 0, myTexture.width, myTexture.height), new Vector2(0.5f, 0.5f));
            questionImg.sprite = newSprite;
        }

    }

    /// <summary>
    /// Method which populate the question on the screen
    /// </summary>
    /// <param name="question"></param>
    public void SetQuestion(master_soal question, bool isSameNumber)
    {
       


        this.question = question;
        //check for questionType 
        if (question.vgambar == "")
        {
            questionImg.transform.parent.gameObject.SetActive(false); 
        }
        else { 
            
            questionImg.transform.parent.gameObject.SetActive(true);    //activate image holder
            questionVideo.transform.gameObject.SetActive(false);        //deactivate questionVideo
            questionImg.transform.gameObject.SetActive(true);           //activate questionImg
            questionAudio.transform.gameObject.SetActive(false);        //deactivate questionAudio   
            sprite = Resources.Load<Sprite>(Path.Combine(Application.streamingAssetsPath + "/image", question.vgambar)); 
            StartCoroutine(LoadImage());
        }
          
        questionInfoText.text = question.vpertanyaan;                      //set the question text
         
        if (!isSameNumber) {
            //suffle the list of options
            List<string> ansOptions = ShuffleList.ShuffleListItems<string>(question.options2); 
            //assign options to respective option buttons
            for (int i = 0; i < options.Count; i++)
            {
                //set the child text
                options[i].GetComponentInChildren<Text>().text = ansOptions[i];
                options[i].name = ansOptions[i];    //set the name of button
                options[i].image.color = normalCol; //set color of button to normal 
            }
            ResetLife();
        } 

        answered = false;                       

    }

    public void ReduceLife(int remainingLife)
    {  
        lifeImageList[remainingLife].color = Color.red;
    }

    public void ResetLife()
    { 
        lifeImageList[0].color = Color.green;
        lifeImageList[1].color = Color.green;
    }

    /// <summary>
    /// IEnumerator to repeate the audio after some time
    /// </summary>
    /// <returns></returns>
    IEnumerator PlayAudio()
    { 
            yield return new WaitForSeconds(audioLength + 0.5f); 
            StartCoroutine(PlayAudio()); 
    }
    
    /// <summary>
    /// Method assigned to the buttons
    /// </summary>
    /// <param name="btn">ref to the button object</param>
    void OnClick(Button btn)
    { 
        if (quizManager.GameStatus == GameStatus.PLAYING)
        { 
            if (!answered)
            { 
                answered = true; 
                bool val = quizManager.Answer(btn.name);
                 
                if (val)
                { 
                    StartCoroutine(BlinkImg(btn.image));
                }
                else
                { 
                    btn.image.color = wrongCol;
                }
            } 
        }
    }

    /// <summary>
    /// Method to create Category Buttons dynamically
    /// </summary>
    void CreateCategoryButtons()
    { 
        CategoryBtnScript categoryBtn = Instantiate(categoryBtnPrefab, scrollHolder.transform);
        categoryBtn.Btn.onClick.AddListener(() => CategoryBtn()); 
    }

    //Method called by Category Button
    private void CategoryBtn()
    {
        //quizManager.StartGame(); //start the game
        mainMenu.SetActive(true);              //deactivate mainMenu
        gamePanel.SetActive(false);              //activate game panel
        levelUI.SetActive(true);   
        scrollRect.SetActive(false);
    }

    //this give blink effect [if needed use or dont use]
    IEnumerator BlinkImg(Image img)
    {
        for (int i = 0; i < 2; i++)
        {
            img.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            img.color = correctCol;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void RestryButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
     
}

