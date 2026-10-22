
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MicSettingsUI : MonoBehaviour
{
    [Header("システム参照")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private MicrophoneInput microphone;

    [Header("設定パネル")]
    [SerializeField] private GameObject settingsPanel;

    [Header("マイク感度")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TMP_Text sensitivityValueText;

    [Header("雑音カット")]
    [SerializeField] private Slider noiseFloorSlider;
    [SerializeField] private TMP_Text noiseFloorValueText;

    [Header("マイク音量確認")]
    [SerializeField] private Slider micLevelSlider;
    [SerializeField] private TMP_Text micLevelValueText;
    [SerializeField] private TMP_Text guideText;

    // 設定画面が開いているか
    public bool IsOpen { get; private set; }

    private void Start()
    {
        // 必須参照を確認
        if (gameManager == null ||
            microphone == null ||
            settingsPanel == null ||
            sensitivitySlider == null ||
            noiseFloorSlider == null ||
            micLevelSlider == null)
        {
            Debug.LogError(
                "MicSettingsUI：必要な参照が未設定です。"
            );

            enabled = false;
            return;
        }

        // 感度スライダー設定
        sensitivitySlider.minValue = 1f;
        sensitivitySlider.maxValue = 50f;
        sensitivitySlider.wholeNumbers = false;

        // 雑音カットスライダー設定
        noiseFloorSlider.minValue = 0f;
        noiseFloorSlider.maxValue = 0.1f;
        noiseFloorSlider.wholeNumbers = false;

        // 音量確認ゲージ設定
        micLevelSlider.minValue = 0f;
        micLevelSlider.maxValue = 1f;
        micLevelSlider.interactable = false;

        // 現在のマイク設定をUIに反映
        RefreshSettingsUI();

        // スライダー変更イベントを登録
        sensitivitySlider.onValueChanged.AddListener(
            OnSensitivityChanged
        );

        noiseFloorSlider.onValueChanged.AddListener(
            OnNoiseFloorChanged
        );

        // 開始時はパネル非表示
        IsOpen = false;
        settingsPanel.SetActive(false);
    }

    private void Update()
    {
        if (gameManager == null)
            return;

        Keyboard keyboard = Keyboard.current;

        // Escキーで設定パネルを開閉
        if (keyboard != null &&
            keyboard.escapeKey.wasPressedThisFrame)
        {
            if (IsOpen)
            {
                CloseSettings();
            }
            else if (!gameManager.HasEnded)
            {
                OpenSettings();
            }
        }

        // 設定画面を閉じている間は更新不要
        if (!IsOpen)
            return;

        UpdateMicrophoneLevel();
    }

    // 設定画面を開く
    public void OpenSettings()
    {
        if (IsOpen || gameManager.HasEnded ||
            gameManager.IsPaused)
            return;

        // ゲームを一時停止
        gameManager.PauseGame();

        // 保存済みではなく現在の設定を表示
        RefreshSettingsUI();

        settingsPanel.SetActive(true);
        IsOpen = true;
    }

    // 設定画面を閉じる
    public void CloseSettings()
    {
        if (!IsOpen)
            return;

        // 変更した設定を保存
        microphone.SaveSettings();

        settingsPanel.SetActive(false);
        IsOpen = false;

        // ゲームを再開
        gameManager.ResumeGame();
    }

    // 感度スライダー変更時
    private void OnSensitivityChanged(float value)
    {
        microphone.SetSensitivity(value);

        if (sensitivityValueText != null)
        {
            sensitivityValueText.text =
                microphone.Sensitivity.ToString("F1");
        }
    }

    // 雑音カットスライダー変更時
    private void OnNoiseFloorChanged(float value)
    {
        microphone.SetNoiseFloor(value);

        if (noiseFloorValueText != null)
        {
            noiseFloorValueText.text =
                microphone.NoiseFloor.ToString("F3");
        }
    }

    // 現在の設定をUIに表示
    private void RefreshSettingsUI()
    {
        // イベントを発生させず値を設定
        sensitivitySlider.SetValueWithoutNotify(
            microphone.Sensitivity
        );

        noiseFloorSlider.SetValueWithoutNotify(
            microphone.NoiseFloor
        );

        if (sensitivityValueText != null)
        {
            sensitivityValueText.text =
                microphone.Sensitivity.ToString("F1");
        }

        if (noiseFloorValueText != null)
        {
            noiseFloorValueText.text =
                microphone.NoiseFloor.ToString("F3");
        }
    }

    // 現在のマイク入力を表示
    private void UpdateMicrophoneLevel()
    {
        float volume = microphone.Volume01;

        // ゲージを更新
        micLevelSlider.SetValueWithoutNotify(volume);

        if (micLevelValueText != null)
        {
            micLevelValueText.text =
                $"{volume * 100f:F0}%";
        }

        if (guideText == null)
            return;

        // 入力状況に応じて説明を変更
        if (!microphone.IsReady)
        {
            guideText.text =
                "マイクの接続・入力を確認してください。";
        }
        else if (volume < 0.03f)
        {
            guideText.text =
                "入力は小さめです。声を出して確認してください。";
        }
        else if (volume < 0.2f)
        {
            guideText.text =
                "声量が小さめです。感度を調整してください。";
        }
        else
        {
            guideText.text =
                "声が検知されています！";
        }
    }

    private void OnDestroy()
    {
        // イベント登録を解除
        if (sensitivitySlider != null)
        {
            sensitivitySlider.onValueChanged.RemoveListener(
                OnSensitivityChanged
            );
        }

        if (noiseFloorSlider != null)
        {
            noiseFloorSlider.onValueChanged.RemoveListener(
                OnNoiseFloorChanged
            );
        }
    }
}
