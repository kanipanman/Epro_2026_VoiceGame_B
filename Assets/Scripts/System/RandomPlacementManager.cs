
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-200)]
public class RandomPlacementManager : MonoBehaviour
{
    // =========================================
    // 配置するオブジェクト
    // =========================================

    [Header("配置するオブジェクト")]
    [SerializeField] private Transform[] generators;
    [SerializeField] private Rigidbody2D killer;

    [Header("プレイヤーと出口")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform exitDoor;

    // =========================================
    // 出現候補地点
    // =========================================

    [Header("出現候補地点")]
    [SerializeField] private Transform[] generatorSpawns;
    [SerializeField] private Transform[] killerSpawns;

    [Header("Phase 9.2 プレイヤーの出現地点")]
    [SerializeField] private Transform[] playerSpawns;

    [Header("Phase 9.3 出口の出現地点")]
    [SerializeField] private Transform[] exitSpawns;

    [Header("出口を塞ぐ壁（Exit Spawnsと同じ順番）")]
    [SerializeField] private GameObject[] exitBlockers;

    // =========================================
    // 壁との重なり判定
    // =========================================

    [Header("障害物判定")]
    [SerializeField] private LayerMask wallLayers;

    [SerializeField] private Vector2 generatorCheckSize =
        new Vector2(1.2f, 1.2f);

    [SerializeField] private Vector2 killerCheckSize =
        new Vector2(1f, 1f);

    [SerializeField] private Vector2 playerCheckSize =
        new Vector2(0.8f, 0.8f);

    [SerializeField] private Vector2 exitCheckSize =
        new Vector2(0.8f, 0.8f);

    // =========================================
    // 装置の配置条件
    // =========================================

    [Header("装置の配置条件")]
    [SerializeField, Min(0f)]
    private float minGeneratorSpacing = 2.5f;

    [SerializeField, Min(0f)]
    private float minGeneratorPlayerDistance = 2.5f;

    [SerializeField, Min(0f)]
    private float minGeneratorExitDistance = 2f;

    [SerializeField, Min(0f)]
    private float minGeneratorKillerDistance = 2.5f;

    // =========================================
    // キラーの配置条件
    // =========================================

    [Header("キラーの配置条件")]
    [SerializeField, Min(0f)]
    private float minKillerPlayerDistance = 6f;

    [SerializeField, Min(0f)]
    private float minKillerExitDistance = 2.5f;

    // =========================================
    // プレイヤーの配置条件
    // =========================================

    [Header("プレイヤーの配置条件")]
    [SerializeField, Min(0f)]
    private float minPlayerExitDistance = 3f;

    // 出口候補の穴付近にキャラや装置を置かない
    [Header("出口候補周辺の安全距離")]
    [SerializeField, Min(0f)]
    private float exitSpawnClearance = 1.5f;

    private Rigidbody2D playerRb;

    // =========================================
    // 初期化
    // =========================================

    private void Start()
    {
        if (!ValidateReferences())
            return;

        RandomizePositions();
    }

    // =========================================
    // 参照の確認
    // =========================================

    private bool ValidateReferences()
    {
        if (generators == null ||
            generators.Length != 3 ||
            killer == null ||
            player == null ||
            exitDoor == null)
        {
            Debug.LogError(
                "Phase9.3：配置対象の参照が不足しています。"
            );
            return false;
        }

        playerRb = player.GetComponent<Rigidbody2D>();

        if (playerRb == null)
        {
            Debug.LogError(
                "Phase9.3：PlayerにRigidbody2Dが必要です。"
            );
            return false;
        }

        if (generatorSpawns == null ||
            generatorSpawns.Length < generators.Length ||
            killerSpawns == null ||
            killerSpawns.Length == 0 ||
            playerSpawns == null ||
            playerSpawns.Length == 0 ||
            exitSpawns == null ||
            exitSpawns.Length == 0)
        {
            Debug.LogError(
                "Phase9.3：Spawn候補地点が不足しています。"
            );
            return false;
        }

        if (exitBlockers == null ||
            exitBlockers.Length != exitSpawns.Length)
        {
            Debug.LogError(
                "Phase9.3：Exit Spawnsと" +
                "Exit Blockersのサイズを揃えてください。"
            );
            return false;
        }

        if (wallLayers.value == 0)
        {
            Debug.LogError(
                "Phase9.3：Wall Layersが未設定です。"
            );
            return false;
        }

        foreach (Transform generator in generators)
        {
            if (generator == null)
            {
                Debug.LogError(
                    "Phase9.3：装置の参照に未設定があります。"
                );
                return false;
            }
        }

        if (!CheckSpawnArray(generatorSpawns, "Generator") ||
            !CheckSpawnArray(killerSpawns, "Killer") ||
            !CheckSpawnArray(playerSpawns, "Player") ||
            !CheckSpawnArray(exitSpawns, "Exit"))
        {
            return false;
        }

        HashSet<GameObject> usedBlockers =
            new HashSet<GameObject>();

        for (int i = 0; i < exitBlockers.Length; i++)
        {
            GameObject blocker = exitBlockers[i];

            if (blocker == null ||
                blocker == exitDoor.gameObject ||
                !usedBlockers.Add(blocker))
            {
                Debug.LogError(
                    $"Phase9.3：ExitBlockerの設定が不正です。位置:{i}"
                );
                return false;
            }
        }

        return true;
    }

    private bool CheckSpawnArray(
        Transform[] spawns,
        string category)
    {
        foreach (Transform spawn in spawns)
        {
            if (spawn == null)
            {
                Debug.LogError(
                    $"Phase9.3：{category}のSpawnが未設定です。"
                );
                return false;
            }
        }

        return true;
    }

    // =========================================
    // ランダム配置メイン処理
    // =========================================

    private void RandomizePositions()
    {
        // 失敗時に元の状態へ戻せるように記録
        bool[] originalBlockerStates =
            new bool[exitBlockers.Length];

        for (int i = 0; i < exitBlockers.Length; i++)
        {
            originalBlockerStates[i] =
                exitBlockers[i].activeSelf;

            // 配置判定中は全出口の壁を一時的に外す
            exitBlockers[i].SetActive(false);
        }

        Physics2D.SyncTransforms();

        // ① 出口候補をランダムに並べる
        List<int> exitCandidates = new List<int>();

        for (int i = 0; i < exitSpawns.Length; i++)
        {
            exitCandidates.Add(i);
        }

        Shuffle(exitCandidates);

        int validExitCount = 0;
        int validPlayerCount = 0;
        int validKillerCount = 0;

        foreach (int exitIndex in exitCandidates)
        {
            Transform exitSpawn = exitSpawns[exitIndex];

            Vector2 exitPosition = exitSpawn.position;

            // 出口候補が壁と重なっていないか
            if (IsBlocked(
                exitPosition,
                exitCheckSize))
            {
                continue;
            }

            validExitCount++;

            // ② プレイヤー候補をランダムに並べる
            List<Transform> playerCandidates =
                new List<Transform>(playerSpawns);

            Shuffle(playerCandidates);

            foreach (Transform playerSpawn
                in playerCandidates)
            {
                Vector2 playerPosition =
                    playerSpawn.position;

                if (!IsValidPlayerPosition(
                    playerPosition,
                    exitPosition))
                {
                    continue;
                }

                validPlayerCount++;

                // ③ キラー候補をランダムに並べる
                List<Transform> killerCandidates =
                    new List<Transform>(killerSpawns);

                Shuffle(killerCandidates);

                foreach (Transform killerSpawn
                    in killerCandidates)
                {
                    Vector2 killerPosition =
                        killerSpawn.position;

                    if (!IsValidKillerPosition(
                        killerPosition,
                        playerPosition,
                        exitPosition))
                    {
                        continue;
                    }

                    validKillerCount++;

                    // ④ 装置候補をランダムに並べる
                    List<Transform> generatorCandidates =
                        new List<Transform>(generatorSpawns);

                    Shuffle(generatorCandidates);

                    List<Vector2> selectedGenerators =
                        new List<Vector2>();

                    bool success = TrySelectGenerators(
                        generatorCandidates,
                        selectedGenerators,
                        playerPosition,
                        killerPosition,
                        exitPosition
                    );

                    if (!success)
                        continue;

                    // すべて配置可能な場合だけ適用
                    ApplyPositions(
                        exitIndex,
                        playerPosition,
                        killerPosition,
                        selectedGenerators
                    );

                    Debug.Log(
                        "Phase9.3：ランダム配置完了！\n" +
                        $"Exit：{exitSpawn.name}\n" +
                        $"Player：{playerSpawn.name}\n" +
                        $"Killer：{killerSpawn.name}\n" +
                        "Generators：3台"
                    );

                    return;
                }
            }
        }

        // 配置に失敗したらBlockerを元の状態へ戻す
        for (int i = 0; i < exitBlockers.Length; i++)
        {
            exitBlockers[i].SetActive(
                originalBlockerStates[i]
            );
        }

        Physics2D.SyncTransforms();

        Debug.LogError(
            "Phase9.3：ランダム配置に失敗しました。\n" +
            $"有効なExit候補：{validExitCount}\n" +
            $"有効なExit/Player組み合わせ：{validPlayerCount}\n" +
            $"有効なExit/Player/Killer組み合わせ：{validKillerCount}\n" +
            "出口の穴、Wall Layers、Spawnの距離を確認してください。"
        );
    }

    // =========================================
    // プレイヤーの配置判定
    // =========================================

    private bool IsValidPlayerPosition(
        Vector2 position,
        Vector2 exitPosition)
    {
        if (IsBlocked(position, playerCheckSize))
            return false;

        if (Vector2.Distance(
            position,
            exitPosition) < minPlayerExitDistance)
        {
            return false;
        }

        if (IsNearAnyExit(position))
            return false;

        return true;
    }

    // =========================================
    // キラーの配置判定
    // =========================================

    private bool IsValidKillerPosition(
        Vector2 position,
        Vector2 playerPosition,
        Vector2 exitPosition)
    {
        if (IsBlocked(position, killerCheckSize))
            return false;

        if (Vector2.Distance(
            position,
            playerPosition) < minKillerPlayerDistance)
        {
            return false;
        }

        if (Vector2.Distance(
            position,
            exitPosition) < minKillerExitDistance)
        {
            return false;
        }

        if (IsNearAnyExit(position))
            return false;

        return true;
    }

    // =========================================
    // 装置の配置判定
    // =========================================

    private bool TrySelectGenerators(
        List<Transform> candidates,
        List<Vector2> selected,
        Vector2 playerPosition,
        Vector2 killerPosition,
        Vector2 exitPosition)
    {
        if (selected.Count == generators.Length)
            return true;

        for (int i = 0; i < candidates.Count; i++)
        {
            Transform candidate = candidates[i];

            Vector2 position = candidate.position;

            if (!IsValidGeneratorPosition(
                position,
                selected,
                playerPosition,
                killerPosition,
                exitPosition))
            {
                continue;
            }

            selected.Add(position);
            candidates.RemoveAt(i);

            bool success = TrySelectGenerators(
                candidates,
                selected,
                playerPosition,
                killerPosition,
                exitPosition
            );

            candidates.Insert(i, candidate);

            if (success)
                return true;

            selected.RemoveAt(selected.Count - 1);
        }

        return false;
    }

    private bool IsValidGeneratorPosition(
        Vector2 position,
        List<Vector2> selected,
        Vector2 playerPosition,
        Vector2 killerPosition,
        Vector2 exitPosition)
    {
        if (IsBlocked(position, generatorCheckSize))
            return false;

        if (Vector2.Distance(
            position,
            playerPosition) < minGeneratorPlayerDistance)
        {
            return false;
        }

        if (Vector2.Distance(
            position,
            killerPosition) < minGeneratorKillerDistance)
        {
            return false;
        }

        if (Vector2.Distance(
            position,
            exitPosition) < minGeneratorExitDistance)
        {
            return false;
        }

        if (IsNearAnyExit(position))
            return false;

        foreach (Vector2 otherPosition in selected)
        {
            if (Vector2.Distance(
                position,
                otherPosition) < minGeneratorSpacing)
            {
                return false;
            }
        }

        return true;
    }

    // =========================================
    // 出口候補の周辺を避ける
    // =========================================

    private bool IsNearAnyExit(Vector2 position)
    {
        foreach (Transform spawn in exitSpawns)
        {
            if (Vector2.Distance(
                position,
                spawn.position) < exitSpawnClearance)
            {
                return true;
            }
        }

        return false;
    }

    // =========================================
    // 壁との重なり判定
    // =========================================

    private bool IsBlocked(
        Vector2 position,
        Vector2 size)
    {
        Collider2D hit = Physics2D.OverlapBox(
            position,
            size,
            0f,
            wallLayers
        );

        return hit != null;
    }

    // =========================================
    // 配置を実際に適用
    // =========================================

    private void ApplyPositions(
        int exitIndex,
        Vector2 playerPosition,
        Vector2 killerPosition,
        List<Vector2> generatorPositions)
    {
        Transform selectedExit = exitSpawns[exitIndex];

        // ① 既存のExitDoorを選ばれた出口へ移動
        Vector3 oldExitPosition = exitDoor.position;

        exitDoor.position = new Vector3(
            selectedExit.position.x,
            selectedExit.position.y,
            oldExitPosition.z
        );

        exitDoor.rotation = selectedExit.rotation;

        // ② プレイヤーを移動
        playerRb.position = playerPosition;

        // ③ キラーを移動
        killer.position = killerPosition;

        // ④ 装置3台を移動
        for (int i = 0; i < generators.Length; i++)
        {
            Vector3 oldPosition =
                generators[i].position;

            generators[i].position = new Vector3(
                generatorPositions[i].x,
                generatorPositions[i].y,
                oldPosition.z
            );
        }

        // ⑤ 選ばれなかった出口だけ壁で塞ぐ
        for (int i = 0; i < exitBlockers.Length; i++)
        {
            bool shouldBlock = i != exitIndex;

            exitBlockers[i].SetActive(shouldBlock);
        }

        // 物理エンジンに位置変更を反映
        Physics2D.SyncTransforms();
    }

    // =========================================
    // 配列のシャッフル
    // =========================================

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    // =========================================
    // Sceneビューに候補地点を表示
    // =========================================

    private void OnDrawGizmosSelected()
    {
        DrawSpawnGizmos(
            playerSpawns,
            playerCheckSize,
            Color.cyan
        );

        DrawSpawnGizmos(
            killerSpawns,
            killerCheckSize,
            Color.red
        );

        DrawSpawnGizmos(
            generatorSpawns,
            generatorCheckSize,
            Color.yellow
        );

        DrawSpawnGizmos(
            exitSpawns,
            exitCheckSize,
            Color.green
        );
    }

    private void DrawSpawnGizmos(
        Transform[] spawns,
        Vector2 size,
        Color color)
    {
        if (spawns == null)
            return;

        Gizmos.color = color;

        foreach (Transform spawn in spawns)
        {
            if (spawn == null)
                continue;

            Gizmos.DrawWireCube(
                spawn.position,
                new Vector3(
                    size.x,
                    size.y,
                    0f
                )
            );
        }
    }
}
