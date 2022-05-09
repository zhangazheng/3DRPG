using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    GUARD, PATROL, CHASE, DEAD
}
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterStats))]
public class EnemyController : MonoBehaviour, IEndGameObserver
{
    // �����ҷ�Χ
    [SerializeField] private float signtRadius;
    // �Ƿ�Ϊվ׮�ĵ���
    [SerializeField] private bool isGuard;
    // Ѳ�߷�Χ
    [SerializeField] private float patrolRange;
    [SerializeField] private float lookAtTime;
    private float remainLookAtTime;
    private float lastAttackTime;
    private CharacterStats characterStats;

    private NavMeshAgent agent;
    private Animator anim;
    private EnemyState enemyStates;
    protected GameObject attackTarget;
    private float speed;
    bool isWalk, isChase, isFollow, isDeath, playerDeath;
    private Vector3 wayPoint, guardPos;
    private Quaternion guardRotation;
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        speed = agent.speed;
        guardPos = transform.position;
        guardRotation = transform.rotation;
        remainLookAtTime = lookAtTime;
        characterStats = GetComponent<CharacterStats>();
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
        // todo fix �л��������޸�ΪOnEnable
        GameManager.Instance.AddObserver(this);
    }
    // �л�����ʱ����
    //void OnEnable()
    //{
    //    GameManager.Instance.AddObserver(this);
    //}
    void OnDisable()
    {
        if (!GameManager.IsInitialized) return;
        GameManager.Instance.RemoveObserver(this);
    }
    // Update is called once per frame
    void Update()
    {
        if(characterStats.CurrentHealth == 0)
        {
            isDeath = true;
        }
        if(!playerDeath)
        {
            SwitchStates();
            SwitchAnimation();
            lastAttackTime -= Time.deltaTime;
        }
    }
    void SwitchAnimation()
    {
        anim.SetBool("Walk", isWalk);
        anim.SetBool("Chase", isChase);
        anim.SetBool("Follow", isFollow);
        anim.SetBool("Critical", characterStats.isCritical);
        anim.SetBool("Death", isDeath);
    }
    void SwitchStates()
    {
        if(isDeath)
        {
            enemyStates = EnemyState.DEAD;
        }
        else if(FoundPlayer())
        {
            enemyStates = EnemyState.CHASE;
        }
        // ����player���л���CHASE
        switch(enemyStates)
        {
            case EnemyState.GUARD:
                isChase = false;
                if(transform.position != guardPos)
                {
                    isWalk = true;
                    agent.isStopped = false;
                    agent.destination = guardPos;
                    transform.rotation = Quaternion.Lerp(transform.rotation, guardRotation, 0.1f);
                    if(Vector3.SqrMagnitude(guardPos - transform.position) <= agent.stoppingDistance)
                    {
                        isWalk = false;
                    }
                }
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
                    agent.isStopped = false;
                    agent.destination = attackTarget.transform.position;
                }
                // ����
                if(TargetInAttackRange() || TargetInSkillRange())
                {
                    isFollow = false;
                    agent.isStopped = true;
                    if(lastAttackTime < 0)
                    {
                        lastAttackTime = characterStats.attackData.collDown;
                        // ����
                        characterStats.isCritical = UnityEngine.Random.value < characterStats.attackData.cirticalChance;
                        Attack();
                    }
                }
                break;
            case EnemyState.DEAD:
                GetComponent<Collider>().enabled = false;
                agent.radius = 0;
                Destroy(gameObject, 2f);
                break;
        }
    }
    bool TargetInAttackRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.attackRange;
        return false;
    }
    bool TargetInSkillRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.skillRange;
        return false;
    }
    void Attack()
    {
        transform.LookAt(attackTarget.transform);
        if(TargetInAttackRange() )
        {
            // ����������
            anim.SetTrigger("Attack");
        }
        if(TargetInSkillRange())
        {
            // Զ�̹�������
            anim.SetTrigger("Skill");
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
    void Hit()
    {
        if(attackTarget != null)
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

    public void EndNotify()
    {
        // ��ʤ����
        // ֹͣ�����ƶ�
        // ֹͣagent
        anim.SetBool("Win", true);
        isChase = false;
        isWalk = false;
        attackTarget = null;
        playerDeath = true;
    }
}
