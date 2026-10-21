
using UnityEngine;

public class KillerVision : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private Transform player;

    [Header("視界設定")]
    [SerializeField, Min(0.1f)]
    private float viewDistance = 6f;

    [SerializeField, Range(1f, 360f)]
    private float viewAngle = 110f;

    [Header("視界を遮る壁")]
    [SerializeField] private LayerMask wallLayers;

    // プレイヤーが視界に入っているか判定
    public bool CanSeePlayer(
        Vector2 origin,
        Vector2 facingDirection)
    {
        if (player == null || wallLayers.value == 0)
            return false;

        Vector2 target = player.position;
        Vector2 direction = target - origin;

        // 距離による判定
        float distance = direction.magnitude;

        if (distance > viewDistance)
            return false;

        if (distance < 0.01f)
            return true;

        // キラーが向いている方向との角度を計算
        if (facingDirection.sqrMagnitude < 0.001f)
            facingDirection = Vector2.down;

        float angle = Vector2.Angle(
            facingDirection,
            direction
        );

        // 視野角の外側なら見えない
        if (angle > viewAngle * 0.5f)
            return false;

        // 壁によって視界が遮られているか確認
        RaycastHit2D hit = Physics2D.Linecast(
            origin,
            target,
            wallLayers
        );

        // 壁に当たらなければプレイヤーが見える
        return hit.collider == null;
    }

    // シーンビューで視界を可視化
    private void OnDrawGizmosSelected()
    {
        KillerController controller =
            GetComponent<KillerController>();

        Vector2 facing = controller != null
            ? controller.FacingDirection
            : Vector2.down;

        Vector3 origin = transform.position;

        Vector3 leftDirection =
            Quaternion.Euler(0f, 0f, -viewAngle / 2f)
            * new Vector3(facing.x, facing.y, 0f);

        Vector3 rightDirection =
            Quaternion.Euler(0f, 0f, viewAngle / 2f)
            * new Vector3(facing.x, facing.y, 0f);

        Gizmos.color = Color.yellow;

        // 視認可能な最大距離
        Gizmos.DrawWireSphere(origin, viewDistance);

        // 視野角の境界線
        Gizmos.DrawLine(
            origin,
            origin + leftDirection * viewDistance
        );

        Gizmos.DrawLine(
            origin,
            origin + rightDirection * viewDistance
        );
    }

    private void Start()
    {
        if (player == null)
            Debug.LogError($"{name}: Playerが未設定です。");

        if (wallLayers.value == 0)
            Debug.LogError($"{name}: Wallレイヤーが未設定です。");
    }
}
