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
    //public Transform LookPoint;EnemyLayer�� �ش��ϴ� �����̵������� LookPointŸ���� �ٶ󺸰�.(�������� ���յ��� ������ �ϳ�)
    public GameObject ShootingRaycastArea;//�� �߻��ϴ� ����origin raycasting
    public Transform followPlayerbody;//�Ѿư��÷��̾�ٵ�
    public Transform AttackenemyBody;//Ÿ��(�ʱ⼳����)�������� ���氡��.
    public LayerMask playerLayer;//Ž������ checkSphere layer
    public LayerMask enemyLayer;//Ž������ checkSphere layer

    public Vector3 OriginSpawn;
    public Transform PlayerCharacter;

    [Header("Player Shooting Var")]
    public float timebtwShoot;//attackRate
    bool previouslyShoot;

    [Header("Player Animation and Spark effect")]
    public Animator anim;
    public ParticleSystem muzzleSpark;

    [Header("Player States")]
    public float visionRadius;//player���� ����
    public float shootingRadius;//���ݹ���
    public bool playerInvisionRadius;//�������������ִ�������
    public bool enemyInshootingRadius;//���ݹ��������ִ�������

    [Header("Sound Effects")]
    public AudioSource audioSource;
    public AudioClip shootingSound;

    public ScoreManager scoreManager;

    private void OnEnable()
    {
        //Debug.Log("PlayerAI Ÿ��Ȱ��ȭ���� �ʿ��ڷ�ƾ ��� ���� �� ���������������");
        PlayerAgent = GetComponent<NavMeshAgent>();
        presentHealth = PlayerHealth;
        PlayerAgent.speed = PlayerSpeed;
        originSpeed = PlayerSpeed;
        OriginSpawn = transform.position;

        StartCoroutine(UpdateFollow());//playerfollow 0.5�ʰ��ݸ���
        StartCoroutine(UpdateAttackTarget());//3�ʰ��ݸ��� ����Ÿ�� ��������
        StartCoroutine(UpdateShoot());//0.1�ʰ��ݸ��� ����Ÿ������
                                      //������ Ÿ��(not null)�� ���ؼ� �����ϸ� ����ó��
    }
    private void ReStartCoroutines()
    {
        StartCoroutine(UpdateFollow());//playerfollow 0.5�ʰ��ݸ���
        StartCoroutine(UpdateAttackTarget());//3�ʰ��ݸ��� ����Ÿ�� ��������
        StartCoroutine(UpdateShoot());//0.1�ʰ��ݸ��� ����Ÿ������   
        //������ Ÿ��(not null)�� ���ؼ� �����ϸ� ����ó��
    }
    private void StopCoroutinesStandard()
    {
        StopCoroutine(UpdateFollow());
        StopCoroutine(UpdateAttackTarget());
        StopCoroutine(UpdateShoot());
    }
    private void OnDisable()
    {
        //Debug.Log("PlayerAIŸ��Remove��Ȱ��ȭ����:��� �ڷ�ƾ ����");
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
            //Debug.Log("playerInvisionRadius�� �÷��̾���� ����Ÿ�:" + follow_distance.magnitude + "," + visionRadius);
            if(follow_distance.magnitude >= visionRadius)
            {
               // Debug.Log("�÷��̾���ǰŸ��� �÷��̾�����ѹ����� �����쿡�� �÷��̾� ����");
                //PlayerAgent.isStopped = false;
                FollowPlayer();
            }
            else
            {
               // Debug.Log("�÷��̾���ǰŸ��� �������ѱ������� ���ϴ°�쿣 ���� �ڽ��� ��ġ�� ��� �����Ѵ�0.5�ʰ���");
                //SetSelfPos();
                //PlayerAgent.isStopped = true;
            }
        }

        yield return new WaitForSeconds(0.5f);//0.5�ʸ���

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
       // Debug.Log("�÷��̾���ġ�� ��ó �Ѿư��� ��ġ:" + playerNearDestination + "," + adapt_playerNearDestination);

        if (PlayerAgent.isActiveAndEnabled)
        {
            if (PlayerAgent.SetDestination(adapt_playerNearDestination))
            {
                //animation
                /*Debug.Log("followPlayerbody�� ��� Ÿ�� ������ġ(������):" +
                    followPlayerbody.transform.name + "," + followPlayerbody.position);*/
                anim.SetBool("Running", true);
                anim.SetBool("Shooting", false);
                Vector3 followPlayer_distance = adapt_playerNearDestination - playerNearDestination;
                //Debug.Log("player���ǰŸ� ����:" + followPlayer_distance.magnitude);
            }
            else
            {
               // Debug.Log("SetDestination�̿� �Ұ��� ���");
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
            UpdateAttackEnemy();//player�� ���ܵ� ���������� �߰�,�׵���n���� �Ѱ������� Ÿ��ü ���� �� ����
        }

        yield return new WaitForSeconds(3f);//3�ʸ��ٽ���.

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
                //Debug.Log("���� ���ݹ��������� ������ ��� enemy�� Ÿ���: " + t + "| " + target.transform.name);
                if (target.tag == "Enemy")
                {
                    filterPerceptions.Add(target);
                }
            }
            for (int r = 0; r < filterPerceptions.Count; r++)
            {
                Collider target_ = filterPerceptions[r];
                //Debug.Log("EnemyLayer>EnemyTag���� ���� ���� ����Ÿ�Ե�:" + r + "| " + target_.transform.name);
            }
            if(filterPerceptions.Count > 0)
            {
                int random_index = Random.Range(0, filterPerceptions.Count);
                // if(filterPerceptions[random_index])
               /* Debug.Log("n���� Ÿ�� Enemy���ü���� 0~n�δ콺�߿��� ����index:" + filterPerceptions.Count
                    + "��" + 0 + "~" + (filterPerceptions.Count - 1) + "," + random_index);*/

                Collider pickRandomTarget = filterPerceptions[random_index];
                //Debug.Log("PlayerAI ���ݹ��������� enemy��Ÿ����� ������ ��ü ��������:" + random_index+"/"+(filterPerceptions.Count)+"��,"+ pickRandomTarget.transform);
                
                if (pickRandomTarget != null)
                {
                    AttackenemyBody = pickRandomTarget.transform;
                    transform.LookAt(AttackenemyBody);//���ݹ��������� ������������(3�ʸ��ٺ���) ������ ����Ÿ���� �ٶ󺻴�.

                   // Debug.Log("�������� AttackEnemyBody! 3�ʰ���!:3�ʸ��� ������ ���� �������ú���" + AttackenemyBody.name);
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
            ShootEnemy();//���ݹݰ泻�� ���̹߰ߵǰ�,Attackenemybody�� Ȱ��ȭ������ �����ν� �����ϰ������� ���� ��������Ȳ�� ���� �����ϸ� �����ϴ� shootenemyó��.
        }
        else
        {
            //Debug.Log("enemyinshootingRadius �ֺ��ݰ濡 ���� ����,AttackenemyBody�� ��Ȱ��ȭ�� ��ó�� ���� ����,�����Ǿ��ִ����� ��Ȱ��ȭ���¶�� ĳ���͸� �ٽ� �������̴�.");
        }

        yield return new WaitForSeconds(0.1f);

        StartCoroutine(UpdateShoot());//0.1�ʸ��� ����
    }
    private void ShootEnemy()
    {
        if (AttackenemyBody != null && AttackenemyBody.gameObject.activeSelf)
        {
            if (!previouslyShoot)//������Ÿ��
            {

                RaycastHit hit;

                //ShootingRaycastArea�� �����ϴ� ��ü playerAi��ü ���������� Ÿ�� ����ü ���� �� ����,������ ��ġ������ �����ϰ��ִ� Ÿ���� �ٶ󺸰� �߱⿡ forward�� �ڿ����� ����ü������ ���Ѵ�.
                if (Physics.Raycast(ShootingRaycastArea.transform.position, ShootingRaycastArea.transform.forward, out hit, shootingRadius,enemyLayer))
                {
                    /*Debug.Log("PlayerAI ShootingAttack �����ɽ�Ʈ �߻��浹��ü,����Ÿ��" + hit.transform.name+","
                        + AttackenemyBody.name);*/

                    if (hit.transform.GetComponent<Enemy>() != null)
                    {
                        muzzleSpark.Play();
                        audioSource.PlayOneShot(shootingSound);

                       // Debug.Log("��ֹ����� ��� ���ϰ� ���� Ÿ�꿡�� �ٰ��� ���� ����:���ݸ����߿� ��������" + hit.transform.name);
                        // PlayerAgent.SetDestination(transform.position);//���� �ڸ����� �����.��������.
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
                       /* Debug.Log("PlayerAI|�浹��ü�� enemy���� �ƴ� �ٸ� ���ü�ΰ��(�繰,���� ��)," +
                            "���ݻ��·� ���� �����������¿��� �ٽ� ����Ÿ�������� ���� �����!" + hit.transform.name);*/
                    }

                    anim.SetBool("Running", false);
                    anim.SetBool("Shooting", true);
                }
                else{
                   // Debug.Log("����RaycastHit��ü�� ���°��" + AttackenemyBody.name);
                }
                previouslyShoot = true;
                Invoke(nameof(ActiveShooting), timebtwShoot);
            }
        }
    }
     private void ActiveShooting()
    {
        previouslyShoot = false;//timebtwShoot ��Ÿ�� �����Ŀ��� �ٽð��ݰ���
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
        PlayerAgent.SetDestination(transform.position);//����������
        PlayerSpeed = 0f;//�ӵ�0
        shootingRadius = 0f;//���ݹ���0
        visionRadius = 0f;//��������0
        playerInvisionRadius = false;//enemy������,�����ʱ�ȭ
        enemyInshootingRadius = false;

        //animations
        //Debug.Log("Dead");
        anim.SetBool("Die", true);
        anim.SetBool("Running", false);
        anim.SetBool("Shooting", false);
        StopCoroutinesStandard();
        gameObject.GetComponent<CapsuleCollider>().enabled = false;//ĸ���ݶ��̴� falseó���Ͽ� ���̻�hitting�������� �ʵ���(�� �ѹ� remove�Ǹ� �ѹ����� Respawn����ǰ�,������ ����ȵǰ�)
        scoreManager.enemyKills += 1;
        PlayerAgent.enabled = false;

        yield return new WaitForSeconds(5f);
        //spawn point
       // Debug.Log("PlayerAi Spawn");
       // Debug.Log("PlayerAi spawn��ġ:(��Ȱ):" + PlayerCharacter + "," + OriginSpawn);
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
