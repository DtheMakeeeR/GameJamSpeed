using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] CarHandler _car;
    [SerializeField] BoxHandler _box;

    private InputActionMap _playerActionMap;
    private Vector2 _moveInput = Vector2.zero;
    private Vector2 _boxInput = Vector2.zero;

    private void Awake()
    {
        _playerActionMap = InputSystem.actions.FindActionMap("Player");
        _playerActionMap.Enable();
        _playerActionMap.FindAction("Move").performed += OnMovePerformed;
        _playerActionMap.FindAction("Move").canceled += OnMovePerformed;
        //_playerActionMap.FindAction("Reload").performed += OnReloadPerformed;
        _playerActionMap.FindAction("Power").performed += OnGearChangePerformed;
        //_playerActionMap.FindAction("Power").canceled += OnGearChangePerformed;

    }

    private void OnGearChangePerformed(InputAction.CallbackContext context)
    {
        Debug.Log($"{gameObject.name} OnGearChangePerformed Step1");
        _boxInput = context.ReadValue<Vector2>();
        Debug.Log($"{gameObject.name} OnGearChangePerformed Step2");
        _box.SetInput(_boxInput);
        Debug.Log($"{gameObject.name} OnGearChangePerformed Step3");
        _box.ChangeGear();
        Debug.Log($"{gameObject.name} OnGearChangePerformed Step4");
    }

    private void OnReloadPerformed(InputAction.CallbackContext context)
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    private void OnDisable()
    {
        if (_playerActionMap != null)
        {
            _playerActionMap.FindAction("Move").performed -= OnMovePerformed;
            _playerActionMap.Disable();
        }
    }
    private void Update()
    {
        _car.SetInput(_moveInput);
    }
}
