using System;
using UnityEngine;



public class BoxHandler : MonoBehaviour
{
    [SerializeField] private GameObject _hand;
    [SerializeField] private float _placesDistance;
    [SerializeField] private GearPlace _currentGearPlace;
    [SerializeField] private CarHandler _car;
    [SerializeField] private AudioSource _hitAS;
    [SerializeField] private AudioSource _changeAS;
    private Vector2 _boxInput;
    private void Update()
    {
        //if(_boxInput.magnitude > 0)
        //    ChangeGear();
    }
    private void Awake()
    {
        TurnColliderOfCurrent(false);
        _car.ChangeGear(_currentGearPlace.GearInfo);
    }

    private void TurnColliderOfCurrent(bool flag)
    {
        _currentGearPlace.GetComponent<Collider>().enabled = flag;
    }

    public void ChangeGear()
    {
        Vector3 dir = new Vector3(_boxInput.x, _boxInput.y, 0);
        RaycastHit hit;

        if(Physics.Raycast(_hand.transform.position, dir, out hit))
        {
            GearPlace gearPl = hit.collider.GetComponent<GearPlace>();

            if (gearPl == null) 
            { 
                return;
            }
            if (!_car.ChangeGear(gearPl.GearInfo))
            {
                _hitAS.Play();
                return;
            }

            TurnColliderOfCurrent(true);
            _currentGearPlace = gearPl;
            TurnColliderOfCurrent(false);
            Vector3 newPos = new Vector3(hit.collider.transform.position.x,
                                         hit.collider.transform.position.y,
                                         _hand.transform.position.z);
            _hand.transform.position = newPos;
            _changeAS.Play();
        }
    }
    public void SetInput(Vector2 input)
    {
        _boxInput = input;
    }
}
