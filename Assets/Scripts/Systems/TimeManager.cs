using Data;
using Systems;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    public Slider timeSlider; // ����������
    public Button PauseButton;
    public TextMeshProUGUI ButtonText;
    public TextMeshProUGUI TimeText;
    private float timeLimit = 120f; // 2����
    private float currentTime = 0f;
    private bool isPaused = false; // ��Ϸ�Ƿ���ͣ
    private bool isOver = false;
    private int day = 1;
    // Start is called before the first frame update
    void Start()
    {
        timeSlider.value = 0f; // ��ʼʱ������Ϊ0
        PauseButton.onClick.AddListener(ResetProgress);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isPaused)
        {
            // ��ʱ�����½�����
            currentTime += Time.deltaTime;
            timeSlider.value = currentTime / timeLimit;

            

            // ��ʱ��ﵽ���ƣ���ͣ��Ϸ
            if (currentTime >= timeLimit)
            {
                isPaused = true;
                Time.timeScale = 0f; // ��ͣ��Ϸ�е������������Ͷ���
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
        timeSlider.value = 0f; // ���ý�����
        Time.timeScale = 1f;  // �ָ���Ϸʱ��

        
    }
}
