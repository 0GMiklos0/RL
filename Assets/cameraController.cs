using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraController : MonoBehaviour
{
    public float moveSmoothness;
    public float rotSmoothness;
    public Vector3 moveOffset;
    public Vector3 rotOffset;
    [SerializeField]public Transform carTarget;
    void HandleMovement()
    {
        Vector3 targetPos = new Vector3();
        targetPos = carTarget.TransformPoint(moveOffset);
        transform.position = Vector3.Lerp(transform.position, targetPos, moveSmoothness * Time.deltaTime);
    }
    void FollowTarget()
    {
        HandleMovement();
        HandleRotation();
    }
    void FixedUpdate()
    {
        FollowTarget();
    }
    void HandleRotation()
    {
        var direction = carTarget.position - transform.position;
        var rotation = new Quaternion();
        rotation = Quaternion.LookRotation(direction + rotOffset, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotSmoothness * Time.deltaTime);
    }
}
