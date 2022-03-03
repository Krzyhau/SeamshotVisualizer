using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtremelyAdvancedObjectRotator : MonoBehaviour
{
    public float speed;
    void Update()
    {
        transform.rotation = Quaternion.Euler(0, 0, transform.eulerAngles.z + speed * Time.deltaTime);
    }
}
