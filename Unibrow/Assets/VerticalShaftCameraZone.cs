using UnityEngine;
using Unity.Cinemachine;

public class VerticalShaftCameraZone : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private VerticalShaftTarget shaftTarget;
    [SerializeField] private CinemachineCamera normalCamera;
    [SerializeField] private CinemachineCamera shaftCamera;
    [SerializeField] private float shaftCenterX;

    [SerializeField] private int normalPriority = 10;
    [SerializeField] private int shaftPriority = 20;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        shaftTarget.SetShaftCenterX(shaftCenterX);
        shaftCamera.Priority = shaftPriority;
        normalCamera.Priority = normalPriority;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        shaftCamera.Priority = 0;
        normalCamera.Priority = normalPriority;
    }
}