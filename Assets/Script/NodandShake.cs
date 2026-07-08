using UnityEngine;
using UnityEngine.InputSystem;

public class NodandShake : MonoBehaviour
{
    [Header("Thresholds")]
    [SerializeField] private float YesCrux = 5f;
    [SerializeField] private float NoCrux = 5f;

    [Header("Tuning")]
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float minMovementThreshold = 0.01f;
    [SerializeField] private float decayPerSecond = 2f;
    [SerializeField] private float answerHoldTime = 1f;

    private float DeltaYes;
    private float DeltaNo;
    private int Answer = 2;

    private int xDirection = 0;
    private int yDirection = 0;
    private float xLegAccum = 0f;
    private float yLegAccum = 0f;
    private float answerTimer = 0f;

    [SerializeField] private bool isInAnsweringMode; // trigger mouse detection only when isInAnsweringMode

    void Update()
    {
        if (!isInAnsweringMode)
            return;

        if (Mouse.current == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float mouseX = mouseDelta.x * mouseSensitivity;
        float mouseY = mouseDelta.y * mouseSensitivity;

        ProcessAxis(mouseX, ref xDirection, ref xLegAccum, ref DeltaNo);
        ProcessAxis(mouseY, ref yDirection, ref yLegAccum, ref DeltaYes);

        DeltaNo = Mathf.Max(0f, DeltaNo - decayPerSecond * Time.deltaTime);
        DeltaYes = Mathf.Max(0f, DeltaYes - decayPerSecond * Time.deltaTime);

        CheckThresholds();
        HandleAnswerLifetime();
    }

    private void ProcessAxis(float movement, ref int direction, ref float legAccum, ref float delta)
    {
        if (Mathf.Abs(movement) < minMovementThreshold)
            return;

        int newDirection = movement > 0 ? 1 : -1;

        if (direction == 0)
        {
            direction = newDirection;
            legAccum = Mathf.Abs(movement);
            return;
        }

        if (newDirection != direction)
        {
            delta += legAccum;
            legAccum = Mathf.Abs(movement);
            direction = newDirection;
        }
        else
        {
            legAccum += Mathf.Abs(movement);
        }
    }

    private void CheckThresholds()
    {
        if (Answer != 2) return;

        if (DeltaNo >= NoCrux) SetAnswer(0);
        else if (DeltaYes >= YesCrux) SetAnswer(1);
    }

    private void SetAnswer(int value)
    {
        Answer = value;
        answerTimer = 0f;
        ResetDeltas();
        Debug.Log(Answer);
    }

    private void HandleAnswerLifetime()
    {
        if (Answer == 2) return;

        answerTimer += Time.deltaTime;
        if (answerTimer >= answerHoldTime)
        {
            Answer = 2;
            ResetDeltas();
        }
    }

    private void ResetDeltas()
    {
        DeltaYes = 0f;
        DeltaNo = 0f;
        xLegAccum = 0f;
        yLegAccum = 0f;
        xDirection = 0;
        yDirection = 0;
    }

    /// <summary>
    /// Call this to start/stop listening for nod/shake input.
    /// Call with true right when you ask the player a yes/no question,
    /// and false once you've consumed the answer (or it auto-times-out).
    /// </summary>
    public void SetAnsweringMode(bool value)
    {
        isInAnsweringMode = value;

        if (value)
        {
            // clean slate whenever we start listening, so leftover
            // mouse motion from before doesn't get misread as an answer
            ResetDeltas();
            answerTimer = 0f;
            Answer = 2;
        }
    }

    public bool IsInAnsweringMode => isInAnsweringMode;

    public int GetAnswer() => Answer;
}