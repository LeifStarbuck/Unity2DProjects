using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueBox : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject dialogueRoot;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Typing")]
    [SerializeField] private float charactersPerSecond = 40f;

    private string[] pages;
    private int currentPage = 0;
    private Coroutine typingCoroutine;
    private bool isTyping;
    private bool pageComplete;

    public bool IsOpen => dialogueRoot != null && dialogueRoot.activeSelf;

    private void Start()
    {
        if (dialogueRoot != null)
            dialogueRoot.SetActive(false);
    }

    public void StartDialogue(string[] newPages)
    {
        if (newPages == null || newPages.Length == 0)
        {
            Debug.LogWarning("DialogueBox: StartDialogue called with no pages.", this);
            return;
        }

        pages = newPages;
        currentPage = 0;
        dialogueRoot.SetActive(true);
        ShowPage(currentPage);
    }

    private void Update()
    {
        if (dialogueRoot == null || !dialogueRoot.activeSelf)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (isTyping)
            {
                FinishPageImmediately();
            }
            else if (pageComplete)
            {
                NextPage();
            }
        }
    }

    private void ShowPage(int pageIndex)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueText.text = "";
        isTyping = false;
        pageComplete = false;

        typingCoroutine = StartCoroutine(TypeText(pages[pageIndex]));
    }

    private IEnumerator TypeText(string fullText)
    {
        dialogueText.text = "";
        isTyping = true;
        pageComplete = false;

        float delay = 1f / Mathf.Max(1f, charactersPerSecond);

        foreach (char c in fullText)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(delay);
        }

        typingCoroutine = null;
        isTyping = false;
        pageComplete = true;
    }

    private void FinishPageImmediately()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueText.text = pages[currentPage];
        isTyping = false;
        pageComplete = true;
    }

    private void NextPage()
    {
        currentPage++;

        if (currentPage >= pages.Length)
        {
            EndDialogue();
            return;
        }

        ShowPage(currentPage);
    }

    public void EndDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
        pageComplete = false;

        if (dialogueText != null)
            dialogueText.text = "";

        if (dialogueRoot != null)
            dialogueRoot.SetActive(false);
    }
}