
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private PlayerController player;
    [SerializeField] private KillerController killer;
    [SerializeField] private KillerHearing killerHearing;

    [Header("結果表示")]
    [SerializeField] private TMP_Text resultText;

    [Header("制限時間")]
    [SerializeField, Min(1f)]
    private float timeLimit = 180f;

    [SerializeField] private TMP_Text timerText;

    // ゲームの終了状態
    public bool HasEnded { get; private set; }

    // ゲームの一時停止状態
    public bool IsPaused { get; private set; }

    // 現在の残り時間
    public float RemainingTime { get; private set; }

    private void Awake()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        HasEnded = false;
        IsPaused = false;

        RemainingTime = timeLimit;

        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }

        UpdateTimerUI();
    }

    private void Update()
    {
        // 終了後はRキーでリスタート
        if (HasEnded)
        {
            if (Keyboard.current != null &&
                Keyboard.current.rKey.wasPressedThisFrame)
            {
                RestartStage();
            }

            return;
        }

        // ポーズ中はタイマーを更新しない
        if (IsPaused)
            return;

        // 制限時間を減らす
        RemainingTime -= Time.deltaTime;
        RemainingTime = Mathf.Max(
            0f,
            RemainingTime
        );

        UpdateTimerUI();

        if (RemainingTime <= 0f)
        {
            EndGame("時間切れ！");
        }
    }

    // タイマーの表示
    private void UpdateTimerUI()
    {
        if (timerText == null)
            return;

        int seconds = Mathf.CeilToInt(RemainingTime);

        int minutes = seconds / 60;
        int remainingSeconds = seconds % 60;

        timerText.text =
            $"{minutes:00}:{remainingSeconds:00}";

        timerText.color =
            RemainingTime <= 30f
                ? Color.red
                : Color.white;
    }

    // ゲームを一時停止
    public void PauseGame()
    {
        if (HasEnded || IsPaused)
            return;

        IsPaused = true;

        // ゲーム内の時間を停止
        Time.timeScale = 0f;

        // BGM・心音なども一時停止
        AudioListener.pause = true;

        Debug.Log("ゲームを一時停止しました。");
    }

    // 一時停止を解除
    public void ResumeGame()
    {
        if (HasEnded || !IsPaused)
            return;

        IsPaused = false;

        Time.timeScale = 1f;
        AudioListener.pause = false;

        Debug.Log("ゲームを再開しました。");
    }

    // キラーに捕まった場合
    public void GameOver()
    {
        EndGame("ゲームオーバー");
    }

    // 脱出した場合
    public void GameClear()
    {
        EndGame("脱出成功！");
    }

    // ゲーム終了時の共通処理
    private void EndGame(string message)
    {
        if (HasEnded || IsPaused)
            return;

        HasEnded = true;

        // プレイヤーの移動禁止
        if (player != null)
        {
            player.SetMovementEnabled(false);
        }

        // キラーAIを停止
        if (killer != null)
        {
            killer.StopAI();
        }

        if (killerHearing != null)
        {
            killerHearing.enabled = false;
        }

        // 結果表示
        if (resultText != null)
        {
            resultText.text =
                message + "\nRキーでリスタート";

            resultText.gameObject.SetActive(true);
        }

        Debug.Log(message);

        Time.timeScale = 0f;
    }

    // ステージをリスタート
    public void RestartStage()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(currentScene.name);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }
}
