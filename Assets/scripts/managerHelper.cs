using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class managerHelper : MonoBehaviour
{
    public static void layoutSetup(GameObject obj, int left, int right, int top, int bottom) {
        obj.GetComponent<RectTransform>().offsetMin = new Vector2(left, bottom);
        obj.GetComponent<RectTransform>().offsetMax = new Vector2(-right, -top);
        obj.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
    }

    public static GameObject setupButton(GameObject objPrefab, GameObject parent) {
        GameObject obj = Instantiate(objPrefab, objPrefab.transform.position, objPrefab.transform.rotation);
        obj.transform.SetParent(parent.transform);
        obj.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        return obj;
    }
    
    public static GameObject setupItem(GameObject objPrefab, GameObject parent) {
        GameObject obj = Instantiate(objPrefab, objPrefab.transform.position, objPrefab.transform.rotation);
        obj.transform.SetParent(parent.transform);
        obj.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        obj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
        return obj;
    }

    public static GameObject setupText(GameObject objPrefab, GameObject parent, string text) {
        GameObject obj = Instantiate(objPrefab, objPrefab.transform.position, objPrefab.transform.rotation);
        obj.transform.SetParent(parent.transform);
        obj.GetComponent<TextMeshProUGUI>().SetText(text);
        obj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        obj.GetComponent<RectTransform>().sizeDelta = parent.transform.parent.GetComponent<GridLayoutGroup>().cellSize;
        obj.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        obj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
        return obj;
    }

    public static GameObject createPopup(GameObject objPrefab, GameObject parent, GameObject textPrefab, string text) {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        GameObject obj = Instantiate(objPrefab, objPrefab.transform.position, objPrefab.transform.rotation);
        obj.transform.SetParent(parent.transform);
        obj.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        obj.GetComponent<RectTransform>().position = new Vector3(mousePos.x, mousePos.y, 0);
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 154);
        obj.GetComponent<Image>().color = new Color(255, 255, 255, 230);

        GameObject textObj = Instantiate(textPrefab, textPrefab.transform.position, textPrefab.transform.rotation);
        textObj.transform.SetParent(obj.transform);
        textObj.GetComponent<TextMeshProUGUI>().SetText(text);
        textObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        textObj.GetComponent<RectTransform>().sizeDelta = obj.GetComponent<RectTransform>().sizeDelta;
        textObj.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        textObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
        return obj;
    }

    public static GameObject createPrintText(GameObject objPrefab, GameObject parent, string text) {
        GameObject obj = Instantiate(objPrefab, objPrefab.transform.position, objPrefab.transform.rotation);
        obj.transform.SetParent(parent.transform, true);
        obj.GetComponent<TextMeshProUGUI>().SetText(text);
        obj.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        return obj;
    }

    public static (int, int) getItemInfo(Transform item, int trait) {
        (int, int) vals;
        if (trait == 0) {
            vals = item.GetComponent<itemInfo>().petalColor;
        } else if (trait == 1) {
            vals = item.GetComponent<itemInfo>().flowerHeight;
        } else {
            vals = item.GetComponent<itemInfo>().growQuality;
        }
        return vals;
    }

    public static string getItemsTable(int vals0, int vals1, int trait) {
        string retString;
        if (trait == 0) {
            if (vals0==0 && vals1==0) {
                retString = "RR (red)";
            } else if (vals0 == 0 || vals1 == 0) {
                retString = "Rr (pink)";
            } else { // valsDom0 != true && valsDom1 != true
                retString = "rr (white)";
            }
        } else if (trait == 1) {
            if (vals0 == 0 && vals1 == 0) {
                retString = "TT (tall)";
            } else if (vals0 == 0 || vals1 == 0) {
                retString = "Tt (tall)";
            } else { // vals0 != 0 && vals1 != 0
                retString = "tt (short)";
            }
        } else {
            if (vals0 == 0 && vals1 == 0) {
                retString = "QQ (growth)"; // fast growth
            } else if (vals0 == 0 || vals1 == 0) {
                retString = "Qq (mixed)"; // mixed yield/growth
            } else { // valsDom0 != true && valsDom1 != true
                retString = "qq (yield)"; // high yield
            }
        }
        return retString;
    }

    public static (string, string) getReadableItemFormat((int, int) vals, int trait) {
        // True on the bools represents a dominant gene
        string str0;
        string str1;
        if (trait == 0) {
            if (vals.Item1==0) { str0 = "R"; } else { str0 = "r"; }
            if (vals.Item2==0) { str1 = "R"; } else { str1 = "r"; }
        } else if (trait == 1) {
            if (vals.Item1==0) { str0 = "T"; } else { str0 = "t"; }
            if (vals.Item2==0) { str1 = "T"; } else { str1 = "t"; }
        } else {
            if (vals.Item1==0) { str0 = "Q"; } else { str0 = "q"; }
            if (vals.Item2==0) { str1 = "Q"; } else { str1 = "q"; }
        }
        return (str0, str1);
    }
}
