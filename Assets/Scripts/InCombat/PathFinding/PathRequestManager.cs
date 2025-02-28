using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PathRequestManager : MonoBehaviour
{
    private static PathRequestManager _instance;
    private CustomLogger logger = new CustomLogger();

    public static PathRequestManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("PathRequestManager");
                _instance = go.AddComponent<PathRequestManager>();
            }
            return _instance;
        }
    }

    private Dictionary<CharacterCTRL, List<HexNode>> characterReservations
        = new Dictionary<CharacterCTRL, List<HexNode>>();
    private List<PathRequest> pathRequestBuffer = new List<PathRequest>();
    private bool isProcessingPath;

    public void RequestPath(CharacterCTRL character, HexNode startNode, HexNode targetNode,
                            Action<List<HexNode>> callback, int range)
    {
        CustomLogger.Log(this, $"character {character} requesting path from {startNode} to {targetNode} , target = {character.Target}, target node occupying {targetNode.OccupyingCharacter}");
        PathRequest newRequest = new PathRequest(character, startNode, targetNode, callback, range);
        pathRequestBuffer.Add(newRequest);

        if (!isProcessingPath)
        {
            StartCoroutine(ProcessBufferedRequests());
        }
    }

    private IEnumerator ProcessBufferedRequests()
    {
        isProcessingPath = true;
        yield return new WaitForSeconds(0.1f);
        List<PathRequest> sortedRequests = pathRequestBuffer
            .OrderBy(r => GetHexDistance(r.startNode, r.targetNode))
            .ToList();
        StringBuilder sb = new StringBuilder();
        foreach (HexNode node in SpawnGrid.Instance.hexNodes.Values)
        {
            node.gCost = Mathf.Infinity;
            node.hCost = Mathf.Infinity;
            node.CameFrom = null;
        }

        foreach (var request in sortedRequests)
        {
            sb.AppendLine($" request: {request.character.characterStats.CharacterName} to target{request.targetNode}");
            List<HexNode> path = PathFinder.FindPath(
                request.character.characterStats.CharacterName,
                request.startNode,
                request.targetNode,
                (int)request.character.stats.GetStat(GameEnum.StatsType.Range)
            );
            ReserveNodes(path, request.character);
            request.callback(path);
        }
        CustomLogger.Log(this, sb.ToString() ); 
        pathRequestBuffer.Clear();
        isProcessingPath = false;
    }
    private int GetHexDistance(HexNode a, HexNode b)
    {
        int dx = b.X - a.X;
        int dy = b.Y - a.Y;
        int dz = b.Z - a.Z;
        return Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy), Mathf.Abs(dz));
    }

    private void ReserveNodes(List<HexNode> path, CharacterCTRL character)
    {
        List<HexNode> reservedNodes = new List<HexNode>();
        foreach (var node in path)
        {
            node.Reserve(character);
            reservedNodes.Add(node);
        }
        characterReservations[character] = reservedNodes;
    }

    public void ReleaseCharacterReservations(CharacterCTRL character)
    {
        if (characterReservations.TryGetValue(character, out List<HexNode> reservedNodes))
        {
            foreach (var node in reservedNodes)
            {
                node.Release();
            }
            characterReservations.Remove(character);
        }
    }

    public void ReleaseRemainingReservations(CharacterCTRL character, int startIndex)
    {
        if (characterReservations.TryGetValue(character, out List<HexNode> reservedNodes))
        {
            for (int i = startIndex; i < reservedNodes.Count; i++)
            {
                reservedNodes[i].Release();
            }
            reservedNodes.RemoveRange(startIndex, reservedNodes.Count - startIndex);
        }
    }

    private void Update()
    {
        // 假如有角色被移除或失效，就釋放它的預約
        var keys = new List<CharacterCTRL>(characterReservations.Keys);
        foreach (var item in keys)
        {
            if (!item || !item.gameObject.activeInHierarchy)
            {
                ReleaseCharacterReservations(item);
            }
        }
    }
}

public struct PathRequest
{
    public CharacterCTRL character;
    public HexNode startNode;
    public HexNode targetNode;
    public Action<List<HexNode>> callback;
    public int range;

    public PathRequest(CharacterCTRL _character, HexNode _startNode, HexNode _targetNode,
                       Action<List<HexNode>> _callback, int _range)
    {
        character = _character;
        startNode = _startNode;
        targetNode = _targetNode;
        callback = _callback;
        range = _range;
    }
}
