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
    public HashSet<string> createdNotINteractableAllyWalls = new HashSet<string>();
    public HashSet<string> createdNotINteractableLogisticAllyWalls = new HashSet<string>();
    public HashSet<string> createdEnemyWalls = new HashSet<string>();
    public List<GameObject> activeWalls = new List<GameObject>();
    public CharacterParent allyParent;
    private readonly Vector3 offset = new Vector3(0, 0.14f, 0);
    public HexNode LogisticNode1, LogisticNode2;
    public CharacterCTRL Logistic1, Logistic2;
    public Texture2D mapTexture;
    public Material hexMaterial;
    public GameObject WallParent;
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
        if (LogisticNode1.OccupyingCharacter != null)
        {
            Logistic1 = LogisticNode1.OccupyingCharacter;
        }
        else
        {
            Logistic1 = null;
        }
        if (LogisticNode2.OccupyingCharacter != null)
        {
            Logistic2 = LogisticNode2.OccupyingCharacter;
        }
        else
        {
            Logistic2 = null;
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
            pair.Value.transform.position = pair.Key.transform.position + offset;
            pair.Value.gameObject.SetActive(true);
            pair.Value.RecalculateStats();
            pair.Key.Reserve(pair.Value);
            pair.Key.OccupyingCharacter = pair.Value;
        }
        if (Logistic1 != null)
        {
            Logistic1.transform.position = LogisticNode1.transform.position + offset;
            Logistic1.gameObject.SetActive(true);
            Logistic1.RecalculateStats();
            LogisticNode1.Reserve(Logistic1);
            LogisticNode1.OccupyingCharacter = Logistic1;
        }
        if (Logistic2 != null)
        {
            Logistic2.transform.position = LogisticNode2.transform.position + offset;
            Logistic2.gameObject.SetActive(true);
            Logistic2.RecalculateStats();
            LogisticNode2.Reserve(Logistic2);
            LogisticNode2.OccupyingCharacter = Logistic2;
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
    public void SpawnMap()
    {
        for (int i = 0; i < gridSize * gridSize; i++)
        {
            int col = i / gridSize;
            int row = i % gridSize;

            float xPos = row * hexWidth + (col % 2 == 1 ? oddRowOffset : 0);
            float zPos = col * hexHeight;
            Vector3 pos = new Vector3(xPos, 0, zPos);

            GameObject obj = ResourcePool.Instance.GetFloor(pos);
            obj.name = $"HexNode [{i}]";
            HexNode node = obj.GetComponent<HexNode>();
            node.Position = pos;
            node.Index = i;

            // Cube coords
            int q = row - (col - (col & 1)) / 2;
            int r = col;
            node.X = q;
            node.Y = -q - r;
            node.Z = r;

            string cubeCoordKey = CubeCoordinatesToKey(node.X, node.Y, node.Z);
            hexNodes[cubeCoordKey] = node;
            indexToCubeKey[i] = cubeCoordKey;
            node.isAllyHex = i < 32;
        }

        // 先初始化鄰居
        InitializeNeighbors();

        // 在有鄰居資訊後生成牆壁
        foreach (var node in hexNodes.Values)
        {
            if (node.Index <= 31)
            {
                CreateWallsAroundNodeWithBoundary(node, false);
            }
        }
        CreateWallsForIsolatedNode(LogisticNode1, true);
        CreateWallsForIsolatedNode(LogisticNode2, true);
        foreach (var item in BenchManager.Instance.benchSlots)
        {
            CreateWallsForSquareNode(item.GetComponent<HexNode>());
        }
        WallParent.SetActive(false);
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
    public string CubeCoordinatesToKey(int x, int y, int z) => $"{x},{y},{z}";

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
    public HexNode FindBestHexNode(
        CharacterCTRL character,
        int radius,
        bool findEnemies,
        bool requireEmpty,
        HexNode currHex,
        bool isLogistic = false
    )
    {
        // 儲存所有符合條件的節點中，擁有最多可攻擊或可支援角色數 (maxCount)
        float maxCount = int.MinValue;
        List<HexNode> occupiedCandidates = new List<HexNode>();
        List<HexNode> emptyCandidates = new List<HexNode>();
        CustomLogger.Log(this, $"Starting FindBestHexNode for character {character.name}, " +
                               $"radius: {radius}, findEnemies: {findEnemies}, requireEmpty: {requireEmpty}");
        foreach (var node in hexNodes.Values)
        {
            // 如果不是戰場格 或 (需要空格時，但該格被佔據)，就略過
            if (!node.IsBattlefield || (requireEmpty && node.OccupyingCharacter != null))
                continue;

            float count = GetCharactersWithinRadius(node, character.IsAlly, radius, findEnemies, character).Item2;
            CustomLogger.Log(this, $"node {node} = {count}");
            if (count > maxCount)
            {
                maxCount = count;
            }
        }

        // 找出符合 maxCount 的節點並分到佔據/空節點清單
        foreach (var node in hexNodes.Values)
        {
            if (!node.IsBattlefield || (requireEmpty && node.OccupyingCharacter != null))
                continue;

            float count = GetCharactersWithinRadius(node, character.IsAlly, radius, findEnemies, character).Item2;
            if (count == maxCount)
            {
                if (node.OccupyingCharacter != null)
                    occupiedCandidates.Add(node);
                else
                    emptyCandidates.Add(node);
            }
        }

        // 若只單純做 Logistic，不做更細的距離或比對
        if (isLogistic)
        {
            // 若 requireEmpty == false，先回傳有角色的格子；否則只能回傳空格子
            if (!requireEmpty && occupiedCandidates.Count > 0)
                return occupiedCandidates.FirstOrDefault();
            return emptyCandidates.FirstOrDefault();
        }
        float currentNeighborsCount = GetCharactersWithinRadius(currHex, character.IsAlly, radius, findEnemies, character).Item2;
        if (currentNeighborsCount >= maxCount)
        {
            CustomLogger.Log(this, $"maxCount = {maxCount}, currentNeighborsCount = {currentNeighborsCount}. " +
                                   $"Current hex {currHex.name} has same or higher neighbor count as best candidates. Keeping current hex.");
            return currHex;
        }
        List<HexNode> finalCandidates;
        if (!requireEmpty && occupiedCandidates.Count > 0)
            finalCandidates = occupiedCandidates;
        else
            finalCandidates = emptyCandidates;
        HexNode bestHexNode = null;
        int minDistance = int.MaxValue;
        foreach (var node in finalCandidates)
        {
            int distance = Mathf.Abs(currHex.Index - node.Index);
            if (distance < minDistance)
            {
                minDistance = distance;
                bestHexNode = node;
            }
        }

        if (bestHexNode != null)
        {
            CustomLogger.Log(this, $"Best node selected: {bestHexNode.name}");
        }
        else
        {
            CustomLogger.LogWarning(this, "No best node found, returning current hex by default.");
            bestHexNode = currHex;
        }

        return bestHexNode;
    }




    public (List<CharacterCTRL>, float) GetCharactersWithinRadius(HexNode centerNode, bool isAlly, int radius, bool findEnemies, CharacterCTRL character)
    {
        int characterCount = 0;
        float weightSum = 0f;
        HashSet<HexNode> visited = new HashSet<HexNode> { centerNode };
        List<HexNode> currentLayer = new List<HexNode> { centerNode };
        List<CharacterCTRL> foundCharacters = new List<CharacterCTRL>();

        for (int distance = 0; distance < radius; distance++)
        {
            List<HexNode> nextLayer = new List<HexNode>();
            foreach (HexNode node in currentLayer)
            {
                if (node.OccupyingCharacter != null)
                {
                    bool isEnemy = node.OccupyingCharacter.IsAlly != isAlly;
                    if (findEnemies == isEnemy && node.OccupyingCharacter != character && node.OccupyingCharacter.gameObject.activeInHierarchy)
                    {
                        characterCount++;
                        foundCharacters.Add(node.OccupyingCharacter);
                        float weight = 1f / (distance + 1f);
                        weightSum += weight;
                    }
                }
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

        return (foundCharacters, weightSum);
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

    public void UpdateDesertifiedTiles(int randomKey, int count)
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
        if (SelectedAugments.Instance.CheckAugmetExist(102, true))
        {
            count = 64;
        }
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
            if (!selectedOrderGroup1[i].oasis)
            {
                selectedOrderGroup1[i].isDesertified = true;
                selectedOrderGroup1[i].UpdateTileColor();
            }

        }

        // 選擇前 maxGroup2 個作為 desertified
        for (int i = 0; i < maxGroup2; i++)
        {
            if (!selectedOrderGroup2[i].oasis)
            {
                selectedOrderGroup2[i].isDesertified = true;
                selectedOrderGroup2[i].UpdateTileColor();
            }

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



    public void CreateWallIfNotExist(CharacterCTRL parent,int resist, HexNode node, HexNode neighbor, bool isAlly, bool normal)
    {
        string wallKey = GetWallKey(node, neighbor);
        HashSet<string> wallSet = isAlly ? createdAllyWalls : createdEnemyWalls;
        wallSet.Add(wallKey);
        CreateWall(parent, resist, node, neighbor, wallKey, isAlly, normal);

    }
    private static readonly Vector3[] SquareDirections =
    {
        new Vector3(1, 0, 0),   // 右
        new Vector3(-1, 0, 0),  // 左
        new Vector3(0, 0, 1),   // 上
        new Vector3(0, 0, -1)   // 下
    };

    public void CreateWallsForSquareNode(HexNode node)
    {
        foreach (var dir in SquareDirections)
        {
            // 依照你的 hexWidth/hexHeight 來決定格子邊長
            Vector3 offset = new Vector3(dir.x * 1, 0, dir.z * 1);

            Vector3 midPos = node.transform.position + offset * 0.5f;
            float angle = Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, -angle, 0);
            GameObject wallObj = Instantiate(ResourcePool.Instance.wallPrefab, new Vector3(midPos.x, 0.64f, midPos.z), rotation, WallParent.transform);
            wallObj.transform.localScale = new Vector3(0.03f, 1, 1f);
            wallObj.transform.rotation = rotation;
            wallObj.GetComponent<BoxCollider>().enabled = false;
            wallObj.transform.SetParent(WallParent.transform, false);

            // 黑色
            wallObj.GetComponent<Renderer>().material.color = Color.black;
        }
    }

    public void CreateWallsForIsolatedNode(HexNode node, bool isNotInteractable)
    {
        foreach (var dir in CubeDirections)
        {
            Vector3 offset = HexDirectionToWorldOffset(dir);
            Vector3 midPos = node.transform.position + offset * 0.5f;
            float angle = Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, -angle, 0);
            GameObject wallObj = Instantiate(ResourcePool.Instance.wallPrefab, new Vector3(midPos.x, 0.64f, midPos.z), rotation, WallParent.transform);
            wallObj.transform.rotation = rotation;
            wallObj.GetComponent<BoxCollider>().enabled = false;
            wallObj.transform.SetParent(WallParent.transform, false);

            if (isNotInteractable)
            {
                wallObj.GetComponent<Renderer>().material.color = Color.green;
            }
        }
    }

    public void CreateWallsAroundNodeWithBoundary(HexNode node, bool isNotInteractable)
    {
        foreach (var dir in CubeDirections)
        {
            string neighborKey = CubeCoordinatesToKey(node.X + dir.x, node.Y + dir.y, node.Z + dir.z);

            // 內部牆
            if (hexNodes.TryGetValue(neighborKey, out HexNode neighbor))
            {
                string wallKey = GetWallKey(node, neighbor);

                // 根據是否可互動，挑選不同的集合
                HashSet<string> wallSet = !isNotInteractable ? createdNotINteractableAllyWalls : createdNotINteractableLogisticAllyWalls;

                if (!wallSet.Contains(wallKey))
                {
                    wallSet.Add(wallKey);

                    // 建立牆
                    GameObject wallObj = CreateWall(null,0, node, neighbor, wallKey, true, true);

                    // 設定顏色
                    if (isNotInteractable)
                    {
                        wallObj.GetComponent<Renderer>().material.color = Color.green;
                    }
                }
            }
            else
            {
                // 外圍邊界牆
                Vector3 offset = HexDirectionToWorldOffset(dir);
                Vector3 midPos = node.transform.position + offset * 0.5f;
                float angle = Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;
                Quaternion rotation = Quaternion.Euler(0, -angle, 0);
                GameObject wallObj = Instantiate(ResourcePool.Instance.wallPrefab, new Vector3(midPos.x, 0.64f, midPos.z), rotation, WallParent.transform);
                wallObj.transform.rotation = rotation;
                wallObj.GetComponent<BoxCollider>().enabled = false;
                wallObj.transform.SetParent(WallParent.transform, false);

                if (isNotInteractable)
                {
                    wallObj.GetComponent<Renderer>().material.color = Color.green;
                }
            }
        }
    }

    // 把 cube 方向轉成世界座標偏移（取決於你的 hexWidth / hexHeight）
    private Vector3 HexDirectionToWorldOffset((int x, int y, int z) dir)
    {
        // 這裡假設你用的是 axial 坐標轉換過來的 layout
        // x 對應 q, z 對應 r
        float xOffset = hexWidth * (dir.x + dir.z * 0.5f);
        float zOffset = hexHeight * (dir.z);
        return new Vector3(xOffset, 0, zOffset);
    }

    private static readonly (int x, int y, int z)[] CubeDirections =
    {
        (1, -1, 0), (1, 0, -1), (0, 1, -1),
        (-1, 1, 0), (-1, 0, 1), (0, -1, 1)
    };


    private GameObject CreateWall(CharacterCTRL parent,int resist, HexNode node, HexNode neighbor, string wallKey, bool isAllyWall, bool normal)
    {
        Vector3 pos = (node.transform.position + neighbor.transform.position) / 2;
        Vector3 direction = (neighbor.transform.position - node.transform.position).normalized;
        direction.y = 0;
        Vector3 Pos = new Vector3(pos.x, 0.64f, pos.z);
        float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, -angle, 0);

        if (normal)
        {
            GameObject obj = Instantiate(ResourcePool.Instance.wallPrefab,pos,rotation,WallParent.transform);
            obj.GetComponent<BoxCollider>().enabled = false;
            return obj;
        }
        if (!normal)
        {
            GameObject wallObj = ResourcePool.Instance.GetWall(Pos);
            wallObj.SetActive(true);
            wallObj.transform.rotation = rotation;
            wallObj.GetComponent<BoxCollider>().enabled = true;
            Wall wall = wallObj.GetComponent<Wall>();
            wall.InitWall(parent, isAllyWall, resist);

            wall.StartWallLifetime(wallKey, isAllyWall);
            activeWalls.Add(wallObj);
            return wallObj;
        }
        else return null;
    }


}
