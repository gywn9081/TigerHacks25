using UnityEngine;
using System.Collections.Generic;

public class GunMechanics : MonoBehaviour
{
    public int damage;
    public float timeBetweenShooting, spread, range, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;
    int bulletsLeft, bulletsShot;

    bool shooting, readyToShoot, reloading;

    public Camera fpsCam;
    public Transform attackPoint;
    public RaycastHit rayHit;
    public LayerMask whatIsEnemy;

    public GameObject muzzleFlash, bulletHoleGraphic;
    public AudioSource audio;

    private void Awake()
    {
        bulletsLeft = magazineSize;
        readyToShoot = true;
    }
    
    private void Update()
    {
        MyInput();
    }
    
    private void MyInput()
    {
        if (allowButtonHold) shooting = Input.GetKey(KeyCode.Mouse0);
        else shooting = Input.GetKeyDown(KeyCode.Mouse0);

        if(Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading) Reload();

        if(readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = bulletsPerTap;
            Shoot();
        }
    }

    private void Shoot()
    {
        readyToShoot = false;

        //Raycast
        if(Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out rayHit, range, whatIsEnemy))
        {
            var npc = rayHit.collider.GetComponent<NPCController>();
            if (npc != null)
            {
                npc.TakeDamage(damage);
            }
        }

        Instantiate(bulletHoleGraphic, rayHit.point, Quaternion.Euler(0,180,0));
        Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);
        audio.Play();

        bulletsLeft--;
        bulletsShot--;
        Invoke("ResetShot", timeBetweenShooting);

        if(bulletsShot > 0 && bulletsLeft > 0)
        Invoke("Shoot", timeBetweenShots);
    }

    private void ResetShot()
    {
        readyToShoot = true;
    }

    private void Reload()
    {
        reloading = true;
        Invoke("ReloadFinished", reloadTime);
    }
    
    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }
    
    

    
}

