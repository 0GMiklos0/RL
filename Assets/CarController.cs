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
    private int targetIndex;
    [SerializeField] private Transform[] targetTransforms;
    private Transform targetTransform;
    public Rigidbody rigidbody;
    
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
        transform.localPosition = new Vector3(0f, 0f, 0f);
        transform.localRotation = Quaternion.Euler(0f,0f,0f);
        targetIndex = 0;
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.velocity = Vector3.zero;
        targetTransform = targetTransforms[targetIndex];
        foreach(var cp in targetTransforms)
        {
            cp.gameObject.tag = "CheckPoint";
        }
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);

        sensor.AddObservation(rigidbody.velocity.x);
        sensor.AddObservation(rigidbody.velocity.z);
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
        AddReward(-0.001f);
    }
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "CheckPoint") {
            if (collision.gameObject.name == targetTransform.name)
            {
                AddReward(1f);
                CollectedCheckpoint++;
                targetIndex++;
            }
            else AddReward(-1f);
            
            targetTransform = targetTransforms[targetIndex];
            collision.gameObject.tag = "Recieved CheckPoint";
            if (targetIndex > targetTransforms.Length -1)
            {
                AddReward(1f);
                EndEpisode();
            }
        }
        if (collision.gameObject.tag == "Wall") {
            AddReward(-1f);
            EndEpisode();
        }
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> actions = actionsOut.ContinuousActions;

        actions[0] = 0;
        actions[1] = 0;

        if (Input.GetKey("w"))
            actions[0] = 1;
        if (Input.GetKey("s"))
            actions[0] = -1;

        if (Input.GetKey("d"))
            actions[1] = +0.5f;

        if (Input.GetKey("a"))
            actions[1] = -0.5f;
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

    public float maxAcceleration = 100.0f;
    public float brakeAcceleration = 150.0f;

    public float turnSensitivity = 1.0f;
    public float maxSteerAngle = 30.0f;

    public Vector3 _centerOfMass;

    public List<Wheel> wheels;

    float moveInput;
    float steerInput;

    void Move()
    {
        foreach(var wheel in wheels)
        {
            wheel.wheelCollider.motorTorque = moveInput * 300 * maxAcceleration * Time.fixedDeltaTime;
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
}
