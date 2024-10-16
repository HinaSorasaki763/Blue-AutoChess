using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private Dictionary<CharacterCTRL, List<HexNode>> characterReservations = new Dictionary<CharacterCTRL, List<HexNode>>();
    private List<PathRequest> pathRequestBuffer = new List<PathRequest>(); // 用於暫存請求的列表
    private bool isProcessingPath;

    public void RequestPath(CharacterCTRL character, HexNode startNode, HexNode targetNode, Action<List<HexNode>> callback, int range)
    {
        PathRequest newRequest = new PathRequest(character, startNode, targetNode, callback, range);
        pathRequestBuffer.Add(newRequest);

        if (!isProcessingPath)
            StartCoroutine(ProcessBufferedRequests());
    }

    private IEnumerator ProcessBufferedRequests()
    {
        isProcessingPath = true;
        yield return new WaitForSeconds(0.1f);

        List<PathRequest> optimalOrder = GetOptimalPathOrder(pathRequestBuffer);
        foreach (var request in optimalOrder)
        {
            List<HexNode> path = PathFinder.FindPath(request.character.characterStats.CharacterName, request.startNode, request.targetNode, request.character.IsAlly, request.character.stats.GetStat(GameEnum.StatsType.Range), false);
            ReserveNodes(path, request.character);
            request.callback(path);
        }

        pathRequestBuffer.Clear();
        isProcessingPath = false;
    }

    private List<PathRequest> GetOptimalPathOrder(List<PathRequest> requests)
    {
        var permutations = GetPermutations(requests);
        List<PathRequest> optimalOrder = null;
        int minimumTotalSteps = int.MaxValue;

        foreach (var permutation in permutations)
        {
            int totalSteps = CalculateTotalPathLength(permutation);
            if (totalSteps < minimumTotalSteps)
            {
                minimumTotalSteps = totalSteps;
                optimalOrder = permutation;
            }
        }

        return optimalOrder;
    }

    private List<List<PathRequest>> GetPermutations(List<PathRequest> requests)
    {
        if (requests.Count == 1) return new List<List<PathRequest>> { requests };

        var permutations = new List<List<PathRequest>>();
        foreach (var request in requests)
        {
            var remainingRequests = new List<PathRequest>(requests);
            remainingRequests.Remove(request);

            foreach (var subPermutation in GetPermutations(remainingRequests))
            {
                var newPermutation = new List<PathRequest> { request };
                newPermutation.AddRange(subPermutation);
                permutations.Add(newPermutation);
            }
        }
        return permutations;
    }

    private int CalculateTotalPathLength(List<PathRequest> requests)
    {
        int totalSteps = 0;
        List<HexNode> temporaryReservations = new List<HexNode>();
        foreach (var request in requests)
        {
            List<HexNode> estimatedPath = PathFinder.FindPath(request.character.characterStats.CharacterName, request.startNode, request.targetNode, request.character.IsAlly, request.character.stats.GetStat(GameEnum.StatsType.Range), true);
            totalSteps += estimatedPath.Count;
            foreach (var node in estimatedPath)
            {
                node.TemporarilyReserve(); // 標記為「假預約」
                temporaryReservations.Add(node);
            }
        }
        foreach (var node in temporaryReservations)
        {
            node.ClearTemporaryReservation();
        }

        return totalSteps;
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
}

public struct PathRequest
{
    public CharacterCTRL character;
    public HexNode startNode;
    public HexNode targetNode;
    public Action<List<HexNode>> callback;
    public int range;

    public PathRequest(CharacterCTRL _character, HexNode _startNode, HexNode _targetNode, Action<List<HexNode>> _callback, int _range)
    {
        character = _character;
        startNode = _startNode;
        targetNode = _targetNode;
        callback = _callback;
        range = _range;
    }
}