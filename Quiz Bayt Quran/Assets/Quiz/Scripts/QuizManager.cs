using System;
using System.Collections;
using System.Collections.Generic; 
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System.IO; 

public class QuizManager : MonoBehaviour
{
#pragma warning disable 649
    //ref to the QuizGameUI script
    [SerializeField] private QuizGameUI quizGameUI; 
    [SerializeField] private float timeInSeconds;
#pragma warning restore 649

    private string currentCategory = "";
    private int correctAnswerCount = 0;
    //questions data
    private List<master_soal> questions;
    //current question data
    private master_soal selectedQuetion = new master_soal();
    private float gameScore;
    private int lifesRemaining;

    bool secondAnswer = false;

    private int questionIndex;
    private float currentTime; 

    private GameStatus gameStatus = GameStatus.NEXT;

    public GameStatus GameStatus { get { return gameStatus; } }
     
    private List<master_soal> QuestionsData;

    public void StartGame(string level)
    {

        openConnection(level);

        if (QuestionsData.Count > 0)
        {
            setupQuestion();
        }
        else {
            quizGameUI.RestryButton();
        }
    }


    void setupQuestion() {
        questionIndex = 0;

        correctAnswerCount = 0;
        gameScore = 0;
        lifesRemaining = 2;
        timeInSeconds = QuestionsData.Count * 60f;
        currentTime = timeInSeconds;
        //set the questions data
        questions = new List<master_soal>(); 
        questions.AddRange(QuestionsData);
        //select the question

        StartCoroutine(SelectQuestion(questionIndex, false, 0.0f));
        gameStatus = GameStatus.PLAYING;
    }

    /// <summary>
    /// Method used to randomly select the question form questions data
    /// </summary>
    IEnumerator SelectQuestion(int indexQuestion, bool isSameNumber, float delayTime)
    {
        yield return new WaitForSeconds(delayTime); 
        int val = UnityEngine.Random.Range(0, questions.Count); 
        selectedQuetion = questions[indexQuestion]; 
        quizGameUI.SetQuestion(selectedQuetion, isSameNumber);

        //questions.RemoveAt(indexQuestion);
    }

    private void Update()
    {
        if (gameStatus == GameStatus.PLAYING)
        {
            currentTime -= Time.deltaTime;
            SetTime(currentTime);
        }
    }

    void SetTime(float value)
    {
        TimeSpan time = TimeSpan.FromSeconds(currentTime);                       //set the time value
        quizGameUI.TimerText.text = time.ToString("mm':'ss");   //convert time to Time format

        if (currentTime <= 0)
        {
            //Game Over
            GameEnd();
        }
    }

    /// <summary>
    /// Method called to check the answer is correct or not
    /// </summary>
    /// <param name="selectedOption">answer string</param>
    /// <returns></returns>
    public bool Answer(string selectedOption)
    {
        //set default to false
        bool correct = false;
        float answerWeight = 0f;

        //if selected answer is similar to the correctAns
        if (selectedQuetion.vjawaban == selectedOption)
        {
            //Yes, Ans is correct
            correctAnswerCount++;
            correct = true; 
            if (secondAnswer) answerWeight = 0.75f;
            else answerWeight = 1f;
            //gameScore += 50;
        }
        else
        { 
            //No, Ans is wrong
            //Reduce Life
            lifesRemaining--; 
            quizGameUI.ReduceLife(lifesRemaining);

           
        }

        if (gameStatus == GameStatus.PLAYING)
        { 
            if (questions.Count > 0 && correct && questionIndex < QuestionsData.Count - 1)
            {
                if (secondAnswer) answerWeight = 0.75f;
                else answerWeight = 1f;
                questionIndex++;
                lifesRemaining = 2;
                StartCoroutine(SelectQuestion(questionIndex, false, 1f));
            }
            else if (lifesRemaining > 0 && !correct)
            {
                secondAnswer = true;
                StartCoroutine(SelectQuestion(questionIndex, true, 1f));
            }
            else if (lifesRemaining == 0 && questionIndex < QuestionsData.Count - 1)
            {
                questionIndex++;
                lifesRemaining = 2;
                StartCoroutine(SelectQuestion(questionIndex, false, 1f));
            }
            else {
                Invoke("GameEnd", 1f);
            }
             
        } 

        gameScore += ((answerWeight / QuestionsData.Count) * 100);
        quizGameUI.ScoreText.text = "Score:" + gameScore; 
        

        return correct;
    }

