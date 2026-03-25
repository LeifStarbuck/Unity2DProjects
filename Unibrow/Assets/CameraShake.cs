using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    private CinemachineBasicMultiChannelPerlin noise;
    private Coroutine co;

    void Awake()
    {
        noise = GetComponent<CinemachineBasicMultiChannelPerlin>();

        if (!noise)
            Debug.LogError("CameraShake: Missing CinemachineBasicMultiChannelPerlin.", this);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            Shake(2f, 0.25f);
    }

    public void Shake(float strength, float time)
    {
        if (!noise)
        {
            Debug.LogWarning("CameraShake: no noise component.", this);
            return;
        }

        Debug.Log($"Shake called on {name} with strength {strength} for {time}s", this);

        if (co != null) StopCoroutine(co);
        co = StartCoroutine(ShakeRoutine(strength, time));
    }

    private IEnumerator ShakeRoutine(float strength, float time)
    {
        noise.AmplitudeGain = strength;
        yield return new WaitForSeconds(time);
        noise.AmplitudeGain = 0f;
        co = null;
    }
}