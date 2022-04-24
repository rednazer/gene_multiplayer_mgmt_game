using System;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class Report
{
    public string version = "0.0.6";
    public string datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    public double epoch = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    public string eventKey;
    public string eventValue;
    public string userId;
    public int seedsCrafted;
    public (int,int,int) seedTraits;
    public int turn;
    public int farms;
    public int currentMoney;
    public Report(string userId, string eventKey, string eventValue)
    {
        this.userId = userId;
        this.eventKey = eventKey;
        this.eventValue = eventValue;
    }
}

public static class Analytics
{
    private const string path = "userLog.json"; // Changd the log location for the build "Assets/Logs/userLog.json";
    private static readonly StreamWriter Writer = new StreamWriter(path, true);

    // Report seed value when craft occurs
    public static void ReportCraft(string userId, (int, int, int)values, int seedCount, int pTurn, int money) {
        var report = new Report(userId, "CRAFT", "PlayerEvent") {
            seedTraits = values,
            seedsCrafted = seedCount,
            turn = pTurn,
            currentMoney = money
        };
        Writer.WriteLine(JsonUtility.ToJson(report));
        Writer.Flush();
    }

    // Reports Max state
    public static void ReportPlayerState(string userId, int seedCount, int pTurn, int pMoney, int pFarms) {
        var report = new Report(userId, "STATE", "KeyFrame") {
            seedsCrafted = seedCount,
            turn = pTurn,
            currentMoney = pMoney,
            farms = pFarms
        };
        Writer.WriteLine(JsonUtility.ToJson(report));
        Writer.Flush();
    }


    /*
    public static void ReportEvent(string userId, string eventKey, string eventValue)
    {
        Writer.WriteLine(JsonUtility.ToJson(new Report(userId, eventKey, eventValue)));
        Writer.Flush();
    }
    
    public static void ReportMove(string userId, Vector3Int pDelta, Vector3Int[] pBoard, Vector3Int[] pPiece)
    {
        var report = new Report(userId, "MOVE", "PlayerEvent")
        {
            delta = pDelta,
            board = pBoard,
            piece = pPiece
        };
        Writer.WriteLine(JsonUtility.ToJson(report));
        Writer.Flush();
    }
    
    public static void ReportRotate(string userId, Vector3Int[] pBoard, Vector3Int[] pPiece)
    {
        var report = new Report(userId, "ROTATE", "PlayerEvent")
        {
            board = pBoard,
            piece = pPiece
        };
        Writer.WriteLine(JsonUtility.ToJson(report));
        Writer.Flush();
    }
    
    public static void ReportState(string userId, Vector3Int[] pBoard, Vector3Int[] pPiece)
    {
        var report = new Report(userId, "STATE", "KeyFrame")
        {
            board = pBoard,
            piece = pPiece
        };
        Writer.WriteLine(JsonUtility.ToJson(report));
        Writer.Flush();
    }
    */

    public static void Close() => Writer.Close();
}
    

