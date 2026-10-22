
using UnityEngine;

public class MicrophoneInput : MonoBehaviour
{
    [Header("マイク設定")]
    [SerializeField] private int sampleRate = 44100;

    [SerializeField, Range(1f, 50f)]
    private float sensitivity = 15f;

    [Header("雑音対策")]
    [SerializeField, Range(0f, 0.1f)]
    private float noiseFloor = 0.01f;

    // PlayerPrefsで使用する保存キー
    private const string SensitivityKey = "MicSensitivity";
    private const string NoiseFloorKey = "MicNoiseFloor";

    // 現在のマイク音量（0～1）
    public float Volume01 { get; private set; }

    // マイクの準備が完了しているか
    public bool IsReady { get; private set; }

    // 設定画面などから現在の設定を取得
    public float Sensitivity => sensitivity;
    public float NoiseFloor => noiseFloor;

    private AudioClip microphoneClip;
    private readonly float[] samples = new float[1024];

    private void Awake()
    {
        // 保存済み設定を読み込む
        LoadSettings();
    }

    private void Start()
    {
        // マイクが接続されているか確認
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("マイクが見つかりません。");
            return;
        }

        // デフォルトマイクで録音開始
        microphoneClip = Microphone.Start(
            null,
            true,
            2,
            sampleRate
        );

        if (microphoneClip == null)
        {
            Debug.LogError("マイクの起動に失敗しました。");
            return;
        }

        Debug.Log("マイクの録音を開始しました。");
    }

    private void Update()
    {
        if (microphoneClip == null ||
            !Microphone.IsRecording(null))
        {
            Volume01 = 0f;
            IsReady = false;
            return;
        }

        // 現在の録音位置を取得
        int position = Microphone.GetPosition(null);

        // 必要なサンプル数が集まるまで待機
        if (!IsReady)
        {
            if (position < samples.Length)
                return;

            IsReady = true;
        }

        int offset = position - samples.Length;

        if (offset < 0)
            offset += microphoneClip.samples;

        if (!microphoneClip.GetData(samples, offset))
        {
            Volume01 = 0f;
            return;
        }

        // RMSを用いて音量を計算
        float sum = 0f;

        foreach (float sample in samples)
        {
            sum += sample * sample;
        }

        float rms = Mathf.Sqrt(
            sum / samples.Length
        );

        // 小さな環境音を除去
        float filteredRms = Mathf.Max(
            0f,
            rms - noiseFloor
        );

        // マイク感度を反映
        float volume = Mathf.Clamp01(
            filteredRms * sensitivity
        );

        // ポーズ中も音量表示を更新できるようにする
        Volume01 = Mathf.Lerp(
            Volume01,
            volume,
            Mathf.Clamp01(
                Time.unscaledDeltaTime * 20f
            )
        );
    }

    // マイク感度を変更
    public void SetSensitivity(float value)
    {
        sensitivity = Mathf.Clamp(value, 1f, 50f);
    }

    // 雑音カットの強さを変更
    public void SetNoiseFloor(float value)
    {
        noiseFloor = Mathf.Clamp(value, 0f, 0.1f);
    }

    // 現在の設定を保存
    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(
            SensitivityKey,
            sensitivity
        );

        PlayerPrefs.SetFloat(
            NoiseFloorKey,
            noiseFloor
        );

        PlayerPrefs.Save();

        Debug.Log("マイク設定を保存しました。");
    }

    // 保存された設定を読み込む
    public void LoadSettings()
    {
        sensitivity = Mathf.Clamp(
            PlayerPrefs.GetFloat(
                SensitivityKey,
                sensitivity
            ),
            1f,
            50f
        );

        noiseFloor = Mathf.Clamp(
            PlayerPrefs.GetFloat(
                NoiseFloorKey,
                noiseFloor
            ),
            0f,
            0.1f
        );
    }

    // マイク録音を終了
    private void OnDisable()
    {
        if (Microphone.IsRecording(null))
        {
            Microphone.End(null);
        }

        IsReady = false;
        Volume01 = 0f;
    }
}
