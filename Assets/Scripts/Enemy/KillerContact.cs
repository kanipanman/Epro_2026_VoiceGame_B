
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class KillerContact : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private GameManager gameManager;

    // プレイヤーと接触したらゲームオーバー
    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckPlayerContact(collision);
    }

    // 接触した状態でゲームが始まった場合にも対応
    private void OnCollisionStay2D(Collision2D collision)
    {
        CheckPlayerContact(collision);
    }

    private void CheckPlayerContact(Collision2D collision)
    {
        if (gameManager == null || gameManager.HasEnded)
            return;

        PlayerController player =
            collision.gameObject
                     .GetComponentInParent<PlayerController>();

        if (player == null)
            return;

        Debug.Log("キラー：プレイヤーを捕獲！");

        gameManager.GameOver();
    }
}
