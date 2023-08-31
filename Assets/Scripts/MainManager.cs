using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.IO;
using System.IO;
using System.Runtime.ExceptionServices;

public class MainManager : MonoBehaviour
{

    [Serializable]
    class Data
    {
        public Tuple<int, string>[] scores;
    }


    public Brick BrickPrefab;
    public int LineCount = 6;
    public Rigidbody Ball;

    public Text ScoreText;
    public GameObject GameOverText;
    
    private bool m_Started = false;
    private int m_Points;
    
    private bool m_GameOver = false;



    public MainManager Instance;


    [SerializeField] public Tuple<int, string>[] scores;

    private bool initialized;

    public string playerName;

    private void Awake()
    {
        Debug.Log("Init");
        if(Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            initialized = false;
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
        }

    }

    public void Init()
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(1))
        {
            scores = 
             new Tuple<int, string>[]{
                new Tuple<int, string>(1, "No Score"), 
                new Tuple<int, string>(2, "No Score"),
                new Tuple<int, string>(3, "No Score"), 
                new Tuple<int, string>(4, "No Score"),
                new Tuple<int, string>(5, "No Score")
            };

            Ball = GameObject.Find("Paddle").transform.Find("Ball").GetComponent<Rigidbody>();
            ScoreText = GameObject.Find("Canvas").transform.Find("ScoreText").GetComponent<Text>();
            GameOverText = GameObject.Find("Canvas").transform.Find("GameoverText").gameObject;
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
    } 

    public void saveData ()
    {
        Data data = new Data();
        data.scores = Instance.scores;
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(Application.persistentDataPath + "/data.json", json);

    }

    public void loadData ()
    {

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
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
            }
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
        Array.Sort(scores);
        m_GameOver = true;
        GameOverText.GetComponent<Text>().enabled = true;
    }

    public bool placeScore(string name, int score)
    {
        for(int i = scores.Length-1; i >= 0; i--)
        {
            if (score > scores[i].Item1)
            {
                scores[scores.Length-1] = new Tuple<int, string>(score, name);
                Array.Sort(scores);
                return true;
            }
        }
        return false;
    }
}