    private void GameEnd()
    {
        gameStatus = GameStatus.NEXT;
        quizGameUI.GameOverPanel.SetActive(true); 
        quizGameUI.ScoreFinalText.text = gameScore.ToString();

        //Save the score
        PlayerPrefs.SetInt(currentCategory, correctAnswerCount); //save the score for this category
    }

    #region sqliteconnection
    // Database name
    private string dbName = "gamequizbaru";
    // Database Connection
    IDbConnection connection;

    public void openConnection(string level)
    {
        //connection = new SqliteConnection(string.Format("URI=file:Assets/Quiz/Database/{0}.sqlite3", dbName));
        //connection = new SqliteConnection(string.Format("URI=file:Assets/Quiz/Database/{0}.sqlite3", dbName));
        string path = "URI=file:" + Path.Combine(Application.streamingAssetsPath, string.Format("{0}.sqlite3", dbName));

        connection = new SqliteConnection(path);
        connection.Open();
        QueryDatabase(true, level);
    }

    public void QueryDatabase(bool isReadQuery, string level)
    {  
        QuestionsData = new List<master_soal>();
        string query = @"   SELECT 
                                m.IDsoal,
                                m.pertanyaan,
                                m.filegambar,
                                m.level,
                                m.jawaban,
                                d.pilihan1,
                                d.pilihan2,
                                d.pilihan3,
                                d.pilihan4
                            FROM 
                                master_soal m
                            INNER JOIN detail_soal d
                                ON m.IDsoal = d.IDsoal
                            WHERE m.level = @level
                            limit (select value2 from setting where value1 = @settingsoal)
                            ";
         
        var dbcmd = connection.CreateCommand();
        dbcmd.CommandText = query;

        dbcmd.Parameters.Add(new SqliteParameter("@level", level)); 
        dbcmd.Parameters.Add(new SqliteParameter("@settingsoal", "soallevel" + level));

        var reader = dbcmd.ExecuteReader();

        List<master_soal> outputMasterSoal = new List<master_soal>();

        if (isReadQuery)
        { 
            while (reader.Read())
            {

                var data = new master_soal();
                data.idSoal = reader["IDsoal"].ToString();
                data.vpertanyaan = reader["pertanyaan"].ToString();
                data.vgambar = reader["filegambar"].ToString();
                data.vlevel = reader["level"].ToString();
                data.vjawaban = reader["jawaban"].ToString();
                var detail = new detail_soal();
                detail.vpilihan1 = reader["pilihan1"].ToString();
                detail.vpilihan2 = reader["pilihan2"].ToString();
                detail.vpilihan3 = reader["pilihan3"].ToString();
                detail.vpilihan4 = reader["pilihan4"].ToString();
                data.options = detail;

                List<String> listOption = new List<String>();
                listOption.Add(reader["pilihan1"].ToString());
                listOption.Add(reader["pilihan2"].ToString());
                listOption.Add(reader["pilihan3"].ToString());
                listOption.Add(reader["pilihan4"].ToString());
                data.options2 = listOption;

                QuestionsData.Add(data);

            }

        }
    }
     


    #endregion


}

//Datastructure for storeing the quetions data
[System.Serializable]
public class Question
{
    public string questionInfo;         //question text
    public QuestionType questionType;   //type
    public Sprite questionImage;        //image for Image Type
    public AudioClip audioClip;         //audio for audio type
    public UnityEngine.Video.VideoClip videoClip;   //video for video type
    public List<string> options;        //options to select
    public string correctAns;           //correct option
}

[System.Serializable]
public enum QuestionType
{
    TEXT,
    IMAGE,
    AUDIO,
    VIDEO
}

[SerializeField]
public enum GameStatus
{
    PLAYING,
    NEXT
}


#region dataDefine
public class master_soal
{
    public string idSoal { get; set; }
    public string vpertanyaan { get; set; }
    public string vgambar { get; set; }
    public string vlevel { get; set; }
    public string vjawaban { get; set; }
    public detail_soal options { get; set; }
    public List<string> options2 { get; set; }
}

public class detail_soal
{
    public string idSoal { get; set; }
    public string vpilihan1 { get; set; }
    public string vpilihan2 { get; set; }
    public string vpilihan3 { get; set; }
    public string vpilihan4 { get; set; }
}
public class tmp_jawaban
{
    public string idSoal { get; set; }
    public string vjawab1 { get; set; }
    public string vjawab2 { get; set; }
    public string vjawaban { get; set; }
}
#endregion