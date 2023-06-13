using UnityEngine;

public class HeadBobControllerVer2 : MonoBehaviour
{
    [Header("Bools")]
    [SerializeField] public bool _enabled;
    [SerializeField] public bool _enabledPos;

    [Header("Main")]
    [SerializeField] private CharacterController _player;
    [SerializeField] private float _strafeRotate;

    [Header("Бездействие")]
    [SerializeField] private float _noiseAmount;
    [SerializeField] private float _maxNoiseJitter;
    [SerializeField] private float _noiseSpeed;
    [Space]
    [SerializeField] private Vector3 RotNoiseAmplitude;

    [Header("Ходьба")]
    [SerializeField] private float _rotAmountWalk = 0.00484f;
    [SerializeField, Range(0f, 30f)] private float _rotFrequencyWalk = 16.0f;
    [Space]
    [SerializeField] private float _posAmountWalk = 0.00484f;
    [SerializeField, Range(0f, 30f)] private float _posFrequencyWalk = 16.0f;
    
    [Header("Бег")]
    [SerializeField] private float _rotAmountRun = 0.00484f;
    [SerializeField, Range(0f, 30f)] private float _rotFrequencyRun = 16.0f;
    [SerializeField, Range(10f, 100f)] private float _rotSmooth = 44.7f;
    [Space]
    [SerializeField] private float _returnSmooth = 10f;
    [SerializeField] private float _returnSmoothPos = 10f;
    [Space]
    [SerializeField] private float _posAmountRun = 0.00484f;
    [SerializeField, Range(0f, 30f)] private float _posFrequencyRun = 16.0f;
    [SerializeField, Range(10f, 100f)] private float _posSmooth = 44.7f;

    [Header("Roation Obsee")]
    [SerializeField, Range(40f, 4f)] private float RoationMovementSmooth = 10.0f;
    [SerializeField, Range(1f, 10f)] private float RoationMovementAmount = 3.0f;

    private float ToggleSpeed = 2.5f;
    private Vector3 StartPos;
    private Vector3 FinalRot;
    private Vector3 FinalNoise;

    private Vector3 _currentRot;

    private float movementRotFrequency;
    private float movementPosFrequency;

    private float _speed;

    private void Awake()
    {
        StartPos = transform.localPosition;
    }

    private void Update()
    {
        if (!_enabled) return;
        CheckMotion();
        ResetPosAndRot();

        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(_currentRot), RoationMovementSmooth * Time.deltaTime);

