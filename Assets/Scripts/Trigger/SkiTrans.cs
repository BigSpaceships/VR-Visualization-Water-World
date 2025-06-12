using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkiTrans : MonoBehaviour
{

    public float dy;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetToTargetPosition(GameObject targetObject) {
        if (targetObject != null) {
            Vector3 tempPos = targetObject.transform.position;
            tempPos.y += dy;
            transform.position = tempPos;
        }
    }

}
