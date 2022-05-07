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
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
    }
    private void Start()
    {
        MouseManager.Instance.OnMouseClicked += (v) => MoveToTarget(v);
        MouseManager.Instance.OnEnemyClicked += (v) => EventAttact(v);
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
    void SwitchAnimation()
    {
        anim.SetFloat("Speed", agent.velocity.sqrMagnitude);
        anim.SetBool("Death", isDeath);
    }
    public void MoveToTarget(Vector3 pos)
    {
        StopAllCoroutines();
        if (isDeath) return;
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
    void Hit()
    {
        var targetStats = attackTarget.GetComponent<CharacterStats>();
        targetStats.TakeDamage(characterStats, targetStats);
    }
}
