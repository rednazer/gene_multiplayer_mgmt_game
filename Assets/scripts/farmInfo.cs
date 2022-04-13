using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class farmInfo : MonoBehaviour
{
    public bool plotOwned;
    public bool hasGreenhouse;
    // decreases by 1 every turn. If -1, nothing is planted
    public int growTime;


    // Calculates if plants die based on resistance
    // season: 0 = winter, 1 = spring, 2 = summer, 3 = fall
    public bool weatherEffect(int season) {

        
        return true;
    }
}
