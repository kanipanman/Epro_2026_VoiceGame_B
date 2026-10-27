
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class LightFlicker2D : MonoBehaviour
{
    [Header("明るさの範囲")]
    [SerializeField, Range(0f, 2f)]
    private float minIntensity = 0.55f;

    [SerializeField, Range(0f, 2f)]
    private float maxIntensity = 0.85f;

    [Header("ちらつき速度")]
    [SerializeField, Min(0.1f)]
    private float flickerSpeed = 2f;

    [Header("ちらつきの滑らかさ")]
    [SerializeField, Range(0f, 1f)]
    private float smoothness = 0.7f;

    private Light2D light2D;
    private float randomOffset;

    private void Awake()
    {
        light2D = GetComponent<Light2D>();

        // 複数のライトが同時に点滅しないようにする
        randomOffset = Random.Range(0f, 100f);
    }

    private void Update()
    {
        if (Time.timeScale <= 0f)
            return;

        float noise = Mathf.PerlinNoise(
            Time.time * flickerSpeed,
            randomOffset
        );

        float minValue = Mathf.Min(
            minIntensity,
            maxIntensity
        );

        float maxValue = Mathf.Max(
            minIntensity,
            maxIntensity
        );

        float targetIntensity = Mathf.Lerp(
            minValue,
            maxValue,
            noise
        );

        // 光が急激に切り替わらないよう滑らかに変化
        float lerpFactor =
            1f - Mathf.Exp(
                -Mathf.Lerp(20f, 2f, smoothness)
                * Time.deltaTime
            );

        light2D.intensity = Mathf.Lerp(
            light2D.intensity,
            targetIntensity,
            lerpFactor
        );
    }
}
