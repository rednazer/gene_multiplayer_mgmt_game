using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class contractInfo : MonoBehaviour
{
    // Contract number for network things
    public int number;

    // Shop for buying seeds
    public bool buySeed;

    // Number of times the shop can be used again. If -1, then is can be used infinite times
    public int remaining;

    // Price to sell the seed at
    public int price;

    // For the shop, determined the type of item that will be sold
    // petalcolor & growspeed are 0-2. flowerheight is 0 or 1
    public int petalColor;
    public int flowerHeight;
    public int growSpeed;
}
