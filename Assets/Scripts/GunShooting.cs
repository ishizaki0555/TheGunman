using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunShooting : MonoBehaviour
{
    private PlayerInputSystem inputActions;

    [Header("�e��d�ݒ�")]
    [SerializeField] private Transform shootPoint; // �e�̔��ˈʒu
    [SerializeField] private GameObject bulletPrehab; // �e�̃v���n�u

    [Header("�e�̐ݒ�")]
    [SerializeField] private float shootForce; // ���˂̈З�
    [SerializeField] private float timeBetweenShooting; // �A�ˑ��x(�A�˕���p)
    [SerializeField] private float timeBetweenShots; // �A�ˑ��x(�V���b�g�K���p)
    [SerializeField] private float spread; // �e�̂΂��
    [SerializeField] private float reloadTime; // �����[�h����
    [SerializeField] private int magazineSize; // �}�K�W���̒e��
    [SerializeField] private int bulletsPerTap; // 1��̎ˌ��Ŕ��˂���e��
    [SerializeField] private bool allowButtonHold; // �A�˕��킩�P�����킩�̃t���O

    [SerializeField] private LayerMask ignoreLayer; // �������Ă������C���[

    GameObject playerCam;

    int bulletsShot, bulletsLeft;

    bool shooting, readyToShoot;

    public bool reloading;
    public bool allowInvoke = true;

    private void Start()
    {
        inputActions = new PlayerInputSystem();
        inputActions.Player.Enable();

        playerCam = GameObject.Find("Main Camera");
        bulletsLeft = magazineSize;
        readyToShoot = true;
    }

    private void Update()
    {
        InputHandle();
    }

    private void InputHandle()
    {
        if (allowButtonHold)
            shooting = inputActions.Player.LongPress.triggered; // �A�˕���
        else 
            shooting = inputActions.Player.Attack.triggered; // �P������

        // ���Ă��ԂȂ̂��`�F�b�N->Shoot()���Ăяo��
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = 0;
            Shoot();
        }

        // �����[�h
        if(inputActions.Player.Reload.triggered && bulletsLeft < magazineSize && !reloading)
        {
            Reload();
        }

        // ���������[�h
        if(readyToShoot && shooting && !reloading && bulletsLeft <= 0)
        {
            Reload();
        }
    }

    /// <summary>
    /// �e�𔭎˂���
    /// </summary>
    private void Shoot()
    {
        readyToShoot = false;

        Ray ray = playerCam.GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
        RaycastHit hit;

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit, 1000f, ~ignoreLayer)) // ���C�ɉ������������̂��`�F�b�N
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(10); // ����������Ȃ������烌�C�̒�������������

        // �e�����猩���^�[�Q�b�g�̕������擾
        Vector3 directionWithoutSpread = targetPoint - shootPoint.position;

        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        // �U�e����e�̏e�����猩���^�[�Q�b�g�̕������擾
        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x, y, 0);

        // �e�𐶐�
        GameObject currntBullet = Instantiate(bulletPrehab, shootPoint.position, Quaternion.Euler(0f, 0f, 90f));

        // �e��O���Ɍ����킹��
        currntBullet.transform.forward = directionWithSpread.normalized;

        // ���܂ɗ͂�������
        currntBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * shootForce, ForceMode.Impulse);

        bulletsLeft--;
        bulletsShot++;

        // �e�ƒe�̊Ԋu���󂯂�
        if(allowInvoke)
        {
            Invoke("ResetShot", timeBetweenShooting);
            allowInvoke = false;
        }

        // ��x�ɏo���e
        if(bulletsShot < bulletsPerTap && bulletsLeft > 0)
        {
            Invoke("Shoot", timeBetweenShots);
        }
    }

    /// <summary>
    /// ���Ă��Ԃɂ��܂�
    /// </summary>
    private void ResetShot()
    {
        readyToShoot = true;
        allowInvoke = true;
    }

    /// <summary>
    /// �����[�h���܂�
    /// </summary>
    private void Reload()
    {
        reloading = true;
        Invoke(nameof(ReloadFinished), reloadTime);
    }

    /// <summary>
    /// �����[�h��Ԃ̏I��
    /// </summary>
    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }
}
