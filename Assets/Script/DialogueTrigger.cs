using UnityEngine;

/// <summary>
/// Attach to a GameObject with a trigger Collider. When the player enters
/// it, tells DialogueManager to jump to and display TriggerTextID.
/// </summary>
public class DialogueTrigger : MonoBehaviour
{
    [Tooltip("The dialogue line ID to jump to when the player enters this trigger.")]
    public int TriggerTextID;

    [Tooltip("Only an object with this tag will activate the trigger.")]
    public string playerTag = "Player";

    [Tooltip("If true, this trigger only fires once, then stays inactive.")]
    public bool triggerOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnce) return;
        if (!other.CompareTag(playerTag)) return;

        if (DialogueManager.Instance == null)
        {
            Debug.LogError("DialogueTrigger: no DialogueManager found in the scene.");
            return;
        }

        DialogueManager.Instance.StartDialogueAt(TriggerTextID);
        hasTriggered = true;
    }
}