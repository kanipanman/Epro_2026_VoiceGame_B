
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GeneratorManager : MonoBehaviour
{
    [Header("UI参照")]
    [SerializeField] private TMP_Text countText;

    // 管理対象の装置
    private readonly List<VoiceGenerator> generators =
        new List<VoiceGenerator>();

    // 起動済み装置数
    public int ActivatedCount { get; private set; }

    // 装置の総数
    public int TotalCount => generators.Count;

    // 全装置の起動が完了したか
    public bool AllActivated =>
        TotalCount > 0 && ActivatedCount == TotalCount;

    // 全装置の起動完了を通知
    public event Action OnAllActivated;

    private bool completionNotified;

    private void Awake()
    {
        // シーン内の装置を検索して登録
        VoiceGenerator[] foundGenerators =
            FindObjectsByType<VoiceGenerator>(
                FindObjectsSortMode.None
            );

        foreach (VoiceGenerator generator in foundGenerators)
        {
            RegisterGenerator(generator);
        }

        UpdateCountUI();
    }

    // 装置を管理対象に追加
    public void RegisterGenerator(VoiceGenerator generator)
    {
        if (generator == null || generators.Contains(generator))
            return;

        generators.Add(generator);

        // 装置の起動イベントを購読
        generator.Activated += HandleGeneratorActivated;

        if (generator.IsActivated)
        {
            ActivatedCount++;
        }

        UpdateCountUI();
    }

    // 装置起動時の処理
    private void HandleGeneratorActivated(VoiceGenerator generator)
    {
        if (!generators.Contains(generator))
            return;

        ActivatedCount++;

        UpdateCountUI();

        Debug.Log(
            $"装置の起動状況: {ActivatedCount}/{TotalCount}"
        );

        // 全装置の起動完了を通知
        if (AllActivated && !completionNotified)
        {
            completionNotified = true;

            Debug.Log("すべての装置が起動しました！");
            OnAllActivated?.Invoke();
        }
    }

    // 装置の起動数を画面に表示
    private void UpdateCountUI()
    {
        if (countText == null) return;

        countText.text =
            $"装置 {ActivatedCount} / {TotalCount}";
    }

    private void OnDestroy()
    {
        // イベントの購読を解除
        foreach (VoiceGenerator generator in generators)
        {
            if (generator != null)
            {
                generator.Activated -= HandleGeneratorActivated;
            }
        }
    }
}
