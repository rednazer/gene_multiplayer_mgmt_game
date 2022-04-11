using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cursorFollower : MonoBehaviour
{
    Vector3 lastMousePos;
    
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, 255);
        lastMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        gameObject.transform.position = lastMousePos + Vector3.forward*10;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector3 movementVector = mousePos - lastMousePos;

        gameObject.transform.position += movementVector;
        lastMousePos = mousePos;
    }
}
