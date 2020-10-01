using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Yamato : MonoBehaviour
{
    private LayerMask probeMask = -1;

    [SerializeField, Range(0f, 100f)] private float maxSpeed = 10f;

    [SerializeField, Range(0f, 100f)] private float maxAcceleration = 10f;

    [SerializeField, Range(0f, 100f)] private float maxDeacceleration = 1f;

    [SerializeField, Range(0f, 100f)] private float maxAirAcceleration = 1f;

    [SerializeField, Range(0f, 90f)] private float maxGroundAngle = 25f;

    [SerializeField, Range(0f, 10f)] private float jumpHeight = 2f;

    [SerializeField, Range(0, 5)] private int maxAirJumps = 0;

    [SerializeField, Range(0f, 100f)] private float maxSnapSpeed = 100f;

    [SerializeField, Min(0f)] private float probeDistance = 1f;

    [SerializeField] private Transform playerInputSpace = default;
    
    [SerializeField, Range(0f, 100f)] private float baseSpeed = 20f;
    
    private int jumpPhase;

    private int groundContactCount, steepContactCount;

    private Rigidbody body;

    private Vector3 velocity, desiredVelocity;

    private bool desiredJump;

    private float minGroundDotProduct;

    private int stepsSinceLastGrounded, stepsSinceLastJump;

    private bool OnGround => groundContactCount > 0;
    private bool OnSteep => steepContactCount > 0;

    private Vector3 contactNormal, steepNormal;

    private void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        OnValidate();
    }

    private void UpdateState()
    {
        stepsSinceLastGrounded += 1;
        stepsSinceLastJump += 1;
        velocity = body.velocity;
        if (OnGround || SnapToGround() || CheckSteepContacts())
        {
            stepsSinceLastGrounded = 0;
            if (stepsSinceLastJump > 1)
            {
                jumpPhase = 0;                
            }
            if (groundContactCount > 1)
            {
                contactNormal.Normalize();
            }
        }
        else
        {
            contactNormal = Vector3.up;
        }
    }

    private bool SnapToGround()
    {
        if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2)
        {
            return false;
        }

        float speed = velocity.magnitude;
        if (speed > maxSnapSpeed)
        {
            return false;
        }

        if (!Physics.Raycast(body.position, Vector3.down, out RaycastHit hit, probeDistance, probeMask))
        {
            return false;
        }

        if (hit.normal.y < minGroundDotProduct)
        {
            return false;
        }

        groundContactCount = 1;
        contactNormal = hit.normal;
        float dot = Vector3.Dot(velocity, hit.normal);
        if (dot > 0f)
        {
            velocity = (velocity - hit.normal * dot).normalized * speed;
        }

        return true;
    }

    void Update()
    {
        GetComponent<Renderer>().material.SetColor(
            "_BaseColor", Color.white * (groundContactCount * 0.25f));
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        desiredJump |= Input.GetButtonDown("Jump");

        float forwardSpeed = 1f;
        if (Input.GetButton("Charge"))
        {
            Debug.Log("Slowing");
            forwardSpeed = 0f;
        }
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        if (playerInputSpace)
        {
            Vector3 forward = playerInputSpace.forward;
            forward.y = 0f;
            forward.Normalize();
            Vector3 right = playerInputSpace.right;
            right.y = 0f;
            right.Normalize();
            desiredVelocity = (forward * forwardSpeed + right * playerInput.x) * maxSpeed;
        }
        else
        {
            desiredVelocity = new Vector3(playerInput.x, 0f, forwardSpeed) * maxSpeed;    
        }
    }

    private void FixedUpdate()
    {
        UpdateState();
        AdjustVelocity();
        if (desiredJump)
        {
            desiredJump = false;
            Jump();
        }

        body.velocity = velocity;
        ClearState();
    }

    private void ClearState()
    {
        groundContactCount = 0;
        steepContactCount = 0;
        contactNormal = Vector3.zero;
        steepNormal = Vector3.zero;
    }

    private void OnCollisionEnter(Collision other)
    {
        EvaluateCollision(other);
    }

    private void OnCollisionStay(Collision other)
    {
        EvaluateCollision(other);
    }

    void EvaluateCollision(Collision other)
    {
        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.GetContact(i).normal;
            if (normal.y >= minGroundDotProduct)
            {
                groundContactCount += 1;
                contactNormal += normal;
            }
            else if (normal.y > -0.01f)
            {
                steepContactCount += 1;
                steepNormal += normal;
            }
        }
    }

    bool CheckSteepContacts()
    {
        if (steepContactCount > 1)
        {
            steepNormal.Normalize();
            if (steepNormal.y >= minGroundDotProduct)
            {
                groundContactCount = 1;
                contactNormal = steepNormal;
                return true;
            }
        }

        return false;
    }

    void AdjustVelocity()
    {
        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

        float currentX = Vector3.Dot(velocity, xAxis);
        float currentZ = Vector3.Dot(velocity, zAxis);

        float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;
        
        float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);
        
        Vector3 currentNorm = velocity.normalized;
        Vector3 desiredNew = velocity + xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
        Vector3 desiredProjection = Vector3.Project(desiredNew, currentNorm);

        Vector3 currentDeceleration = velocity - desiredProjection;
        float currentDecelerationMagnitude = currentDeceleration.magnitude;
        float maxSpeedDecrease = maxDeacceleration * Time.deltaTime;
        if (currentDecelerationMagnitude > maxSpeedDecrease)
        {
            desiredNew += desiredProjection.normalized * (currentDecelerationMagnitude - maxSpeedDecrease);
        }

        velocity = desiredNew;
    }

    Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        return vector - contactNormal * Vector3.Dot(vector, contactNormal);
    }

    void Jump()
    {
        Vector3 jumpDirection;
        if (OnGround)
        {
            jumpDirection = contactNormal;
        }
        else if (OnSteep)
        {
            jumpDirection = steepNormal;
            jumpPhase = 0;
        }
        else if (maxAirJumps > 0 && jumpPhase <= maxAirJumps)
        {
            if (jumpPhase == 0)
            {
                jumpPhase = 1;
            }
            jumpDirection = contactNormal;
        }
        else
        {
            return;
        }
        stepsSinceLastJump = 0;
        jumpPhase += 1;
        float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
        jumpDirection = (jumpDirection + Vector3.up).normalized;
        float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
        if (alignedSpeed > 0f)
        {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }

        velocity += jumpDirection * jumpSpeed;
    }
}
