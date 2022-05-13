using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    private GameObject attackTarget;
    private float lastAttactTime;
    private CharacterStats characterStats;
    private bool isDeath;
    private float stopDistance;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        stopDistance = agent.stoppingDistance;
    }
    private void Start()
    {
        // 将player character stats 注册到GameObject
        GameManager.Instance.RegisterPlayer(characterStats);
    }

    private void Update()
    {
        isDeath = characterStats.CurrentHealth == 0;
        if(isDeath)
        {
            GameManager.Instance.NotifyObservers();
        }
        SwitchAnimation();
        lastAttactTime -= Time.deltaTime;
    }
    private void OnEnable()
    {
        MouseManager.Instance.OnMouseClicked += MoveToTarget;
        MouseManager.Instance.OnEnemyClicked += EventAttact;
    }
    private void OnDisable()
    {
        if (!MouseManager.IsInitialized) return;
        MouseManager.Instance.OnMouseClicked -= MoveToTarget;
        MouseManager.Instance.OnEnemyClicked -= EventAttact;
    }
    void SwitchAnimation()
    {
        anim.SetFloat("Speed", agent.velocity.sqrMagnitude);
        anim.SetBool("Death", isDeath);
    }
    public void MoveToTarget(Vector3 pos)
    {
        StopAllCoroutines();
        if (isDeath) return;
        agent.stoppingDistance = stopDistance;
        agent.isStopped = false;
        agent.destination = pos;
    }
    private void EventAttact(GameObject target)
    {
        if (isDeath) return;
        if (target != null)
        {
            attackTarget = target;
            characterStats.isCritical = UnityEngine.Random.value < characterStats.attackData.cirticalChance;
            StartCoroutine(MoveToAttactTarget());
        }
    }
    private IEnumerator MoveToAttactTarget()
    {
        agent.isStopped = false;
        agent.stoppingDistance = characterStats.attackData.attackRange;
        transform.LookAt(attackTarget.transform);
        while(Vector3.Distance(attackTarget.transform.position, transform.position) > characterStats.attackData.attackRange) 
        {
            agent.destination = attackTarget.transform.position;
            yield return null;
        }
        agent.isStopped = true;
        // attack
        if(lastAttactTime < 0)
        {
            anim.SetBool("Critical", characterStats.isCritical);
            anim.SetTrigger("Attack");

            // 重置冷却时间
            lastAttactTime = 0.5f;
        }
    }
    // Animation Event
    void Hit()
    {
        if(attackTarget.CompareTag("Attackable"))
        {
            if(attackTarget.GetComponent<Rock>() && attackTarget.GetComponent<Rock>().states == Rock.RockStates.HitNothing)
            {
                attackTarget.GetComponent<Rock>().states = Rock.RockStates.HitEnemy;
                attackTarget.GetComponent<Rigidbody>().velocity = Vector3.one;
                attackTarget.GetComponent<Rigidbody>().AddForce(transform.forward * 20, ForceMode.Impulse);
            }
        } 
        else
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }
}
