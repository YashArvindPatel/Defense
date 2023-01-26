using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public CharacterController characterController;
    public float speed = 3;
    public float turnSmoothTime = 0.1f;

    public Animator animator;

    // gravity
    private float gravity = 9.87f;
    private float verticalSpeed = 0;
    private float turnSmoothVelocity;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        Move();
    }

    private void Move()
    {
        float horizontalMove = Input.GetAxis("Horizontal");
        float verticalMove = Input.GetAxis("Vertical");

        if (characterController.isGrounded) verticalSpeed = 0;
        else verticalSpeed -= gravity * Time.deltaTime;
        Vector3 gravityMove = new Vector3(0, verticalSpeed, 0);

        Vector3 dir = new Vector3(horizontalMove, 0, verticalMove).normalized;
        Vector3 moveDir = Vector3.zero;

        if (dir.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg + cam.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0, angle, 0);

            moveDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            animator.SetBool("Move", true);
        }
        else
        {
            animator.SetBool("Move", false);
        }

        characterController.Move(speed * Time.deltaTime * moveDir.normalized + gravityMove * Time.deltaTime);
    }
}