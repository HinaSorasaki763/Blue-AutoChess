using GameEnum;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class SpawnGrid : MonoBehaviour
{
    const int gridSize = 8;
    const float hexWidth = 1;
    const float hexHeight = 0.866f;
    const float oddRowOffset = 0.5f;
    public static SpawnGrid Instance;
    public GameEvent ResourcesInitialized;
    public Dictionary<string, HexNode> hexNodes = new Dictionary<string, HexNode>();
    public Dictionary<int, string> indexToCubeKey = new Dictionary<int, string>();
    private Dictionary<HexNode, CharacterCTRL> preparationPositions = new Dictionary<HexNode, CharacterCTRL>();
    public HashSet<string> createdAllyWalls = new HashSet<string>();
    public HashSet<string> createdEnemyWalls = new HashSet<string>();
    public List<GameObject> activeWalls = new List<GameObject>();
    public CharacterParent allyParent;
    void OnEnable()
    {
        Instance = this;
        foreach (var tile in hexNodes.Values)
        {
            if (tile.TryGetComponent<HexNode>(out HexNode H) && H.IsBattlefield && !H.IsHexReserved())
            {
                tile.head.GetComponent<Renderer>().material.color = Color.white;
            }
        }
    }
    public void SavePreparationPositions()
    {
        preparationPositions.Clear();
        foreach (var node in hexNodes.Values)
        {
            if (node.OccupyingCharacter != null && node.OccupyingCharacter.IsAlly)
            {
                preparationPositions[node] = node.OccupyingCharacter;
            }
        }
    }
    public void RestorePreparationPositions()
    {
        foreach (var item in preparationPositions.Keys)
        {
            item.OccupyingCharacter = null;
        }
        foreach (var pair in preparationPositions)
        {
            GameController.Instance.TryMoveCharacter(pair.Value, pair.Key);
            pair.Value.transform.position = pair.Key.transform.position;
            pair.Value.gameObject.SetActive(true);
            pair.Value.ResetStats();
            allyParent.childCharacters.Add(pair.Value.gameObject);

        }
    }
    public void ResetAll()
    {
        createdEnemyWalls.Clear();
        createdAllyWalls.Clear();
        foreach (var item in hexNodes.Values)
        {
            item.HardResetAll();
        }
    }
    public void RemoveCenterPoint(HexNode node)
    {
        if (node.EnemyBlockingZonecenter)
        {
            foreach (var neighbor in node.Neighbors)
            {
                neighbor.TargetedEnemyzone = false;
            }
            node.TargetedEnemyzone = false;
        }
        if (node.AllyBlockingZonecenter)
        {
            foreach (var neighbor in node.Neighbors)
            {
                neighbor.TargetedAllyZone = false;
            }
            node.TargetedAllyZone = false;
        }
        node.AllyBlockingZonecenter = false;
        node.EnemyBlockingZonecenter = false;
        CustomLogger.Log(this, $"removing {node.name} as center , pos int = {node.Position}");
        UpdateBlockingZoneWalls();
    }


    public void UpdateBlockingZoneWalls()
    {
        ClearPreviousWalls();
        foreach (var node in hexNodes.Values)
        {
            if (node.AllyBlockingZonecenter)
            {
                foreach (var neighbor in node.Neighbors)
                {
                    neighbor.TargetedAllyZone = true;
                }

            }
            if (node.EnemyBlockingZonecenter)
            {
                foreach (var neighbor in node.Neighbors)
                {
                    neighbor.TargetedEnemyzone = true;
                }

            }
        }
        foreach (var node in hexNodes.Values)
        {
            if (node.TargetedAllyZone)
            {
                foreach (var neighbor in node.Neighbors)
                {
                    if (!neighbor.TargetedAllyZone)
                    {
                        CreateWallIfNotExist(node, neighbor, true);
                    }
                }
            }
            if (node.TargetedEnemyzone)
            {
                foreach (var neighbor in node.Neighbors)
                {
                    if (!neighbor.TargetedEnemyzone)
                    {
                        CreateWallIfNotExist(node, neighbor, false);
                    }
                }
            }
        }
    }

    private void ClearPreviousWalls()
    {
        foreach (var wallObj in activeWalls)
        {
            wallObj.SetActive(false);
        }
        activeWalls.Clear();
        createdAllyWalls.Clear();
        createdEnemyWalls.Clear();
    }
    public void SpawnMap()
    {
        for (int i = 0; i < gridSize * gridSize; i++)
        {
            int col = i / gridSize, row = i % gridSize;
            float xPos = row * hexWidth + (col % 2 == 1 ? oddRowOffset : 0);
            float zPos = col * hexHeight;
            Vector3 pos = new Vector3(xPos, 0, zPos);

            GameObject obj = ResourcePool.Instance.GetFloor(pos);
            obj.name = $"HexNode [{i}]";
            HexNode node = obj.GetComponent<HexNode>();
            node.Position = pos;
            node.Index = i;

            // Assign cube coordinates
            int q = row - (col - (col & 1)) / 2, r = col;
            node.X = q;
            node.Y = -q - r;
            node.Z = r;

            string cubeCoordKey = CubeCoordinatesToKey(node.X, node.Y, node.Z);
            hexNodes[cubeCoordKey] = node;
            indexToCubeKey[i] = cubeCoordKey;
            if (i < 32)
            {
                node.isAllyHex = true;
            }
            else
            {
                node.isAllyHex = false;
            }
        }

        InitializeNeighbors();
        ResourcesInitialized.Raise();
        foreach (var item in hexNodes.Values)
        {
            item.SetColorState(GameEnum.ColorState.Default);
        }
    }
    /// <summary>
    /// 取得從 (startCol, startRow) 開始、寬度為 width、高度固定 3 的六角格清單。
    /// </summary>
    public List<HexNode> GetRectangleHexNodes(int startCol, int startRow, int width)
    {
        var result = new List<HexNode>();

        // 高度(垂直方向) = 3，所以 col 從 startCol ~ startCol+2
        for (int col = startCol; col < startCol + 3; col++)
        {
            // 邊界檢查
            if (col < 0 || col >= gridSize)
                continue;

            // 寬度(水平方向) = width，所以 row 從 startRow ~ startRow+(width-1)
            for (int row = startRow; row < startRow + width; row++)
            {
                if (row < 0 || row >= gridSize)
                    continue;

                // 依您 SpawnMap 的公式 i = col*gridSize + row 取得 index
                int index = col * gridSize + row;

                if (indexToCubeKey.TryGetValue(index, out string cubeKey))
                {
                    result.Add(hexNodes[cubeKey]);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 計算一組 HexNode 中的「敵方單位」數量。
    /// (若 OccupyingCharacter != null、且其 IsAlly != parentIsAlly，即視為敵方)
    /// </summary>
    private int GetEnemyCount(List<HexNode> nodes, bool parentIsAlly)
    {
        int count = 0;
        foreach (var node in nodes)
        {
            if (node.OccupyingCharacter != null
                && node.OccupyingCharacter.IsAlly != parentIsAlly
                && node.OccupyingCharacter.isTargetable)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// 在地圖上找出可容納最多敵方單位的 3×width 區域，並返回其 HexNode 清單。
    /// </summary>
    public List<HexNode> FindBestEnemyRectangle(int width, bool parentIsAlly)
    {
        int maxEnemies = -1;
        List<HexNode> bestRect = null;

        // 需要容納 3 列 (col, col+1, col+2)，所以最遠只能到 gridSize - 3
        for (int col = 0; col <= gridSize - 3; col++)
        {
            // 寬度為 width，row 方向最遠只能到 gridSize - width
            for (int row = 0; row <= gridSize - width; row++)
            {
                // 取得該位置的 3×width 區域
                var rectNodes = GetRectangleHexNodes(col, row, width);

                // 計算其中的「敵方單位」數量
                int enemyCount = GetEnemyCount(rectNodes, parentIsAlly);

                if (enemyCount > maxEnemies)
                {
                    maxEnemies = enemyCount;
                    bestRect = rectNodes;
                }
            }
        }

        CustomLogger.Log(this, $"最佳 3×{width} 區域可容納敵方數量 = {maxEnemies}");
        return bestRect;
    }

    public HexNode GetEmptyHex()
    {
        for (int i = 31; i > 1; i--)
        {
            if (hexNodes[indexToCubeKey[i]].OccupyingCharacter == null)
            {
                return hexNodes[indexToCubeKey[i]];
            }
        }
        CustomLogger.Log(this, "No empty hex found.");
        return null;
    }
    private string CubeCoordinatesToKey(int x, int y, int z) => $"{x},{y},{z}";

    public HexNode GetHexNodeByPosition(Vector3 position)
    {
        Vector3 adjustedPos = position;
        foreach (var hexNode in hexNodes.Values)
        {
            if (Vector3.Distance(hexNode.Position, adjustedPos) < 0.1f)
                return hexNode;
        }

        Debug.LogError($"No HexNode found for position {position}");
        return null;
    }

    public Vector3 GetPositionByIndex(int index)
    {
        if (indexToCubeKey.TryGetValue(index, out string cubeKey) && hexNodes.TryGetValue(cubeKey, out HexNode node))
        {
            Debug.Log($"GetPositionByIndex - Index: {index}, CubeKey: {cubeKey}, Position: {node.Position}");
            return node.Position;
        }

        Debug.LogError($"No position found for GridIndex {index}");
        return Vector3.zero;
    }
    public HexNode FindBestHexNode(CharacterCTRL character, int radius, bool findEnemies, bool requireEmpty, HexNode currHex, bool isLogistic = false)
    {
        HexNode bestHexNode = null;
        int maxCount = int.MinValue;
        List<HexNode> candidates = new List<HexNode>();

        Debug.Log($"Starting FindBestHexNode for character {character.name}, radius: {radius}, findEnemies: {findEnemies}, requireEmpty: {requireEmpty}");

        // 篩選符合條件的節點並找到最大鄰居數
        foreach (var node in hexNodes.Values)
        {
            if (!node.IsBattlefield || (requireEmpty && node.OccupyingCharacter != null))
                continue;

            int count = GetCharactersWithinRadius(node, character.IsAlly, radius, findEnemies, character).Count;

            if (count > maxCount)
            {
                maxCount = count;
            }
        }
        foreach (var node in hexNodes.Values)
        {
            if (!node.IsBattlefield || (requireEmpty && node.OccupyingCharacter != null))
                continue;

            int count = GetCharactersWithinRadius(node, character.IsAlly, radius, findEnemies, character).Count;

            if (count == maxCount)
            {
                candidates.Add(node);
            }
        }
        // 如果是後勤模式，直接返回找到的最佳節點
        if (isLogistic)
            return candidates.FirstOrDefault();

        // 比較當前格子與最佳候選節點
        int currentNeighborsCount = GetCharactersWithinRadius(currHex, character.IsAlly, radius, findEnemies, character).Count;

        if (currentNeighborsCount >= maxCount)
        {
            Debug.Log($"Current hex {currHex.name} has same or higher neighbor count as best candidates. Keeping current hex.");
            return currHex;
        }

        // 從候選節點中選擇距離當前格子最近的
        int minDistance = int.MaxValue;
        foreach (var node in candidates)
        {
            int distance = Mathf.Abs(currHex.Index - node.Index);
            if (distance < minDistance)
            {
                minDistance = distance;
                bestHexNode = node;
            }
        }

        Debug.Log($"Best node selected: {bestHexNode.name}");
        return bestHexNode;
    }



    public List<CharacterCTRL> GetCharactersWithinRadius(HexNode centerNode, bool isAlly, int radius, bool findEnemies, CharacterCTRL character)
    {
        int count = 0;
        HashSet<HexNode> visited = new HashSet<HexNode> { centerNode };
        List<HexNode> currentLayer = new List<HexNode> { centerNode };
        List<CharacterCTRL> c = new List<CharacterCTRL> { };
        for (int i = 0; i < radius; i++)
        {
            List<HexNode> nextLayer = new List<HexNode>();
            foreach (HexNode node in currentLayer)
            {
                // 檢查該格子是否有角色佔領
                if (node.OccupyingCharacter != null)
                {
                    bool isEnemy = node.OccupyingCharacter.IsAlly != isAlly;
                    if (findEnemies == isEnemy && node.OccupyingCharacter != character && node.OccupyingCharacter.gameObject.activeInHierarchy)
                    {
                        count++;
                        c.Add(node.OccupyingCharacter);
                    }
                }

                // 遍歷該格子的鄰居，將未訪問過的加入下一層
                foreach (HexNode neighbor in node.Neighbors)
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        nextLayer.Add(neighbor);
                    }
                }
            }
            currentLayer = nextLayer;
        }
        return c;
    }

    public List<HexNode> GetHexNodesWithinRange(HexNode centerNode, int range)
    {
        List<HexNode> nodesInRange = new List<HexNode>();
        HashSet<HexNode> visited = new HashSet<HexNode> { centerNode };
        List<HexNode> currentLayer = new List<HexNode> { centerNode };

        for (int i = 0; i < range; i++)
        {
            List<HexNode> nextLayer = new List<HexNode>();

            foreach (HexNode node in currentLayer)
            {
                // 將當前格子添加到結果中（只要它是戰場上的格子）
                if (node.IsBattlefield)
                {
                    nodesInRange.Add(node);
                }

                // 遍歷當前格子的鄰居，並加入下一層
                foreach (HexNode neighbor in node.Neighbors)
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        nextLayer.Add(neighbor);
                    }
                }
            }

            currentLayer = nextLayer;
        }

        return nodesInRange;
    }


    private void InitializeNeighbors()
    {
        Vector3Int[] directions = {
            new Vector3Int(1, -1, 0), new Vector3Int(1, 0, -1), new Vector3Int(0, 1, -1),
            new Vector3Int(-1, 1, 0), new Vector3Int(-1, 0, 1), new Vector3Int(0, -1, 1)
        };

        foreach (HexNode node in hexNodes.Values)
        {
            foreach (Vector3Int dir in directions)
            {
                string key = CubeCoordinatesToKey(node.X + dir.x, node.Y + dir.y, node.Z + dir.z);
                if (hexNodes.TryGetValue(key, out HexNode neighborNode))
                    node.AddNeighbor(neighborNode);
            }
        }
    }

    public void UpdateDesertifiedTiles(int randomKey,int count)
    {
        SetDesertifiedTiles(count, randomKey);
    }
    public void ResetDesertifiedTiles()
    {
        foreach (var tile in hexNodes.Values)
        {
            tile.isDesertified = false;
            tile.UpdateTileColor();
        }
    }

    public void SetDesertifiedTiles(int count, int randomKey)
    {
        // 確保 count 是合法的羈絆數量（6、8、10、12）
        int[] validCounts = { 6, 8, 10, 12 };
        if (!validCounts.Contains(count)) return;

        List<HexNode> deployableTiles = new List<HexNode>();
        foreach (var tile in hexNodes.Values)
        {
            if (tile.IsBattlefield)
                deployableTiles.Add(tile);
        }

        count = Mathf.Min(count, deployableTiles.Count);
        if (count % 2 != 0) count--; // 確保偶數

        int perGroup = count / 2;

        List<HexNode> group1 = new List<HexNode>();
        List<HexNode> group2 = new List<HexNode>();

        // 將可用的格子分成兩組
        foreach (var tile in deployableTiles)
        {
            if (tile.Index >= 0 && tile.Index <= 31)
                group1.Add(tile);
            else if (tile.Index >= 32 && tile.Index <= 63)
                group2.Add(tile);
        }

        // 統一使用相同的隨機種子來確保一致性
        System.Random prng = new System.Random(randomKey);

        // 產生一個固定的選擇順序
        List<HexNode> selectedOrderGroup1 = group1.OrderBy(t => prng.Next()).ToList();
        List<HexNode> selectedOrderGroup2 = group2.OrderBy(t => prng.Next()).ToList();

        // 確保 count 增加時是累積選擇
        int maxGroup1 = Mathf.Min(perGroup, selectedOrderGroup1.Count);
        int maxGroup2 = Mathf.Min(perGroup, selectedOrderGroup2.Count);

        // 設定格子的狀態
        foreach (var tile in hexNodes.Values)
        {
            tile.isDesertified = false; // 重置所有格子
            tile.UpdateTileColor();
        }

        // 選擇前 maxGroup1 個作為 desertified
        for (int i = 0; i < maxGroup1; i++)
        {
            selectedOrderGroup1[i].isDesertified = true;
            selectedOrderGroup1[i].UpdateTileColor();
        }

        // 選擇前 maxGroup2 個作為 desertified
        for (int i = 0; i < maxGroup2; i++)
        {
            selectedOrderGroup2[i].isDesertified = true;
            selectedOrderGroup2[i].UpdateTileColor();
        }
    }

    public string GetHexNodeKey(HexNode node)
    {
        return CubeCoordinatesToKey(node.X, node.Y, node.Z);
    }

    // 生成牆壁的唯一鍵值
    public string GetWallKey(HexNode node1, HexNode node2)
    {
        string key1 = GetHexNodeKey(node1);
        string key2 = GetHexNodeKey(node2);
        return key1.CompareTo(key2) < 0 ? $"{key1}_{key2}" : $"{key2}_{key1}";
    }



    private void CreateWallIfNotExist(HexNode node, HexNode neighbor, bool isAlly)
    {
        string wallKey = GetWallKey(node, neighbor);
        HashSet<string> wallSet = isAlly ? createdAllyWalls : createdEnemyWalls;

        if (wallSet.Contains(wallKey))
        {
            return;
        }

        wallSet.Add(wallKey);
        CreateWall(node, neighbor, wallKey, isAlly);
    }


    private void CreateWall(HexNode node, HexNode neighbor, string wallKey, bool isAllyWall)
    {
        Vector3 pos = (node.transform.position + neighbor.transform.position) / 2;
        Vector3 direction = (neighbor.transform.position - node.transform.position).normalized;
        direction.y = 0;
        Vector3 Pos = new Vector3(pos.x, 0.64f, pos.z);
        float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, -angle, 0);
        GameObject wallObj = ResourcePool.Instance.GetWall(Pos);
        wallObj.transform.rotation = rotation;
        Wall wall = wallObj.GetComponent<Wall>();
        wall.SetWallType(isAllyWall);
        wall.StartWallLifetime(wallKey, isAllyWall);
        activeWalls.Add(wallObj);
    }


}
