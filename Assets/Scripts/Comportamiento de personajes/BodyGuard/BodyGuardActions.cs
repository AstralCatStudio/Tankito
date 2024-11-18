using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourAPI.Core;
using BehaviourAPI.UnityToolkit;


// Use this attribute to include this action in a group: 
// [SelectionGroup("groupName")]
public class BodyGuardActions : UnityAction
{
    
    // Override this method to get references to unity components before the execution
    // using the "context" property.
    protected override void OnSetContext()
    {
        // To get a component of the object that is running this action:
        // context.GameObject.GetComponent<SpriteRenderer>();

        // Some components like transform, rigidbody, etc. are directly accessible from context:
        // context.Transform
        // context.RigidBody
    }

    // Called at the start of the execution. Use it to initialize the action.
    public override void Start()
    {
        throw new System.NotImplementedException();
    }

    // Called every execution frame.
    public override Status Update()
    {
        throw new System.NotImplementedException();
    }

    // Called at the end of the execution. Use it to reset the action.
    public override void Stop()
    {
        throw new System.NotImplementedException();
    }
}
