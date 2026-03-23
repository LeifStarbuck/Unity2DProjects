using UnityEngine;
using Unity.Cinemachine;

public class HiddenRoomCameraZone : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] private CinemachineCamera normalCamera;
    [SerializeField] private CinemachineCamera hiddenRoomCamera;

    [Header("Priorities")]
    [SerializeField] private int normalPriority = 10;
    [SerializeField] private int hiddenRoomPriority = 20;
    [SerializeField] private int inactivePriority = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (normalCamera == null || hiddenRoomCamera == null) return;

        normalCamera.Priority = normalPriority;
        hiddenRoomCamera.Priority = hiddenRoomPriority;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (normalCamera == null || hiddenRoomCamera == null) return;

        hiddenRoomCamera.Priority = inactivePriority;
        normalCamera.Priority = normalPriority;
    }
}