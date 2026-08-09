using UnityEngine;
using MEC;
using System.Collections.Generic;

public class EndlessSectionHandler : MonoBehaviour
{
    private Transform _playerCarTransform;

    private void Start()
    {
        _playerCarTransform = GameObject.FindGameObjectWithTag("Player").transform;
        if (_playerCarTransform == null)
        {
            Debug.LogError("Player car not found in the scene!");
            return;
        }
    }

    private void OnEnable()
    {
        Timing.RunCoroutine(_UpdateLessOftenCoroutine().CancelWith(gameObject));
    }

    private IEnumerator<float> _UpdateLessOftenCoroutine()
    {
        while (true)
        {
            yield return Timing.WaitForSeconds(0.05f);
            UpdateSectionPosition();
        }
    }

    private void UpdateSectionPosition()
    {
        Debug.Log($"{gameObject.name} UpdateSectionPosition");
        float distanceToPlayer = transform.position.z - _playerCarTransform.position.z;
        //if(distanceToPlayer <= 0)
        //{
        //    Debug.LogWarning($"{gameObject.name} distance is {distanceToPlayer} no need to move");
        //    return;
        //}
        float lerpPrecentage = 1.0f - Mathf.InverseLerp(10, 100, distanceToPlayer/EndlessLevelHandler.Instance.CurveCoef);
        lerpPrecentage = Mathf.Clamp01(lerpPrecentage);
        transform.position = Vector3.Lerp(new Vector3(transform.position.x, EndlessLevelHandler.Instance.BelowPos, transform.position.z),
                                          new Vector3(transform.position.x, 0, transform.position.z), lerpPrecentage);
    }
}
