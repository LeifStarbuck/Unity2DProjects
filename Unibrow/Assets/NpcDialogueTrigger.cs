using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class NpcDialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private DialogueBox dialogueBox;

    [TextArea(2, 6)]
    [SerializeField] private string[] pages;

    [Header("Optional Prompt")]
    [SerializeField] private GameObject talkPrompt;

    private bool playerInRange = false;

    private void Start()
    {
        if (talkPrompt != null)
            talkPrompt.SetActive(false);
    }

private void Update()
{
    if (!playerInRange)
        return;

    if (dialogueBox != null && dialogueBox.IsOpen)
        return;

    if (Keyboard.current == null)
        return;

    if (Keyboard.current.eKey.wasPressedThisFrame)
    {
        if (dialogueBox == null)
        {
            Debug.LogError("NpcDialogueTrigger: Cannot start dialogue because DialogueBox is null.", this);
            return;
        }

        dialogueBox.StartDialogue(pages);

        if (talkPrompt != null)
            talkPrompt.SetActive(false);
    }
}

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;

        if (talkPrompt != null)
            talkPrompt.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;

        if (talkPrompt != null)
            talkPrompt.SetActive(false);
    }
}