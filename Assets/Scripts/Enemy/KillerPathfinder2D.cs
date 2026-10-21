
using System.Collections.Generic;
using UnityEngine;

public class KillerPathfinder2D : MonoBehaviour
{
    [Header("経路探索範囲（ワールド座標）")]
    [SerializeField] private Vector2 mapMin =
        new Vector2(-13.5f, -9.5f);

    [SerializeField] private Vector2 mapMax =
        new Vector2(13.5f, 9.5f);

    [Header("グリッド設定")]
    [SerializeField, Min(0.25f)]
    private float cellSize = 0.5f;

    [SerializeField] private Vector2 agentSize =
        new Vector2(0.95f, 0.95f);

    [Header("壁のレイヤー")]
    [SerializeField] private LayerMask wallLayers;

    private static readonly Vector2Int[] directions =
    {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left
    };

    // 出発地点から目的地までの経路を求める
    public List<Vector2> FindPath(Vector2 start, Vector2 goal)
    {
        List<Vector2> result = new List<Vector2>();

        if (wallLayers.value == 0)
            return result;

        int width = Mathf.CeilToInt(
            (mapMax.x - mapMin.x) / cellSize
        );

        int height = Mathf.CeilToInt(
            (mapMax.y - mapMin.y) / cellSize
        );

        if (width <= 0 || height <= 0 ||
            width * height > 20000)
        {
            return result;
        }

        int total = width * height;

        // 各マスが通れるかの判定を一時保存
        byte[] walkableCache = new byte[total];

        Vector2Int startCell = WorldToCell(start, width, height);
        Vector2Int goalCell = WorldToCell(goal, width, height);

        // 壁と重なった座標は近くの通行可能なマスに補正
        int startIndex = FindNearestWalkable(
            startCell, width, height, walkableCache
        );

        int goalIndex = FindNearestWalkable(
            goalCell, width, height, walkableCache
        );

        if (startIndex < 0 || goalIndex < 0)
            return result;

        // BFSで最短経路を探索
        int[] previous = new int[total];

        for (int i = 0; i < total; i++)
            previous[i] = -1;

        Queue<int> queue = new Queue<int>();
        queue.Enqueue(startIndex);
        previous[startIndex] = startIndex;

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();

            if (current == goalIndex)
                break;

            int x = current % width;
            int y = current / width;

            foreach (Vector2Int direction in directions)
            {
                int nx = x + direction.x;
                int ny = y + direction.y;

                if (nx < 0 || nx >= width ||
                    ny < 0 || ny >= height)
                    continue;

                int next = ny * width + nx;

                // 訪問済み、または壁のマスを除外
                if (previous[next] != -1)
                    continue;

                if (!IsWalkable(
                    next, width, walkableCache))
                    continue;

                previous[next] = current;
                queue.Enqueue(next);
            }
        }

        // 目的地に到達できなかった
        if (previous[goalIndex] == -1)
            return result;

        // 探索結果を出発地からの順番へ変換
        int node = goalIndex;

        while (node != startIndex)
        {
            result.Add(CellToWorld(node, width));
            node = previous[node];
        }

        result.Reverse();

        // 目的地自体が通行可能なら最後に追加
        if (Physics2D.OverlapBox(
            goal, agentSize, 0f, wallLayers) == null)
        {
            result.Add(goal);
        }

        return result;
    }

    // ワールド座標をグリッド座標へ変換
    private Vector2Int WorldToCell(
        Vector2 position, int width, int height)
    {
        int x = Mathf.FloorToInt(
            (position.x - mapMin.x) / cellSize
        );

        int y = Mathf.FloorToInt(
            (position.y - mapMin.y) / cellSize
        );

        return new Vector2Int(
            Mathf.Clamp(x, 0, width - 1),
            Mathf.Clamp(y, 0, height - 1)
        );
    }

    // グリッドのマス番号をワールド座標へ変換
    private Vector2 CellToWorld(int index, int width)
    {
        int x = index % width;
        int y = index / width;

        return mapMin + new Vector2(
            (x + 0.5f) * cellSize,
            (y + 0.5f) * cellSize
        );
    }

    // 近くの通行可能なマスを探す
    private int FindNearestWalkable(
        Vector2Int cell,
        int width,
        int height,
        byte[] cache)
    {
        for (int radius = 0; radius <= 6; radius++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    int nx = cell.x + x;
                    int ny = cell.y + y;

                    if (nx < 0 || nx >= width ||
                        ny < 0 || ny >= height)
                        continue;

                    int index = ny * width + nx;

                    if (IsWalkable(index, width, cache))
                        return index;
                }
            }
        }

        return -1;
    }

    // 壁と重ならないマスか判定
    private bool IsWalkable(
        int index, int width, byte[] cache)
    {
        if (cache[index] != 0)
            return cache[index] == 1;

        Vector2 position = CellToWorld(index, width);

        bool clear = Physics2D.OverlapBox(
            position,
            agentSize,
            0f,
            wallLayers
        ) == null;

        cache[index] = (byte)(clear ? 1 : 2);

        return clear;
    }
}
