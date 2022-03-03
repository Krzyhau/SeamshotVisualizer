using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Plane2D : MonoBehaviour
{
    public Color entryColor;
    public Color exitColor;
    public Color notHitColor;
    public float colorInterpolation;
    public float labelScale = 1;
    public Shooter colorSwitchShooter;

    private LineRenderer line;

    private Transform intersectionPoint;
    private TextMesh intersectionText;

    // Start is called before the first frame update
    private void Start() {
        OnValidate();
    }
    void OnValidate()
    {
        line = GetComponent<LineRenderer>();
        intersectionPoint = transform.GetChild(0);
        intersectionText = intersectionPoint.GetChild(0).GetComponent<TextMesh>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!colorSwitchShooter) return;

        // i lined stuff up weirdly, so that's super unintuitive
        var planeNormal = -transform.right;

        // figure out what color of ray we should use.
        Color interpColor = notHitColor;
        bool intersected = false;
        (Ray2D ray, float rayLength) = colorSwitchShooter.GetShootingRay();


        Vector2 p1 = ray.origin;
        Vector2 p2 = ray.origin + ray.direction * rayLength;
        float dist = Vector2.Dot(transform.position, planeNormal);

        float d1 = Vector2.Dot(p1, planeNormal) - dist;
        float d2 = Vector2.Dot(p2, planeNormal) - dist;

        if (d1 < 0 && d2 > 0) {
            interpColor = exitColor;
            intersected = true;
        }
        else if(d1 > 0 && d2 < 0) {
            interpColor = entryColor;
            intersected = true;
        }

        line.startColor = Color.Lerp(line.startColor, interpColor, colorInterpolation);
        line.endColor = line.startColor;

        if (intersected) {
            if (!intersectionPoint.gameObject.activeSelf) intersectionPoint.gameObject.SetActive(true);

            float fraction = d1 / (d1 - d2);

            Vector3 hitPoint = p1 + (p2 - p1) * fraction;
            hitPoint.z = transform.position.z-0.1f;
            intersectionPoint.position = hitPoint;
            intersectionPoint.rotation = colorSwitchShooter.transform.rotation;

            float textScale = Camera.main.orthographicSize * 0.02f * labelScale;
            Vector3 intersectionOffset = Vector3.Scale(planeNormal, new Vector3(12, 6, 0)) * textScale;
            intersectionText.transform.position = intersectionPoint.transform.position + intersectionOffset;
            intersectionText.transform.rotation = Quaternion.identity;
            intersectionText.transform.localScale = new Vector3(textScale, textScale, textScale);
            intersectionText.text = string.Format("{0:0.####0}",fraction).Replace(",", ".");
        } else {
            if (intersectionPoint.gameObject.activeSelf) intersectionPoint.gameObject.SetActive(false);
        }
    }
}
