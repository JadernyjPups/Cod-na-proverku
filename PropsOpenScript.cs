using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropsOpenScript : MonoBehaviour //Попытка сделать открытие дверей, шкафчиков и выдвижных ящиков как в амнезии
{
    [Header("База")]
    [SerializeField] private Camera _cam;
    [SerializeField] private LayerMask _layerMask;
    [Space]
    [SerializeField] private float _throwForce;
    [Space]
    [SerializeField] private float _distanceToDisableGrab;

    [Header("Райкаст")]
    [SerializeField] private float _rayDistance;

    private Vector3 _mouseVelocity;
    private Vector3 _playerLastPosition;

    private float _distance;

    private RaycastHit _hit;
    private Ray _ray;

    private Rigidbody _currentProp;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float inputX = Input.GetAxis("Mouse X");
        float inputY = Input.GetAxis("Mouse Y");
        _mouseVelocity = new Vector3(inputX, inputY, 0);
        _ray = _cam.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));

        if (Input.GetButtonDown("Fire1"))
        {
            int layerMaskWithoutPlayer = ~_layerMask;
            if (Physics.Raycast(_ray, out _hit, _rayDistance, layerMaskWithoutPlayer) && _hit.transform.GetComponent<Rigidbody>() && (_hit.transform.tag == "PhysicDoor" || _hit.transform.tag == "PhysicDrawer" || _hit.transform.tag == "PhysicDverca"))
            {
                _currentProp = _hit.transform.GetComponent<Rigidbody>();
                transform.GetComponent<PlayerController>().mouselook._canLook = false;

                _playerLastPosition = transform.position;
            }
        }
        _distance = Vector3.Distance(transform.position, _playerLastPosition);

        ObshcheeIfElse();
    }

    private void InteractWithPhysicProp()
    {
        Vector3 vPushAmount = (_cam.transform.up + _cam.transform.forward) * _mouseVelocity.y + _cam.transform.right * _mouseVelocity.x;
        //Vector3 vPushRotateDir = Vector3.Cross(transform.position, -vPushAmount);
        //float fSpeedAdd = Vector3.Dot(vPushRotateDir, _currentProp.position);

        _currentProp.AddForce(vPushAmount);
        //_currentProp.AddForce(Vector3.forward * (fSpeedAdd*5));
    }
    
    private void SwingDoorOnThrow()
    {
        _currentProp.AddForce(_cam.transform.forward * _throwForce);
    }

    private void ObshcheeIfElse()
    {
        if (Input.GetButtonUp("Fire1") && _currentProp != null)
        {
            transform.GetComponent<PlayerController>().mouselook._canLook = true;
        }

        if (Input.GetButton("Fire1") && _currentProp != null && _distance <= _distanceToDisableGrab)
        {
            InteractWithPhysicProp();
            transform.GetComponent<PlayerController>().mouselook._canLook = false;

            if (_currentProp.transform.tag == "PhysicDoor")
            {
                _currentProp.GetComponent<DoorSettings>().isOtkryvajetsja = true;
                if (Input.GetButtonDown("Fire2"))
                {
                    SwingDoorOnThrow();
                }
            }
        }
        else
        {
            if (_currentProp != null && _currentProp.transform.tag == "PhysicDoor")
                _currentProp.GetComponent<DoorSettings>().isOtkryvajetsja = false;

            transform.GetComponent<PlayerController>().mouselook._canLook = true;
            _currentProp = null;
        }
    }
}
