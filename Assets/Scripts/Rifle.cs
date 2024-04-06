using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rifle : MonoBehaviour
{
    [Header("Rifle")]
    public Camera cam;
    public float giveDamage = 10f;
    public float shootingRange = 100f;
    public float fireCharge = 15f;
    public PlayerMovement player;
    public Animator animator;

    [Header("Rifle Animation and shooting")]
    private float nextTimeToShoot = 0f;
    private int maximumAmmunition = 20;
    private int mag = 15;
    private int presentAmmunition;
    public float reloadingTime = 1.3f;
    private bool setReloading = false;

    [Header("Rifle Effects")]
    public ParticleSystem muzzleSpark;
    public GameObject WoodedEffect;
    public GameObject goreEffect;

    [Header("Sound Effects")]
    public AudioSource audioSource;
    public AudioClip shootingSound;
    public AudioClip reloadingSound;

    private void Awake()
    {
        presentAmmunition = maximumAmmunition;
    }

    private void Update()
    {
        if (setReloading)
            return;
        if(presentAmmunition <= 0)
        {
            StartCoroutine(Reload());
            return;
        }
        //Debug.Log("Time.time , nextTimeToShoot" + Time.time + "/" + nextTimeToShoot);
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToShoot)
        {
            animator.SetBool("Fire", true);
            animator.SetBool("Idle", false);
            nextTimeToShoot = Time.time + 1f / fireCharge;
           // Debug.Log("Shoot적용");
            Shoot();
        }else if(Input.GetButton("Fire1") && Time.time >= nextTimeToShoot || ( Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ) ){
            animator.SetBool("Idle", false);
            animator.SetBool("Firewalk", true);
        }
        else if (Input.GetButton("Fire1") && Time.time >= nextTimeToShoot && Input.GetButton("Fire2"))
        {
            animator.SetBool("Idle", false);
            animator.SetBool("IdleAim", true);
            animator.SetBool("Firewalk", true);
            animator.SetBool("Walk", true);
            animator.SetBool("Reloading", false);
        }
        else if(Input.GetButton("Fire1") && Time.time < nextTimeToShoot)
        {
            //Debug.Log("Shoot쿨타임");
        }
        else
        {
            animator.SetBool("Fire", false);
            animator.SetBool("Idle", true);
            animator.SetBool("FireWalk", false);
        }
    }

    void Shoot()
    {
        if(mag == 0)
        {
            //show ammo out text
        }
        presentAmmunition--;

        if(presentAmmunition == 0)
        {
            mag--;//탄창 하나 감소시킨다.
        }

        //update UI
        AmmoCount.occurrence.UpdateAmmoText(presentAmmunition);
        AmmoCount.occurrence.UpdateMagText(mag);

        muzzleSpark.Play();
        audioSource.PlayOneShot(shootingSound);

        RaycastHit hitInfo;

        if(Physics.Raycast(cam.transform.position,cam.transform.forward,out hitInfo, shootingRange))
        {
            //Debug.Log("Shoot hitInfo:" + hitInfo.transform.name);

            Objects objects = hitInfo.transform.GetComponent<Objects>();

            Enemy enemy = hitInfo.transform.GetComponent<Enemy>();

            if(objects != null)
            {
                objects.objectHitDamage(giveDamage);
                GameObject WoodGo = Instantiate(WoodedEffect,hitInfo.point,Quaternion.LookRotation(hitInfo.normal));
                Destroy(WoodGo, 1f);
            }
            else if(enemy != null)
            {
                enemy.enemyHitDamage(giveDamage);
                GameObject goreGo = Instantiate(goreEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                Destroy(goreGo, 1f);
            }
        }
    }

    IEnumerator Reload()
    {
        player.playerSpeed = 0f;
        player.playerSprint = 0f;
        setReloading = true;
        //Debug.Log("Reloading...");
        animator.SetBool("Reloading", true);
        audioSource.PlayOneShot(reloadingSound);
        //animation and audio
        yield return new WaitForSeconds(reloadingTime);
        animator.SetBool("Reloading", false);
        //animations
        presentAmmunition = maximumAmmunition;
        player.playerSpeed = 1.9f;
        player.playerSprint = 3f;
        setReloading = false;
    }
}
