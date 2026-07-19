using UnityEngine;

public class ElevatorTrigger : MonoBehaviour
{
    public Elevator elevator;
    public bool isOpenZone; // true = calls OpenDoor, false = calls CloseDoor
    public string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (isOpenZone)
            elevator.OpenDoor();
        else
            elevator.CloseDoor();
    }
}
