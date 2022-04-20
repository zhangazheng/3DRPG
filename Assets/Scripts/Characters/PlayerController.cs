using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    NavMeshAgent agent;
    Animator anim;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }
    private void Start()
    {
        MouseManager.Instance.OnMouseClicked += (v) => MoveToTarget(v);
    }
    private void Update()
    {
        SwitchAnimation();
    }
    public void MoveToTarget(Vector3 pos)
    {
        agent.destination = pos;
    }
    void SwitchAnimation()
    {
        anim.SetFloat("Speed", agent.velocity.sqrMagnitude);
    }
}
