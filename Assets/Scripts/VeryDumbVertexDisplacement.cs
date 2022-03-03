using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class VeryDumbVertexDisplacement : MonoBehaviour
{
    public int vectorID;
    public Vector2 vectorPos;

    private int vectorIDBuf = -1;

    void Update()
    {
        var col = GetComponent<PolygonCollider2D>();

        var path = col.GetPath(0);

        if(vectorID != vectorIDBuf) {
            vectorID = Mathf.Clamp(vectorID, 0, path.Length-1);
            vectorIDBuf = vectorID;
            vectorPos = path[vectorID];
        }

        path[vectorID] = vectorPos;
        col.SetPath(0, path);
    }
}
