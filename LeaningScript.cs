using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaningScript : MonoBehaviour //Попытка сделать выглядывание изо угла как в Alien Isolation
{
    [Header("Флажки")]
    [SerializeField] private bool _canLeftLean;
    [SerializeField] private bool _canRightLean;
    [SerializeField] private bool _canFwdLean;
    [SerializeField] private bool _canUpPos;

    [Header("Ссылки")]
    [SerializeField] private PlayerController _playeController;
    [SerializeField] private Transform _raycastTransform;
    [SerializeField] private LayerMask _layerMask;

    [Header("База")]
    [SerializeField] private Transform _bodyTransform;
    [Space]
    [SerializeField] private float _leanRayDistance;
    [Space]
    [SerializeField] private float _newVerh;
    [SerializeField] private float _newNiz;

    [Header("Поворот")]
    [SerializeField] private float _smoothResetRot;
    [SerializeField] private float _smoothResetPos;
    [Space]
    [SerializeField] private float _smoothRot;
    [SerializeField] private float _smoothPos;
    [Space]
    [SerializeField] private float _maxBodyRotationSide;
    [SerializeField] private float _maxBodyRotationForward;
    [Space]
    [SerializeField] private float _forwardBodyPos;
    [SerializeField] private float _backwardBodyPos;
    [SerializeField] private float _horizontalBodyPos;

    private float _oldVerh;
    private float _oldNiz;

    private float _verticalBodyPos;
    private float customHorizontalAxis;
    private float customVerticalAxis;
    private float customUpPos;

    private Vector3 _startPos;
    private Vector3 _startRot;

    private void Awake()
    {
        _startPos = _bodyTransform.localPosition;
    }

    private void Update()
    {
        float _horizontalAxis = -Input.GetAxisRaw("Horizontal");
        float _verticalAxis = Input.GetAxisRaw("Vertical");

        Vector3 angle = new Vector3(Mathf.Clamp(customVerticalAxis * _maxBodyRotationForward, 0, 100), 0, customHorizontalAxis * _maxBodyRotationSide);
        Vector3 customPos = new Vector3(-customHorizontalAxis * _horizontalBodyPos, _startPos.y + (customUpPos * _verticalBodyPos), 0);

        if (Input.GetKey(KeyCode.LeftControl))
        {
            _playeController.canMoveWithOutCamera = false;

            if (!_playeController._isCrouching)
            {
                _bodyTransform.localRotation = Quaternion.Lerp(_bodyTransform.localRotation, Quaternion.Euler(angle), Time.deltaTime * _smoothRot);
                _bodyTransform.localPosition = Vector3.Lerp(_bodyTransform.localPosition, customPos, Time.deltaTime * _smoothPos);
            }
            else
            {
                _bodyTransform.localRotation = Quaternion.Lerp(_bodyTransform.localRotation, Quaternion.Euler(new Vector3(0,0, angle.z)), Time.deltaTime * _smoothRot);
                _bodyTransform.localPosition = Vector3.Lerp(_bodyTransform.localPosition, customPos, Time.deltaTime * _smoothPos);
            }
        }
        else
        {
            _bodyTransform.localRotation = Quaternion.RotateTowards(_bodyTransform.localRotation, Quaternion.Euler(_startRot), Time.deltaTime * _smoothResetRot);
            _bodyTransform.localPosition = Vector3.MoveTowards(_bodyTransform.localPosition, _startPos, Time.deltaTime * _smoothResetPos);
            _playeController.canMoveWithOutCamera = true;
        }

        #region -Raycast-
        RaycastHit hit;
        int layerMaskWithoutPlayer = ~_layerMask;
        if (Physics.Raycast(_raycastTransform.position, _raycastTransform.TransformDirection(Vector3.left * _leanRayDistance), out hit, _leanRayDistance, layerMaskWithoutPlayer))
            _canLeftLean = false;
        else
            _canLeftLean = true;

        if (Physics.Raycast(_raycastTransform.position, _raycastTransform.TransformDirection(Vector3.right * _leanRayDistance), out hit, _leanRayDistance, layerMaskWithoutPlayer))
            _canRightLean = false;
        else
            _canRightLean = true;

        if (Physics.Raycast(_raycastTransform.position, _raycastTransform.TransformDirection(Vector3.forward * _leanRayDistance), out hit, _leanRayDistance, layerMaskWithoutPlayer))
            _canFwdLean = false;
        else
            _canFwdLean = true;

        if (Physics.Raycast(_raycastTransform.position, _raycastTransform.TransformDirection(Vector3.up * _leanRayDistance), out hit, _leanRayDistance, layerMaskWithoutPlayer))
            _canUpPos = false;
        else
            _canUpPos = true;
        #endregion
        #region -IfElse-
        if (_canLeftLean && _canRightLean)
        {
            customHorizontalAxis = Mathf.Clamp(_horizontalAxis, -2, 2);
        }
        else if (!_canLeftLean && _canRightLean)
        {
            customHorizontalAxis = Mathf.Clamp(_horizontalAxis, -2, 0);
        }
        else if (_canLeftLean && !_canRightLean)
        {
            customHorizontalAxis = Mathf.Clamp(_horizontalAxis, 0, 2);
        }
        else if (!_canLeftLean && !_canRightLean)
        {
            customHorizontalAxis = Mathf.Clamp(_horizontalAxis, 0, 0);
        }

        if (_verticalAxis > 0)
        {
            _verticalBodyPos = _forwardBodyPos;
        }
        else if (_verticalAxis < 0)
        {
            _verticalBodyPos = _backwardBodyPos;
        }
        else if (_verticalAxis == 0)
        {
            _verticalBodyPos = 0;
        }

        if (_canFwdLean)
        {
            customVerticalAxis = Mathf.Clamp(_verticalAxis, 0, 2);
        }
        else
        {
            customVerticalAxis = Mathf.Clamp(_verticalAxis, 0, 0);
        }

        if (_canUpPos)
        {
            customUpPos = Mathf.Clamp(_verticalAxis, -2, 2);
        }
        else
        {
            customUpPos = Mathf.Clamp(_verticalAxis, -2, 0);
        }
        #endregion
    }
}
