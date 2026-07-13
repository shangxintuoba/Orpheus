using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.InputSystem;

[Serializable]
public class TextData // Single line of dialogue. Field names must match the JSON keys exactly
{
    public int ID;
    public string Character;
    public string Text;
    public int nextID;
    public bool isAChoice;
    public int nextID_true;
    public int nextID_false;
    public int nextID_notAnswered;
}

[System.Serializable]
public class TextDataList
{
    public TextData[] textList;
}

public class DialogueManager : MonoBehaviour
{
    private Keyboard keyboard;

    public TextMeshProUGUI SubtitleText;
    public static TextDataList textDataList;

    private Dictionary<int, TextData> textDict;
    private TextData CurrentText;
    private int CurrentTextID;
    private int NextTextID;

    // ID of the very first line of dialogue. Set this to whatever your JSON uses as the start.
    [SerializeField] private int startID = 0;

    [Header("Typing Effect")]
    [SerializeField] private float typingSpeed = 0.03f; // seconds per character
    private Coroutine typingCoroutine;
    private bool isTyping = false;

    //load text
    void Awake()
    {
        keyboard = Keyboard.current;

        string path = Path.Combine(Application.streamingAssetsPath, "Text.json");
        string json = File.ReadAllText(path);
        string wrappedJson = "{\"textList\":" + json + "}";
        textDataList = JsonUtility.FromJson<TextDataList>(wrappedJson);

        // Build the lookup once, so UpdateText() doesn't have to loop through
        // the array every time it needs the next line.
        textDict = new Dictionary<int, TextData>();
        foreach (TextData entry in textDataList.textList)
        {
            if (textDict.ContainsKey(entry.ID))
            {
                Debug.LogWarning($"DialogueManager: duplicate dialogue ID '{entry.ID}' found — skipping duplicate.");
                continue;
            }
            textDict.Add(entry.ID, entry);
        }
    }

    private void Start()
    {
        NextTextID = startID;
        UpdateText();
    }

    private void Update()
    {
        //for test
        AdvanceText();
    }

    private void UpdateText()
    {
        // find the textdata in the json file has ID that match the NextTextID, and set it as the CurrentText
        if (!textDict.TryGetValue(NextTextID, out CurrentText))
        {
            Debug.LogError($"DialogueManager: no dialogue entry found for ID '{NextTextID}'.");
            return;
        }

        CurrentTextID = CurrentText.ID;
        StartTyping(CurrentText.Text);

        if (!CurrentText.isAChoice)
        {
            NextTextID = CurrentText.nextID;
        }
        else
        {
            // It's a choice line: don't advance automatically.
            // Wait here — call SelectChoice(true/false) from your UI buttons,
            // or call it with a "no answer" timeout, to decide where to go next.
        }
    }

    /// <summary>
    /// Begins revealing the given line one character at a time.
    /// Cancels any typing effect already in progress first.
    /// </summary>
    private void StartTyping(string fullText)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(fullText));
    }

    private IEnumerator TypeText(string fullText)
    {
        isTyping = true;
        SubtitleText.text = "";

        foreach (char c in fullText)
        {
            SubtitleText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        typingCoroutine = null;
    }

    /// <summary>
    /// Instantly fills in the rest of the current line, skipping the typing animation.
    /// </summary>
    private void SkipTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        if (CurrentText != null)
        {
            SubtitleText.text = CurrentText.Text;
        }
        isTyping = false;
    }

    /// <summary>
    /// Debug-only advance: press R to step to the next line and print the
    /// current line's info to the console. Useful for testing dialogue flow
    /// without hooking up UI buttons yet.
    /// </summary>
    public void AdvanceText()
    {
        if (keyboard == null)
        {
            // Keyboard.current can be null if no keyboard device has been
            // detected yet (e.g. very first frame). Re-check each call
            // rather than caching a stale null reference.
            keyboard = Keyboard.current;
            if (keyboard == null) return;
        }

        if (keyboard.rKey.wasPressedThisFrame)
        {
            // First press while typing: just finish revealing the line instantly.
            // This is standard dialogue-system behavior (press-to-skip-typing,
            // press-again-to-advance) so players don't accidentally skip lines.
            if (isTyping)
            {
                SkipTyping();
                return;
            }

            if (CurrentText != null)
            {
                Debug.Log(
                    $"[DialogueManager] ID: {CurrentText.ID} | Character: {CurrentText.Character} | " +
                    $"Text: {CurrentText.Text} | isAChoice: {CurrentText.isAChoice} | NextTextID: {NextTextID}"
                );
            }

            if (CurrentText != null && CurrentText.isAChoice)
            {
                Debug.LogWarning("DialogueManager: current line is a choice — press won't auto-advance. Call SelectChoice() instead.");
                return;
            }

            UpdateText();
        }
    }

    /// <summary>
    /// Call this from a UI button (or a timer) once the player has answered
    /// the current choice. Advances to the correct branch and displays it.
    /// </summary>
    public void SelectChoice(bool? answer)
    {
        if (CurrentText == null || !CurrentText.isAChoice)
        {
            Debug.LogWarning("DialogueManager: SelectChoice called but current line isn't a choice.");
            return;
        }

        if (answer == true)
        {
            NextTextID = CurrentText.nextID_true;
        }
        else if (answer == false)
        {
            NextTextID = CurrentText.nextID_false;
        }
        else // null = no answer given (e.g. player let a timer run out)
        {
            NextTextID = CurrentText.nextID_notAnswered;
        }

        UpdateText();
    }

    /// <summary>
    /// Call this (e.g. from an "advance dialogue" input) to move to the next line
    /// when the current line is NOT a choice.
    /// </summary>
    public void Advance()
    {
        if (CurrentText != null && CurrentText.isAChoice)
        {
            Debug.LogWarning("DialogueManager: current line is a choice — call SelectChoice() instead.");
            return;
        }

        UpdateText();
    }
}