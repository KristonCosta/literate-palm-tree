using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class MovingSphere : MonoBehaviour
{
    [SerializeField, Range(0f, 100f)] 
    private float maxSpeed = 10f;

    [SerializeField, Range(0f, 100f)] 
    private float maxAcceleration = 10f;
    
    [SerializeField]
    Rect allowedArea = new Rect(-4.5f, -4.5f, 9f, 9f);

    [SerializeField, Range(0f, 1f)] 
    private float bounciness = 0.5f;
    
    private Vector3 velocity;
    // Update is called once per frame
    void Update()
    {
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);
        Vector3 desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        float maxSpeedChange = maxAcceleration * Time.deltaTime;
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);
        Vector3 displacement = velocity * Time.deltaTime;
        Vector3 newPosition = transform.localPosition + displacement;
        if (!allowedArea.Contains(new Vector2(newPosition.x, newPosition.z)))
        {
            if (newPosition.x < allowedArea.xMin || newPosition.x > allowedArea.xMax)
            {
                newPosition.x = Mathf.Clamp(newPosition.x, allowedArea.xMin, allowedArea.xMax);
                velocity.x = -velocity.x * bounciness;
            }
            if (newPosition.z < allowedArea.yMin || newPosition.z > allowedArea.yMax)
            {
                newPosition.z = Mathf.Clamp(newPosition.z, allowedArea.yMin, allowedArea.yMax);
                velocity.z = -velocity.z * bounciness;
            }
        }

        transform.localPosition = newPosition;
    }
}
