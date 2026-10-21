
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
    [SerializeField, Min(1f)] private float timeLimit = 180f;
    [SerializeField] private TMP_Text timerText;

    // ゲームが終了しているか
    public bool HasEnded { get; private set; }

    // 現在の残り時間
    public float RemainingTime { get; private set; }

    private void Awake()
    {
        Time.timeScale = 1f;

        HasEnded = false;
        RemainingTime = timeLimit;

        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }

        UpdateTimerUI();
    }

    private void Update()
    {
        // ゲーム終了後はリスタート入力のみ受け付ける
        if (HasEnded)
        {
            if (Keyboard.current != null &&
                Keyboard.current.rKey.wasPressedThisFrame)
            {
                RestartStage();
            }

            return;
        }

        // 残り時間を減らす
        RemainingTime -= Time.deltaTime;
        RemainingTime = Mathf.Max(0f, RemainingTime);

        UpdateTimerUI();

        // 制限時間を超えたらゲームオーバー
        if (RemainingTime <= 0f)
        {
            EndGame("時間切れ！");
        }
    }

    // 残り時間を分:秒で表示
    private void UpdateTimerUI()
    {
        if (timerText == null) return;

        int seconds = Mathf.CeilToInt(RemainingTime);

        int minutes = seconds / 60;
        int remainingSeconds = seconds % 60;

        timerText.text =
            $"{minutes:00}:{remainingSeconds:00}";

        // 残り30秒以下で赤色に変更
        timerText.color =
            RemainingTime <= 30f
                ? Color.red
                : Color.white;
    }

    // キラーに捕まった場合
    public void GameOver()
    {
        EndGame("ゲームオーバー");
    }

    // 出口から脱出した場合
    public void GameClear()
    {
        EndGame("脱出成功！");
    }

    // ゲーム終了時の共通処理
    private void EndGame(string message)
    {
        if (HasEnded) return;

        HasEnded = true;

        // プレイヤーを操作できないようにする
        if (player != null)
        {
            player.SetMovementEnabled(false);
        }

        // キラーのAIを停止
        if (killer != null)
        {
            killer.StopAI();
        }

        // キラーの聴覚判定を停止
        if (killerHearing != null)
        {
            killerHearing.enabled = false;
        }

        // 結果を表示
        if (resultText != null)
        {
            resultText.text =
                message + "\nRキーでリスタート";

            resultText.gameObject.SetActive(true);
        }

        Debug.Log(message);

        // ゲーム内時間を停止
        Time.timeScale = 0f;
    }

    // ステージを最初からやり直す
    public void RestartStage()
    {
        Time.timeScale = 1f;

        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(currentScene.name);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
