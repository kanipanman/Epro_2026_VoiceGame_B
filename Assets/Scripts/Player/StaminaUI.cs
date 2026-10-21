
using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private PlayerController player;
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Image fillImage;

    [Header("ゲージの色")]
    [SerializeField] private Color normalColor = Color.green;
    [SerializeField] private Color lowColor = Color.red;

    private void Start()
    {
        if (staminaSlider != null)
        {
            staminaSlider.minValue = 0f;
            staminaSlider.maxValue = 1f;
        }
    }

    private void Update()
    {
        if (player == null || staminaSlider == null)
            return;

        // 現在のスタミナ割合を表示
        float ratio = player.StaminaRatio;
        staminaSlider.value = ratio;

        // スタミナが少なくなると赤く表示
        if (fillImage != null)
        {
            fillImage.color =
                player.IsExhausted || ratio <= 0.25f
                    ? lowColor
                    : normalColor;
        }
    }
}
