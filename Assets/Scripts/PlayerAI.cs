using System.Collections;
using System.Collections.Generic;
//using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.AI;

public class PlayerAI : MonoBehaviour
{
    [Header("Player Health and Damage")]
    private float PlayerHealth = 120f;
    public float presentHealth;
    public float giveDamage = 3f;
    public float PlayerSpeed;
    private float originSpeed;

    [Header("Player Things")]
    public NavMeshAgent PlayerAgent;
    //public Transform LookPoint;EnemyLayer에 해당하는 무엇이든지간의 LookPoint타깃을 바라보게.(범위내의 집합들중 랜덤한 하나)
    public GameObject ShootingRaycastArea;//총 발사하는 기준origin raycasting
    public Transform followPlayerbody;//쫓아갈플레이어바디
    public Transform AttackenemyBody;//타깃(초기설정값)동적으로 변경가능.
    public LayerMask playerLayer;//탐색감지 checkSphere layer
    public LayerMask enemyLayer;//탐색감지 checkSphere layer

    public Vector3 OriginSpawn;
    public Transform PlayerCharacter;

    [Header("Player Shooting Var")]
    public float timebtwShoot;//attackRate
    bool previouslyShoot;

    [Header("Player Animation and Spark effect")]
    public Animator anim;
    public ParticleSystem muzzleSpark;

    [Header("Player States")]
    public float visionRadius;//player감지 범위
    public float shootingRadius;//공격범위
    public bool playerInvisionRadius;//감지범위내에있는지여부
    public bool enemyInshootingRadius;//공격범위내에있는지여부

    [Header("Sound Effects")]
    public AudioSource audioSource;
    public AudioClip shootingSound;

    public ScoreManager scoreManager;

    private void OnEnable()
    {
        //Debug.Log("PlayerAI 타깃활성화때에 필요코루틴 모두 실행 및 기반정보설정시작");
        PlayerAgent = GetComponent<NavMeshAgent>();
        presentHealth = PlayerHealth;
        PlayerAgent.speed = PlayerSpeed;
        originSpeed = PlayerSpeed;
        OriginSpawn = transform.position;

        StartCoroutine(UpdateFollow());//playerfollow 0.5초간격마다
        StartCoroutine(UpdateAttackTarget());//3초간격마다 공격타깃 랜덤변경
        StartCoroutine(UpdateShoot());//0.1초간격마다 공격타깃으로
                                      //지정된 타깃(not null)에 대해서 존재하면 공격처리
    }
    private void ReStartCoroutines()
    {
        StartCoroutine(UpdateFollow());//playerfollow 0.5초간격마다
        StartCoroutine(UpdateAttackTarget());//3초간격마다 공격타깃 랜덤변경
        StartCoroutine(UpdateShoot());//0.1초간격마다 공격타깃으로   
        //지정된 타깃(not null)에 대해서 존재하면 공격처리
    }
    private void StopCoroutinesStandard()
    {
        StopCoroutine(UpdateFollow());
        StopCoroutine(UpdateAttackTarget());
        StopCoroutine(UpdateShoot());
    }
    private void OnDisable()
    {
        //Debug.Log("PlayerAI타깃Remove비활성화때에:모든 코루틴 종료");
        StopAllCoroutines();
    }
    private IEnumerator UpdateFollow()
    {
        playerInvisionRadius = Physics.CheckSphere(transform.position, visionRadius, playerLayer);

        if (playerInvisionRadius)
        {
            Vector3 playerPos = followPlayerbody.position;
            Vector3 selfPos = transform.position;
            Vector3 follow_distance = playerPos - selfPos;
            //Debug.Log("playerInvisionRadius와 플레이어와의 현재거리:" + follow_distance.magnitude + "," + visionRadius);
            if(follow_distance.magnitude >= visionRadius)
            {
               // Debug.Log("플레이어와의거리가 플레이어감지제한범위를 벗어난경우에만 플레이어 추적");
                //PlayerAgent.isStopped = false;
                FollowPlayer();
            }
            else
            {
               // Debug.Log("플레이어와의거리가 감지제한구역내에 속하는경우엔 현재 자신의 위치를 계속 유지한다0.5초간격");
                //SetSelfPos();
                //PlayerAgent.isStopped = true;
            }
        }

        yield return new WaitForSeconds(0.5f);//0.5초마다

        StartCoroutine(UpdateFollow());
    }
    private void SetSelfPos()
    {
        if (PlayerAgent.isActiveAndEnabled)
        {
            PlayerAgent.SetDestination(transform.position);
        }
    }
    private void FollowPlayer()
    {
        Vector3 playerNearDestination = followPlayerbody.position;
        float randomrangeX = Random.Range(playerNearDestination.x - 3f,playerNearDestination.x + 3f);
        float randomrangeZ = Random.Range(playerNearDestination.z - 3f, playerNearDestination.z + 3f);
        Vector3 adapt_playerNearDestination = new Vector3(randomrangeX, playerNearDestination.y, randomrangeZ);
       // Debug.Log("플레이어위치와 근처 쫓아가는 위치:" + playerNearDestination + "," + adapt_playerNearDestination);

        if (PlayerAgent.isActiveAndEnabled)
        {
            if (PlayerAgent.SetDestination(adapt_playerNearDestination))
            {
                //animation
                /*Debug.Log("followPlayerbody류 대상 타깃 현재위치(추적중):" +
                    followPlayerbody.transform.name + "," + followPlayerbody.position);*/
                anim.SetBool("Running", true);
                anim.SetBool("Shooting", false);
                Vector3 followPlayer_distance = adapt_playerNearDestination - playerNearDestination;
                //Debug.Log("player와의거리 측정:" + followPlayer_distance.magnitude);
            }
            else
            {
               // Debug.Log("SetDestination이용 불가한 경우");
                anim.SetBool("Running", false);
                anim.SetBool("Shooting", false);
            }
        }    
    }
    private IEnumerator UpdateAttackTarget()
    {
        enemyInshootingRadius = Physics.CheckSphere(transform.position, shootingRadius, enemyLayer);

        if (enemyInshootingRadius)
        {
            UpdateAttackEnemy();//player류 집단들 범위내에서 발견,그들중n개중 한개랜덤한 타깃체 설정 및 공격
        }

        yield return new WaitForSeconds(3f);//3초마다실행.

        StartCoroutine(UpdateAttackTarget());
    }

