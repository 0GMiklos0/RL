using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CarController : Agent
{
    public int CollectedCheckpoint;
    [SerializeField] private Transform targetTransform;
    //actions
    public enum ActionTurn
    {
        Left, Right, Center
    }
    public enum ActionMove
    {
        Forward, Backward
    }
    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(-24.15858f, 0.03f, 8.39f);
        CollectedCheckpoint = 0;
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);

        sensor.AddObservation(targetTransform.localPosition.x);
        sensor.AddObservation(targetTransform.localPosition.y);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        var actionTaken = actions.ContinuousActions;
        float actionsSpeed = actionTaken[0];
        float actionsSteering = actionTaken[1];
        moveInput = actionsSpeed;
        steerInput = actionsSteering;
        Move();
        Steer();
    }
    private void OnTriggerEvent(Collision collision)
    {
        if (collision.collider.tag == "CheckPoint") { 
            AddReward(1f);
            CollectedCheckpoint++;
        }
        if (collision.collider.tag == "Wall") { 
            AddReward(-1f);
            EndEpisode();
        }
        if (collision.collider.tag == "FinishLine" && CollectedCheckpoint == 3) {
            SetReward(8f);
            EndEpisode();
        }
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> actions = actionsOut.ContinuousActions;
        steerInput = Input.GetAxis("Horizontal");
        moveInput = Input.GetAxis("Vertical");
        actions[0] = moveInput;
        actions[1] = steerInput;
        Move();
        Steer();
    }
    //The car controller

    public enum Axel
    {
        Front,
        Rear
    }

    [Serializable]
    public struct Wheel
    {
        public GameObject wheelModel;
        public WheelCollider wheelCollider;
        public Axel axel;
    }

    public float maxAcceleration = 30.0f;
    public float brakeAcceleration = 50.0f;

    public float turnSensitivity = 1.0f;
    public float maxSteerAngle = 30.0f;

    public Vector3 _centerOfMass;

    public List<Wheel> wheels;

    float moveInput;
    float steerInput;

    private Rigidbody carRb;

    void Start()
    {
        carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = _centerOfMass;
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
            wheel.wheelCollider.motorTorque = moveInput * 600 * maxAcceleration * Time.deltaTime;
        }
    }

    void Steer()
    {
        foreach(var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                var _steerAngle = steerInput * turnSensitivity * maxSteerAngle;
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, _steerAngle, 0.6f);
            }
        }
    }

    void Brake()
    {
        if (Input.GetKey(KeyCode.Space) || moveInput == 0)
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 300 * brakeAcceleration * Time.deltaTime;
            }
        }
        else
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 0;
            }
        }
    }
}
