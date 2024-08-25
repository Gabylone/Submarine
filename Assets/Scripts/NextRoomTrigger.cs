using UnityEngine;

public class NextRoomTrigger : MonoBehaviour {
   
    public void GenerateNextRoom() {
        Debug.Log($"making next room");
        RoomManager.Instance.GenerateNewRoom(transform.position + transform.forward *1f, transform.forward, 1);
    }
}
