using Data;
using Systems;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    public Slider timeSlider; // 进度条引用
    public Button PauseButton;
    public TextMeshProUGUI ButtonText;
    public TextMeshProUGUI TimeText;
    private float timeLimit = 120f; // 2分钟
    private float currentTime = 0f;
    private bool isPaused = false; // 游戏是否暂停
    private bool isOver = false;
    private int day = 1;
    // Start is called before the first frame update
    void Start()
    {
        timeSlider.value = 0f; // 开始时进度条为0
        PauseButton.onClick.AddListener(ResetProgress);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isPaused)
        {
            // 计时，更新进度条
            currentTime += Time.deltaTime;
            timeSlider.value = currentTime / timeLimit;

            

            // 当时间达到限制，暂停游戏
            if (currentTime >= timeLimit)
            {
                isPaused = true;
                Time.timeScale = 0f; // 暂停游戏中的所有物理计算和动画
                PauseButton.gameObject.SetActive(true);
                if (CardManager.Food < CardManager.People * 2)
                {
                    ButtonText.text = "Game is over, click to restart";
                    isOver = true;
                }
                else
                {
                    
                    ButtonText.text = "Day " + day + " is over" + ", click to continue.";
                    int requireFood = CardManager.People * 2;
                    day++;
                    while (requireFood > 0)
                    {
                        requireFood -= CardManager.CardsByClass[CardClass.Food][0].Data.Food;
                        CardManager.DestroyCards(CardManager.CardsByClass[CardClass.Food][0]);
                    }
                    
                }
            }
            
        }
    }
  
    void ResetProgress()
    {
        if (isOver)
        {
            day = 1;
            GameManager.instance.RestartGame();
            isOver = false;
        }

        isPaused = false;
        currentTime = 0f;
        TimeText.text = "Day "+(day)+":";
        PauseButton.gameObject.SetActive(false);
        ButtonText.text = "";
        timeSlider.value = 0f; // 重置进度条
        Time.timeScale = 1f;  // 恢复游戏时间

        
    }
}
