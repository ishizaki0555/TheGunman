using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunShooting : MonoBehaviour
{
    private PlayerInputSystem inputActions;

    [Header("弾のd設定")]
    [SerializeField] private Transform shootPoint; // 弾の発射位置
    [SerializeField] private GameObject bulletPrehab; // 弾のプレハブ

    [Header("銃の設定")]
    [SerializeField] private float shootForce; // 発射の威力
    [SerializeField] private float timeBetweenShooting; // 連射速度(連射武器用)
    [SerializeField] private float timeBetweenShots; // 連射速度(ショットガン用)
    [SerializeField] private float spread; // 弾のばらつき
    [SerializeField] private float reloadTime; // リロード時間
    [SerializeField] private int magazineSize; // マガジンの弾数
    [SerializeField] private int bulletsPerTap; // 1回の射撃で発射する弾数
    [SerializeField] private bool allowButtonHold; // 連射武器か単発武器かのフラグ

    [SerializeField] private LayerMask ignoreLayer; // 無視していいレイヤー

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
            shooting = inputActions.Player.LongPress.triggered; // 連射武器
        else 
            shooting = inputActions.Player.Attack.triggered; // 単発武器

        // 撃てる状態なのかチェック->Shoot()を呼び出す
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = 0;
            Shoot();
        }

        // リロード
        if(inputActions.Player.Reload.triggered && bulletsLeft < magazineSize && !reloading)
        {
            Reload();
        }

        // 自動リロード
        if(readyToShoot && shooting && !reloading && bulletsLeft <= 0)
        {
            Reload();
        }
    }

    /// <summary>
    /// 弾を発射する
    /// </summary>
    private void Shoot()
    {
        readyToShoot = false;

        Ray ray = playerCam.GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
        RaycastHit hit;

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit, 1000f, ~ignoreLayer)) // レイに何が当たったのかチェック
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(10); // 何も当たらなかったらレイの長さを強制決定

        // 銃口から見たターゲットの方向を取得
        Vector3 directionWithoutSpread = targetPoint - shootPoint.position;

        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        // 散弾する銃の銃口から見たターゲットの方向を取得
        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x, y, 0);

        // 弾を生成
        GameObject currntBullet = Instantiate(bulletPrehab, shootPoint.position, Quaternion.Euler(0f, 0f, 90f));

        // 弾を前方に向かわせる
        currntBullet.transform.forward = directionWithSpread.normalized;

        // たまに力を加える
        currntBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * shootForce, ForceMode.Impulse);

        bulletsLeft--;
        bulletsShot++;

        // 弾と弾の間隔を空ける
        if(allowInvoke)
        {
            Invoke("ResetShot", timeBetweenShooting);
            allowInvoke = false;
        }

        // 一度に出す弾
        if(bulletsShot < bulletsPerTap && bulletsLeft > 0)
        {
            Invoke("Shoot", timeBetweenShots);
        }
    }

    /// <summary>
    /// 撃てる状態にします
    /// </summary>
    private void ResetShot()
    {
        readyToShoot = true;
        allowInvoke = true;
    }

    /// <summary>
    /// リロードします
    /// </summary>
    private void Reload()
    {
        reloading = true;
        Invoke(nameof(ReloadFinished), reloadTime);
    }

    /// <summary>
    /// リロード状態の終了
    /// </summary>
    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }
}