        if (_speed >= 4)
        {
            movementRotFrequency = new Vector3(_player.velocity.x, 0, _player.velocity.z).magnitude * _rotFrequencyRun;
            movementPosFrequency = new Vector3(_player.velocity.x, 0, _player.velocity.z).magnitude * _posFrequencyRun;
        }
        else
        {
            movementRotFrequency = new Vector3(_player.velocity.x, 0, _player.velocity.z).magnitude * _rotFrequencyWalk;
            movementPosFrequency = new Vector3(_player.velocity.x, 0, _player.velocity.z).magnitude * _posFrequencyWalk;
        }
        if (_speed < ToggleSpeed && !_player.GetComponent<PlayerController>()._isWalking)
        {
            _currentRot = FinalNoise;
        }
        else
        {
            _currentRot = FinalRot;
        }

    }

    private void CheckMotion()
    {
        _speed = new Vector3(_player.velocity.x, 0, _player.velocity.z).magnitude;
        //if (speed < ToggleSpeed) return;
        if (!_player.isGrounded) return;
        if (_speed < ToggleSpeed)
        {
            PlayIdleMotion(IdleMotionRot());
        }
        else if(_speed > ToggleSpeed && _player.GetComponent<PlayerController>()._isWalking && _player.GetComponent<PlayerController>().canMove && _player.GetComponent<PlayerController>().canMoveWithOutCamera)
        {
            PlayMotionRot(HeadBobMotionRot(), HeadBobMotionPos());
        }
    }

    #region -Bezdejstvije-

    private void PlayIdleMotion(Vector3 noise)
    {
        FinalNoise += noise * RoationMovementAmount;
    }

    private Vector3 IdleMotionRot()
    {
        Vector3 rot = Vector3.zero;
        rot.x += Mathf.Lerp(rot.x, Mathf.Cos(Time.time * _noiseSpeed) * RotNoiseAmplitude.x, _rotSmooth * Time.deltaTime);
        //rot.y += Mathf.Lerp(rot.y, Mathf.Sin(Time.time * _noiseSpeed) * RotNoiseAmplitude.y, _rotSmooth * Time.deltaTime);
        rot.z += Mathf.Lerp(rot.z, Mathf.Cos(Time.time * (_noiseSpeed + 0.5f)) * RotNoiseAmplitude.z, _rotSmooth * Time.deltaTime);

        //rot.x += (Mathf.PerlinNoise(Random.Range(0, _maxNoiseJitter), Time.time * _noiseSpeed) - 0.5f) * RotNoiseAmplitude.x / 10;
        //rot.y += (Mathf.PerlinNoise(Random.Range(0, _maxNoiseJitter), Time.time * _noiseSpeed) - 0.5f) * RotNoiseAmplitude.y / 10;
        //rot.z += (Mathf.PerlinNoise(Random.Range(0, _maxNoiseJitter), Time.time * _noiseSpeed) - 0.5f) * RotNoiseAmplitude.z / 10;
        return rot;
    }

    #endregion

    #region -Khod'ba-
    private void PlayMotionRot(Vector3 Movement, Vector3 MovementPos)
    {
        //transform.localRotation *= Quaternion.Euler(Movement);
        if(_enabledPos)
        transform.localPosition += MovementPos;
        float _horizontalAxis = -Input.GetAxisRaw("Horizontal") * _strafeRotate;

        FinalRot += new Vector3(0, 0, Movement.z) * RoationMovementAmount;
    }

    private Vector3 HeadBobMotionRot()
    {
        Vector3 rot = Vector3.zero;
        //rot.x += Mathf.Lerp(rot.x, Mathf.Cos(Time.time * Frequency / 2f) * Amount, Smooth * Time.deltaTime);
        if (_speed >= 4)
        {
            rot.z += Mathf.Lerp(rot.z, Mathf.Cos(Time.time * movementRotFrequency / 2f) * _rotAmountRun * 2f, _rotSmooth * Time.deltaTime);
        }
        else
        {
            rot.z += Mathf.Lerp(rot.z, Mathf.Cos(Time.time * movementRotFrequency / 2f) * _rotAmountWalk * 2f, _rotSmooth * Time.deltaTime);
        }
        //rot.x = Mathf.Clamp(rot.x, -10, 0);
        return rot;
    }
    private Vector3 HeadBobMotionPos()
    {
        Vector3 pos = Vector3.zero;
        if (_speed >= 4)
        {
            pos.y += Mathf.Lerp(pos.y, Mathf.Cos(Time.time * movementPosFrequency) * _posAmountRun * 1.4f, _posSmooth * Time.deltaTime);
        }
        else
        {
            pos.y += Mathf.Lerp(pos.y, Mathf.Cos(Time.time * movementPosFrequency) * _posAmountWalk * 1.4f, _posSmooth * Time.deltaTime);
        }
        return pos;
    }
    #endregion

    private void ResetPosAndRot()
    {
        if (Mathf.Approximately(transform.localRotation.z, 0) && Mathf.Approximately(transform.localPosition.y, 0)) return;
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, StartPos, _returnSmoothPos * Time.deltaTime);
        FinalRot = Vector3.Lerp(FinalRot, new Vector3(0,0,0), _returnSmooth * Time.deltaTime);
        FinalNoise = Vector3.Lerp(FinalNoise, new Vector3(0,0,0), _returnSmooth * Time.deltaTime);
    }
}
