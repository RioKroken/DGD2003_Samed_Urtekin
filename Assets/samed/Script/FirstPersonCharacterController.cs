using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonCharacterController : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private Transform cameraTransform;

    [Header("Bakış (Mouse)")]
    [SerializeField] private float mouseSensitivity = 0.2f;
    [SerializeField] private float defaultMouseSensitivity = 0.2f;
    [SerializeField] private float pitchMin = -80f;
    [SerializeField] private float pitchMax = 80f;
    [Tooltip("Hareket sadece imleç kilitliyken yapılsın mı?")]
    [SerializeField] private bool requireCursorLockedForMovement = true;
    [Tooltip("Bakış (mouse look) sadece imleç kilitliyken yapılsın mı? (Varsayılan: kilit yokken de döndürür)")]
    [SerializeField] private bool requireCursorLockedForLook = false;

    [Header("Hareket")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool allowSprinting = true;
    [SerializeField] private float sprintMultiplier = 1.5f;

    [Header("Fizik")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpHeight = 1.5f;

    private CharacterController _controller;
    private Vector3 _velocity;
    private float _yaw;
    private float _pitch;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();

        // Inspector'a bağlanmadıysa olabildiğince otomatik bul.
        if (cameraTransform == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null) cameraTransform = mainCam.transform;
        }

        if (cameraTransform == null)
        {
            Camera childCam = GetComponentInChildren<Camera>();
            if (childCam != null) cameraTransform = childCam.transform;
        }

        _yaw = transform.eulerAngles.y;

        if (cameraTransform != null)
        {
            // Kamera player'ın child'ıysa local pitch, child değilse world pitch al.
            float pitchSource = (cameraTransform.parent == transform)
                ? cameraTransform.localEulerAngles.x
                : cameraTransform.eulerAngles.x;

            _pitch = pitchSource;
            if (_pitch > 180f) _pitch -= 360f; // 0..360 yerine -180..180
        }

        defaultMouseSensitivity = mouseSensitivity;

        if (GameSaveManager.Instance != null)
            SetMouseSensitivityScale(GameSaveManager.Instance.MouseSensitivity, 0.05f, 0.5f);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SetMouseSensitivityScale(float normalized01, float min, float max)
    {
        mouseSensitivity = Mathf.Lerp(min, max, Mathf.Clamp01(normalized01));
    }

    private void Update()
    {
        HandleCursorToggle();
        HandleMouseLook();
        HandleMovementAndJump();
    }

    private void HandleCursorToggle()
    {
        if (Keyboard.current == null) return;
        if (PauseMenuController.IsOpen) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // Pause menü varsa ESC onun işi — sadece imleç açma, panel açma burada yapılmaz
            if (PauseMenuController.Instance != null)
                return;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame &&
            Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void HandleMouseLook()
    {
        if (Mouse.current == null) return;
        if (PauseMenuController.IsOpen) return;

        if (requireCursorLockedForLook && Cursor.lockState != CursorLockMode.Locked)
            return;

        Vector2 delta = Mouse.current.delta.ReadValue();
        _yaw += delta.x * mouseSensitivity;
        _pitch -= delta.y * mouseSensitivity;
        _pitch = Mathf.Clamp(_pitch, pitchMin, pitchMax);

        Quaternion yawRotation = Quaternion.Euler(0f, _yaw, 0f);
        transform.rotation = yawRotation;

        if (cameraTransform != null)
        {
            if (cameraTransform.parent == transform)
            {
                // Child ise pitch'i localdan uygula.
                cameraTransform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
            }
            else
            {
                // Child değilse hem yaw hem pitch'i world rotation'da uygula.
                cameraTransform.rotation = yawRotation * Quaternion.Euler(_pitch, 0f, 0f);
            }
        }
    }

    private void HandleMovementAndJump()
    {
        if (Keyboard.current == null) return;
        if (requireCursorLockedForMovement && Cursor.lockState != CursorLockMode.Locked)
            return;

        float h = 0f;
        float v = 0f;
        if (Keyboard.current.dKey.isPressed) h += 1f;
        if (Keyboard.current.aKey.isPressed) h -= 1f;
        if (Keyboard.current.wKey.isPressed) v += 1f;
        if (Keyboard.current.sKey.isPressed) v -= 1f;

        bool isMoving = (h != 0f || v != 0f);

        float speed = moveSpeed;
        if (allowSprinting && Keyboard.current.leftShiftKey.isPressed)
            speed *= sprintMultiplier;

        if (isMoving)
        {
            Vector3 inputDir = new Vector3(h, 0f, v).normalized;
            Vector3 moveDirWorld = transform.TransformDirection(inputDir);
            _controller.Move(moveDirWorld * speed * Time.deltaTime);
        }

        if (_controller.isGrounded && Keyboard.current.spaceKey.wasPressedThisFrame)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        if (_controller.isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;

        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(new Vector3(0f, _velocity.y, 0f) * Time.deltaTime);
    }
}
