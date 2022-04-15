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

    public static (int, bool, int, bool) getItemInfo(Transform item, int trait) {
        (int, bool, int, bool) vals;
        if (trait == 0) {
            vals = item.GetComponent<itemInfo>().quantity;
        } else if (trait == 1) {
            vals = item.GetComponent<itemInfo>().growRate;
        } else {
            vals = item.GetComponent<itemInfo>().resistance;
        }
        return vals;
    }

    public static string getItemsTable(int vals0, bool valsDom0, int vals1, bool valsDom1, int trait) {
        string retString;
        if (trait == 0) {
            if (valsDom0 && valsDom1) {
                retString = "QQ (" + ((vals0 + vals1) / 2) + ")";
            } else if (valsDom0) {
                retString = "Qq (" + vals0 + ")";
            } else if (valsDom1) {
                retString = "Qq (" + vals1 + ")";
            } else { // valsDom0 != true && valsDom1 != true
                retString = "qq (" + ((vals0 + vals1) / 2) + ")";
            }
        } else if (trait == 1) {
            if (valsDom0 && valsDom1) {
                retString = "GG (" + ((vals0 + vals1) / 2) + ")";
            } else if (valsDom0) {
                retString = "Gg (" + vals0 + ")";
            } else if (valsDom1) {
                retString = "Gg (" + vals1 + ")";
            } else { // valsDom0 != true && valsDom1 != true
                retString = "gg (" + ((vals0 + vals1) / 2) + ")";
            }
        } else {
            if (valsDom0 && valsDom1) {
                retString = "RR (" + ((vals0 + vals1) / 2) + ")";
            } else if (valsDom0) {
                retString = "Rr (" + vals0 + ")";
            } else if (valsDom1) {
                retString = "Rr (" + vals1 + ")";
            } else { // valsDom0 != true && valsDom1 != true
                retString = "rr (" + ((vals0 + vals1) / 2) + ")";
            }
        }
        return retString;
    }

    public static (string, string) getReadableItemFormat((int, bool, int, bool) vals, int trait) {
        // True on the bools represents a dominant gene
        string str0;
        string str1;
        if (trait == 0) {
            if (vals.Item2) { str0 = "Q (" + vals.Item1 + ")"; } else { str0 = "q (" + vals.Item1 + ")"; }
            if (vals.Item4) { str1 = "Q (" + vals.Item3 + ")"; } else { str1 = "q (" + vals.Item3 + ")"; }
        } else if (trait == 1) {
            if (vals.Item2) { str0 = "G (" + vals.Item1 + ")"; } else { str0 = "g (" + vals.Item1 + ")"; }
            if (vals.Item4) { str1 = "G (" + vals.Item3 + ")"; } else { str1 = "g (" + vals.Item3 + ")"; }
        } else {
            if (vals.Item2) { str0 = "R (" + vals.Item1 + ")"; } else { str0 = "r (" + vals.Item1 + ")"; }
            if (vals.Item4) { str1 = "R (" + vals.Item3 + ")"; } else { str1 = "r (" + vals.Item3 + ")"; }
        }
        return (str0, str1);
    }
}
