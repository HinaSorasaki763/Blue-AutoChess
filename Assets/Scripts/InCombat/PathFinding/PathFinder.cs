using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
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
    public static List<HexNode> FindPath(string name, HexNode startNode, HexNode targetNode, int range = 0, CharacterCTRL character = null)
    {
        CustomLogger.Log(name, $"Finding {name}'s path");

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

        HexNode closestNode = startNode;
        float closestDistance = GetHexDistance(startNode, targetNode);

        while (openList.Count > 0)
        {
            HexNode currentNode = openList.OrderBy(node => node.fCost).ThenBy(node => node.hCost).First();
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            int distanceToTarget = GetHexDistance(currentNode, targetNode);

            // 記錄目前最接近目標的點
            if (distanceToTarget < closestDistance)
            {
                closestDistance = distanceToTarget;
                closestNode = currentNode;
            }

            if (currentNode == targetNode || (range > 0 && distanceToTarget <= range))
            {
                return RetracePath(startNode, currentNode);
            }

            foreach (HexNode neighbor in currentNode.Neighbors)
            {
                if (neighbor == null || closedList.Contains(neighbor)) continue;
                if (neighbor != startNode && neighbor != targetNode && character != null && neighbor.IsHexReserved())
                    continue;

                float movementCost = currentNode.gCost + 1f;

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

        // 沒有直接路徑，回傳最近可到的路徑
        CustomLogger.LogWarning(name, $"{name} cannot reach target, fallback to closestNode {closestNode}");

        if (closestNode != startNode)
            return RetracePath(startNode, closestNode);
        else
            return new List<HexNode>(); // 如果連一格都沒法動
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
        path.Reverse();
        if (path.Count >=1)
        {
            path = new List<HexNode> { path[0] };
        }
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


