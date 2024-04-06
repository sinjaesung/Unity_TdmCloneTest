using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Health Things")]
    private float playerHealth = 3000f;
    public float presentHealth;
    public HealthBar healthBar;

    [Header("Player Movement")]
    public float playerSpeed = 1.9f;
    public float currentPlayerSpeed = 0f;
    public float playerSprint = 3f;
    public float currentPlayerSprint = 0f;

    [Header("Player Camera")]
    public Transform playerCamera;

    [Header("Player Animator and Gravity")]
    public CharacterController cC;
    public float gravity = -9.81f;
    public Animator animator;

    [Header("Player Jumping & velocity")]
    public float jumpRange = 1f;
    public float turnCalmTime = 0.1f;
    float turnCalmVelocity;
    Vector3 velocity;
    public Transform surfaceCheck;
    bool onSurface;
    public float surfaceDistance = 0.4f;
    public LayerMask surfaceMask;

    // public bool mobileInputs;
    //public FixedJoystick joystick;
    //public FixedJoystick Sprintjoystick;

    public ScoreManager scoremanager;
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        presentHealth = playerHealth;
        healthBar.GiveFullHealth(playerHealth);

        scoremanager = FindObjectOfType<ScoreManager>();
    }
    private void Update()
    {
        /*if(currentPlayerSpeed > 0)
        {
            Sprintjoystick = null;
        }
        else
        {
            FixedJoystick sprintJS = GameObject.Find("PlayerSprintJoystick").GetComponent<FixedJoystick>();
            Debug.Log("sprintJs:" + sprintJS);
            Sprintjoystick = sprintJS;
        }*/
        onSurface = Physics.CheckSphere(surfaceCheck.position, surfaceDistance, surfaceMask);

        if(onSurface && velocity.y < 0)
        {
            velocity.y = -2f;
        }
       // Debug.Log("onSurface?:" + onSurface);

        //gravity
        velocity.y += gravity * Time.deltaTime;
       // Debug.Log("gravity Checking?:" + surfaceCheck+","+ velocity);
        cC.Move(velocity * Time.deltaTime);

        playerMove();

        Jump();

        Sprint();
    }
    void playerMove()
    {
        /*if(mobileInputs == true)
        {
            float horizontal_axis = joystick.Horizontal;
            float vertical_axis = joystick.Vertical;

            Vector3 direction = new Vector3(horizontal_axis, 0f, vertical_axis).normalized;

            if (direction.magnitude >= 0.1f)
            {
                animator.SetBool("Walk", true);
                animator.SetBool("Running", false);
                animator.SetBool("Idle", false);
                animator.SetTrigger("Jump");
                animator.SetBool("AimWalk", false);
                animator.SetBool("IdleAim", false);

                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + playerCamera.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnCalmVelocity, turnCalmTime);
                Debug.Log("MoveDirection targetAngle and calm angle" + Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + "+"
                    + playerCamera.eulerAngles.y + "," + turnCalmVelocity + "," + angle);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                cC.Move(moveDirection.normalized * playerSpeed * Time.deltaTime);
                currentPlayerSpeed = playerSpeed;
            }
            else
            {
                animator.SetBool("Idle", true);
                animator.SetTrigger("Jump");
                animator.SetBool("Walk", false);
                animator.SetBool("Running", false);
                animator.SetBool("AimWalk", false);
                currentPlayerSpeed = 0;
            }
        }
        else
        {*/
            float horizontal_axis = Input.GetAxisRaw("Horizontal");
            float vertical_axis = Input.GetAxisRaw("Vertical");

            Vector3 direction = new Vector3(horizontal_axis, 0f, vertical_axis).normalized;

            if (direction.magnitude >= 0.1f)
            {
                animator.SetBool("Walk", true);
                animator.SetBool("Running", false);
                animator.SetBool("Idle", false);
                animator.SetTrigger("Jump");
                animator.SetBool("AimWalk", false);
                animator.SetBool("IdleAim", false);

                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + playerCamera.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnCalmVelocity, turnCalmTime);
                /*Debug.Log("MoveDirection targetAngle and calm angle" + Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + "+"
                    + playerCamera.eulerAngles.y + "," + turnCalmVelocity + "," + angle);*/
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                cC.Move(moveDirection.normalized * playerSpeed * Time.deltaTime);
                currentPlayerSpeed = playerSpeed;
            }
            else
            {
                animator.SetBool("Idle", true);
                animator.SetTrigger("Jump");
                animator.SetBool("Walk", false);
                animator.SetBool("Running", false);
                animator.SetBool("AimWalk", false);
                currentPlayerSpeed = 0;
            }
        //}
    }

    void Jump()
    {
        if(Input.GetButtonDown("Jump") && onSurface)
        {
            animator.SetBool("Walk", false);
            animator.SetTrigger("Jump");
            velocity.y = Mathf.Sqrt(jumpRange * -2 * gravity);
        }
        else
        {
            animator.ResetTrigger("Jump");
        }
    }

    void Sprint()
    {
        /*if (mobileInputs == true)
        {
            float horizontal_axis = Sprintjoystick.Horizontal;
            float vertical_axis = Sprintjoystick.Vertical;

            Vector3 direction = new Vector3(horizontal_axis, 0f, vertical_axis).normalized;

            if (direction.magnitude >= 0.1f)
            {
                animator.SetBool("Walk", false);
                animator.SetBool("Running", true);
                animator.SetBool("Idle", false);
               // animator.SetTrigger("Jump");
                animator.SetBool("AimWalk", false);
                animator.SetBool("IdleAim", false);

                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + playerCamera.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnCalmVelocity, turnCalmTime);
                Debug.Log("MoveDirection targetAngle and calm angle" + Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + "+"
                    + playerCamera.eulerAngles.y + "," + turnCalmVelocity + "," + angle);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                cC.Move(moveDirection.normalized * playerSprint * Time.deltaTime);
                currentPlayerSprint = playerSprint;
            }
            else
            {
                animator.SetBool("Idle", true);
                animator.SetBool("Walk", false);
                animator.SetBool("Running", false);
                animator.SetBool("AimWalk", false);
                currentPlayerSprint = 0;
            }
        }
        else
        {*/
        if (Input.GetButton("Sprint") && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) && onSurface)
        {
            float horizontal_axis = Input.GetAxisRaw("Horizontal");
            float vertical_axis = Input.GetAxisRaw("Vertical");

            Vector3 direction = new Vector3(horizontal_axis, 0f, vertical_axis).normalized;

            if (direction.magnitude >= 0.1f)
            {
                animator.SetBool("Walk", false);
                animator.SetBool("Running", true);
                animator.SetBool("Idle", false);
                // animator.SetTrigger("Jump");
                animator.SetBool("AimWalk", false);
                animator.SetBool("IdleAim", false);

                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + playerCamera.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnCalmVelocity, turnCalmTime);
                /*Debug.Log("MoveDirection targetAngle and calm angle" + Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + "+"
                    + playerCamera.eulerAngles.y + "," + turnCalmVelocity + "," + angle);*/
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                cC.Move(moveDirection.normalized * playerSprint * Time.deltaTime);
                currentPlayerSprint = playerSprint;
            }
            else
            {
                animator.SetBool("Idle", true);
                animator.SetBool("Walk", false);
                animator.SetBool("Running", false);
                animator.SetBool("AimWalk", false);
                currentPlayerSprint = 0;
            }
        }
        //}
    }

    //playerhitdamage
    public void playerHitDamage(float takeDamage)
    {
        presentHealth -= takeDamage;
        healthBar.SetHealth(presentHealth);

        if(presentHealth <= 0)
        {
            playerDie();
        }
    }
    //playerdie
    private void playerDie()
    {
        Cursor.lockState = CursorLockMode.None;

        //Object.Destroy(gameObject);
        scoremanager.CharacterLose();
    }
}
