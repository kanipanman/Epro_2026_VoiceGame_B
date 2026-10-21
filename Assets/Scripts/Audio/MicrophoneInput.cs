
using UnityEngine;

public class MicrophoneInput : MonoBehaviour
{
    [Header("マイク設定")]
    [SerializeField] private int sampleRate = 44100;
    [SerializeField, Range(1f, 50f)]
    private float sensitivity = 15f;

    // ゲーム内で利用する音量（0～1）
    public float Volume01 { get; private set; }

    // マイクから音声データを取得できているか
    public bool IsReady { get; private set; }

    private AudioClip microphoneClip;
    private readonly float[] samples = new float[1024];

    private void Start()
    {
        // 利用できるマイクがあるか確認
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("マイクが見つかりません。");
            return;
        }

        // OSのデフォルトマイクでループ録音を開始
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

        // 初回は十分なサンプルが録音されるまで待つ
        if (!IsReady)
        {
            if (position < samples.Length)
                return;

            IsReady = true;
        }

        // 最新の音声データを取得
        int offset = position - samples.Length;

        if (offset < 0)
            offset += microphoneClip.samples;

        if (!microphoneClip.GetData(samples, offset))
        {
            Volume01 = 0f;
            return;
        }

        // RMS（二乗平均平方根）から音量を計算
        float sum = 0f;

        foreach (float sample in samples)
        {
            sum += sample * sample;
        }

        float rms = Mathf.Sqrt(sum / samples.Length);

        // 感度を反映して0～1の範囲に変換
        float volume = Mathf.Clamp01(rms * sensitivity);

        // 音量の急激な変化を少し滑らかにする
        Volume01 = Mathf.Lerp(
            Volume01,
            volume,
            Mathf.Clamp01(Time.deltaTime * 20f)
        );
    }

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
