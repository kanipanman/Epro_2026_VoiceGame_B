
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

    // ゲームが終了しているか
    public bool HasEnded { get; private set; }

    private void Awake()
    {
        // ゲーム開始時は時間を通常速度にする
        Time.timeScale = 1f;

        HasEnded = false;

        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // 終了後、Rキーでリスタート
        if (HasEnded &&
            Keyboard.current != null &&
            Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartStage();
        }
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
        // 二重に終了処理しない
        if (HasEnded)
            return;

        HasEnded = true;

        // プレイヤーの操作を停止
        if (player != null)
        {
            player.SetMovementEnabled(false);
        }

        // キラーのAIと聴覚処理を停止
        if (killer != null)
        {
            killer.StopAI();
        }

        if (killerHearing != null)
        {
            killerHearing.enabled = false;
        }

        // 結果を画面に表示
        if (resultText != null)
        {
            resultText.text =
                message + "\nRキーでリスタート";

            resultText.gameObject.SetActive(true);
        }

        Debug.Log(message);

        // ゲーム全体の時間を停止
        Time.timeScale = 0f;
    }

    // 現在のステージを最初からやり直す
    public void RestartStage()
    {
        Time.timeScale = 1f;

        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(currentScene.name);
    }

    private void OnDestroy()
    {
        // 停止状態が次のシーンへ残るのを防ぐ
        Time.timeScale = 1f;
    }
}
