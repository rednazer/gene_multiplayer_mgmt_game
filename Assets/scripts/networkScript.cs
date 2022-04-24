using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class networkScript : NetworkBehaviour
{
    public NetworkVariable<bool> gameStart = new NetworkVariable<bool>();
    public NetworkVariable<bool> endTurn = new NetworkVariable<bool>();
    public NetworkVariable<int> turnNum = new NetworkVariable<int>();
    
    // Holds the contract info between players (I know this is a terrible way to do this)
    public NetworkVariable<int> contract1Remaining = new NetworkVariable<int>();
    public NetworkVariable<int> contract1Price = new NetworkVariable<int>();
    public NetworkVariable<int> contract1PetalColor = new NetworkVariable<int>();
    public NetworkVariable<int> contract1FlowerHeight = new NetworkVariable<int>();
    public NetworkVariable<int> contract1GrowSpeed = new NetworkVariable<int>();

    public NetworkVariable<int> contract2Remaining = new NetworkVariable<int>();
    public NetworkVariable<int> contract2Price = new NetworkVariable<int>();
    public NetworkVariable<int> contract2PetalColor = new NetworkVariable<int>();
    public NetworkVariable<int> contract2FlowerHeight = new NetworkVariable<int>();
    public NetworkVariable<int> contract2GrowSpeed = new NetworkVariable<int>();

    public NetworkVariable<int> contract3Remaining = new NetworkVariable<int>();
    public NetworkVariable<int> contract3Price = new NetworkVariable<int>();
    public NetworkVariable<int> contract3PetalColor = new NetworkVariable<int>();
    public NetworkVariable<int> contract3FlowerHeight = new NetworkVariable<int>();
    public NetworkVariable<int> contract3GrowSpeed = new NetworkVariable<int>();


    public override void OnNetworkSpawn() {
        gameStart.Value = false;
        endTurn.Value = false;
        if (IsOwnedByServer) {
            gameObject.name = "networkPlayerHost";
        } else {
            gameObject.name = "networkPlayerClient";
        }
    }

    public bool IsGameStarted() {
        return gameStart.Value;
    }

    public void TellStartGame() {
        if(NetworkManager.Singleton.IsServer) {
            gameStart.Value = true;
        } else {
            TellStartGameServerRpc();
        }
    }

    public void ReadyEndTurn() {
        if(NetworkManager.Singleton.IsServer) {
            endTurn.Value = true;
        } else {
            ReadyEndTurnServerRpc();
        }
    }

    public void EndTurn(int num) {
        turnNum.Value = num;
    }

    public void SetContractInfo(int contract, contractInfo info) {
        if (NetworkManager.Singleton.IsServer) {
            if (contract == 1) {
                contract1Remaining.Value = info.remaining;
                contract1Price.Value = info.price;
                contract1PetalColor.Value = info.petalColor;
                contract1FlowerHeight.Value = info.flowerHeight;
                contract1GrowSpeed.Value = info.growSpeed;
            } else if (contract == 2) {
                contract2Remaining.Value = info.remaining;
                contract2Price.Value = info.price;
                contract2PetalColor.Value = info.petalColor;
                contract2FlowerHeight.Value = info.flowerHeight;
                contract2GrowSpeed.Value = info.growSpeed;
            } else {
                contract3Remaining.Value = info.remaining;
                contract3Price.Value = info.price;
                contract3PetalColor.Value = info.petalColor;
                contract3FlowerHeight.Value = info.flowerHeight;
                contract3GrowSpeed.Value = info.growSpeed;
            }
        } else {
            SetContractInfoServerRpc(contract, info.remaining, info.price,info.petalColor, info.flowerHeight, info.growSpeed);
        }
    }

    public void BuyFlower(int contract) {
        if(NetworkManager.Singleton.IsServer) {
            if (contract == 1) {
                contract1Remaining.Value -= 1;
            } else if (contract == 2) {
                contract2Remaining.Value -= 1;
            } else {
                contract3Remaining.Value -= 1;
            }
        } else {
            BuyFlowerServerRpc(contract);
        }
    }

    // Starts game from main menu
    [ServerRpc]
    void TellStartGameServerRpc(ServerRpcParams rpcParams = default) {
        gameStart.Value = true;
    }

    // Readys up to end turn. Waits for other player to ready up
    [ServerRpc]
    void ReadyEndTurnServerRpc(ServerRpcParams rpcParams = default) {
        endTurn.Value = true;
    }

    [ServerRpc]
    void EndTurnServerRpc(ServerRpcParams rpcParams = default) {
        turnNum.Value += 1;
    }

    [ServerRpc]
    void BuyFlowerServerRpc(int contract, ServerRpcParams rpcParams = default) {
        if(contract == 1) {
            contract1Remaining.Value -= 1;
        } else if(contract == 2) {
            contract2Remaining.Value -= 1;
        } else {
            contract3Remaining.Value -= 1;
        }
    }

    [ServerRpc]
    public void SetContractInfoServerRpc(int contract, int remaining, int price, int petalColor, int flowerHeight, int growSpeed, ServerRpcParams rpcParams = default) {
        if (contract == 1) {
            contract1Remaining.Value = remaining;
            contract1Price.Value = price;
            contract1PetalColor.Value = petalColor;
            contract1FlowerHeight.Value = flowerHeight;
            contract1GrowSpeed.Value = growSpeed;
        } else if (contract == 2) {
            contract2Remaining.Value = remaining;
            contract2Price.Value = price;
            contract2PetalColor.Value = petalColor;
            contract2FlowerHeight.Value = flowerHeight;
            contract2GrowSpeed.Value = growSpeed;
        } else {
            contract3Remaining.Value = remaining;
            contract3Price.Value = price;
            contract3PetalColor.Value = petalColor;
            contract3FlowerHeight.Value = flowerHeight;
            contract3GrowSpeed.Value = growSpeed;
        }
    }
}
