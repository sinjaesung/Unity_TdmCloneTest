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
    //public Transform LookPoint;//Player Layer�� �ش��ϴ� �����̵������� LookPointŸ���� �ٶ󺸰�.(�������� ���յ��� ������ �ϳ�)
    public GameObject ShootingRaycastArea;//�� �߻��ϴ� ����origin raycasting
    public Transform PursuitPlayerBody;//�������÷��̾�ٵ�
    public Transform AttackplayerBody=null;//����Ÿ��(�ʱ⼳����)�������� ���氡��.
    public LayerMask PlayerLayer;//Ž������ checkSphere layer
    public Vector3 OriginSpawn;
    public Transform EnemyCharacter;

    [Header("Enemy Shooting Var")]
    public float timebtwShoot;//attackRate
    bool previouslyShoot;

    [Header("Enemy Animation and Spark effect")]
    public Animator anim;
    public ParticleSystem muzzleSpark;

    [Header("Enemy States")]
    public float visionRadius;//�÷��̾������ ����
    public float shootingRadius;//���ݹ���
    public bool playerInvisionRadius;//�������������ִ�������
    public bool playerInshootingRadius;//���ݹ��������ִ�������
  //  public bool IsPlayer = false;

    public ScoreManager scoreManager;

    [Header("Sound Effects")]
    public AudioSource audioSource;
    public AudioClip shootingSound;

    private void OnEnable()
    {
       // Debug.Log("EnemyŸ��Ȱ��ȭ���� �ʿ��ڷ�ƾ ��� ���� �� ���������������");
        enemyAgent = GetComponent<NavMeshAgent>();
        presentHealth = enemyHealth;
        enemyAgent.speed = enemySpeed;
        originSpeed = enemySpeed;
        OriginSpawn = transform.position;
      //  Debug.Log("�ʱ�Enemy spawn��ġ:" + OriginSpawn);
        
        StartCoroutine(UpdatePursuit());//playerPursuit 0.5�ʰ��ݸ���
        StartCoroutine(UpdateAttackTarget());//3�ʰ��ݸ��� ����Ÿ�� ��������
        StartCoroutine(UpdateShoot());//0.1�ʰ��ݸ��� ����Ÿ������   
        //������ Ÿ��(not null)�� ���ؼ� �����ϸ� ����ó��
    }
    private void ReStartCoroutines()
    {
        StartCoroutine(UpdatePursuit());//playerPursuit 0.5�ʰ��ݸ���
        StartCoroutine(UpdateAttackTarget());//3�ʰ��ݸ��� ����Ÿ�� ��������
        StartCoroutine(UpdateShoot());//0.1�ʰ��ݸ��� ����Ÿ������   
        //������ Ÿ��(not null)�� ���ؼ� �����ϸ� ����ó��
    }
    private void StopCoroutinesStandard()
    {
        StopCoroutine(UpdatePursuit());
        StopCoroutine(UpdateAttackTarget());
        StopCoroutine(UpdateShoot());
    }
    private void OnDisable()
    {
        //Debug.Log("EnemyŸ��Remove��Ȱ��ȭ����:��� �ڷ�ƾ ����");
        StopAllCoroutines();
    }
    private IEnumerator UpdatePursuit()
    {
        playerInvisionRadius = Physics.CheckSphere(transform.position, visionRadius, PlayerLayer);

        if (!playerInvisionRadius)
        {
            //Debug.Log("�÷��̾���� �������� �ƴϿ��� ������ �����:");
            SetSelfPos();//���� �ڸ����� �����.��������.
        }
        if (playerInvisionRadius)
        {
            Vector3 playerPos = PursuitPlayerBody.position;
            Vector3 selfPos = transform.position;
            Vector3 pursuit_distance = playerPos - selfPos;
            //Debug.Log("playerInvisionRadius�� �÷��̾���� ����Ÿ�:" + pursuit_distance.magnitude + "," + visionRadius);
            if(pursuit_distance.magnitude <= (visionRadius*1/3))
            {
               /* Debug.Log("�÷��̾���ǰŸ��� �������ѱ������� 1/3�������� " +
                    "�������� �����ڽ���ġ �״�� �����Ѵ�0.5�ʰ���"+ (visionRadius * 1 / 3)+"~"+pursuit_distance.magnitude+"~"+ visionRadius);*/
                //SetSelfPos();
                //enemyAgent.isStopped=true;
            }
            else if( (pursuit_distance.magnitude > (visionRadius * 1 / 3)) && (pursuit_distance.magnitude<visionRadius)){
                /*Debug.Log("�÷��̾���ǰŸ��� �������ѱ������� Limit�������� ���ϴ°�쿡 �÷��̾ �Ѵ´�"
                    + (visionRadius * 1 / 3) + "~" + pursuit_distance.magnitude + "~" + visionRadius);*/
                enemyAgent.isStopped = false;
                PursuitPlayer();//player�� ���ܵ� ���������� �߰�,�׵���n���� �Ѱ������� Ÿ��ü ���� �� ����
            }
        }

        yield return new WaitForSeconds(0.5f);//0.5�ʸ���

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
                //Debug.Log("PursuitPlayer�� ��� Ÿ�� ������ġ(������):" + PursuitPlayerBody.name + "," + PursuitPlayerBody.position);
                anim.SetBool("Running", true);
                anim.SetBool("Shooting", false);
            }
            else
            {
                //Debug.Log("SetDestination�̿� �Ұ��� ���");
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
            UpdateAttackPlayer();//player�� ���ܵ� ���������� �߰�,�׵���n���� �Ѱ������� Ÿ��ü ���� �� ����
        }

        yield return new WaitForSeconds(3f);//3�ʸ��ٽ���.

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
               // Debug.Log("���� ���ݹ��������� ������ ��� player�� Ÿ���: " + t + "| " + target.transform.name);
                if (target.tag == "Player")
                {
                    filterPerceptions.Add(target);
                }
            }
            for (int r = 0; r < filterPerceptions.Count; r++)
            {
                Collider target_ = filterPerceptions[r];
               // Debug.Log("PlayerLayer>PlayerTag���� ���� ���� ����Ÿ�Ե�:" + r + "| " + target_.transform.name);
            }
            if (filterPerceptions.Count > 0)
            {
                int random_index = Random.Range(0, filterPerceptions.Count);
                // if(filterPerceptions[random_index])
               /* Debug.Log("n���� Ÿ�� Players���ü���� 0~n�δ콺�߿��� ����index:" + filterPerceptions.Count
                    + "��" + 0 + "~" + (filterPerceptions.Count - 1) + "," + random_index);*/

                Collider pickRandomTarget = filterPerceptions[random_index];
              //  Debug.Log("Enemy ���ݹ��������� players��Ÿ����� ������ ��ü ��������:" + random_index + "/" + (filterPerceptions.Count) + "��," + pickRandomTarget.transform);

                if (pickRandomTarget != null)
                {
                    AttackplayerBody = pickRandomTarget.transform;
                    transform.LookAt(AttackplayerBody);// ���ݹ��������� ������������(3�ʸ��ٺ���) ������ ����Ÿ���� �ٶ󺻴�.

                   // Debug.Log("�������� AttackplayerBody! 3�ʰ���!:3�ʸ��� ������ �÷��̾�� �������ú���" + AttackplayerBody.name);
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
            ShootPlayer();//���ݹ������� players���� �߰ߵǰ�,Attackplayerbody�� Ȱ��ȭ������ �����ν� �����ϰ������� ����� ������ ��Ȳ������ �����ϸ� �����ϴ� shootplayeró��
        }

        yield return new WaitForSeconds(0.1f);//0.1�ʸ��ٽ���.

        StartCoroutine(UpdateShoot());
    }
   
    
    private void ShootPlayer()
    {
        if (AttackplayerBody != null && AttackplayerBody.gameObject.activeSelf)
        {
            if (!previouslyShoot)//������Ÿ��
            {
               
                RaycastHit hit;

                //ShootingRaycastArea�� �����ϴ� ��ü enemy��ü ���������� Ÿ�� ����ü ���� �� ����,������ ��ġ������ �����ϰ��ִ� Ÿ���� �ٶ󺸰� �߱⿡ forward�� �ڿ����� ����ü������ ���Ѵ�.
                if (Physics.Raycast(ShootingRaycastArea.transform.position, ShootingRaycastArea.transform.forward, out hit, shootingRadius,PlayerLayer))
                {                

                    /*Debug.Log("Enemy ShootingAttack �����ɽ�Ʈ �߻��浹��ü,����Ÿ��" + hit.transform.name+","+
                        AttackplayerBody.name);*/

                    if (hit.transform.GetComponent<PlayerMovement>() != null)
                    {
                        muzzleSpark.Play();
                        audioSource.PlayOneShot(shootingSound);

                       // Debug.Log("��ֹ����� ��� ���ϰ� ���� Ÿ�꿡�� �ٰ��� ���� ����:���ݸ����߿� ��������" + hit.transform.name);
                        // enemyAgent.SetDestination(transform.position);//���� �ڸ����� �����.��������.
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

                        //Debug.Log("��ֹ����� ��� ���ϰ� ���� Ÿ�꿡�� �ٰ��� ���� ����:���ݸ����߿� ��������" + hit.transform.name);
                        //enemyAgent.SetDestination(transform.position);//���� �ڸ����� �����.��������.
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
                      /* Debug.Log("Enemy|�浹��ü�� �÷��̾���� �ƴ� �ٸ� ���ü�ΰ��(�繰,���� ��)," +
                            "���ݻ��·� ���� �����������¿��� �ٽ� ����Ÿ�������� ���� �����!" + hit.transform.name);*/
                    }

                    anim.SetBool("Running", false);
                    anim.SetBool("Shooting", true);
                }
                else
                {
                    //Debug.Log("����RaycastHit��ü�� ���°��" + AttackplayerBody.name);
                }
                previouslyShoot = true;
                Invoke(nameof(ActiveShooting), timebtwShoot);
            }
        }
    }

    private void ActiveShooting()
    {
        previouslyShoot = false;//timebtwShoot ��Ÿ�� �����Ŀ��� �ٽð��ݰ���.
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
        enemyAgent.SetDestination(transform.position);//����������
        enemySpeed = 0f;//�ӵ�0
        shootingRadius = 0f;//���ݹ���0
        visionRadius = 0f;//��������0
        playerInvisionRadius = false;//�÷��̾������,�����ʱ�ȭ
        playerInshootingRadius = false;
       
        //animations
       // Debug.Log("Enemy Dead");
        anim.SetBool("Die", true);
        anim.SetBool("Running", false);
        anim.SetBool("Shooting", false);
        StopCoroutinesStandard();
        gameObject.GetComponent<CapsuleCollider>().enabled = false;//ĸ���ݶ��̴� falseó���Ͽ� ���̻�hitting�������� �ʵ���(�� �ѹ� remove�Ǹ� �ѹ����� Respawn����ǰ�,������ ����ȵǰ�)
        scoreManager.kills += 1;
        enemyAgent.enabled = false;

        yield return new WaitForSeconds(5f);
        //spawn point
      //  Debug.Log("Enemy spawn��ġ:(��Ȱ)" + OriginSpawn);
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
