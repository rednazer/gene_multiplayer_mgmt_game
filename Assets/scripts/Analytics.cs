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
    public int bestSeedValue;
    public int seedValue;
    public int turn;
    public int farms;
    public int money;
    public Report(string userId, string eventKey, string eventValue)
    {
        this.userId = userId;
        this.eventKey = eventKey;
        this.eventValue = eventValue;
    }
}

public static class Analytics
{
    private const string path = "Assets/Logs/userLog.json";
    private static readonly StreamWriter Writer = new(path, true);

    // Report seed value when craft occurs
    public static void ReportCraft(string userId, (int, int, int)value, int pTurn, int money) {
        var report = new Report(userId, "CRAFT", "PlayerEvent") {
            seedValue = value.Item1 + value.Item2 + value.Item3,
            turn = pTurn
        };
        Writer.WriteLine(JsonUtility.ToJson(report));
        Writer.Flush();
    }

    // Reports Max state
    public static void ReportPlayerState(string userId, int maxValue, int pTurn, int pMoney, int pFarms) {
        var report = new Report(userId, "STATE", "KeyFrame") {
            bestSeedValue = maxValue,
            turn = pTurn,
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
    

