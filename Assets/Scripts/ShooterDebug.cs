using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class ShooterDebug : MonoBehaviour
{
    private Text infoHud;

    [SerializeField] private Shooter shooter;

    public bool showEpsilon;
    public bool showAngles;
    public bool showPositionX;
    public float positionXDelta;
    private void Start() {
        OnValidate();
    }
    void OnValidate()
    {
        infoHud = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!shooter) return;


        string output = "";
        if (showEpsilon) {
            output += string.Format("Distance Epsilon: {0:0.#####0}\n", shooter.distanceEpsilon);
        }
        if (showAngles) {
            var ang = shooter.transform.eulerAngles.z;
            if (ang > 180) ang -= 360.0f;
            output += string.Format("Angle: {0:0.#####0}\n", ang);
        }
        if (showPositionX) {
            var pos = shooter.transform.position.x - positionXDelta;
            output += string.Format("Position X: {0:0.#####0}\n", pos);
        }

        infoHud.text = output.Replace(",", ".");
    }
}
