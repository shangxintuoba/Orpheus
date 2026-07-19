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
    private bool OnGround = true;

    public float floorHeight = 3f;   // distance between floors
    public float elevatorSpeed = 1.5f;

    private Coroutine doorRoutine;
    private Coroutine floorRoutine;

    public void CloseDoor()
    {
        if (!doorsOpen) return;
        doorsOpen = false;

        Vector3 targetL = DoorL.transform.position + Vector3.left * doorMoveDistance;
        Vector3 targetR = DoorR.transform.position + Vector3.right * doorMoveDistance;

        StartCoroutine(CloseThenChangeFloor(targetL, targetR));
    }

    private IEnumerator CloseThenChangeFloor(Vector3 targetL, Vector3 targetR)
    {
        yield return MoveDoors(targetL, targetR);
        ChangeFloor();
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

        doorRoutine = StartCoroutine(MoveDoorsWrapper(targetL, targetR));
    }

    private IEnumerator MoveDoorsWrapper(Vector3 targetL, Vector3 targetR)
    {
        yield return MoveDoors(targetL, targetR);
        doorRoutine = null;
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
    }

    public void ChangeFloor()
    {
        if (floorRoutine != null)
            StopCoroutine(floorRoutine);

        Vector3 targetPos = transform.position + (OnGround ? Vector3.up : Vector3.down) * floorHeight;
        floorRoutine = StartCoroutine(MoveElevator(targetPos));

        OnGround = !OnGround;
    }

    private IEnumerator MoveElevator(Vector3 targetPos)
    {
        Vector3 startPos = transform.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * elevatorSpeed;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;
        floorRoutine = null;

    }

}