using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueBox : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject dialogueRoot;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Typing")]
    [SerializeField] private float charactersPerSecond = 40f;

    private DialogueLine[] lines;
    private int currentLineIndex = 0;
    private Coroutine typingCoroutine;
    private bool isTyping;
    private bool pageComplete;

    public bool IsOpen => dialogueRoot != null && dialogueRoot.activeSelf;

    private void Start()
    {
        if (dialogueRoot != null)
            dialogueRoot.SetActive(false);
    }

    public void StartDialogue(DialogueLine[] newLines)
    {
        if (newLines == null || newLines.Length == 0)
        {
            Debug.LogWarning("DialogueBox: StartDialogue called with no lines.", this);
            return;
        }

        lines = newLines;
        currentLineIndex = 0;

        if (dialogueRoot != null)
            dialogueRoot.SetActive(true);

        ShowLine(currentLineIndex);
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
                FinishLineImmediately();
            }
            else if (pageComplete)
            {
                NextLine();
            }
        }
    }

    private void ShowLine(int index)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        DialogueLine line = lines[index];

        if (speakerNameText != null)
        {
            speakerNameText.enableVertexGradient = false;
            speakerNameText.colorGradientPreset = null;
            speakerNameText.text = line.speakerName;
            speakerNameText.color = line.speakerColor;
            speakerNameText.ForceMeshUpdate();
        }

        if (dialogueText != null)
        {
            dialogueText.text = "";
            dialogueText.color = Color.white; // or line.speakerColor if you want
        }

        isTyping = false;
        pageComplete = false;

        typingCoroutine = StartCoroutine(TypeText(line.text));
    }

    private IEnumerator TypeText(string fullText)
    {
        if (dialogueText == null)
            yield break;

        isTyping = true;
        pageComplete = false;

        dialogueText.text = fullText;
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();

        int totalVisibleCharacters = dialogueText.textInfo.characterCount;
        float baseDelay = 1f / Mathf.Max(1f, charactersPerSecond);

        for (int i = 0; i <= totalVisibleCharacters; i++)
        {
            dialogueText.maxVisibleCharacters = i;

            float wait = baseDelay;

            if (i > 0 && i - 1 < fullText.Length)
            {
                char currentChar = fullText[i - 1];

                if (currentChar == '.' || currentChar == '!' || currentChar == '?')
                    wait *= 4f;
                else if (currentChar == ',' || currentChar == ';')
                    wait *= 2f;
            }

            yield return new WaitForSeconds(wait);
        }

        typingCoroutine = null;
        isTyping = false;
        pageComplete = true;
    }

    private void FinishLineImmediately()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (dialogueText != null)
        {
            dialogueText.text = lines[currentLineIndex].text;
            dialogueText.ForceMeshUpdate();
            dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;
        }

        isTyping = false;
        pageComplete = true;
    }

    private void NextLine()
    {
        currentLineIndex++;

        if (currentLineIndex >= lines.Length)
        {
            EndDialogue();
            return;
        }

        ShowLine(currentLineIndex);
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

        if (speakerNameText != null)
            speakerNameText.text = "";

        if (dialogueText != null)
        {
            dialogueText.text = "";
            dialogueText.maxVisibleCharacters = 999999;
        }

        if (dialogueRoot != null)
            dialogueRoot.SetActive(false);
    }
}