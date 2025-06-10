using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPos : MonoBehaviour
{
    public float dx, dy, dz;

    //public GameObject targetObject;

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
            tempPos.x += dx;
            tempPos.y += dy;
            tempPos.z += dz;
            transform.position = tempPos;
        }
    }
}
