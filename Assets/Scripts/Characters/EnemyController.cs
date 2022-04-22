using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    GUARD, PATROL, CHASE, DEAD
}
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    // �����ҷ�Χ
    [SerializeField] private float signtRadius;
    // �Ƿ�Ϊվ׮�ĵ���
    [SerializeField] private bool isGuard;
    // Ѳ�߷�Χ
    [SerializeField] private float patrolRange;
    [SerializeField] private float lookAtTime;
    private float remainLookAtTime;

    private NavMeshAgent agent;
    private Animator anim;
    private EnemyState enemyStates;
    private GameObject attackTarget;
    private float speed;
    bool isWalk, isChase, isFollow;
    private Vector3 wayPoint, guardPos;
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        speed = agent.speed;
        guardPos = transform.position;
        remainLookAtTime = lookAtTime;
    }
    // Start is called before the first frame update
    void Start()
    {
        if(isGuard)
        {
            enemyStates = EnemyState.GUARD;
        } else
        {
            enemyStates = EnemyState.PATROL;
            GetNewWayPoint();
        }
    }

    // Update is called once per frame
    void Update()
    {
        SwitchStates();
        SwitchAnimation();
    }
    void SwitchAnimation()
    {
        anim.SetBool("Walk", isWalk);
        anim.SetBool("Chase", isChase);
        anim.SetBool("Follow", isFollow);
    }
    void SwitchStates()
    {
        if(FoundPlayer())
        {
            enemyStates = EnemyState.CHASE;
        }
        // ����player���л���CHASE
        switch(enemyStates)
        {
            case EnemyState.GUARD:
                break;
            case EnemyState.PATROL:
                isChase = false;
                agent.speed = speed * 0.5f;
                // �ж��Ƿ��ߵ������Ѳ�ߵ�
                if(Vector3.Distance(transform.position, wayPoint) <= agent.stoppingDistance)
                {
                    isWalk = false;
                    if (remainLookAtTime > 0)
                    {
                        remainLookAtTime -= Time.deltaTime;
                    } 
                    else
                    {
                        GetNewWayPoint();
                    }
                } else
                {
                    isWalk = true;
                    agent.destination = wayPoint;
                }
                break;
            case EnemyState.CHASE:
                isWalk = false;
                isChase = true;

                agent.speed = speed;
                if(!FoundPlayer())
                {
                    // ���ѣ��ص���һ��״̬
                    isFollow = false;
                    if (remainLookAtTime > 0)
                    {
                        remainLookAtTime -= Time.deltaTime;
                        agent.destination = transform.position;
                    }
                    else
                    {
                        if(isGuard)
                        {
                            enemyStates = EnemyState.GUARD;
                        } else
                        {
                            enemyStates = EnemyState.PATROL;

                        }
                    }
                }
                else
                {
                    // ׷��player
                    isFollow = true;
                    agent.destination = attackTarget.transform.position;
                }
                // ����
                break;
            case EnemyState.DEAD:
                break;
        }
    }
    bool FoundPlayer()
    {
        var colliders = Physics.OverlapSphere(transform.position, signtRadius);
        foreach(var c in colliders)
        {
            if(c.CompareTag("Player"))
            {
                attackTarget = c.gameObject;
                return true;
            }
        }
        return false;
    }
    void GetNewWayPoint()
    {
        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);
        Vector3 randomPoint = new Vector3(guardPos.x + randomX, transform.position.y, guardPos.z + randomZ);
        NavMeshHit hit;
        wayPoint = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1) ? hit.position: transform.position;
        remainLookAtTime = lookAtTime;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, signtRadius);
    }
}
