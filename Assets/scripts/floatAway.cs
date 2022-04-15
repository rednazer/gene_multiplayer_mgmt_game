using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class floatAway : MonoBehaviour
{
    float timeToLive;
    [SerializeField] byte alpha;

    // Start is called before the first frame update
    void Start()
    {
        timeToLive = 3f;
        alpha = 255;
        //gameObject.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition); ;
    }

    private void FixedUpdate() {
        Color32 lastColor = gameObject.GetComponent<TextMeshProUGUI>().color;
        gameObject.GetComponent<TextMeshProUGUI>().color = new Color32(lastColor.r, lastColor.g, lastColor.b, alpha);
        if (alpha > 4) {
           alpha -= 4;
        }
        gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.up;
        timeToLive -= Time.fixedDeltaTime;
        if (timeToLive < 0) {
            Destroy(gameObject);
        }
    }

}
