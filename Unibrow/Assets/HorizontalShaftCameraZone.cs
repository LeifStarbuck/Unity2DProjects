using UnityEngine;
using Unity.Cinemachine;

public class HorizontalShaftCameraZone : MonoBehaviour
{
    [SerializeField] private HorizontalShaftTarget shaftTarget;
    [SerializeField] private CinemachineCamera normalCamera;
    [SerializeField] private CinemachineCamera horizontalShaftCamera;
    [SerializeField] private float shaftCenterY;

    [SerializeField] private int normalPriority = 10;
    [SerializeField] private int shaftPriority = 20;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        shaftTarget.SetShaftCenterY(shaftCenterY);
        horizontalShaftCamera.Priority = shaftPriority;
        normalCamera.Priority = normalPriority;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        horizontalShaftCamera.Priority = 0;
        normalCamera.Priority = normalPriority;
    }
}