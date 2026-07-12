using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI SubtitleText;


    [Serializable]
    public class DialogueEntry // Single line of dialogue. Field names must match the JSON keys exactly
    {
        public string id;
        public string character;
        public string text;
        public string nextId;
    }


}
