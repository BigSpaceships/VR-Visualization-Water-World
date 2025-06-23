using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkiTrans : MonoBehaviour
{

    public float dy;


    public void SetToTargetPosition(GameObject targetObject) {
        if (targetObject != null) {
            Vector3 tempPos = targetObject.transform.position;
            tempPos.y += dy;
            transform.position = tempPos;
        }
    }

}
