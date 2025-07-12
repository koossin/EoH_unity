using UnityEngine;
using UnityEngine.InputSystem;

public class SlowMotionController : MonoBehaviour
{
    public float slowTimeScale = 0.1f;
    public float normalTimeScale = 1f;

    private PlayerInputActions playerActions;

    private void Awake()
    {
        playerActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        playerActions.Player.Enable();
        playerActions.Player.SlowMotion.performed += OnSlowMotionPerformed;
        playerActions.Player.SlowMotion.canceled += OnSlowMotionCanceled;
    }

    private void OnDisable()
    {
        playerActions.Player.SlowMotion.performed -= OnSlowMotionPerformed;
        playerActions.Player.SlowMotion.canceled -= OnSlowMotionCanceled;
        playerActions.Player.Disable();
    }

    private void OnSlowMotionPerformed(InputAction.CallbackContext context)
    {
        Time.timeScale = slowTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    private void OnSlowMotionCanceled(InputAction.CallbackContext context)
    {
        Time.timeScale = normalTimeScale;
        Time.fixedDeltaTime = 0.02f;
    }
}
