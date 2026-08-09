using UnityEngine;
using MEC;
using System.Collections.Generic;
public class EndlessObstaclesHandler : MonoBehaviour
{
    [Header("Префабы")]
    [SerializeField] private GameObject[] _obstaclesPrefabs;
    [SerializeField] private float _obstaclesLength = 648f;

    [Header("Параметры")]
    private GameObject[] _obstaclesPool = new GameObject[20];

    private GameObject[] _obstacles = new GameObject[10];

    private Transform _playerCarTransform;

    public float SectionLength => _obstaclesLength;

    public static EndlessObstaclesHandler Instance;
    private void Awake()
    {
        if (Instance != null) Destroy(this);
        Instance = this;
    }
    private void Start()
    {
        _playerCarTransform = GameObject.FindGameObjectWithTag("Player").transform;
        if (_playerCarTransform == null)
        {
            Debug.LogError("Player car not found in the scene!");
            return;
        }

        int prefabIndex = 0;
        //fill pool
        for (int i = 0; i < _obstaclesPool.Length; i++)
        {
            _obstaclesPool[i] = Instantiate(_obstaclesPrefabs[prefabIndex]);
            _obstaclesPool[i].gameObject.name = "Section " + i;
            _obstaclesPool[i].SetActive(false);

            prefabIndex++;
            if (prefabIndex > _obstaclesPrefabs.Length - 1)
            {
                prefabIndex = 0;
            }
        }
        //make visible ones
        for (int i = 0; i < _obstacles.Length; i++)
        {
            if(i>0)
            {
                _obstacles[i] = GetRandomObstaclesFromPool();
            }
            else
            {
                _obstacles[i] = _obstaclesPool[0];
            }
            _obstacles[i].SetActive(true);
            _obstacles[i].transform.position = new Vector3(0, 0, i * _obstaclesLength);
        }

        Timing.RunCoroutine(_UpdateLessOftenCoroutine().CancelWith(gameObject));
    }

    private IEnumerator<float> _UpdateLessOftenCoroutine()
    {
        while (true)
        {
            yield return Timing.WaitForSeconds(0.1f);
            UpdateObstaclesPositions();
        }
    }

    private void UpdateObstaclesPositions()
    {
        for (int i = 0; i < _obstacles.Length; i++)
        {
            if (_obstaclesLength < _playerCarTransform.position.z - _obstacles[i].transform.position.z)
            {
                Vector3 lastSectionPosition = _obstacles[i].transform.position;
                _obstacles[i].SetActive(false);

                _obstacles[i] = GetRandomObstaclesFromPool();

                _obstacles[i].transform.position = new Vector3(lastSectionPosition.x, 0, lastSectionPosition.z + _obstaclesLength * _obstacles.Length);
                if(Random.value > 0.5)
                {
                    _obstacles[i].transform.rotation = Quaternion.Euler(0, _obstacles[i].transform.rotation.y +  180, 0);
                }                
                _obstacles[i].SetActive(true);
            }
        }
    }

    private GameObject GetRandomObstaclesFromPool()
    {
        int randomIndex = Random.Range(0, _obstaclesPool.Length);

        bool isNewSectionFound = false;

        while (!isNewSectionFound)
        {
            if (!_obstaclesPool[randomIndex].activeInHierarchy)
            {
                isNewSectionFound = true;
            }
            else
            {
                randomIndex++;
                if (randomIndex > _obstaclesPool.Length - 1)
                {
                    randomIndex = 0;
                }
            }

        }

        return _obstaclesPool[randomIndex];
    }
}
