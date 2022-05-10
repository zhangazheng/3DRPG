using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Rock : MonoBehaviour
{
    private Rigidbody rb;
    [Header("Basic Settings")]
    [SerializeField] private float force;
    [SerializeField] private int damage;
    [SerializeField] private GameObject breakEffect;
    public enum RockStates
    {
        HitPlayer, HitEnemy, HitNothing
    }
    public RockStates states;
    public GameObject target;
    private Vector3 dir;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.one;
        states = RockStates.HitPlayer;
        Fly2Taget();
    }
    public void Fly2Taget()
    {
        if(target == null)
        {
            target = FindObjectOfType<PlayerController>().gameObject;
        }
        dir = (target.transform.position - transform.position + Vector3.up).normalized;
        rb.AddForce(dir * force, ForceMode.Impulse);
    }
    private void OnCollisionEnter(Collision collision)
    {
        switch(states)
        {
            case RockStates.HitPlayer:
                if(collision.gameObject.CompareTag("Player"))
                {
                    var agent = collision.gameObject.GetComponent<NavMeshAgent>();
                    agent.isStopped = true;
                    agent.velocity = dir * force;
                    collision.gameObject.GetComponent<Animator>().SetTrigger("Dizzy");
                    collision.gameObject.GetComponent<CharacterStats>().TakeDamage(damage, collision.gameObject.GetComponent<CharacterStats>());
                    states = RockStates.HitNothing;
                }
                break;
            case RockStates.HitEnemy:
                if (collision.gameObject.GetComponent<Golem>())
                {
                    var otherStats = collision.gameObject.GetComponent<CharacterStats>();
                    otherStats.TakeDamage(damage, otherStats);
                    Instantiate(breakEffect, transform.position, Quaternion.identity);
                    Destroy(gameObject);
                }
                break;
        }
    }
    private void FixedUpdate()
    {
        if(rb.velocity.sqrMagnitude < 1)
        {
            states = RockStates.HitNothing;
        }
    }
}
