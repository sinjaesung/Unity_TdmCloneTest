using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Health and Damage")]
    private float enemyHealth = 260f;
    public float presentHealth;
    public float giveDamage = 5f;
    public float enemySpeed;
    private float originSpeed;

    [Header("Enemy Things")]
    public NavMeshAgent enemyAgent;
    //public Transform LookPoint;//Player Layer에 해당하는 무엇이든지간의 LookPoint타깃을 바라보게.(범위내의 집합들중 랜덤한 하나)
    public GameObject ShootingRaycastArea;//총 발사하는 기준origin raycasting
    public Transform PursuitPlayerBody;//추적할플레이어바디
    public Transform AttackplayerBody=null;//공격타깃(초기설정값)동적으로 변경가능.
    public LayerMask PlayerLayer;//탐색감지 checkSphere layer
    public Vector3 OriginSpawn;
    public Transform EnemyCharacter;

    [Header("Enemy Shooting Var")]
    public float timebtwShoot;//attackRate
    bool previouslyShoot;

    [Header("Enemy Animation and Spark effect")]
    public Animator anim;
    public ParticleSystem muzzleSpark;

    [Header("Enemy States")]
    public float visionRadius;//플레이어류감지 범위
    public float shootingRadius;//공격범위
    public bool playerInvisionRadius;//감지범위내에있는지여부
    public bool playerInshootingRadius;//공격범위내에있는지여부
  //  public bool IsPlayer = false;

    public ScoreManager scoreManager;

    [Header("Sound Effects")]
    public AudioSource audioSource;
    public AudioClip shootingSound;

    private void OnEnable()
    {
       // Debug.Log("Enemy타깃활성화때에 필요코루틴 모두 실행 및 기반정보설정시작");
        enemyAgent = GetComponent<NavMeshAgent>();
        presentHealth = enemyHealth;
        enemyAgent.speed = enemySpeed;
        originSpeed = enemySpeed;
        OriginSpawn = transform.position;
      //  Debug.Log("초기Enemy spawn위치:" + OriginSpawn);
        
        StartCoroutine(UpdatePursuit());//playerPursuit 0.5초간격마다
        StartCoroutine(UpdateAttackTarget());//3초간격마다 공격타깃 랜덤변경
        StartCoroutine(UpdateShoot());//0.1초간격마다 공격타깃으로   
        //지정된 타깃(not null)에 대해서 존재하면 공격처리
    }
    private void ReStartCoroutines()
    {
        StartCoroutine(UpdatePursuit());//playerPursuit 0.5초간격마다
        StartCoroutine(UpdateAttackTarget());//3초간격마다 공격타깃 랜덤변경
        StartCoroutine(UpdateShoot());//0.1초간격마다 공격타깃으로   
        //지정된 타깃(not null)에 대해서 존재하면 공격처리
    }
    private void StopCoroutinesStandard()
    {
        StopCoroutine(UpdatePursuit());
        StopCoroutine(UpdateAttackTarget());
        StopCoroutine(UpdateShoot());
    }
    private void OnDisable()
    {
        //Debug.Log("Enemy타깃Remove비활성화때에:모든 코루틴 종료");
        StopAllCoroutines();
    }
    private IEnumerator UpdatePursuit()
    {
        playerInvisionRadius = Physics.CheckSphere(transform.position, visionRadius, PlayerLayer);

        if (!playerInvisionRadius)
        {
            //Debug.Log("플레이어류를 감지범위 아니여서 추적을 멈춘다:");
            SetSelfPos();//현재 자리에서 멈춘다.추적중지.
        }
        if (playerInvisionRadius)
        {
            Vector3 playerPos = PursuitPlayerBody.position;
            Vector3 selfPos = transform.position;
            Vector3 pursuit_distance = playerPos - selfPos;
            //Debug.Log("playerInvisionRadius와 플레이어와의 현재거리:" + pursuit_distance.magnitude + "," + visionRadius);
            if(pursuit_distance.magnitude <= (visionRadius*1/3))
            {
               /* Debug.Log("플레이어와의거리가 감지제한구역범위 1/3범위내로 " +
                    "들어왔으면 현재자신위치 그대로 유지한다0.5초간격"+ (visionRadius * 1 / 3)+"~"+pursuit_distance.magnitude+"~"+ visionRadius);*/
                //SetSelfPos();
                //enemyAgent.isStopped=true;
            }
            else if( (pursuit_distance.magnitude > (visionRadius * 1 / 3)) && (pursuit_distance.magnitude<visionRadius)){
                /*Debug.Log("플레이어와의거리가 감지제한구역범위 Limit범위내에 속하는경우에 플레이어를 쫓는다"
                    + (visionRadius * 1 / 3) + "~" + pursuit_distance.magnitude + "~" + visionRadius);*/
                enemyAgent.isStopped = false;
                PursuitPlayer();//player류 집단들 범위내에서 발견,그들중n개중 한개랜덤한 타깃체 설정 및 추적
            }
        }

        yield return new WaitForSeconds(0.5f);//0.5초마다

        StartCoroutine(UpdatePursuit());
    }
    private void SetSelfPos()
    {
        if (enemyAgent.isActiveAndEnabled)
        {
            enemyAgent.SetDestination(transform.position);
        }
    }
    private void PursuitPlayer()
    {
        if (enemyAgent.isActiveAndEnabled)
        {
            if (enemyAgent.SetDestination(PursuitPlayerBody.position))
            {
                //animation
                //Debug.Log("PursuitPlayer류 대상 타깃 현재위치(추적중):" + PursuitPlayerBody.name + "," + PursuitPlayerBody.position);
                anim.SetBool("Running", true);
                anim.SetBool("Shooting", false);
            }
            else
            {
                //Debug.Log("SetDestination이용 불가한 경우");
                anim.SetBool("Running", false);
                anim.SetBool("Shooting", false);
            }
        }     
    }
    private IEnumerator UpdateAttackTarget()
    {
        playerInshootingRadius = Physics.CheckSphere(transform.position, shootingRadius, PlayerLayer);

        if (playerInshootingRadius)
        {
            UpdateAttackPlayer();//player류 집단들 범위내에서 발견,그들중n개중 한개랜덤한 타깃체 설정 및 공격
        }

        yield return new WaitForSeconds(3f);//3초마다실행.

        StartCoroutine(UpdateAttackTarget());
    }
    private void UpdateAttackPlayer()
    {
        Collider[] Perceptiontargets = Physics.OverlapSphere(transform.position, shootingRadius, PlayerLayer);
        List<Collider> filterPerceptions = new List<Collider>();
        if (Perceptiontargets.Length > 0)
        {
            for (int t = 0; t < Perceptiontargets.Length; t++)
            {
                Collider target = Perceptiontargets[t];
               // Debug.Log("현재 공격범위내에서 감지된 모든 player류 타깃들: " + t + "| " + target.transform.name);
                if (target.tag == "Player")
                {
                    filterPerceptions.Add(target);
                }
            }
            for (int r = 0; r < filterPerceptions.Count; r++)
            {
                Collider target_ = filterPerceptions[r];
               // Debug.Log("PlayerLayer>PlayerTag까지 만족 순수 감지타입들:" + r + "| " + target_.transform.name);
            }
            if (filterPerceptions.Count > 0)
            {
                int random_index = Random.Range(0, filterPerceptions.Count);
                // if(filterPerceptions[random_index])
               /* Debug.Log("n개의 타깃 Players대상체들중 0~n인댁스중에서 선택index:" + filterPerceptions.Count
                    + "개" + 0 + "~" + (filterPerceptions.Count - 1) + "," + random_index);*/

                Collider pickRandomTarget = filterPerceptions[random_index];
              //  Debug.Log("Enemy 공격범위감지된 players류타깃들중 랜덤한 개체 지정공격:" + random_index + "/" + (filterPerceptions.Count) + "명," + pickRandomTarget.transform);

                if (pickRandomTarget != null)
                {
                    AttackplayerBody = pickRandomTarget.transform;
                    transform.LookAt(AttackplayerBody);// 공격범위내에서 랜덤지정선택(3초마다변경) 선정한 공격타깃을 바라본다.

                   // Debug.Log("최종지정 AttackplayerBody! 3초간격!:3초마다 어택할 플레이어류 랜덤선택변경" + AttackplayerBody.name);
                }
            }
        }
    }

    private IEnumerator UpdateShoot()
    {
        playerInshootingRadius = Physics.CheckSphere(transform.position, shootingRadius, PlayerLayer);

        if (playerInshootingRadius && AttackplayerBody!= null && AttackplayerBody.gameObject.activeSelf)
        {
           // Debug.Log("AttackplayerBody gameObject activeSelf:" + AttackplayerBody.gameObject.activeSelf);
            ShootPlayer();//공격범위내에 players류가 발견되고,Attackplayerbody가 활성화상태인 것으로써 존재하고있으면 대상을 적절히 상황에따라 추적하며 공격하는 shootplayer처리
        }

        yield return new WaitForSeconds(0.1f);//0.1초마다실행.

        StartCoroutine(UpdateShoot());
    }
   
    
    private void ShootPlayer()
    {
        if (AttackplayerBody != null && AttackplayerBody.gameObject.activeSelf)
        {
            if (!previouslyShoot)//공격쿨타임
            {
               
                RaycastHit hit;

                //ShootingRaycastArea를 포함하는 전체 enemy개체 개별적으로 타깃 추적체 설정 및 추적,추적한 위치에서의 추적하고있는 타깃을 바라보게 했기에 forward는 자연스래 추적체방향을 향한다.
                if (Physics.Raycast(ShootingRaycastArea.transform.position, ShootingRaycastArea.transform.forward, out hit, shootingRadius,PlayerLayer))
                {                

                    /*Debug.Log("Enemy ShootingAttack 레이케스트 발사충돌개체,공격타깃" + hit.transform.name+","+
                        AttackplayerBody.name);*/

                    if (hit.transform.GetComponent<PlayerMovement>() != null)
                    {
                        muzzleSpark.Play();
                        audioSource.PlayOneShot(shootingSound);

                       // Debug.Log("장애물등을 모두 피하고 순수 타깃에게 다가가 명중 성공:공격명중중엔 추적중지" + hit.transform.name);
                        // enemyAgent.SetDestination(transform.position);//현재 자리에서 멈춘다.추적중지.
                        SetSelfPos();
                        PlayerMovement playerBody = hit.transform.GetComponent<PlayerMovement>();
                        if (playerBody != null)
                        {
                            playerBody.playerHitDamage(giveDamage);
                        }
                    }
                    else if (hit.transform.GetComponent<PlayerAI>() != null)
                    {
                        muzzleSpark.Play();
                        audioSource.PlayOneShot(shootingSound);

                        //Debug.Log("장애물등을 모두 피하고 순수 타깃에게 다가가 명중 성공:공격명중중엔 추적중지" + hit.transform.name);
                        //enemyAgent.SetDestination(transform.position);//현재 자리에서 멈춘다.추적중지.
                        SetSelfPos();
                        PlayerAI playerBody = hit.transform.GetComponent<PlayerAI>();
                        if (playerBody != null)
                        {
                            playerBody.PlayerAIHitDamage(giveDamage);
                        }
                    }
                    else
                    {
                        muzzleSpark.Play();
                        audioSource.PlayOneShot(shootingSound);

                        if (enemyAgent.isActiveAndEnabled)
                        {
                            enemyAgent.SetDestination(AttackplayerBody.position);
                        }
                      /* Debug.Log("Enemy|충돌개체가 플레이어류가 아닌 다른 대상체인경우(사물,지형 등)," +
                            "공격상태로 인한 추적중지상태에서 다시 추적타깃으로의 추적 재실행!" + hit.transform.name);*/
                    }

                    anim.SetBool("Running", false);
                    anim.SetBool("Shooting", true);
                }
                else
                {
                    //Debug.Log("만족RaycastHit개체가 없는경우" + AttackplayerBody.name);
                }
                previouslyShoot = true;
                Invoke(nameof(ActiveShooting), timebtwShoot);
            }
        }
    }

    private void ActiveShooting()
    {
        previouslyShoot = false;//timebtwShoot 쿨타임 지난후에야 다시공격가능.
    }

    public void enemyHitDamage(float takeDamage)
    {
        presentHealth -= takeDamage;

        if(presentHealth <= 0)
        {
            StartCoroutine(Respawn());
        }
    }
    IEnumerator Respawn()
    {
        enemyAgent.SetDestination(transform.position);//움직임중지
        enemySpeed = 0f;//속도0
        shootingRadius = 0f;//공격범위0
        visionRadius = 0f;//감지범위0
        playerInvisionRadius = false;//플레이어류감지,공격초기화
        playerInshootingRadius = false;
       
        //animations
       // Debug.Log("Enemy Dead");
        anim.SetBool("Die", true);
        anim.SetBool("Running", false);
        anim.SetBool("Shooting", false);
        StopCoroutinesStandard();
        gameObject.GetComponent<CapsuleCollider>().enabled = false;//캡슐콜라이더 false처리하여 더이상hitting감지되지 않도록(즉 한번 remove되면 한번씩만 Respawn실행되게,여러번 실행안되게)
        scoreManager.kills += 1;
        enemyAgent.enabled = false;

        yield return new WaitForSeconds(5f);
        //spawn point
      //  Debug.Log("Enemy spawn위치:(부활)" + OriginSpawn);
       // Debug.Log("Enemy ReSpawn");
        EnemyCharacter.position = OriginSpawn;
        EnemyCharacter.rotation = Quaternion.Euler(0, 0, 0);

        gameObject.GetComponent<CapsuleCollider>().enabled = true;
        enemyAgent.enabled = true;
        ReStartCoroutines();
        presentHealth = enemyHealth;
        enemySpeed = originSpeed;
        shootingRadius = 22f;
        visionRadius = 30f;
        playerInvisionRadius = true;
        playerInshootingRadius = false;

        //animations
        anim.SetBool("Die", false);
        anim.SetBool("Running", true);

        PursuitPlayer();
    }
}
