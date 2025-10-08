using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerContoroller : MonoBehaviour
{
    [SerializeField] private float _speed = 3;
    [SerializeField] private float _jumpSpeed = 7;
    [SerializeField] private float _gravity = 15f;
    [SerializeField] private float _fallSpeed = 10f;
    [SerializeField] private float _initFallSpeed = 2f;

    private Transform _transform;
    private CharacterController _characterController;

    private Vector2 _inputMove;
    private float _verticalVelocity;
    private float _turnVelocity;
    private bool _isGroundedPrev;

    /// <summary>
    /// 移動Action(PlayerInput側から呼ばれる)
    /// </summary>
    public void OnMove(InputAction.CallbackContext context)
    {
        // 入力値を保持しておく
        _inputMove = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// ジャンプAction(PlayerInput側から呼ばれる)
    /// </summary>
    public void OnJump(InputAction.CallbackContext context)
    {
        // ボタンが押された瞬間かつ着地している時だけ処理
        if (!context.performed || !_characterController.isGrounded) return;

        // 鉛直上向きに速度を与える
        _verticalVelocity = _jumpSpeed;
    }

    private void Awake()
    {
        _transform = transform;
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        PlayerMove(); // プレイヤーの移動
    }

    /*
    private void CameraWork()
    {
        // カメラの方向から、X-Z平面の単位ベクトルを取得
        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;

        // 移動方向の入力値とカメラの向きから、移動方向を決定
        Vector3 moveForward = cameraForward * _inputMove.x + Camera.main.transform.right * _inputMove.y;

        // プレイヤーの向きを、移動方向に合わせる
        if(moveForward != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveForward);
        }
    }

    private void PlayerMove()
    {
        var isGrounded = _characterController.isGrounded;

        if (isGrounded && !_isGroundedPrev)
        {
            // 着地する瞬間に落下の初速を指定しておく
            _verticalVelocity = -_initFallSpeed;
        }
        else if (!isGrounded)
        {
            // 空中にいるときは、下向きに重力加速度を与えて落下させる
            _verticalVelocity -= _gravity * Time.deltaTime;

            // 落下する速さ以上にならないように補正
            if (_verticalVelocity < -_fallSpeed)
                _verticalVelocity = -_fallSpeed;
        }

        _isGroundedPrev = isGrounded;

        // 操作入力と鉛直方向速度から、現在速度を計算
        var moveVelocity = new Vector3(
            _inputMove.x * _speed,
            _verticalVelocity,
            _inputMove.y * _speed
        );
        // 現在フレームの移動量を移動速度から計算
        var moveDelta = moveVelocity * Time.deltaTime;

        // CharacterControllerに移動量を指定し、オブジェクトを動かす
        _characterController.Move(moveDelta);

        if (_inputMove != Vector2.zero)
        {
            // 移動入力がある場合は、振り向き動作も行う

            // 操作入力からy軸周りの目標角度[deg]を計算
            var targetAngleY = -Mathf.Atan2(_inputMove.y, _inputMove.x)
                * Mathf.Rad2Deg + 90;

            // イージングしながら次の回転角度[deg]を計算
            var angleY = Mathf.SmoothDampAngle(
                _transform.eulerAngles.y,
                targetAngleY,
                ref _turnVelocity,
                0.1f
            );

            // オブジェクトの回転を更新
            _transform.rotation = Quaternion.Euler(0, angleY, 0);
        }
    }*/

    private void PlayerMove()
    {
        var isGrounded = _characterController.isGrounded;

        if (isGrounded && !_isGroundedPrev)
        {
            _verticalVelocity = -_initFallSpeed;
        }
        else if (!isGrounded)
        {
            _verticalVelocity -= _gravity * Time.deltaTime;
            if (_verticalVelocity < -_fallSpeed)
                _verticalVelocity = -_fallSpeed;
        }

        _isGroundedPrev = isGrounded;

        // カメラの向きに合わせて移動方向を変換
        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 cameraRight = Camera.main.transform.right;

        Vector3 moveDirection = cameraForward * _inputMove.y + cameraRight * _inputMove.x;
        moveDirection.Normalize();

        // 移動速度を適用
        Vector3 moveVelocity = moveDirection * _speed;
        moveVelocity.y = _verticalVelocity;

        Vector3 moveDelta = moveVelocity * Time.deltaTime;
        _characterController.Move(moveDelta);

        // プレイヤーの向きを移動方向に合わせる
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            _transform.rotation = Quaternion.Slerp(_transform.rotation, targetRotation, 0.1f);
        }
    }

}
