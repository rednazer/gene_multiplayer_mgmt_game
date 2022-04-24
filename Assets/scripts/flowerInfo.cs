using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flowerInfo : MonoBehaviour
{
    // Info gotten from the parent seed
    // petalColor: 0 = red, 1 = pink, 2 = white
    // flowerHeight: 0 = tall, 1 = short
    // growSpeed: 0 = fast, 1 = mixed, 2 = high quantity
    public int petalColor;
    public int flowerHeight;
    public int growSpeed;

    // Var about how many in the stack remaining (starting about depends on the parent seed's growQuality)
    // Can be anywhere from 1-5. Once reaches 0, gets destroyed
    public int numRemaining;

    public string itemInfo() {
        string textInfo = "A ";

        if(flowerHeight == 0) {
            textInfo += "tall ";
        } else {
            textInfo += "short ";
        }

        if (petalColor == 0) {
            textInfo += "red ";
        } else if (petalColor == 1) {
            textInfo += "pink ";
        } else {
            textInfo += "white ";
        }

        if(growSpeed == 0) {
            textInfo += "fast growing ";
        } else if(growSpeed == 1) {
            textInfo += "mixed yield/growth ";
        } else {
            textInfo += "high yielding ";
        }
        textInfo += "flower.\nRemaining: " + numRemaining;
        
        return textInfo;
    }
}
