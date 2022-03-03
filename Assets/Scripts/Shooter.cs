using System.Collections;
using UnityEngine;

[ExecuteAlways]
public class Shooter : MonoBehaviour
{
    public float distanceEpsilon = 0.03125f;
    public Transform aimPoint;


    [SerializeField] private Transform hitPointObject;

    public bool collisionDetectionOnly;
    public bool allowDumbBindForEpsilonToggle;

    void Update() {
        // aimbot lol
        if (aimPoint) {
            transform.rotation = Quaternion.LookRotation(Vector3.forward, (aimPoint.transform.position - transform.position).normalized);
        }

        // update hitpoint visuals
        (Ray2D ray, float rayLength) = GetShootingRay();

        var brushes = GameObject.FindGameObjectsWithTag("Brush");

        float minFactor = ray.direction.magnitude;
        bool hit = false;

        foreach (GameObject brush in brushes) {
            float factor = brush.GetComponent<Brush2D>().IntersectRay(this, ray, rayLength);
            if (factor < minFactor) {
                hit = true;
                minFactor = factor;
            }
        }

        Vector2 hitPoint = ray.origin + ray.direction * Mathf.Max(0,minFactor) * rayLength;

        Vector2 lineEnd = collisionDetectionOnly ? (ray.origin + ray.direction * rayLength) : hitPoint;

        var lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.SetPositions(new Vector3[] { ray.origin, lineEnd });

        var color = (hit && collisionDetectionOnly) ? new Color(1.0f, 0.5f, 0.5f) : Color.white;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        hitPointObject.position = new Vector3(hitPoint.x, hitPoint.y, hitPointObject.position.z);

        if (allowDumbBindForEpsilonToggle && Input.GetKeyDown(KeyCode.E)) {
            if(distanceEpsilon != 0.03125f) {
                distanceEpsilon = 0.03125f;
            } else {
                distanceEpsilon = 4.0f;
            }
        }
    }

    public (Ray2D,float) GetShootingRay() {
        Ray2D ray = new Ray2D(transform.position, transform.up);
        const float RAY_LENGTH = 56755.84f; // 2^31 * sqrt(3)
        return (ray, RAY_LENGTH);
    }
}
