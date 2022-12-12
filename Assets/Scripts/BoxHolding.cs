using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxHolding : MonoBehaviour
{
    [SerializeField] private new Rigidbody rigidbody = null;

    public AgentActions Owner { get; set; } = null;
    public Rigidbody Rigidbody { get { return rigidbody; } }
}
