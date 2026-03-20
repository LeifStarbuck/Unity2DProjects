using UnityEngine;

public class VerticalShaftTarget : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float shaftCenterX;

    void LateUpdate()
    {
        if (player == null) return;

        transform.position = new Vector3(
            shaftCenterX,
            player.position.y,
            transform.position.z
        );
    }

    public void SetShaftCenterX(float x)
    {
        shaftCenterX = x;
    }
}