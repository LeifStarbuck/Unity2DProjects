using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject pauseMenuRoot;

    [Header("Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text continueText;
    [SerializeField] private TMP_Text restartText;
    [SerializeField] private TMP_Text quitText;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Style")]
    [SerializeField] private Color normalColor = new Color(0.75f, 0.75f, 0.75f, 1f);
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private float normalSize = 26f;
    [SerializeField] private float selectedSize = 30f;

    private int selectedIndex = 0;
    private bool isPaused = false;

    private TMP_Text[] items;

    void Awake()
    {
        items = new TMP_Text[] { continueText, restartText, quitText };

        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(false);

        UpdateVisuals();
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }

        if (!isPaused) return;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedIndex--;
            if (selectedIndex < 0) selectedIndex = items.Length - 1;
            UpdateVisuals();
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedIndex++;
            if (selectedIndex >= items.Length) selectedIndex = 0;
            UpdateVisuals();
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            ActivateSelection();
        }
    }

    private void PauseGame()
    {
        isPaused = true;
        selectedIndex = 0;

        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(true);

        Time.timeScale = 0f;
        UpdateVisuals();
    }

    private void ResumeGame()
    {
        isPaused = false;

        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(false);

        Time.timeScale = 1f;
    }

    private void ActivateSelection()
    {
        switch (selectedIndex)
        {
            case 0:
                ResumeGame();
                break;

            case 1:
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                break;

            case 2:
                Time.timeScale = 1f;
                SceneManager.LoadScene(mainMenuSceneName);
                break;
        }
    }

    private void UpdateVisuals()
    {
        if (titleText != null)
            titleText.text = "THE GREAT UNIBROW";

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null) continue;

            bool selected = (i == selectedIndex);
            items[i].color = selected ? selectedColor : normalColor;
            items[i].fontSize = selected ? selectedSize : normalSize;
        }
    }
}