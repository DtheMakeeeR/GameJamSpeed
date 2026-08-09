using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class UIHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _distanceTravelledText;
    [SerializeField] private TextMeshProUGUI _speedometerText;
    [SerializeField] private TextMeshProUGUI _gameOverText;
    [SerializeField] private CanvasGroup _gameOverCanvasGroup;
    [SerializeField] private CanvasGroup _pauseCanvasGroup;

    private InputActionMap _playerActionMap;



    private CarHandler _playerCarHandler;
    private void Awake()
    {
        _playerCarHandler = GameObject.FindGameObjectWithTag("Player").GetComponent<CarHandler>();
        if( _playerCarHandler == null )
        {
            Debug.LogError("Player car not found in the scene!");
        }
        _playerCarHandler.OnPLayerCrashed += OnPlayerCrashed;
    }

    public void OnEnable()
    {
        _playerActionMap = InputSystem.actions.FindActionMap("Player");
        _playerActionMap.Enable();
        _playerActionMap.FindAction("Escape").performed += OnEscapePerformed;
    }

    private void OnEscapePerformed(InputAction.CallbackContext context)
    {
        CloseOrOpenPauseMenu();
    }

    private void CloseOrOpenPauseMenu()
    {
        if(_pauseCanvasGroup.interactable)
        {
            Time.timeScale = 1.0f;
            _pauseCanvasGroup.interactable = false;
            _pauseCanvasGroup.alpha = 0;
            _pauseCanvasGroup.gameObject.SetActive(false);
        }
        else
        {
            Time.timeScale = 0f;
            _pauseCanvasGroup.interactable = true;
            _pauseCanvasGroup.alpha = 1;
            _pauseCanvasGroup.gameObject.SetActive(true);
        }
    }

    IEnumerator _StartGameOverAnimationCoroutine()
    {
        yield return new WaitForSecondsRealtime(3.0f);

        _gameOverCanvasGroup.gameObject.SetActive(true);
        _gameOverCanvasGroup.interactable = true;

        while(_gameOverCanvasGroup.alpha < 0.8f)
        {
            _gameOverCanvasGroup.alpha += Mathf.MoveTowards(_gameOverCanvasGroup.alpha, 1f, Time.deltaTime);
            yield return null;
        }
    }

    private void OnPlayerCrashed(CarHandler handler)
    {
        _gameOverText.text = $"DISTANCE TRAVELLED: {_distanceTravelledText.text}" + $"\nRECORD: {PlayerPrefs.GetInt("Record", 0)}";

        StartCoroutine(_StartGameOverAnimationCoroutine());
    }

    public void OnRestartClick()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnMainMenuClick()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(0);
    }

    private void Start()
    {
        _gameOverCanvasGroup.interactable = false;
        _gameOverCanvasGroup.alpha = 0;
        _gameOverCanvasGroup.gameObject.SetActive(false);
        _pauseCanvasGroup.interactable = false;
        _pauseCanvasGroup.alpha = 0;
        _pauseCanvasGroup.gameObject.SetActive(false);
    }
    private void Update()
    {
        _distanceTravelledText.text = _playerCarHandler.DistanceTravelled.ToString("000000");
        _speedometerText.text = "KM/h: " + (int)_playerCarHandler.CurrentVelocity;
    }
    public void OnResume()
    {
        Time.timeScale = 1.0f;
        _pauseCanvasGroup.interactable = false;
        _pauseCanvasGroup.alpha = 0;
        _pauseCanvasGroup.gameObject.SetActive(false);
    }
}
