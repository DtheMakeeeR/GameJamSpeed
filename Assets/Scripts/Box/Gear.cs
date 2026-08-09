using UnityEngine;

[CreateAssetMenu(fileName = "Gear", menuName = "Scriptable Objects/Gear")]
public class Gear : ScriptableObject
{
    [Header("Множители")]
    public float _accelerationMultiplier;
    public float _steerMultiplier;

    [Header("Поворот")]
    public float _steerCoef;

    [Header("Мин/Макс")]
    public float _maxForwardVelocity;
    public float _minForwardVelocity;
    public float _maxSteerVelocity;
}
