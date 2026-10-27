
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CharacterSpriteAnimator2D : MonoBehaviour
{
    // 操作キャラか、キラーか
    public enum CharacterType
    {
        Player,
        Killer
    }

    // キャラクターの向き
    private enum Direction
    {
        Down,
        Up,
        Left,
        Right
    }

    [Header("キャラクター設定")]
    [SerializeField] private CharacterType characterType =
        CharacterType.Player;

    [SerializeField] private Transform characterRoot;

    [SerializeField] private KillerController killerController;

    [Header("待機画像")]
    [SerializeField] private Sprite idleDown;
    [SerializeField] private Sprite idleUp;
    [SerializeField] private Sprite idleLeft;
    [SerializeField] private Sprite idleRight;

    [Header("下向きの歩行画像")]
    [SerializeField] private Sprite[] walkDown;

    [Header("上向きの歩行画像")]
    [SerializeField] private Sprite[] walkUp;

    [Header("左向きの歩行画像")]
    [SerializeField] private Sprite[] walkLeft;

    [Header("右向きの歩行画像")]
    [SerializeField] private Sprite[] walkRight;

    [Header("アニメーション速度")]
    [SerializeField, Min(1f)]
    private float walkFramesPerSecond = 7f;

    [SerializeField, Min(1f)]
    private float fastFramesPerSecond = 11f;

    [Header("移動速度判定")]
    [SerializeField, Min(0f)]
    private float movementThreshold = 0.25f;

    // Playerの通常移動3.5、ダッシュ5.0を想定
    [SerializeField, Min(0f)]
    private float sprintSpeedThreshold = 4.0f;

    // コンポーネント参照
    private SpriteRenderer spriteRenderer;

    // 現在向いている方向
    private Direction currentDirection = Direction.Down;

    // 前フレームの位置
    private Vector2 previousPosition;

    // 移動速度の平滑化
    private float smoothedSpeed;

    // アニメーションの状態
    private bool wasMoving;
    private float frameTimer;
    private int frameIndex;

    // =====================================
    // 初期化
    // =====================================

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 親オブジェクトを移動判定の基準にする
        if (characterRoot == null)
        {
            characterRoot = transform.parent != null
                ? transform.parent
                : transform;
        }

        // キラーの場合はAIの参照を取得
        if (characterType == CharacterType.Killer &&
            killerController == null)
        {
            killerController =
                GetComponentInParent<KillerController>();
        }
    }

    private void Start()
    {
        // ランダムスポーン後の位置を基準にする
        previousPosition = characterRoot.position;

        smoothedSpeed = 0f;
        wasMoving = false;
        frameTimer = 0f;
        frameIndex = 0;

        // 初期状態では下向きの待機画像を表示
        SetSprite(GetIdleSprite());
    }

    // =====================================
    // 毎フレームの表示更新
    // =====================================

    private void LateUpdate()
    {
        if (characterRoot == null)
            return;

        // ポーズ中やゲーム終了時はアニメーション停止
        if (Time.timeScale <= 0f)
            return;

        float deltaTime = Time.deltaTime;

        if (deltaTime <= 0f)
            return;

        // 現在位置から移動量を計算
        Vector2 currentPosition = characterRoot.position;

        Vector2 movement =
            currentPosition - previousPosition;

        previousPosition = currentPosition;

        // 実際の移動速度を計算
        float currentSpeed =
            movement.magnitude / deltaTime;

        // 物理更新の間隔による表示のばらつきを抑える
        float smoothFactor = 1f -
            Mathf.Exp(-15f * deltaTime);

        smoothedSpeed = Mathf.Lerp(
            smoothedSpeed,
            currentSpeed,
            smoothFactor
        );

        // キラーはAIの向きを優先する
        Vector2 facing = movement;

        if (characterType == CharacterType.Killer &&
            killerController != null)
        {
            facing = killerController.FacingDirection;
        }

        // 向きを更新
        UpdateDirection(facing);

        // 移動中か確認
        bool isMoving =
            smoothedSpeed >= movementThreshold;

        if (!isMoving)
        {
            // 停止中は待機画像
            ShowIdle();
            return;
        }

        // 歩行アニメーションを再生
        PlayWalkAnimation(deltaTime);
    }

    // =====================================
    // 向きの更新
    // =====================================

    private void UpdateDirection(Vector2 facing)
    {
        if (facing.sqrMagnitude < 0.0001f)
            return;

        Direction nextDirection;

        // 左右と上下の大きい成分を優先
        if (Mathf.Abs(facing.x) >
            Mathf.Abs(facing.y))
        {
            nextDirection = facing.x >= 0f
                ? Direction.Right
                : Direction.Left;
        }
        else
        {
            nextDirection = facing.y >= 0f
                ? Direction.Up
                : Direction.Down;
        }

        // 向きが変わったらアニメーションを最初から
        if (currentDirection != nextDirection)
        {
            currentDirection = nextDirection;

            frameIndex = 0;
            frameTimer = 0f;
        }
    }

    // =====================================
    // 待機アニメーション
    // =====================================

    private void ShowIdle()
    {
        wasMoving = false;

        frameTimer = 0f;
        frameIndex = 0;

        SetSprite(GetIdleSprite());
    }

    // 向きに対応する待機画像を取得
    private Sprite GetIdleSprite()
    {
        Sprite idle = null;

        switch (currentDirection)
        {
            case Direction.Down:
                idle = idleDown;
                break;

            case Direction.Up:
                idle = idleUp;
                break;

            case Direction.Left:
                idle = idleLeft;
                break;

            case Direction.Right:
                idle = idleRight;
                break;
        }

        if (idle != null)
            return idle;

        // 待機画像がなければ歩行の最初の画像を使用
        Sprite[] frames = GetWalkSprites();

        if (frames != null &&
            frames.Length > 0)
        {
            return frames[0];
        }

        return null;
    }

    // =====================================
    // 歩行アニメーション
    // =====================================

    private void PlayWalkAnimation(float deltaTime)
    {
        Sprite[] frames = GetWalkSprites();

        // 画像がない場合は待機画像のまま
        if (frames == null || frames.Length == 0)
        {
            SetSprite(GetIdleSprite());
            return;
        }

        // 移動開始時は最初のコマから再生
        if (!wasMoving)
        {
            wasMoving = true;

            frameIndex = 0;
            frameTimer = 0f;
        }

        // ダッシュ・追跡中なら再生速度を上げる
        bool isFast = IsMovingFast();

        float fps = isFast
            ? fastFramesPerSecond
            : walkFramesPerSecond;

        fps = Mathf.Max(1f, fps);

        float frameInterval = 1f / fps;

        frameTimer += deltaTime;

        // 経過時間に応じてコマを進める
        while (frameTimer >= frameInterval)
        {
            frameTimer -= frameInterval;

            frameIndex =
                (frameIndex + 1) % frames.Length;
        }

        SetSprite(frames[frameIndex]);
    }

    // =====================================
    // ダッシュ・追跡判定
    // =====================================

    private bool IsMovingFast()
    {
        // キラーの場合
        if (characterType == CharacterType.Killer)
        {
            return killerController != null &&
                killerController.State ==
                KillerController.KillerState.Chase;
        }

        // プレイヤーの場合
        return smoothedSpeed >= sprintSpeedThreshold;
    }

    // =====================================
    // 歩行画像の取得
    // =====================================

    private Sprite[] GetWalkSprites()
    {
        switch (currentDirection)
        {
            case Direction.Down:
                return walkDown;

            case Direction.Up:
                return walkUp;

            case Direction.Left:
                return walkLeft;

            case Direction.Right:
                return walkRight;
        }

        return walkDown;
    }

    // =====================================
    // スプライト表示の変更
    // =====================================

    private void SetSprite(Sprite sprite)
    {
        if (spriteRenderer == null || sprite == null)
            return;

        spriteRenderer.sprite = sprite;
    }
}
