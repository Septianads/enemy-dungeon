using System.Collections.Generic;
using UnityEngine;
// <summary>
// Enemy AI untuk dungeon:
// 1. Deteksi player (Range/Distance Detection)
// 2. Cek jangkauan (detection radius & attack range)
// 3. Cari jalur menuju player (A* Pathfinding pada grid)
// 4. Bergerak menuju player mengikuti jalur (Seek Movement)
// </summary>
public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public DungeonGrid grid;              // grid dungeon (kumpulan node walkable/blocked)

    [Header("Detection Settings")]
    public float detectionRadius = 8f;    // jarak enemy bisa "melihat" player
    public float attackRange = 1.2f;      // jarak enemy bisa menyerang langsung

    [Header("Movement Settings")]
    public float moveSpeed = 3.5f;
    public float waypointTolerance = 0.15f;

    private List<Node> currentPath = new List<Node>();
    private int pathIndex = 0;

    void Update()
    {
        // ---------- 1 & 2. DETEKSI PLAYER + CEK JANGKAUAN ----------
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > detectionRadius)
        {
            // Player di luar jangkauan -> idle / patrol
            Idle();
            return;
        }

        if (distanceToPlayer <= attackRange)
        {
            // Sudah dekat -> serang, tidak perlu pathfinding
            AttackPlayer();
            return;
        }

        // ---------- 3. PATHFINDING (A*) ----------
        // Cari path baru hanya jika belum ada path atau target bergerak jauh dari path lama
        if (currentPath.Count == 0 || pathIndex >= currentPath.Count)
        {
            currentPath = FindPathAStar(grid.WorldToNode(transform.position),
                                         grid.WorldToNode(player.position));
            pathIndex = 0;
        }

        // ---------- 4. MOVEMENT (SEEK sepanjang path) ----------
        MoveAlongPath();
    }

    void Idle()
    {
        // Enemy diam / bisa diisi logika patroli sederhana
    }

    void AttackPlayer()
    {
        // Logika serangan enemy ke player
        Debug.Log($"{name} menyerang player!");
    }

    // A* PATHFINDING
    List<Node> FindPathAStar(Node start, Node goal)
    {
        var openSet = new List<Node> { start };
        var closedSet = new HashSet<Node>();

        var gScore = new Dictionary<Node, float> { [start] = 0 };
        var fScore = new Dictionary<Node, float> { [start] = Heuristic(start, goal) };
        var cameFrom = new Dictionary<Node, Node>();

        while (openSet.Count > 0)
        {
            // Ambil node dengan fScore terkecil
            Node current = GetLowestFScore(openSet, fScore);

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (Node neighbor in grid.GetNeighbors(current))
            {
                if (!neighbor.isWalkable || closedSet.Contains(neighbor))
                    continue; // lewati tembok / node yang sudah diproses

                float tentativeG = gScore[current] + Vector2Int.Distance(
                    current.gridPosition, neighbor.gridPosition);

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, goal);

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return new List<Node>(); // path tidak ditemukan
    }

    float Heuristic(Node a, Node b)
    {
        // Manhattan distance, cocok untuk grid 4-arah
        return Mathf.Abs(a.gridPosition.x - b.gridPosition.x) +
               Mathf.Abs(a.gridPosition.y - b.gridPosition.y);
    }

    Node GetLowestFScore(List<Node> openSet, Dictionary<Node, float> fScore)
    {
        Node best = openSet[0];
        foreach (var node in openSet)
        {
            if (fScore.TryGetValue(node, out float f) && f < fScore[best])
                best = node;
        }
        return best;
    }

    List<Node> ReconstructPath(Dictionary<Node, Node> cameFrom, Node current)
    {
        var path = new List<Node> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        return path;
    }

    // MOVEMENT (Seek sepanjang waypoint hasil A*)
    void MoveAlongPath()
    {
        if (currentPath.Count == 0 || pathIndex >= currentPath.Count) return;

        Vector3 targetWorldPos = grid.NodeToWorld(currentPath[pathIndex]);
        Vector3 direction = (targetWorldPos - transform.position).normalized;

        transform.position += direction * moveSpeed * Time.deltaTime;

        if (Vector3.Distance(transform.position, targetWorldPos) < waypointTolerance)
        {
            pathIndex++; // lanjut ke waypoint berikutnya
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

// <summary>Representasi satu sel/node grid dungeon.</summary>
public class Node
{
    public Vector2Int gridPosition;
    public bool isWalkable;
}

// <summary>
// Contoh sederhana kelas grid dungeon.
// Implementasi nyata bisa memakai Tilemap Unity untuk membangun grid ini.
// </summary>
public class DungeonGrid : MonoBehaviour
{
    public float cellSize = 1f;
    private Node[,] nodes;

    public Node WorldToNode(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / cellSize);
        int y = Mathf.RoundToInt(worldPos.y / cellSize);
        return nodes[x, y];
    }

    public Vector3 NodeToWorld(Node node)
    {
        return new Vector3(node.gridPosition.x * cellSize, node.gridPosition.y * cellSize, 0);
    }

    public List<Node> GetNeighbors(Node node)
    {
        var result = new List<Node>();
        int x = node.gridPosition.x, y = node.gridPosition.y;

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var d in dirs)
        {
            int nx = x + d.x, ny = y + d.y;
            if (nx >= 0 && ny >= 0 && nx < nodes.GetLength(0) && ny < nodes.GetLength(1))
                result.Add(nodes[nx, ny]);
        }
        return result;
    }
}