    private void UpdateAttackEnemy()
    {
        Collider[] Perceptiontargets = Physics.OverlapSphere(transform.position, shootingRadius, enemyLayer);
        List<Collider> filterPerceptions = new List<Collider>();
        if (Perceptiontargets.Length > 0)
        {
            for (int t = 0; t < Perceptiontargets.Length; t++)
            {
                Collider target = Perceptiontargets[t];
                //Debug.Log("현재 공격범위내에서 감지된 모든 enemy류 타깃들: " + t + "| " + target.transform.name);
                if (target.tag == "Enemy")
                {
                    filterPerceptions.Add(target);
                }
            }
            for (int r = 0; r < filterPerceptions.Count; r++)
            {
                Collider target_ = filterPerceptions[r];
                //Debug.Log("EnemyLayer>EnemyTag까지 만족 순수 감지타입들:" + r + "| " + target_.transform.name);
            }
            if(filterPerceptions.Count > 0)
            {
                int random_index = Random.Range(0, filterPerceptions.Count);
                // if(filterPerceptions[random_index])
               /* Debug.Log("n개의 타깃 Enemy대상체들중 0~n인댁스중에서 선택index:" + filterPerceptions.Count
                    + "개" + 0 + "~" + (filterPerceptions.Count - 1) + "," + random_index);*/

                Collider pickRandomTarget = filterPerceptions[random_index];
                //Debug.Log("PlayerAI 공격범위감지된 enemy류타깃들중 랜덤한 개체 지정공격:" + random_index+"/"+(filterPerceptions.Count)+"명,"+ pickRandomTarget.transform);
                
                if (pickRandomTarget != null)
                {
                    AttackenemyBody = pickRandomTarget.transform;
                    transform.LookAt(AttackenemyBody);//공격범위내에서 랜덤지정선택(3초마다변경) 선정한 공격타깃을 바라본다.

                   // Debug.Log("최종지정 AttackEnemyBody! 3초간격!:3초마다 어택할 적군 랜덤선택변경" + AttackenemyBody.name);
                }
            }
        }
    }

