using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class itemInfo : MonoBehaviour
{
    // # of creations it took to get here
    public int generation;

    // These are used when deciding how to breed and how useful they are
    // Trait1 and Trait2 are the two different traits gotten from parents
    // (Trait1, T1 dominant?, Trait2, T2 dominant?)
    //public (int, bool, int, bool) quantity; // Higher = more once grown
    //public (int, bool, int, bool) growRate; // Higher = faster grow rate (something like 5 - score)?
    //public (int, bool, int, bool) resistance; // Higher = better resist

    // New Traits
    // colors:  0: R - red - codominant w/ white
    //          1: r - white - codominant w/ red
    //          (if one parent red and other white, makes pink color)
    // height:  0: T - tall - dominant to short flower
    //          1: t - short - recessive to tall flowers
    // growQual 0: Q - fast grow speed (codiminant with high quantity)
    //          1: q - high quantity (codiminant)
    public (int, int) petalColor;
    public (int, int) flowerHeight;
    public (int, int) growQuality;

    public void createNewSeed(GameObject p0, GameObject p1) {
        itemInfo info0 = p0.GetComponent<itemInfo>();
        itemInfo info1 = p1.GetComponent<itemInfo>();
        generation = Mathf.RoundToInt(Mathf.Max(info0.generation, info1.generation)) + 1;

        // Sets values
        setAllValues(info0, info1);
    }

    private void setAllValues(itemInfo info0, itemInfo info1) {
        // Set petal color
        petalColor = getChildValue(info0.petalColor, info1.petalColor);

        // Set height
        flowerHeight = getChildValue(info0.flowerHeight, info1.flowerHeight);
        
        // Set the type of growth
        growQuality = getChildValue(info0.growQuality, info1.growQuality);
    }

    static int zeros = 0;
    static int ones = 0;
    static int twos = 0;
    static int threes = 0;

    // Takes in parents' seeds' info and gives back the components to use
    // Also has chance to mutate
    private (int, int) getChildValue((int, int) info0, (int, int) info1) {
        // Gets a random number from 0-3 to choose parent traits
        int RNG = Mathf.FloorToInt(Random.Range(0, 4));
        (int, int) returnInfo;
        
        // Decides parent info to use
        if (RNG == 0) { // 00
            returnInfo.Item1 = info0.Item1;
            returnInfo.Item2 = info1.Item1;
            zeros++;
        } else if(RNG == 1) { // 01
            returnInfo.Item1 = info0.Item1;
            returnInfo.Item2 = info1.Item2;
            ones++;
        } else if(RNG == 2) { // 10
            returnInfo.Item1 = info0.Item2;
            returnInfo.Item2 = info1.Item1;
            twos++;
        } else { // 11
            returnInfo.Item1 = info0.Item2;
            returnInfo.Item2 = info1.Item2;
            threes++;
        }

        // Testing to make sure I'm not doing dumb things
        // Debug.Log("0:"+zeros+" 1:"+ones+" 2:"+twos+" 3:"+threes);

        return returnInfo;
    }

    // Grow speeds: fast = 1 turn, mid = 2 turn, slow = 3 turn
    public int growSpeed() {
        if (growQuality.Item1 == 0 && growQuality.Item2 == 0) {
            return 1;
        } else if (growQuality.Item1 == 0 || growQuality.Item2 == 0) {
            return 2;
        } else {
            return 3;
        }
    }

    // Grow quantities: fast = 1, mid = 3, slow = 5
    public int growQuantity() {
        if (growQuality.Item1 == 0 && growQuality.Item2 == 0) { // fast growth low yield
            return 1;
        } else if (growQuality.Item1 == 0 || growQuality.Item2 == 0) {
            return 3;
        } else { // slow growth high yield
            return 5;
        }
    }

    public (int,int,int) getValues() {
        (int,int,int) values;
        //0=red,1=pink,2=white
        values.Item1 = (petalColor.Item1 == 0 && petalColor.Item2 == 0) ? (0) : ((petalColor.Item1 == 0 || petalColor.Item2 == 0) ? (1) : (2));
        //0=tall,1=short
        values.Item2 = (flowerHeight.Item1 == 0 || flowerHeight.Item2 == 0) ? (0) : (1);
        //0=fast,1=mixed speed/yield,2=high yield
        values.Item3 = (growQuality.Item1 == 0 && growQuality.Item2 == 0) ? (0) : ((growQuality.Item1 == 0 || growQuality.Item2 == 0) ? (1) : (2));

        return values;
    }

    public (string, string, string) getStrings() {
        string color;
        string height;
        string growSpeed;

        // If both parents are red
        if(petalColor.Item1 == 0 && petalColor.Item2 == 0) {
            color = "red (RR)";
        // If 1 parent is red
        } else if(petalColor.Item1 == 0 || petalColor.Item2 == 0) {
            color = "pink (Rr)";
        } else { // if no parents are red
            color = "white (rr)";
        }

        // If 1 or more parents are tall, then flower is tall
        if (flowerHeight.Item1 == 0 && flowerHeight.Item2 == 0) {
            height = "tall (TT)";
        } else if (flowerHeight.Item1 == 0 || flowerHeight.Item2 == 0) {
            height = "tall (Tt)";
        } else {
            height = "short (tt)";
        }

        if(growQuality.Item1 == 0 && growQuality.Item2 == 0) {
            growSpeed = "fast growth (QQ)";
        } else if(growQuality.Item1 == 0 || growQuality.Item2 == 0) {
            growSpeed = "mixed yield/growth (Qq)";
        } else {
            growSpeed = "high yield (qq)";
        }

        return (color, height, growSpeed);
    }

}
