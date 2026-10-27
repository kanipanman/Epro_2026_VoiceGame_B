
using UnityEngine;
using UnityEngine.UI;

// キラーに追跡されている間、画面の周囲を赤くする
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class ChaseVignetteUI : MonoBehaviour
{
    [Header("キラーの参照")]
    [SerializeField] private KillerController killer;

    [Header("ビネットの色")]
    [SerializeField] private Color vignetteColor =
        new Color(0.85f, 0.03f, 0.05f, 1f);

    [Header("赤色の強さ")]
    [SerializeField, Range(0f, 1f)]
    private float maxOpacity = 0.35f;

    [Header("赤くなる範囲")]
    [SerializeField, Range(0f, 1.5f)]
    private float innerRadius = 0.55f;

    [SerializeField, Range(0.1f, 2f)]
    private float outerRadius = 1.1f;

    [Header("フェード時間")]
    [SerializeField, Min(0.01f)]
    private float fadeInTime = 0.35f;

    [SerializeField, Min(0.01f)]
    private float fadeOutTime = 0.75f;

    [Header("追跡終了後の表示保持")]
    [SerializeField, Min(0f)]
    private float chaseHoldTime = 0.5f;

    [Header("動作確認用")]
    [SerializeField] private bool testPreview = false;

    private Image vignetteImage;
    private CanvasGroup canvasGroup;

    private Texture2D runtimeTexture;
    private Sprite runtimeSprite;

    private float lastChaseTime =
        float.NegativeInfinity;

    // =====================================
    // 初期化
    // =====================================

    private void Awake()
    {
        vignetteImage = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();

        // キラーが未設定なら自動で取得
        if (killer == null)
        {
            killer =
                FindFirstObjectByType<KillerController>();
        }

        // UI操作を妨げない
        vignetteImage.raycastTarget = false;
        vignetteImage.color = Color.white;
        vignetteImage.type = Image.Type.Simple;
        vignetteImage.preserveAspect = false;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0f;

        // 画面周囲だけ赤い画像を生成
        CreateVignetteSprite();
    }

    // =====================================
    // 毎フレームの表示更新
    // =====================================

    private void Update()
    {
        // ポーズ中やゲーム終了時は非表示へ
        bool paused = Time.timeScale <= 0f;

        bool isChasing = false;

        if (!paused)
        {
            // テスト中は強制表示できる
            isChasing = testPreview;

            // キラーが追跡状態なら表示
            if (killer != null &&
                killer.State ==
                KillerController.KillerState.Chase)
            {
                isChasing = true;
            }
        }

        if (isChasing)
        {
            lastChaseTime = Time.unscaledTime;
        }

        // 追跡終了直後は少しだけ表示を維持
        bool holding =
            !paused &&
            Time.unscaledTime - lastChaseTime <
            chaseHoldTime;

        float targetAlpha =
            (isChasing || holding)
                ? maxOpacity
                : 0f;

        // 表示・非表示の速度を分ける
        float fadeDuration =
            targetAlpha > canvasGroup.alpha
                ? fadeInTime
                : fadeOutTime;

        float speed =
            Mathf.Max(0.05f, maxOpacity) /
            Mathf.Max(0.01f, fadeDuration);

        // 滑らかに透明度を変更
        canvasGroup.alpha = Mathf.MoveTowards(
            canvasGroup.alpha,
            targetAlpha,
            speed * Time.unscaledDeltaTime
        );
    }

    // =====================================
    // 赤いビネット画像を自動生成
    // =====================================

    private void CreateVignetteSprite()
    {
        const int textureSize = 256;

        runtimeTexture = new Texture2D(
            textureSize,
            textureSize,
            TextureFormat.RGBA32,
            false
        );

        runtimeTexture.filterMode =
            FilterMode.Bilinear;

        runtimeTexture.wrapMode =
            TextureWrapMode.Clamp;

        Color32[] pixels =
            new Color32[textureSize * textureSize];

        float startRadius = Mathf.Max(
            0f,
            innerRadius
        );

        float endRadius = Mathf.Max(
            startRadius + 0.01f,
            outerRadius
        );

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                // 中央を(0,0)、四隅を外側にする
                float nx =
                    ((x + 0.5f) / textureSize - 0.5f)
                    * 2f;

                float ny =
                    ((y + 0.5f) / textureSize - 0.5f)
                    * 2f;

                float distance = Mathf.Sqrt(
                    nx * nx + ny * ny
                );

                // 中央を透明、外側を不透明にする
                float t = Mathf.InverseLerp(
                    startRadius,
                    endRadius,
                    distance
                );

                // 境界を自然にぼかす
                float opacity =
                    t * t * (3f - 2f * t);

                byte alpha = (byte)Mathf.RoundToInt(
                    Mathf.Clamp01(
                        opacity * vignetteColor.a
                    ) * 255f
                );

                pixels[y * textureSize + x] =
                    new Color32(
                        (byte)Mathf.RoundToInt(
                            vignetteColor.r * 255f
                        ),
                        (byte)Mathf.RoundToInt(
                            vignetteColor.g * 255f
                        ),
                        (byte)Mathf.RoundToInt(
                            vignetteColor.b * 255f
                        ),
                        alpha
                    );
            }
        }

        runtimeTexture.SetPixels32(pixels);
        runtimeTexture.Apply();

        runtimeSprite = Sprite.Create(
            runtimeTexture,
            new Rect(
                0,
                0,
                textureSize,
                textureSize
            ),
            new Vector2(0.5f, 0.5f),
            100f
        );

        vignetteImage.sprite = runtimeSprite;
    }

    // =====================================
    // 生成画像の後片付け
    // =====================================

    private void OnDestroy()
    {
        if (runtimeSprite != null)
        {
            Destroy(runtimeSprite);
        }

        if (runtimeTexture != null)
        {
            Destroy(runtimeTexture);
        }
    }
}
