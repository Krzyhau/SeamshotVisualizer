using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterController : MonoBehaviour
{
    public float moveSpeed;
    public float acceleration;
    public float groundFriction;
    public float stopSpeed;

    private Rigidbody2D rigid;
    private Vector2 wishDir;

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        ParseInputs();
    }

    void FixedUpdate() {
        ProcessMovement();
        RotateToMouse();
    }


    private float GetKeyValue(KeyCode key) {
        if (Input.GetKeyDown(key)) {
            return 0.5f;
        } else if (Input.GetKey(key)) {
            return 1.0f;
        }
        return 0.0f;
    }

    void ParseInputs() {
        wishDir = Vector2.zero;

        wishDir.y += GetKeyValue(KeyCode.W);
        wishDir.y -= GetKeyValue(KeyCode.S);

        wishDir.x -= GetKeyValue(KeyCode.A);
        wishDir.x += GetKeyValue(KeyCode.D);
        if (wishDir.magnitude > 1) wishDir.Normalize();
    }

    // rotates the shooter so it aims towards the mouse position
    void RotateToMouse() {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var relPos = mousePos - transform.position;
        relPos.z = 0;
        var ang = Vector3.SignedAngle(Vector3.up, relPos, Vector3.forward);
        transform.eulerAngles = new Vector3(0, 0, ang);
    }


    // source-like movement. no idea why i went so far as implementing it so accurately but whatever
    void ProcessMovement() {
        Vector2 vel = rigid.velocity;

        // friction
        float friction = Time.fixedDeltaTime * groundFriction;
        if (vel.magnitude > stopSpeed) {
            vel = vel * (1.0f - friction);
        } else if (vel.magnitude >= friction * stopSpeed) {
            vel -= vel.normalized * stopSpeed * friction;
        } else vel.Set(0, 0);

        // movement
        Vector2 inputs = wishDir * moveSpeed;

        if (Input.GetKey(KeyCode.LeftControl)) inputs *= 0.333f;

        float maxSpeed = inputs.magnitude;
        float maxAccel = Time.fixedDeltaTime * maxSpeed * acceleration;

        float accelDiff = maxSpeed - Vector2.Dot(vel, inputs.normalized);
        if (accelDiff > 0) {
            float accelForce = Mathf.Min(maxAccel, accelDiff);
            vel += inputs.normalized * accelForce;
        }

        rigid.velocity = vel;
    }
}
