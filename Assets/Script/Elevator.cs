using System.Collections;
using TMPro;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    public GameObject DoorL;
    public GameObject DoorR;
    public TextMeshProUGUI Level_text;
    private int Level;

    public float doorMoveDistance = 1f;
    public float doorSpeed = 2f;
    private bool doorsOpen = false;

    private Coroutine doorRoutine;

    public void CloseDoor()
    {
        if (!doorsOpen) return;
        doorsOpen = false;

        Vector3 targetL = DoorL.transform.position + Vector3.left * doorMoveDistance;
        Vector3 targetR = DoorR.transform.position + Vector3.right * doorMoveDistance;

        StartDoorMove(targetL, targetR);
    }

    public void OpenDoor()
    {
        if (doorsOpen) return;
        doorsOpen = true;

        Vector3 targetL = DoorL.transform.position + Vector3.right * doorMoveDistance;
        Vector3 targetR = DoorR.transform.position + Vector3.left * doorMoveDistance;

        StartDoorMove(targetL, targetR);
    }

    private void StartDoorMove(Vector3 targetL, Vector3 targetR)
    {
        if (doorRoutine != null)
            StopCoroutine(doorRoutine);

        doorRoutine = StartCoroutine(MoveDoors(targetL, targetR));
    }

    private IEnumerator MoveDoors(Vector3 targetL, Vector3 targetR)
    {
        Vector3 startL = DoorL.transform.position;
        Vector3 startR = DoorR.transform.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * doorSpeed;
            DoorL.transform.position = Vector3.Lerp(startL, targetL, t);
            DoorR.transform.position = Vector3.Lerp(startR, targetR, t);
            yield return null;
        }

        DoorL.transform.position = targetL;
        DoorR.transform.position = targetR;

        doorRoutine = null;
    }

    private void UpdateLevelText()
    {
        Level_text.text = Level.ToString();
    }

    private void GoToFloor(int floor)
    {
    }
}