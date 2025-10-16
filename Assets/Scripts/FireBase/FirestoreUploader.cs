using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameEnum;
using System;

public class FirestoreUploader
{
    private FirebaseFirestore db;

    public async Task InitializeAsync()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            db = FirebaseFirestore.DefaultInstance;
            Debug.Log("Firebase Firestore ready.");
        }
        else
        {
            Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
        }
    }

    public async Task UploadTeamAsync(TeamData teamData)
    {
        Debug.Log("Start Upload");

        if (db == null)
        {
            Debug.LogError("Upload failed: Firestore not initialized (db is null)");
            return;
        }

        try
        {
            var slotDicts = teamData.slots.Select(FirestoreConverter.ToDict).ToList();

            var statsDict = new Dictionary<string, object>();
            foreach (var stat in teamData.statsContainer.GetAllStats())
            {
                statsDict[stat.statType.ToString()] = stat.value;
            }

            var data = new Dictionary<string, object>
        {
            { "playerName", teamData.Name },
            { "playerId", teamData.playerId },
            { "round", teamData.round },
            { "stage", teamData.stage },
            { "totalGames", teamData.totalGames },
            { "winGames", teamData.winGames },
            { "slots", slotDicts },
            { "stats", statsDict },
            { "SelectedAugments", teamData.selectedAugments },
            { "timestamp", Timestamp.GetCurrentTimestamp() },
            { "tag", "testbuild" }
        };

            var result = await db.Collection("teams").AddAsync(data);
            Debug.Log($"Team uploaded. DocID={result.Id}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Upload failed: {e.Message}\n{e.StackTrace}");
        }
    }




    public async Task<List<DocumentSnapshot>> GetRandomOpponentsAsync(int round, int stage, int count = 3)
    {
        var snapshot = await db.Collection("teams")
            .WhereEqualTo("round", round)
            .WhereEqualTo("stage", stage)
            .GetSnapshotAsync();

        if (snapshot.Count == 0)
        {
            snapshot = await db.Collection("teams")
                .WhereEqualTo("playerId", "TestBuildDummy")
                .GetSnapshotAsync();
            if (snapshot.Count == 0)
            {
                Debug.LogError("No dummy data found either.");
                return new List<DocumentSnapshot>();
            }
        }

        var docs = snapshot.Documents.ToList();
        var sysRand = new System.Random();
        var randomList = docs.OrderBy(_ => sysRand.NextDouble()).Take(count).ToList();

        foreach (var doc in randomList)
        {
            Debug.Log($"Opponent: DocID={doc.Id}, Data={FirestoreDebugExt.DictToString(doc.ToDictionary())}");
        }

        return randomList;
    }

    public async Task DeleteAllTeamsAsync()
    {
        var snapshot = await db.Collection("teams").GetSnapshotAsync();
        foreach (var doc in snapshot.Documents)
        {
            await doc.Reference.DeleteAsync();
        }
        Debug.Log("All teams deleted.");
    }

    public async Task DeleteTestBuildTeamsAsync()
    {
        var snapshot = await db.Collection("teams")
            .WhereEqualTo("tag", "testbuild")
            .GetSnapshotAsync();

        foreach (var doc in snapshot.Documents)
        {
            await doc.Reference.DeleteAsync();
        }
        Debug.Log("All testbuild teams deleted.");
    }

    public async Task GetAllTeamsAsync()
    {
        var snapshot = await db.Collection("teams").GetSnapshotAsync();

        foreach (var doc in snapshot.Documents)
        {
            Debug.Log($"DocID: {doc.Id}");
            Debug.Log(doc.ToDictionary().ToDebugString());
        }
    }
}
public class TeamData
{
    public string Name;
    public string playerId;
    public int round;
    public int totalGames;
    public int winGames;
    public int stage;
    public List<WaveGridSlotData> slots;
    public StatsContainer statsContainer;
    public List<int> selectedAugments;
}
