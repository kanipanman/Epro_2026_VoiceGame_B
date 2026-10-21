
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(KillerPathfinder2D))]
[RequireComponent(typeof(KillerVision))]
public class KillerController : MonoBehaviour
{
    // キラーの行動状態
    public enum KillerState
    {
        Patrol,       // 巡回
        Investigate,  // 調査
        Chase         // 追跡
    }

    [Header("参照")]
    [SerializeField] private Transform player;

    [Header("巡回設定")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolSpeed = 2.5f;

    [Header("調査設定")]
    [SerializeField] private float investigateSpeed = 2.8f;
    [SerializeField] private float searchDuration = 2f;
    [SerializeField] private float investigationTimeout = 12f;

    [Header("追跡設定")]
    [SerializeField] private float chaseSpeed = 4.2f;
    [SerializeField] private float lostSightDelay = 0.6f;
    [SerializeField] private float chaseRepathInterval = 0.4f;

    [Header("移動判定")]
    [SerializeField] private float arrivalDistance = 0.2f;

    private Rigidbody2D rb;
    private KillerPathfinder2D pathfinder;
    private KillerVision vision;

    // 現在の行動状態
    public KillerState State { get; private set; }
        = KillerState.Patrol;

    // キラーの向いている方向
    public Vector2 FacingDirection { get; private set; }
        = Vector2.down;

    private int patrolIndex;

    // 最後に聞いた・見た位置
    private Vector2 lastKnownPosition;

    private bool reachedInvestigationPoint;
    private float searchTimer;
    private float investigationTimer;
    private float lostSightTimer;

    // 経路探索
    private List<Vector2> currentPath = new List<Vector2>();
    private int pathIndex;
    private bool needsNewPath = true;
    private float nextPathRetryTime;
    private float nextChaseRepathTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        pathfinder = GetComponent<KillerPathfinder2D>();
        vision = GetComponent<KillerVision>();
    }

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError($"{name}: Playerが未設定です。");
            enabled = false;
        }
    }

    private void FixedUpdate()
    {
        // プレイヤーが視界に入っているか確認
        bool canSeePlayer = vision.CanSeePlayer(
            rb.position,
            FacingDirection
        );

        if (canSeePlayer)
        {
            // 視認している間は最後の目撃位置を更新
            lastKnownPosition = player.position;
            lostSightTimer = 0f;

            if (State != KillerState.Chase)
            {
                StartChase();
            }
        }
        else if (State == KillerState.Chase)
        {
            // 見失った時間を計測
            lostSightTimer += Time.fixedDeltaTime;

            if (lostSightTimer >= lostSightDelay)
            {
                Debug.Log(
                    "キラー：プレイヤーを見失った"
                );

                BeginInvestigation(lastKnownPosition);
            }
        }

        // 現在の状態に応じた処理
        switch (State)
        {
            case KillerState.Patrol:
                UpdatePatrol();
                break;

            case KillerState.Investigate:
                UpdateInvestigation();
                break;

            case KillerState.Chase:
                UpdateChase();
                break;
        }
    }

    // 通常時の巡回処理
    private void UpdatePatrol()
    {
        if (patrolPoints == null ||
            patrolPoints.Length == 0)
            return;

        Transform target = patrolPoints[patrolIndex];

        if (target == null)
        {
            NextPatrolPoint();
            return;
        }

        if (MoveAlongPath(target.position, patrolSpeed))
        {
            NextPatrolPoint();
        }
    }

    // 次の巡回地点に変更
    private void NextPatrolPoint()
    {
        patrolIndex =
            (patrolIndex + 1) % patrolPoints.Length;

        ResetPath();
    }

    // プレイヤーを発見したときの処理
    private void StartChase()
    {
        State = KillerState.Chase;

        ResetPath();
        nextChaseRepathTime = 0f;

        Debug.Log("キラー：プレイヤーを発見！追跡開始");
    }

    // 追跡中の処理
    private void UpdateChase()
    {
        // プレイヤーの移動に合わせて経路を更新
        if (Time.time >= nextChaseRepathTime)
        {
            ResetPath();

            nextChaseRepathTime = Time.time +
                Mathf.Max(0.1f, chaseRepathInterval);
        }

        // 最後に目撃した位置へ移動
        MoveAlongPath(
            lastKnownPosition,
            chaseSpeed
        );
    }

    // 声を聞いたときの処理
    // KillerHearingから呼び出される
    public void HearNoise(Vector2 noisePosition)
    {
        // 追跡中は視覚情報を優先
        if (State == KillerState.Chase)
            return;

        // 同じ場所を調査中なら経路は変更しない
        if (State == KillerState.Investigate &&
            Vector2.Distance(
                lastKnownPosition,
                noisePosition
            ) < 0.5f)
        {
            if (reachedInvestigationPoint)
            {
                searchTimer = searchDuration;
            }

            return;
        }

        Debug.Log("キラー：声を検知！");

        BeginInvestigation(noisePosition);
    }

    // 調査状態に切り替える
    private void BeginInvestigation(Vector2 position)
    {
        State = KillerState.Investigate;

        lastKnownPosition = position;
        reachedInvestigationPoint = false;

        searchTimer = searchDuration;
        investigationTimer = investigationTimeout;

        ResetPath();

        Debug.Log("キラー：調査開始");
    }

    // 最後に聞いた・見た場所へ向かう
    private void UpdateInvestigation()
    {
        // 到達できない場所で止まり続けるのを防止
        investigationTimer -= Time.fixedDeltaTime;

        if (investigationTimer <= 0f)
        {
            ReturnToPatrol();
            return;
        }

        if (!reachedInvestigationPoint)
        {
            bool arrived = MoveAlongPath(
                lastKnownPosition,
                investigateSpeed
            );

            if (arrived)
            {
                reachedInvestigationPoint = true;
                searchTimer = searchDuration;

                Debug.Log("キラー：調査地点に到着");
            }

            return;
        }

        // 到着した場所で一定時間待つ
        searchTimer -= Time.fixedDeltaTime;

        if (searchTimer <= 0f)
        {
            ReturnToPatrol();
        }
    }

    // 調査終了後、巡回に戻る
    private void ReturnToPatrol()
    {
        State = KillerState.Patrol;

        // 現在位置に最も近い巡回地点を探す
        float nearestDistance = float.MaxValue;

        if (patrolPoints != null)
        {
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] == null)
                    continue;

                float distance = Vector2.Distance(
                    rb.position,
                    patrolPoints[i].position
                );

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    patrolIndex = i;
                }
            }
        }

        ResetPath();

        Debug.Log("キラー：巡回再開");
    }

    // 経路探索を利用した移動
    private bool MoveAlongPath(Vector2 target, float speed)
    {
        // すでに目的地にいる場合
        if (Vector2.Distance(
            rb.position, target) <= arrivalDistance)
        {
            return true;
        }

        // 必要に応じて経路を計算する
        if (needsNewPath &&
            Time.time >= nextPathRetryTime)
        {
            currentPath = pathfinder.FindPath(
                rb.position, target
            );

            pathIndex = 0;
            needsNewPath = false;

            // 経路が見つからなかった場合
            if (currentPath.Count == 0)
            {
                nextPathRetryTime = Time.time + 1f;
                needsNewPath = true;
            }
        }

        if (needsNewPath)
            return false;

        // 到達済みの経由地点を飛ばす
        while (pathIndex < currentPath.Count &&
               Vector2.Distance(
                   rb.position,
                   currentPath[pathIndex]
               ) <= arrivalDistance)
        {
            pathIndex++;
        }

        // 全経由地点を通過した場合
        if (pathIndex >= currentPath.Count)
        {
            if (Vector2.Distance(
                rb.position, target) <= 0.5f)
            {
                return true;
            }

            ResetPath();
            return false;
        }

        // 次の経由地点へ移動
        Vector2 nextPosition = Vector2.MoveTowards(
            rb.position,
            currentPath[pathIndex],
            speed * Time.fixedDeltaTime
        );

        // 移動方向を視線方向にも反映する
        Vector2 direction = nextPosition - rb.position;

        if (direction.sqrMagnitude > 0.0001f)
        {
            FacingDirection = direction.normalized;
        }

        rb.MovePosition(nextPosition);

        return false;
    }

    // 移動経路を初期化
    private void ResetPath()
    {
        currentPath.Clear();
        pathIndex = 0;
        needsNewPath = true;
        nextPathRetryTime = 0f;
    }

    // ゲーム終了時にAIを停止
    public void StopAI()
    {
        rb.linearVelocity = Vector2.zero;
        enabled = false;
    }

    // シーンビューに経路と調査地点を表示
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;

        Gizmos.color = State == KillerState.Chase
            ? Color.red
            : Color.cyan;

        Vector3 previous = transform.position;

        for (int i = pathIndex; i < currentPath.Count; i++)
        {
            Vector3 point = currentPath[i];

            Gizmos.DrawLine(previous, point);
            Gizmos.DrawWireSphere(point, 0.12f);

            previous = point;
        }

        if (State == KillerState.Investigate)
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawWireSphere(
                lastKnownPosition, 0.35f
            );
        }
    }
}
