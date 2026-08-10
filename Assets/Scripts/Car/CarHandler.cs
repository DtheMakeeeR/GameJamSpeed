using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class CarHandler : MonoBehaviour
{
    [Header("Множители")]
    [SerializeField] private float _accelerationMultiplier = 3;
    [SerializeField] private float _brakeMultiplier = 15;
    [SerializeField] private float _steerMultiplier = 3;

    [Header("Поворот")]
    [SerializeField] private float _steerCoef = 5;
    [SerializeField] private float _maxRotation = 70f;
    [SerializeField] private float _maxRotSpeed = 3f;
    [SerializeField] private float _minRotSpeed = 6f;
    [SerializeField] private float _rotAccelerationTime = 1.5f;
    [SerializeField] private float _rotSpeed = 1f;
    

    [Header("Мин/Макс")]
    [SerializeField] private float _maxForwardVelocity;
    [SerializeField] private float _minForwardVelocity;
    [SerializeField] private float _maxSteerVelocity = 2;

    [Header("Ссылки")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Transform _gameModel;
    [SerializeField] private ExplodeHandler _explodeHandler;

    [Header("SFX")]
    [SerializeField] private AudioSource _carEngineAS;
    [SerializeField] private AudioSource _carSkidAS;
    [SerializeField] private AudioSource _carCrashAS;

    [SerializeField]
    private AnimationCurve _CarPitchAnimationCurve;

    private Vector2 _moveInput = Vector2.zero;
    private Vector2 _previousMoveInput = Vector2.zero;
    private float _carMaxSpeedPercentage = 0f;
    private float _currentRotationTime = 0f;
    private bool _isExploded = false;
    private float _carStrartPositionZ = 0f;
    private float _distanceTravelled = 0f;
    public event Action<CarHandler> OnPLayerCrashed;
    public float DistanceTravelled => _distanceTravelled;
    public float CurrentVelocity => _rb.linearVelocity.magnitude;
    public Gear Gear;
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null) Debug.LogWarning("No RB in CarHandler");
        _carStrartPositionZ = transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log($"Is exploded: {_isExploded}");
        if (_isExploded)
        {
            FadeOutCarVolume();
            return;
        }
        UpdateCarAudio();
        //_gameModel.transform.rotation = Quaternion.Euler(0, _rb.linearVelocity.x * 2f, 0);
        _distanceTravelled = transform.position.z - _carStrartPositionZ;
        if(_distanceTravelled >= PlayerPrefs.GetInt("Record", 0))
        {
            PlayerPrefs.SetInt("Record", (int)_distanceTravelled);
        }
    }

    private void ApplyRotation()
    {
        if (_rb.linearVelocity.z <=  0)
        {
            _currentRotationTime = 0f;
            return;
        }
        if(_previousMoveInput.x * _moveInput.x <= 0)
        {
            _currentRotationTime = 0f;
        }
        _currentRotationTime += Time.deltaTime;
        float rotationSpeed = Mathf.Lerp(_minRotSpeed, _maxRotSpeed, _currentRotationTime / _rotAccelerationTime);
        rotationSpeed *= Mathf.Clamp01(_rb.linearVelocity.z / 50);
        rotationSpeed = Mathf.Clamp(rotationSpeed, _minRotSpeed, _maxRotSpeed);
        float percentage = 1f; // _rb.linearVelocity.z / 250f;
        float rotationAngle = 0f;

        if (_moveInput.x > 0f)
        {
            rotationAngle = _maxRotation;
        }
        else if (_moveInput.x < 0f)
        {
            rotationAngle = -_maxRotation;
        }
        else
        {
            return;
        }

        // Поворачиваем объект
        Quaternion targetRot = Quaternion.Euler(0, rotationAngle, 0);
        Quaternion currentRot = transform.rotation;
        Quaternion newRot = Quaternion.Slerp(currentRot, targetRot, Time.deltaTime * rotationSpeed * percentage);
        transform.rotation = newRot;

        
        Quaternion deltaRotation = newRot * Quaternion.Inverse(currentRot);
        _rb.linearVelocity = deltaRotation * _rb.linearVelocity;

    }

    private void FixedUpdate()
    {
        if(_isExploded)
        {
            _rb.linearDamping = _rb.linearVelocity.z * 0.1f;
            _rb.linearDamping = Mathf.Clamp(_rb.linearDamping, 1.5f, 10f);

            _rb.MovePosition(Vector3.Lerp(transform.position, new Vector3(0, 0, transform.position.z), Time.deltaTime * .5f));

            return;
        }
        if(_moveInput.y > 0)
        {
            Acceleration();
        }
        else
        {
            //_rb.linearDamping = 1f;
            IdleMoving();
        }
        if (_moveInput.y < 0)
        {
            Brake();
        }
        ApplyRotation();
        //Steer();
        if (_rb.linearVelocity.z <= 0)
        {
            _rb.linearVelocity = Vector3.zero;
        }
        if (_rb.linearVelocity.z > Gear._maxForwardVelocity)
        {
            _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, new Vector3(_rb.linearVelocity.x, 0, Gear._maxForwardVelocity), Time.deltaTime*0.2f);
        }
    }

    private void Acceleration()
    {
        if(_rb.linearVelocity.z >= Gear._maxForwardVelocity)
        {
            return;
        }
        if (Gear._accelerationMultiplier < 1f) return;
        _rb.linearDamping = 0.0f;
        _rb.AddForce(_rb.transform.forward * Gear._accelerationMultiplier * _moveInput.y);
    }
    private void IdleMoving()
    {
        _rb.linearDamping = 0.4f;
        _rb.AddForce(_rb.transform.forward * Gear._minForwardVelocity/2);
    }
    private void Brake()
    {
        if(_rb.linearVelocity.z <= 0)
        {
            return;
        }
        _rb.AddForce(_rb.transform.forward * _brakeMultiplier * _moveInput.y);
    }
    
    private void Steer()
    {
        if(Mathf.Abs(_moveInput.x) > 0)
        {
            float speedBaseSteerLimit = _rb.linearVelocity.z / _steerCoef;
            speedBaseSteerLimit = Mathf.Clamp01(speedBaseSteerLimit);
            _rb.AddForce(_rb.transform.right * _moveInput.x * Gear._steerMultiplier * speedBaseSteerLimit);

            float normalizedSpeed = _rb.linearVelocity.x / Gear._maxSteerVelocity;

            normalizedSpeed = Mathf.Clamp(normalizedSpeed, -1.0f, 1.0f);

            _rb.linearVelocity = new Vector3(normalizedSpeed * Gear._maxSteerVelocity, 0, _rb.linearVelocity.z);
        }
        else
        {
            _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, new Vector3(0, 0, _rb.linearVelocity.z), Time.fixedDeltaTime * 7f);
        }
    }
    public void SetInput(Vector2 moveInput)
    {
        _previousMoveInput = _moveInput;
        _moveInput = moveInput.normalized;
        Debug.Log($"MoveInput: {_moveInput}");
    }


    private void UpdateCarAudio()
    {
        _carMaxSpeedPercentage = _rb.linearVelocity.z / 200;

        _carEngineAS.pitch = _CarPitchAnimationCurve.Evaluate(_carMaxSpeedPercentage);

        if(_moveInput.y < 0 && _carMaxSpeedPercentage > 0.2f)
        {
            if(!_carSkidAS.isPlaying)
            {
                _carSkidAS.Play();
            }
            _carSkidAS.volume = Mathf.Lerp(_carSkidAS.volume, 1.0f, Time.deltaTime * 10f);
        }
        else
        {
            _carSkidAS.volume = Mathf.Lerp(_carSkidAS.volume, 0, Time.deltaTime * 30f);
        }
    }

    private void FadeOutCarVolume()
    {
        _carEngineAS.volume = Mathf.Lerp(_carEngineAS.volume, 0, Time.deltaTime * 7f);
        _carSkidAS.volume = Mathf.Lerp(_carSkidAS.volume, 0, Time.deltaTime * 10f);
        _carCrashAS.volume = Mathf.Lerp(_carSkidAS.volume, 0, Time.deltaTime * 3f);
    }

    IEnumerator<float> _SlowDownTimeCoroutine()
    {
        while(Time.timeScale > 0.2f)
        {
            Time.timeScale -= Time.deltaTime*2;
            yield return Timing.WaitForOneFrame;
        }
        yield return Timing.WaitForSeconds(0.5f);
        while (Time.timeScale < 1.0f)
        {
            Time.timeScale += Time.deltaTime ;
            yield return Timing.WaitForOneFrame;
        }

        Time.timeScale = 1.0f;
    }




    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Collision with {collision.gameObject.name}");
        _explodeHandler.Explode(1000);
        _carCrashAS.volume = _carMaxSpeedPercentage;
        _carCrashAS.volume = Mathf.Clamp(0.25f, 1, _carCrashAS.volume);

        _carCrashAS.pitch = _carMaxSpeedPercentage;
        _carCrashAS.pitch = Mathf.Clamp(0.3f, 1, _carCrashAS.volume);

        _carCrashAS.Play();
        OnPLayerCrashed?.Invoke(this);
        Timing.RunCoroutine(_SlowDownTimeCoroutine().CancelWith(gameObject));
        _isExploded = true;
    }

    public bool ChangeGear(Gear gear)
    {
        if(_rb.linearVelocity.z < gear._minVelocityToChange)
        {
            return false;
        }
        Gear = gear;
        _rb.linearDamping = 0.4f;
        return true;
    }
}
