using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

public class Controller : MonoBehaviour
{
    public float[] startingPosition = {-24.15858f, 0f, 1f};
    
    public enum Axel
    {
        Front, Rear
    }
    [Serializable]
    public struct Wheel
    {
        public GameObject wheelModel;
        public WheelCollider wheelCollider;
        public Axel axel;
    }

    public float maxaxcel = 60;
    public float brakeaxcel = 80;
    public float turnSensitivity = 1f;
    public float maxSteerAngle = 30f;

    public List<Wheel> wheels;
    public float moveInput;
    public float steerInput;

    private Rigidbody carRb;
    void Start()
    {
        carRb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        GetInputs();
    }
    void LateUpdate()
    {
        Move();
        Steer();
    }
    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.tag == "Checkpoint") Debug.Log("Checkpoint Reached");
    }
    void GetInputs()
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
    }

    void Move()
    {
        foreach(var wheel in wheels)
            {
            wheel.wheelCollider.motorTorque = moveInput * 100 * maxaxcel * Time.deltaTime;
            }
    }
    void Steer()
    {
        foreach(var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                var SteerAngle = steerInput * maxSteerAngle * turnSensitivity;
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, SteerAngle, 0.6f);
            }
        }
    }
}
