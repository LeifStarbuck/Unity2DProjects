using UnityEngine;

public class HorizontalShaftTarget : MonoBehaviour
{
    [SerializeField] private Transform player;
    private float shaftCenterY;

    void LateUpdate()
    {
        if (player == null) return;

        transform.position = new Vector3(
            player.position.x,
            shaftCenterY,
            transform.position.z
        );
    }

    public void SetShaftCenterY(float y)
    {
        shaftCenterY = y;
    }
}