    private IEnumerator UpdateShoot()
    {
        enemyInshootingRadius = Physics.CheckSphere(transform.position, shootingRadius, enemyLayer);

        if (enemyInshootingRadius && AttackenemyBody != null && AttackenemyBody.gameObject.activeSelf)
        {
            //Debug.Log("AttackenemyBody gameObject activeSelf:" + AttackenemyBody.gameObject.activeSelf);
            ShootEnemy();//공격반경내에 적이발견되고,Attackenemybody가 활성화상태인 적으로써 존재하고있으면 적을 적절히상황에 따라 추적하며 공격하는 shootenemy처리.
        }
        else
        {
            //Debug.Log("enemyinshootingRadius 주변반경에 적이 없고,AttackenemyBody가 비활성화로 근처에 적이 없고,설정되어있던적도 비활성화상태라면 캐릭터를 다시 쫓을것이다.");
        }

        yield return new WaitForSeconds(0.1f);

        StartCoroutine(UpdateShoot());//0.1초마다 지정
    }
    private void ShootEnemy()
    {
        if (AttackenemyBody != null && AttackenemyBody.gameObject.activeSelf)
        {
            if (!previouslyShoot)//공격쿨타임
            {

                RaycastHit hit;

                //ShootingRaycastArea를 포함하는 전체 playerAi개체 개별적으로 타깃 추적체 설정 및 추적,추적한 위치에서의 추적하고있는 타깃을 바라보게 했기에 forward는 자연스래 추적체방향을 향한다.
                if (Physics.Raycast(ShootingRaycastArea.transform.position, ShootingRaycastArea.transform.forward, out hit, shootingRadius,enemyLayer))
                {
                    /*Debug.Log("PlayerAI ShootingAttack 레이케스트 발사충돌개체,공격타깃" + hit.transform.name+","
                        + AttackenemyBody.name);*/

                    if (hit.transform.GetComponent<Enemy>() != null)
                    {
                        muzzleSpark.Play();
                        audioSource.PlayOneShot(shootingSound);

                       // Debug.Log("장애물등을 모두 피하고 순수 타깃에게 다가가 명중 성공:공격명중중엔 추적중지" + hit.transform.name);
                        // PlayerAgent.SetDestination(transform.position);//현재 자리에서 멈춘다.추적중지.
                        SetSelfPos();
                         Enemy enemy = hit.transform.GetComponent<Enemy>();
                        if (enemy != null)
                        {
                            enemy.enemyHitDamage(giveDamage);
                            //GameObject goreGo = Instantiate(goreEffect, hitInfo.point, Quaternion.LookRotation(hit.normal));
                            //Destroy(goreGo, 1f);
                        }
                    }
                    else
                    {
                        muzzleSpark.Play();
                        audioSource.PlayOneShot(shootingSound);

                        PlayerAgent.SetDestination(AttackenemyBody.position);
                       /* Debug.Log("PlayerAI|충돌개체가 enemy류가 아닌 다른 대상체인경우(사물,지형 등)," +
                            "공격상태로 인한 추적중지상태에서 다시 추적타깃으로의 추적 재실행!" + hit.transform.name);*/
                    }

                    anim.SetBool("Running", false);
                    anim.SetBool("Shooting", true);
                }
                else{
                   // Debug.Log("만족RaycastHit개체가 없는경우" + AttackenemyBody.name);
                }
                previouslyShoot = true;
                Invoke(nameof(ActiveShooting), timebtwShoot);
            }
        }
    }
     private void ActiveShooting()
    {
        previouslyShoot = false;//timebtwShoot 쿨타임 지난후에야 다시공격가능
    }

    public void PlayerAIHitDamage(float takeDamage)
    {
        presentHealth -= takeDamage;

        if (presentHealth <= 0)
        {
            StartCoroutine(Respawn());
        }
    }
    IEnumerator Respawn()
    {
        PlayerAgent.SetDestination(transform.position);//움직임중지
        PlayerSpeed = 0f;//속도0
        shootingRadius = 0f;//공격범위0
        visionRadius = 0f;//감지범위0
        playerInvisionRadius = false;//enemy류감지,공격초기화
        enemyInshootingRadius = false;

        //animations
        //Debug.Log("Dead");
        anim.SetBool("Die", true);
        anim.SetBool("Running", false);
        anim.SetBool("Shooting", false);
        StopCoroutinesStandard();
        gameObject.GetComponent<CapsuleCollider>().enabled = false;//캡슐콜라이더 false처리하여 더이상hitting감지되지 않도록(즉 한번 remove되면 한번씩만 Respawn실행되게,여러번 실행안되게)
        scoreManager.enemyKills += 1;
        PlayerAgent.enabled = false;

        yield return new WaitForSeconds(5f);
        //spawn point
       // Debug.Log("PlayerAi Spawn");
       // Debug.Log("PlayerAi spawn위치:(부활):" + PlayerCharacter + "," + OriginSpawn);
        PlayerCharacter.position = OriginSpawn;
        PlayerCharacter.rotation = Quaternion.Euler(0, 0, 0);

        gameObject.GetComponent<CapsuleCollider>().enabled = true;
        PlayerAgent.enabled = true;
        ReStartCoroutines();
        presentHealth = PlayerHealth;
        PlayerSpeed = originSpeed;
        shootingRadius = 22f;
        visionRadius = 3f;
        playerInvisionRadius = true;
        enemyInshootingRadius = false;
        //animations
        anim.SetBool("Die", false);
        anim.SetBool("Running", true);

        FollowPlayer();
    }
}
