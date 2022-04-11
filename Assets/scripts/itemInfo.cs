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
    public (int, bool, int, bool) quantity; // Higher = more once grown
    public (int, bool, int, bool) growRate; // Higher = faster grow rate (something like 5 - score)?
    public (int, bool, int, bool) resistance; // Higher = better resist

    public bool createNewSeed(GameObject p0, GameObject p1) {
        itemInfo info0 = p0.GetComponent<itemInfo>();
        itemInfo info1 = p1.GetComponent<itemInfo>();
        generation = Mathf.RoundToInt(Mathf.Max(info0.generation, info1.generation));

        // Sets values
        return setAllValues(info0, info1);
    }

    private bool setAllValues(itemInfo info0, itemInfo info1) {
        bool qMut, gMut, rMut;
        // Set quantity
        (quantity.Item1, quantity.Item2, quantity.Item3, quantity.Item4, qMut) = getChildValue(info0.quantity, info1.quantity);
        // Set Grow rate
        (growRate.Item1, growRate.Item2, growRate.Item3, growRate.Item4, gMut) = getChildValue(info0.growRate, info1.growRate);
        // Set Resistance
        (resistance.Item1, resistance.Item2, resistance.Item3, resistance.Item4, rMut) = getChildValue(info0.resistance, info1.resistance);

        // Cap grow rate at 1 turn
        if(growRate.Item1 >= 5) {
            growRate.Item1 = 4;
        }
        if(growRate.Item3 >= 5) {
            growRate.Item3 = 4;
        }

        return (qMut || gMut || rMut);
    }

    // Takes in parents' seeds' info and gives back the components to use
    // Also has chance to mutate
    private (int, bool, int, bool, bool) getChildValue((int, bool, int, bool) info0, (int, bool, int, bool) info1) {
        int RNG = Mathf.FloorToInt(Random.Range(0, 4));
        (int, bool, int, bool, bool) returnInfo;
        // Decides parent info to use
        if (RNG == 0) {
            returnInfo.Item1 = info0.Item1;
            returnInfo.Item2 = info0.Item2;
            returnInfo.Item3 = info1.Item1;
            returnInfo.Item4 = info1.Item2;
        } else if(RNG == 1) {
            returnInfo.Item1 = info0.Item1;
            returnInfo.Item2 = info0.Item2;
            returnInfo.Item3 = info1.Item3;
            returnInfo.Item4 = info1.Item4;
        } else if(RNG == 2) {
            returnInfo.Item1 = info0.Item3;
            returnInfo.Item2 = info0.Item4;
            returnInfo.Item3 = info1.Item1;
            returnInfo.Item4 = info1.Item2;
        } else {
            returnInfo.Item1 = info0.Item3;
            returnInfo.Item2 = info0.Item4;
            returnInfo.Item3 = info1.Item3;
            returnInfo.Item4 = info1.Item4;
        }

        // We want around 50% overall, so .2^3 ~ .5
        // There are 6 ways to mutate (I will have bias towards good mutations)
        // int0: up: 0-1, down: 2
        // int1: up 3-4, down: 5
        // dom0: 6
        // dom1: 7
        // No mutation: 8-39 (around 5x the likelyhood of mutation)
        returnInfo.Item5 = true;
        int mutationRNG = Mathf.FloorToInt(Random.Range(0, 40));
        if(mutationRNG == 0 || mutationRNG == 1) {
            returnInfo.Item1 += 1;
        } else if(mutationRNG == 2) {
            if (returnInfo.Item1 > 1) {
                returnInfo.Item1 -= 1;
            }
        } else if(mutationRNG == 3 || mutationRNG == 4) {
            returnInfo.Item3 += 1;
        } else if(mutationRNG == 5) {
            if (returnInfo.Item3 > 1) {
                returnInfo.Item3 -= 1;
            }
        } else if(mutationRNG == 6) {
            returnInfo.Item2 = !returnInfo.Item2;
        } else if(mutationRNG == 7) {
            returnInfo.Item4 = !returnInfo.Item4;
        } else {
            returnInfo.Item5 = false;
        }

        return returnInfo;
    }


    public void setResistance(int p1, bool dom1, int p2, bool dom2) {
        resistance = (p1, dom1, p2, dom2);
    }

    public void setGrowthRate(int p1, bool dom1, int p2, bool dom2) {
        growRate = (p1, dom1, p2, dom2);
    }

    public void setQuantity(int p1, bool dom1, int p2, bool dom2) {
        quantity = (p1, dom1, p2, dom2);
    }

    public (int, int, int) getValues() {
        int resVal = 0;
        int growVal = 0;
        int quantityVal = 0;

        // For resistance
        if(resistance.Item2 == resistance.Item4) {
            resVal = (resistance.Item1 + resistance.Item3) / 2;
        } else if(resistance.Item2) {
            resVal = resistance.Item1;
        } else { // resistance.Item4
            resVal = resistance.Item3;
        }

        // For grow rate
        if (growRate.Item2 == growRate.Item4) {
            growVal = (growRate.Item1 + growRate.Item3) / 2;
        } else if (growRate.Item2) {
            growVal = growRate.Item1;
        } else { // resistance.Item4
            growVal = growRate.Item3;
        }

        // For quantity
        if (quantity.Item2 == quantity.Item4) {
            quantityVal = (quantity.Item1 + quantity.Item3) / 2;
        } else if (quantity.Item2) {
            quantityVal = quantity.Item1;
        } else { // resistance.Item4
            quantityVal = quantity.Item3;
        }

        // Returns gotten vals
        return (resVal, growVal, quantityVal);
    }

}
