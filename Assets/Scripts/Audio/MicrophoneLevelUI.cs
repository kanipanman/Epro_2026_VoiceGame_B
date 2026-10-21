
using UnityEngine;
using UnityEngine.UI;

public class MicrophoneLevelUI : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private MicrophoneInput microphone;
    [SerializeField] private Slider volumeSlider;

    private void Update()
    {
        if (microphone == null || volumeSlider == null)
            return;

        // マイク音量をUIに反映
        volumeSlider.value = microphone.Volume01;
    }
}
