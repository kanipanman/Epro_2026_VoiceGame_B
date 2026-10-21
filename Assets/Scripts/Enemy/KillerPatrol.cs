
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class KillerPatrol : MonoBehaviour
{
    [Header("巡回設定")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolSpeed = 2.5f;
    [SerializeField] private float arrivalDistance = 0.1f;

    private Rigidbody2D rb;
    private int currentPointIndex;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        Patrol();
    }

    // 指定された巡回地点へ移動する
    private void Patrol()
    {
        Transform target = patrolPoints[currentPointIndex];

        if (target == null)
        {
            NextPoint();
            return;
        }

        Vector2 targetPosition = target.position;

        // 巡回地点に到着したら次の地点へ進む
        if (Vector2.Distance(
            rb.position, targetPosition) <= arrivalDistance)
        {
            NextPoint();
            return;
        }

        // 現在位置から目標地点に向かって移動
        Vector2 nextPosition = Vector2.MoveTowards(
            rb.position,
            targetPosition,
            patrolSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(nextPosition);
    }

    // 次の巡回地点へ変更
    private void NextPoint()
    {
        currentPointIndex++;

        // 最後の地点に到着したら最初へ戻る
        if (currentPointIndex >= patrolPoints.Length)
        {
            currentPointIndex = 0;
        }
    }

    // シーンビューで巡回経路を表示する
    private void OnDrawGizmosSelected()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        Gizmos.color = Color.red;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null) continue;

            // 巡回地点を球で表示
            Gizmos.DrawWireSphere(
                patrolPoints[i].position, 0.2f
            );

            // 次の地点まで線を描画
            int next = (i + 1) % patrolPoints.Length;

            if (patrolPoints[next] != null)
            {
                Gizmos.DrawLine(
                    patrolPoints[i].position,
                    patrolPoints[next].position
                );
            }
        }
    }
}
