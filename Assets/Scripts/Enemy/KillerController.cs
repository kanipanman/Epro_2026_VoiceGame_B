
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
        Patrol,
        Investigate,
        Chase
    }

    [Header("参照")]
    [SerializeField] private Transform player;

    [Header("通常巡回")]
    [SerializeField] private Transform[] patrolPoints;

    [SerializeField] private float patrolSpeed = 2f;

    [Header("音の調査")]
    [SerializeField] private float investigateSpeed = 2.7f;
    [SerializeField] private float searchDuration = 2f;
    [SerializeField] private float investigationTimeout = 8f;

    [Header("プレイヤー追跡")]
    [SerializeField] private float chaseSpeed = 4.2f;
    [SerializeField] private float lostSightDelay = 0.6f;
    [SerializeField] private float chaseRepathInterval = 0.4f;

    [Header("移動判定")]
    [SerializeField] private float arrivalDistance = 0.45f;

    [Header("Phase 9.1 ランダム巡回")]
    [SerializeField] private VoiceGenerator[] patrolGenerators;

    [SerializeField, Range(0f, 1f)]
    private float generatorVisitChance = 0.35f;

    [Header("巡回地点での待機")]
    [SerializeField] private float minPatrolWait = 0.5f;
    [SerializeField] private float maxPatrolWait = 1.5f;

    [SerializeField] private float patrolArrivalDistance = 0.65f;

    [Header("移動できない場合の対策")]
    [SerializeField] private float stuckCheckInterval = 2f;
    [SerializeField] private float stuckMinMoveDistance = 0.15f;

    [Header("デバッグ")]
    [SerializeField] private bool showPatrolLog = false;

    // 外部スクリプトから取得する現在の状態
    public KillerState State { get; private set; }
        = KillerState.Patrol;

    // キラーが現在向いている方向
    public Vector2 FacingDirection { get; private set; }
        = Vector2.down;

    // コンポーネント参照
    private Rigidbody2D rb;
    private KillerPathfinder2D pathfinder;
    private KillerVision vision;

    // AIの停止状態
    private bool aiStopped;

    // =========================================
    // 経路探索
    // =========================================

    private List<Vector2> currentPath =
        new List<Vector2>();

    private int pathIndex;
    private bool needsNewPath = true;
    private float nextPathRetryTime;

    // =========================================
    // ランダム巡回
    // =========================================

    private Transform currentPatrolTarget;
    private Transform previousPatrolTarget;

    private bool lastTargetWasGenerator;

    private bool isPatrolWaiting;
    private float patrolWaitEndTime;
    private float patrolLookTime;
    private bool hasLookedAround;

    // 移動停止の検出
    private float lastMovementCheckTime;
    private Vector2 lastMovementCheckPosition;

    // =========================================
    // 調査・追跡
    // =========================================

    private Vector2 investigatePosition;
    private float investigationStartTime;
    private float searchStartTime = -1f;

    private Vector2 lastSeenPosition;
    private float lastSeenTime;
    private float nextChaseRepathTime;

    // =========================================
    // 初期化
    // =========================================

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        pathfinder = GetComponent<KillerPathfinder2D>();
        vision = GetComponent<KillerVision>();

        State = KillerState.Patrol;
        aiStopped = false;
    }

    private void Start()
    {
        // Phase 9のランダム配置が完了した位置を基準にする
        lastMovementCheckPosition = rb.position;
        lastMovementCheckTime = Time.time;
    }

    // =========================================
    // メインAI処理
    // =========================================

    private void FixedUpdate()
    {
        // ゲーム終了・ポーズ中は更新しない
        if (aiStopped || Time.timeScale <= 0f)
            return;

        // プレイヤーを視認できるか判定
        bool canSeePlayer = false;

        if (player != null && vision != null)
        {
            canSeePlayer = vision.CanSeePlayer(
                rb.position,
                FacingDirection
            );
        }

        // プレイヤーを発見した場合
        if (canSeePlayer)
        {
            lastSeenPosition = player.position;
            lastSeenTime = Time.time;

            if (State != KillerState.Chase)
            {
                StartChase();
            }
        }

        // 現在の状態に応じて行動
        switch (State)
        {
            case KillerState.Patrol:
                UpdatePatrol();
                break;

            case KillerState.Investigate:
                UpdateInvestigate();
                break;

            case KillerState.Chase:
                UpdateChase(canSeePlayer);
                break;
        }
    }

    // =========================================
    // Phase 9.1 ランダム巡回
    // =========================================

    private void UpdatePatrol()
    {
        if (patrolPoints == null ||
            patrolPoints.Length == 0)
        {
            return;
        }

        // 巡回地点で待機中
        if (isPatrolWaiting)
        {
            // 待機中に一度だけ向きを変更
            if (!hasLookedAround &&
                Time.time >= patrolLookTime)
            {
                LookAround();
                hasLookedAround = true;
            }

            // 待機時間が終わるまで停止
            if (Time.time < patrolWaitEndTime)
                return;

            isPatrolWaiting = false;

            // 新しい巡回先を決定
            NextPatrolPoint();
        }

        // 最初の巡回先を選ぶ
        if (currentPatrolTarget == null)
        {
            NextPatrolPoint();
        }

        if (currentPatrolTarget == null)
            return;

        Vector2 targetPosition =
            currentPatrolTarget.position;

        float distance = Vector2.Distance(
            rb.position,
            targetPosition
        );

        // 巡回地点に到着
        if (distance <= patrolArrivalDistance)
        {
            StartPatrolWaiting();
            return;
        }

        // 移動できているか確認
        if (Time.time - lastMovementCheckTime >=
            stuckCheckInterval)
        {
            float movedDistance = Vector2.Distance(
                rb.position,
                lastMovementCheckPosition
            );

            // ほとんど移動していない
            if (movedDistance < stuckMinMoveDistance)
            {
                if (showPatrolLog)
                {
                    Debug.Log(
                        "キラー：移動できないため巡回先を変更"
                    );
                }

                NextPatrolPoint();
                return;
            }

            lastMovementCheckPosition = rb.position;
            lastMovementCheckTime = Time.time;
        }

        // 経路探索で目的地へ移動
        MoveAlongPath(
            targetPosition,
            patrolSpeed
        );
    }

    // 次の巡回先をランダムに決定
    private void NextPatrolPoint()
    {
        List<Transform> candidates =
            new List<Transform>();

        bool visitingGenerator = false;

        // 一定確率で未起動の装置を確認
        if (!lastTargetWasGenerator &&
            Random.value < generatorVisitChance &&
            patrolGenerators != null)
        {
            foreach (VoiceGenerator generator
                in patrolGenerators)
            {
                if (generator == null ||
                    generator.IsActivated)
                {
                    continue;
                }

                Transform target = generator.transform;

                // 同じ場所を連続で選ばない
                if (target == previousPatrolTarget)
                    continue;

                // 近すぎる場所は選ばない
                if (Vector2.Distance(
                    rb.position,
                    target.position) < 1f)
                {
                    continue;
                }

                candidates.Add(target);
            }

            visitingGenerator = candidates.Count > 0;
        }

        // 装置を選ばなかった場合は通常巡回
        if (candidates.Count == 0)
        {
            foreach (Transform point in patrolPoints)
            {
                if (point == null)
                    continue;

                if (point == previousPatrolTarget)
                    continue;

                if (Vector2.Distance(
                    rb.position,
                    point.position) < 1f)
                {
                    continue;
                }

                candidates.Add(point);
            }
        }

        // 候補がない場合は距離条件を解除
        if (candidates.Count == 0)
        {
            foreach (Transform point in patrolPoints)
            {
                if (point != null &&
                    point != previousPatrolTarget)
                {
                    candidates.Add(point);
                }
            }
        }

        // 最後の候補
        if (candidates.Count == 0)
        {
            foreach (Transform point in patrolPoints)
            {
                if (point != null)
                    candidates.Add(point);
            }
        }

        if (candidates.Count == 0)
        {
            currentPatrolTarget = null;
            return;
        }

        // ランダム選択
        int randomIndex = Random.Range(
            0,
            candidates.Count
        );

        currentPatrolTarget = candidates[randomIndex];

        // 同じ目的地を連続選択しないために記録
        previousPatrolTarget = currentPatrolTarget;

        lastTargetWasGenerator = visitingGenerator;

        // 移動停止検出を初期化
        ResetMovementCheck();

        // 目的地を変更したため経路を再計算
        ResetPath();

        if (showPatrolLog)
        {
            Debug.Log(
                $"キラーの次の巡回先：{currentPatrolTarget.name}"
            );
        }
    }

    // 巡回地点で待機を開始
    private void StartPatrolWaiting()
    {
        float minWait = Mathf.Min(
            minPatrolWait,
            maxPatrolWait
        );

        float maxWait = Mathf.Max(
            minPatrolWait,
            maxPatrolWait
        );

        float waitTime = Random.Range(
            minWait,
            maxWait
        );

        isPatrolWaiting = true;

        patrolWaitEndTime =
            Time.time + waitTime;

        patrolLookTime =
            Time.time + waitTime * 0.5f;

        hasLookedAround = false;

        ResetPath();

        if (showPatrolLog)
        {
            Debug.Log(
                $"キラー：巡回地点に到着。" +
                $"{waitTime:F1}秒待機"
            );
        }
    }

    // 巡回地点でランダムな方向を見る
    private void LookAround()
    {
        int direction = Random.Range(0, 4);

        switch (direction)
        {
            case 0:
                FacingDirection = Vector2.up;
                break;

            case 1:
                FacingDirection = Vector2.down;
                break;

            case 2:
                FacingDirection = Vector2.left;
                break;

            case 3:
                FacingDirection = Vector2.right;
                break;
        }
    }

    // 移動チェックを初期化
    private void ResetMovementCheck()
    {
        lastMovementCheckPosition = rb.position;
        lastMovementCheckTime = Time.time;
    }

    // =========================================
    // 声による調査
    // =========================================

    // KillerHearingなどから呼び出される
    public void HearNoise(Vector2 noisePosition)
    {
        if (aiStopped || Time.timeScale <= 0f)
            return;

        // 追跡中は声による調査に切り替えない
        if (State == KillerState.Chase)
            return;

        investigatePosition = noisePosition;

        // 巡回から調査へ切り替える
        if (State != KillerState.Investigate)
        {
            State = KillerState.Investigate;

            investigationStartTime = Time.time;
        }

        // 新しい音の位置へ向かう
        searchStartTime = -1f;

        isPatrolWaiting = false;

        ResetPath();
    }

    // 調査中の行動
    private void UpdateInvestigate()
    {
        // 調査が長引きすぎた場合
        if (Time.time - investigationStartTime >=
            investigationTimeout)
        {
            ReturnToPatrol();
            return;
        }

        float distance = Vector2.Distance(
            rb.position,
            investigatePosition
        );

        // 音の発生地点に到着
        if (distance <= arrivalDistance)
        {
            if (searchStartTime < 0f)
            {
                searchStartTime = Time.time;
            }

            // 周囲を調査する
            if (Time.time - searchStartTime >=
                searchDuration)
            {
                ReturnToPatrol();
            }

            return;
        }

        // 音がした場所へ移動
        MoveAlongPath(
            investigatePosition,
            investigateSpeed
        );
    }

    // =========================================
    // プレイヤー追跡
    // =========================================

    // プレイヤーを発見
    private void StartChase()
    {
        State = KillerState.Chase;

        isPatrolWaiting = false;

        nextChaseRepathTime = 0f;

        ResetPath();

        if (showPatrolLog)
        {
            Debug.Log("キラー：プレイヤーを発見！");
        }
    }

    // 追跡中の行動
    private void UpdateChase(bool canSeePlayer)
    {
        if (player == null)
        {
            ReturnToPatrol();
            return;
        }

        // 一定時間見失ったら最後の位置を調査
        if (!canSeePlayer &&
            Time.time - lastSeenTime >= lostSightDelay)
        {
            StartInvestigatingLastSeen();
            return;
        }

        // 視認中は現在地、見失った直後は最終位置
        Vector2 targetPosition = canSeePlayer
            ? (Vector2)player.position
            : lastSeenPosition;

        // プレイヤーの移動に合わせて経路を再計算
        if (Time.time >= nextChaseRepathTime)
        {
            ResetPath();

            nextChaseRepathTime =
                Time.time + Mathf.Max(
                    0.1f,
                    chaseRepathInterval
                );
        }

        MoveAlongPath(
            targetPosition,
            chaseSpeed
        );
    }

    // 最後に見かけた場所を調査
    private void StartInvestigatingLastSeen()
    {
        State = KillerState.Investigate;

        investigatePosition = lastSeenPosition;
        investigationStartTime = Time.time;
        searchStartTime = -1f;

        ResetPath();

        if (showPatrolLog)
        {
            Debug.Log(
                "キラー：プレイヤーを見失ったため調査へ移行"
            );
        }
    }

    // =========================================
    // 巡回へ復帰
    // =========================================

    private void ReturnToPatrol()
    {
        State = KillerState.Patrol;

        isPatrolWaiting = false;
        searchStartTime = -1f;

        ResetPath();
        ResetMovementCheck();

        // 最も近い巡回地点を探す
        Transform nearestPoint = null;
        float nearestDistance = float.MaxValue;

        if (patrolPoints != null)
        {
            foreach (Transform point in patrolPoints)
            {
                if (point == null)
                    continue;

                float distance = Vector2.Distance(
                    rb.position,
                    point.position
                );

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestPoint = point;
                }
            }
        }

        currentPatrolTarget = nearestPoint;

        previousPatrolTarget = nearestPoint;
        lastTargetWasGenerator = false;

        if (showPatrolLog)
        {
            Debug.Log(
                "キラー：調査・追跡終了。巡回へ復帰"
            );
        }
    }

    // =========================================
    // 経路探索による移動
    // =========================================

    private void MoveAlongPath(
        Vector2 targetPosition,
        float moveSpeed)
    {
        if (pathfinder == null)
            return;

        // 経路が必要な場合は計算
        if (needsNewPath &&
            Time.time >= nextPathRetryTime)
        {
            List<Vector2> result =
                pathfinder.FindPath(
                    rb.position,
                    targetPosition
                );

            currentPath = result ??
                new List<Vector2>();

            pathIndex = 0;
            needsNewPath = false;

            // 経路が見つからない場合は再試行
            if (currentPath.Count == 0)
            {
                needsNewPath = true;
                nextPathRetryTime = Time.time + 0.5f;
                return;
            }
        }

        if (currentPath.Count == 0)
            return;

        // すでに通過した経路点をスキップ
        while (pathIndex < currentPath.Count)
        {
            float distance = Vector2.Distance(
                rb.position,
                currentPath[pathIndex]
            );

            if (distance > 0.15f)
                break;

            pathIndex++;
        }

        // 最後の経路点まで進んだ場合
        if (pathIndex >= currentPath.Count)
        {
            // 目的地がまだ少し離れていれば近づく
            if (Vector2.Distance(
                rb.position,
                targetPosition) > arrivalDistance)
            {
                needsNewPath = true;
            }

            return;
        }

        Vector2 nextPosition = currentPath[pathIndex];

        Vector2 direction =
            nextPosition - rb.position;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        // 移動方向を更新
        FacingDirection = direction.normalized;

        // Rigidbody2Dで移動
        Vector2 newPosition = Vector2.MoveTowards(
            rb.position,
            nextPosition,
            moveSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(newPosition);
    }

    // 経路情報を初期化
    private void ResetPath()
    {
        currentPath.Clear();

        pathIndex = 0;
        needsNewPath = true;
        nextPathRetryTime = 0f;
    }

    // =========================================
    // AI停止
    // =========================================

    // GameManagerから呼び出される
    public void StopAI()
    {
        aiStopped = true;
        ResetPath();
    }
}
