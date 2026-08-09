using UnityEngine;
using MEC;
using System.Collections.Generic;
public class EndlessLevelHandler : MonoBehaviour
{
    [Header("Префабы")]
    [SerializeField] private GameObject[] _sectionsPrefabs;
    [SerializeField] private float _sectionLength = 54f;

    [Header("Параметры")]
    [SerializeField]
    private float _belowPos = -10;
    [SerializeField]
    private float _curveCoef = 10;
    private GameObject[] _sectionsPool = new GameObject[20];

    private GameObject[] _sections = new GameObject[10];

    private Transform _playerCarTransform;

    public float BelowPos => _belowPos;
    public float SectionLength => _sectionLength;
    public float CurveCoef => _curveCoef;

    public static EndlessLevelHandler Instance;
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
        for (int i = 0; i < _sectionsPool.Length; i++)
        {
            _sectionsPool[i] = Instantiate(_sectionsPrefabs[prefabIndex]);
            _sectionsPool[i].gameObject.name = "Section " + i;
            _sectionsPool[i].SetActive(false);

            prefabIndex++;
            if(prefabIndex > _sectionsPrefabs.Length - 1)
            {
                prefabIndex = 0;
            }
        }
        //make visible ones
        for(int i = 0; i < _sections.Length; i++)
        {
            _sections[i] = GetRandomSectionFromPool();
            _sections[i].SetActive(true);
            _sections[i].transform.position = new Vector3(0, BelowPos, i * _sectionLength);
        }

        Timing.RunCoroutine(_UpdateLessOftenCoroutine().CancelWith(gameObject));
    }

    private IEnumerator<float> _UpdateLessOftenCoroutine()
    {
        while(true)
        {
            yield return Timing.WaitForSeconds(0.1f);
            UpdateSectionsPositions();
        }    
    }

    private void UpdateSectionsPositions()
    {
        for(int i = 0; i < _sections.Length; i++)
        {
            if (_sectionLength < _playerCarTransform.position.z - _sections[i].transform.position.z)
            {
                Vector3 lastSectionPosition = _sections[i].transform.position;
                _sections[i].SetActive(false);

                _sections[i] = GetRandomSectionFromPool();

                _sections[i].transform.position = new Vector3(lastSectionPosition.x, BelowPos, lastSectionPosition.z + _sectionLength * _sections.Length);
                _sections[i].SetActive(true);
            }
        }
    }

    private GameObject GetRandomSectionFromPool()
    {
        int randomIndex = Random.Range(0, _sectionsPool.Length);

        bool isNewSectionFound = false;

        while (!isNewSectionFound)
        {
            if (!_sectionsPool[randomIndex].activeInHierarchy)
            {
                isNewSectionFound = true;
            }
            else
            {
                randomIndex++;
                if (randomIndex > _sectionsPool.Length - 1)
                {
                    randomIndex = 0;
                }
            }
            
        }

        return _sectionsPool[randomIndex];
    }
}
