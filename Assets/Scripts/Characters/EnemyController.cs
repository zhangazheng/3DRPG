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
    // 检测玩家范围
    [SerializeField] private float signtRadius;
    // 是否为站桩的敌人
    [SerializeField] private bool isGuard;
    // 巡逻范围
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
        // todo fix 切换场景后修改为OnEnable
        GameManager.Instance.AddObserver(this);
    }
    // 切换场景时启用
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
        // 发现player，切换到CHASE
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
                // 判断是否走到了随机巡逻点
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
                    // 拉脱，回到上一个状态
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
                    // 追踪player
                    isFollow = true;
                    agent.isStopped = false;
                    agent.destination = attackTarget.transform.position;
                }
                // 攻击
                if(TargetInAttackRange() || TargetInSkillRange())
                {
                    isFollow = false;
                    agent.isStopped = true;
                    if(lastAttackTime < 0)
                    {
                        lastAttackTime = characterStats.attackData.collDown;
                        // 暴击
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
            // 近身攻击动画
            anim.SetTrigger("Attack");
        }
        if(TargetInSkillRange())
        {
            // 远程攻击动画
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
        // 获胜动画
        // 停止所有移动
        // 停止agent
        anim.SetBool("Win", true);
        isChase = false;
        isWalk = false;
        attackTarget = null;
        playerDeath = true;
    }
}
