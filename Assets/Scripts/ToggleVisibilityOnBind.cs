using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleVisibilityOnBind : MonoBehaviour
{

    public KeyCode bind;

    private CanvasGroup group;

    void Start() {
        group = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        if (Input.GetKeyDown(bind)) {
            if (group.alpha > 0) {
                group.alpha = 0;
            } else {
                group.alpha = 1;
            }
        }
    }
}
