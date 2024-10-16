using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    private static PathFinder _instance;
    public static PathFinder Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new PathFinder();
            }
            return _instance;
        }
    }
    public static List<HexNode> FindPath(string name, HexNode startNode, HexNode targetNode, bool isAlly, int range = 0,bool simulated = false)
    {
        foreach (HexNode node in SpawnGrid.Instance.hexNodes.Values)
        {
            node.gCost = Mathf.Infinity;
            node.hCost = Mathf.Infinity;
            node.CameFrom = null;
        }

        startNode.gCost = 0;
        startNode.hCost = GetHexDistance(startNode, targetNode);

        List<HexNode> openList = new List<HexNode> { startNode };
        HashSet<HexNode> closedList = new HashSet<HexNode>();

        while (openList.Count > 0)
        {
            // 找到 fCost 最小的节点
            HexNode currentNode = openList.OrderBy(node => node.fCost).ThenBy(node => node.hCost).First();

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            // 检查是否达到目标或在范围内
            int distanceToTarget = GetHexDistance(currentNode, targetNode);
            if (currentNode == targetNode || (range > 0 && distanceToTarget <= range))
            {
                return RetracePath(startNode, currentNode);
            }


            foreach (HexNode neighbor in currentNode.Neighbors)
            {
                bool reserved = neighbor.IsHexReserved() || (simulated && neighbor.IsTemporarilyReserved());

                if (neighbor == null || closedList.Contains(neighbor) || (neighbor != startNode && neighbor != targetNode && reserved))
                    continue;
                float movementCost = currentNode.gCost + 1f;

                // 根据 isAlly 调整移动成本
                int moveDeltaZ = neighbor.Z - currentNode.Z;
                //movementCost += (isAlly && moveDeltaZ > 0) || (!isAlly && moveDeltaZ < 0) ? -1f : 0f;

                if (movementCost < neighbor.gCost)
                {
                    neighbor.gCost = movementCost;
                    neighbor.hCost = GetHexDistance(neighbor, targetNode);
                    neighbor.CameFrom = currentNode;

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }

        Debug.LogError($"{name} No path found");
        return new List<HexNode>();
    }


    private static List<HexNode> RetracePath(HexNode startNode, HexNode endNode)
    {
        List<HexNode> path = new List<HexNode>();
        HexNode currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.CameFrom;
        }

        path.Add(startNode); // 包含起始节点

        path.Reverse();
        return path;
    }

    private static int GetHexDistance(HexNode nodeA, HexNode nodeB)
    {
        int dx = nodeB.X - nodeA.X;
        int dy = nodeB.Y - nodeA.Y;
        int dz = nodeB.Z - nodeA.Z;

        int distance = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy), Mathf.Abs(dz));
        return distance;
    }



    private static float GetDistance(HexNode nodeA, HexNode nodeB)
    {
        return Vector3.Distance(nodeA.Position, nodeB.Position);
    }
}


