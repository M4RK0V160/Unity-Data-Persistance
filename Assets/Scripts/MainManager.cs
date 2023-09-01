using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.IO;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Data;

public class MainManager : MonoBehaviour
{

    [Serializable]
    class Data
    {
        public int score_1;
        public int score_2;
        public int score_3;
        public int score_4;
        public int score_5;

        public string name_1;
        public string name_2;
        public string name_3;
        public string name_4;
        public string name_5;

        public void loadScores(Tuple<int, string>[] scores)
        {
            score_1 = scores[0].Item1;
            score_2 = scores[1].Item1;
            score_3 = scores[2].Item1;
            score_4 = scores[3].Item1;
            score_5 = scores[4].Item1;

            name_1 = scores[0].Item2;
            name_2 = scores[1].Item2;
            name_3 = scores[2].Item2;
            name_4 = scores[3].Item2;
            name_5 = scores[4].Item2;
        }

        public void unloadScores(Tuple<int, string>[] scores)
        {
            scores[0] = new Tuple<int, string>(score_1,name_1);
            scores[1] = new Tuple<int, string>(score_2,name_2);
            scores[2] = new Tuple<int, string>(score_3,name_3);
            scores[3] = new Tuple<int, string>(score_4,name_4);
            scores[4] = new Tuple<int, string>(score_5,name_5);
        }
    }


    public Brick BrickPrefab;
    public int LineCount = 6;
    public Rigidbody Ball;

    public Text ScoreText;
    public Text BestScoreText;
    public GameObject GameOverText;
    
    private bool m_Started = false;
    private int m_Points;
    
    private bool m_GameOver = false;



    public MainManager Instance;


    [SerializeField] public Tuple<int, string>[] scores;

    private bool initialized;
    private bool scoresSet;

    public string playerName = "Player Name";

    private Text playerNameField;
    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            
            initialized = false;
            scoresSet = false;
            scores =
             new Tuple<int, string>[]{
                new Tuple<int, string>(0, "No Score"),
                new Tuple<int, string>(0, "No Score"),
                new Tuple<int, string>(0, "No Score"),
                new Tuple<int, string>(0, "No Score"),
                new Tuple<int, string>(0, "No Score")
            };
            
            playerNameField = GameObject.Find("Canvas").transform.Find("NameField").transform.Find("Text").GetComponent<Text>();
            Instance = this;
            Init();
            DontDestroyOnLoad(gameObject);
            
        }

    }

    public void Init()
    {
        loadData();
        initStateVariables();
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(1))
        {
            //Set the object references when the main scene is loaded

            Ball = GameObject.Find("Paddle").transform.Find("Ball").GetComponent<Rigidbody>();
            ScoreText = GameObject.Find("Canvas").transform.Find("ScoreText").GetComponent<Text>();
            GameOverText = GameObject.Find("Canvas").transform.Find("GameoverText").gameObject;
            BestScoreText = GameObject.Find("Canvas").transform.Find("BestScoreText").GetComponent<Text>();

            //set the instance as the main manager for the DeathZone controller
            GameObject.Find("DeathZone").GetComponent<DeathZone>().setManager(Instance);

            const float step = 0.6f;
            int perLine = Mathf.FloorToInt(4.0f / step);

            int[] pointCountArray = new[] { 1, 1, 2, 2, 5, 5 };
            for (int i = 0; i < LineCount; ++i)
            {
                for (int x = 0; x < perLine; ++x)
                {
                    Vector3 position = new Vector3(-1.5f + step * x, 2.5f + i * 0.3f, 0);
                    var brick = Instantiate(BrickPrefab, position, Quaternion.identity);
                    brick.PointValue = pointCountArray[i];
                    brick.onDestroyed.AddListener(AddPoint);
                }
            }
        }
        else
        {  
            setScores();
        }
    } 
    public void initStateVariables()
    {
        scoresSet = false;
        m_Started = false;
        m_Points = 0;
    }


    public void saveData ()
    {
        Data data = new Data();
        data.loadScores(scores);
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(Application.persistentDataPath + "/data.json", json);
        
    }

    public void loadData ()
    {
        Data data = new Data();
        string json = File.ReadAllText(Application.persistentDataPath + "/data.json");
        Debug.Log(Application.persistentDataPath);
        data = JsonUtility.FromJson<Data>(json);
        data.unloadScores(scores);
    }
    // Start is called before the first frame update

    private void Update()
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(1))
        {
            if (!initialized)
            {
                Init();
                initialized = true;
            }
            if (!m_Started)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    m_Started = true;
                    float randomDirection = UnityEngine.Random.Range(-1.0f, 1.0f);
                    Vector3 forceDir = new Vector3(randomDirection, 1, 0);
                    forceDir.Normalize();

                    Ball.transform.SetParent(null);
                    Ball.AddForce(forceDir * 2.0f, ForceMode.VelocityChange);
                }
            }
            else if (m_GameOver)
            {

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    initialized = false;
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
            }
        }
    }

    private void setScores()
    {
        var reversedScores = scores.Reverse().ToArray();
        for (int i = 0; i < scores.Length; i++)
        {
            GameObject.Find("Canvas").transform.Find("Container").transform.Find("ScoreBoard").transform.Find("Score_" + (i + 1).ToString()).GetComponent<Text>().text
                = "|" + reversedScores[i].Item2 + " : " + reversedScores[i].Item1.ToString();
        }
    }

    void AddPoint(int point)
    {
        m_Points += point;
        ScoreText.text = $"Score : {m_Points}";
    }

    public void GameOver()
    {
        placeScore(playerName, m_Points);
        BestScoreText.text = "Best Score| " + scores[scores.Length-1].Item2 + " : " + scores[scores.Length - 1].Item1.ToString();
        saveData();
        m_GameOver = true;
        GameOverText.GetComponent<Text>().enabled = true;
    }

    public bool placeScore(string name, int score)
    {
        for(int i = 0; i < scores.Length ; i++)
        {
            if (score > scores[i].Item1)
            {
               
                scores[0] = new Tuple<int, string>(score, name);
                Array.Sort(scores);
                return true;
            }
        }
        return false;
    }

    public void printScores()
    {
        foreach(Tuple<int, string> pair in scores)
        {
            Debug.Log(pair.Item1.ToString() + pair.Item2);
        }
    }

    public bool GetPlayerName()
    {
        if(playerNameField.text == "")
        {
            playerNameField.transform.parent.GetComponent<Image>().color = Color.red;
            return false;
        }
        else
        {
            playerName = playerNameField.text;
            return true;
        }
    }
}
