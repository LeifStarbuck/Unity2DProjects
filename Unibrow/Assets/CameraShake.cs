using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    private CinemachineBasicMultiChannelPerlin noise;
    private Coroutine co;

    void Awake()
    {
        // In CM3, Noise is a component you add to the same CinemachineCamera GameObject
        noise = GetComponent<CinemachineBasicMultiChannelPerlin>();

        if (!noise)
            Debug.LogError("CameraShake: Missing CinemachineBasicMultiChannelPerlin on this CinemachineCamera.", this);
    }

    public void Shake(float strength, float time)
    {
        if (!noise) return;

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