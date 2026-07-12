using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

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
    public TextMeshProUGUI SubtitleText;
    public static TextDataList textDataList;

    private Dictionary<int, TextData> textDict; 
    private TextData CurrentText;
    private int CurrentTextID;
    private int NextTextID;

    // ID of the very first line of dialogue. Set this to whatever your JSON uses as the start.
    [SerializeField] private int startID = 0;

    //load text
    void Awake()
    {
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
        SubtitleText.text = CurrentText.Character + CurrentText.Text;

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