using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Grunt : EnemyController
{
    [Header("Skill")]
    public float kickForce = 10;
    public void KickOff()
    {
        if(attackTarget != null)
        {
            transform.LookAt(attackTarget.transform);
            Vector3 dir = (attackTarget.transform.position - transform.position).normalized;
            attackTarget.GetComponent<NavMeshAgent>().isStopped = true;
            attackTarget.GetComponent<NavMeshAgent>().velocity = dir * kickForce;
            attackTarget.GetComponent<Animator>().SetTrigger("Dizzy");
        }
    }
}
