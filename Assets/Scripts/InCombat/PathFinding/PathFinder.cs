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
    public static List<HexNode> FindPath(string name, HexNode startNode, HexNode targetNode,int range = 0)
    {
        CustomLogger.Log(name,$"Finding {name}'s path");
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
            HexNode currentNode = openList.OrderBy(node => node.fCost).ThenBy(node => node.hCost).First();
            openList.Remove(currentNode);
            closedList.Add(currentNode);
            int distanceToTarget = GetHexDistance(currentNode, targetNode);
            if (currentNode == targetNode || (range > 0 && distanceToTarget <= range))
            {
                return RetracePath(startNode, currentNode);
            }

            foreach (HexNode neighbor in currentNode.Neighbors)
            {
                if (neighbor == null || closedList.Contains(neighbor) || (neighbor != startNode && neighbor != targetNode && neighbor.IsHexReserved()))
                    continue;
                float movementCost = currentNode.gCost + 1f;
                int moveDeltaZ = neighbor.Z - currentNode.Z;
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
        StringBuilder sb = new StringBuilder();
        foreach (var item in path)
        {
            sb.AppendLine(item.name);
        }
        CustomLogger.Log(startNode, sb.ToString());
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


