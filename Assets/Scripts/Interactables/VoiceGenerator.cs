
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(SpriteRenderer))]
public class VoiceGenerator : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private PlayerController player;
    [SerializeField] private MicrophoneInput microphone;
    [SerializeField] private Slider progressSlider;

    [Header("装置設定")]
    [SerializeField] private float interactionRange = 1.6f;
    [SerializeField] private float requiredTime = 3f;

    [Header("必要な声量")]
    [SerializeField, Range(0f, 1f)]
    private float voiceThreshold = 0.2f;

    private SpriteRenderer spriteRenderer;

    // 装置の状態
    private bool isInteracting;
    private bool isActivated;
    private float currentTime;

    // 現在操作している装置を共有
    private static VoiceGenerator activeGenerator;

    // 外部から装置の起動状態を取得
    public bool IsActivated => isActivated;

    // 装置起動時に外部へ通知するイベント
    public event Action<VoiceGenerator> Activated;

    // 再生開始時に操作状態を初期化
    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        activeGenerator = null;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = Color.yellow;

        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = 0f;
            progressSlider.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        if (player == null || microphone == null)
        {
            Debug.LogError(
                $"{name}: 参照が設定されていません。"
            );
            enabled = false;
        }
    }

    private void Update()
    {
        // ゲーム停止中は装置を操作できない
        if (Time.timeScale <= 0f)
        return;

        // 起動済みの装置は操作しない
        if (isActivated) return;

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            StopInteraction();
            return;
        }

        if (isInteracting)
        {
            // Eキーを離したら操作を終了
            if (!keyboard.eKey.isPressed || !IsPlayerNear())
            {
                StopInteraction();
                return;
            }

            // 一定以上の声量で起動ゲージを進める
            if (microphone.IsReady &&
                microphone.Volume01 >= voiceThreshold)
            {
                currentTime += Time.deltaTime;
            }

            // 起動ゲージの表示を更新
            if (progressSlider != null)
            {
                progressSlider.value = Mathf.Clamp01(
                    currentTime / Mathf.Max(requiredTime, 0.1f)
                );
            }

            // 規定時間に達したら起動完了
            if (currentTime >= Mathf.Max(requiredTime, 0.1f))
            {
                ActivateGenerator();
            }
        }
        else
        {
            // 他の装置を操作中なら開始しない
            if (activeGenerator != null) return;

            if (IsPlayerNear() &&
                keyboard.eKey.wasPressedThisFrame)
            {
                StartInteraction();
            }
        }
    }

    // プレイヤーとの距離を確認
    private bool IsPlayerNear()
    {
        if (player == null) return false;

        return Vector2.Distance(
            transform.position,
            player.transform.position
        ) <= interactionRange;
    }

    // 装置の操作開始
    private void StartInteraction()
    {
        if (activeGenerator != null) return;

        activeGenerator = this;
        isInteracting = true;

        player.SetMovementEnabled(false);
        spriteRenderer.color = Color.cyan;

        if (progressSlider != null)
        {
            progressSlider.gameObject.SetActive(true);
            progressSlider.value = Mathf.Clamp01(
                currentTime / Mathf.Max(requiredTime, 0.1f)
            );
        }
    }

    // 装置の操作終了
    private void StopInteraction()
    {
        if (!isInteracting) return;

        isInteracting = false;

        if (activeGenerator == this)
        {
            activeGenerator = null;
        }

        if (player != null)
        {
            player.SetMovementEnabled(true);
        }

        if (!isActivated)
        {
            spriteRenderer.color = Color.yellow;
        }

        if (progressSlider != null)
        {
            progressSlider.gameObject.SetActive(false);
        }
    }

    // 装置の起動完了
    private void ActivateGenerator()
    {
        if (isActivated) return;

        isActivated = true;

        StopInteraction();

        spriteRenderer.color = Color.green;

        Debug.Log($"{name}: 装置の起動完了");

        // GeneratorManagerなどへ起動を通知
        Activated?.Invoke(this);
    }

    private void OnDisable()
    {
        StopInteraction();
    }
}
