using UnityEngine;

public class ExplodeHandler : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField]
    private GameObject _originalObject;
    [SerializeField]
    private GameObject _rootModel;
    [SerializeField]
    private Transform _explosionPos;

    [Header("Параметры")]
    [SerializeField]
    private float _explosionRadius;

    

    private Rigidbody[] _rigidBodies;

    private void Awake()
    {
        _rigidBodies = _rootModel.GetComponentsInChildren<Rigidbody>(true);
    }


    private void Start()
    {
       // Explode(50);
    }

    public void Explode(float explosionForce)
    {
        _originalObject.SetActive(false);

        foreach (var rb in _rigidBodies)
        {
            rb.transform.parent = null;

            rb.GetComponent<MeshCollider>().enabled = true;

            rb.gameObject.SetActive(true);

            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.AddExplosionForce(explosionForce, _explosionPos.position, _explosionRadius, 10f);
        }
    }
}
