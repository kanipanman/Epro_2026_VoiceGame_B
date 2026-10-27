
using UnityEngine;

// ゲーム本体の処理を変えずに見た目を制御
[RequireComponent(typeof(SpriteRenderer))]
public class ObjectSpriteVisual2D : MonoBehaviour
{
    public enum VisualType
    {
        Generator,
        Locker,
        Exit
    }

    [Header("見た目の種類")]
    [SerializeField] private VisualType visualType;

    [Header("画像")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite changedSprite;

    [Header("発電機")]
    [SerializeField] private VoiceGenerator generator;

    [Header("ロッカー")]
    [SerializeField] private LockerHiding locker;
    [SerializeField] private PlayerHidingState playerHiding;

    [Header("出口")]
    [SerializeField] private VoiceGenerator[] generators;

    [Header("表示色")]
    [SerializeField] private Color normalColor =
        Color.white;

    [SerializeField] private Color completedColor =
        new Color(0.7f, 1f, 0.75f, 1f);

    [SerializeField] private Color occupiedColor =
        new Color(0.7f, 0.9f, 1f, 1f);

    [SerializeField] private Color cooldownColor =
        new Color(1f, 1f, 1f, 0.45f);

    private SpriteRenderer renderer2D;

    private void Awake()
    {
        renderer2D = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        switch (visualType)
        {
            case VisualType.Generator:
                if (generator != null)
                {
                    bool done = generator.IsActivated;

                    SetVisual(
                        done,
                        done ? completedColor : normalColor
                    );
                }
                break;

            case VisualType.Locker:
                if (locker == null || playerHiding == null)
                    return;

                if (locker.IsOccupied)
                {
                    SetVisual(true, occupiedColor);
                }
                else if (!playerHiding.CanUseLocker)
                {
                    SetVisual(false, cooldownColor);
                }
                else
                {
                    SetVisual(false, normalColor);
                }
                break;

            case VisualType.Exit:
                if (generators == null ||
                    generators.Length == 0)
                    return;

                bool allDone = true;

                foreach (VoiceGenerator item in generators)
                {
                    if (item == null || !item.IsActivated)
                    {
                        allDone = false;
                        break;
                    }
                }

                SetVisual(allDone, normalColor);
                break;
        }
    }

    private void SetVisual(bool changed, Color color)
    {
        Sprite selected =
            changed && changedSprite != null
                ? changedSprite
                : normalSprite;

        if (selected != null &&
            renderer2D.sprite != selected)
        {
            renderer2D.sprite = selected;
        }

        if (renderer2D.color != color)
        {
            renderer2D.color = color;
        }
    }
}
