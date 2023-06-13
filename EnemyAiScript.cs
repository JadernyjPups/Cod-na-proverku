using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAiScript : MonoBehaviour
{
    [Header("������")]
    [SerializeField] public bool canChangeThePoints;
    [SerializeField] public bool _playerDetected; //����� ��������� � ���� ���������
    [SerializeField] private bool _followPlayer; //��� �� �������� �� �������

    [Header("����")]
    [SerializeField] public Transform _playerTransform; //��������� ������, ����������, ����� �� ��������� � ����
    [SerializeField] private Transform _targetTransform; //�����, �� ������� ��������� ��� ��-���
    [SerializeField] private List<Transform> _waypoints; //�����, �� ������� ����� ������ ��-���

    [Header("��������� ��-���")]
    [SerializeField] private float _walkSpeed, _runSpeed;
    [Space]
    [SerializeField] private float _stopDistanceMin, _stopDistanceMax; //1-� ��� ��������� ���������, ����� ����� �� � ���� ������, 2-� �������������, ����� ����� � ���� ������ 
    [Space]
    [SerializeField] private float _stayTime; //�����, ������� ��-��� ����� ������ �� �����
    [SerializeField] private float _followTime; //�����, ������� ��-��� ����� ���� �� ������� ����� ����, ��� �� ����� �� ������� ���� ������

    [Header("��������")]
    [SerializeField] public Transform _fovTarget; //�����, ������� ����� ������� ������� ����� ��������
    [Space]
    [SerializeField] public float _fovRadius;
    [SerializeField] public float _fovAngle;
    [Space]
    [SerializeField] private LayerMask _targetMask;
    [SerializeField] private LayerMask _otherMask;

    private int _iTarget;
    private float _currentSpeed;

    private bool _deloyTargetUpdate; //�������� ����� ����������� �����
    private bool _followDeloy; //�������� ����� ���, ��� ��������� ������� �� �������

    private Animator _enemyAnim;
    private NavMeshAgent _enemyAgent;

    private Vector2 _velocity;
    private Vector2 _smoothDeltaPosition;

    #region -Built-in-methods-

    #region -Awake'n'Start-
    private void Awake()
    {
        _enemyAnim = GetComponent<Animator>();
        _enemyAgent = GetComponent<NavMeshAgent>();
        _enemyAgent.updatePosition = false;

        _stopDistanceMin = _enemyAgent.stoppingDistance; //������ �������� ��� �������, ���� ��������
    }

    private void Start()
    {
        StartCoroutine(FOVRputine());
    }
    #endregion

    #region -Update-
    private void Update()
    {
        AnimSync();

        _enemyAgent.speed = _currentSpeed;
        float distance = Vector3.Distance(_enemyAgent.transform.position, _targetTransform.position); //���������� ����� ��-���� � �������� ������
        #region -If'n'Else-
        if (distance < 1 && !_deloyTargetUpdate && !_playerDetected && !_followPlayer)
        {
            _deloyTargetUpdate = true;

            if(canChangeThePoints)
            StartCoroutine(UpdateTargetPoint());
        }
        
        if(distance < 1.5 && _playerDetected)
        {
            _enemyAnim.SetBool("canAttack", true);
        }
        else
        {
            _enemyAnim.SetBool("canAttack", false);
        }

        if (!_deloyTargetUpdate)
            _enemyAgent.SetDestination(_targetTransform.position); //���������� ���� ����� �� ������

        if (!_followPlayer)
        {
            _currentSpeed = _walkSpeed;
            _enemyAgent.stoppingDistance = _stopDistanceMin;
        }
        else
        {
            _targetTransform.position = _playerTransform.position;
            _enemyAgent.stoppingDistance = _stopDistanceMax;
            _currentSpeed = _runSpeed;
        }

        if(!_playerDetected && _followPlayer && !_followDeloy)
        {
            StartCoroutine(FollowThePlayerDeloy());
        }

        if (_playerDetected)
        {
            _followPlayer = true;
        }
        #endregion
    }
    #endregion

    #endregion

    #region -Custom-methods-

    #region -Animations-
    private void AnimSync() //��� ���������� � ������� ��������� :/
    {
        Vector3 worldPos = _enemyAgent.nextPosition - transform.position;
        worldPos.y = 0;

        float dx = Vector3.Dot(transform.right, worldPos);
        float dy = Vector3.Dot(transform.forward, worldPos);
        Vector2 deltaPos = new Vector2(dx, dy);

        float smooth = Mathf.Min(1, Time.deltaTime / 0.1f);
        _smoothDeltaPosition = Vector2.Lerp(_smoothDeltaPosition, deltaPos, smooth);

        _velocity = _smoothDeltaPosition / Time.deltaTime;
        if(_enemyAgent.remainingDistance <= _enemyAgent.stoppingDistance)
        {
            _velocity = Vector2.Lerp(Vector2.zero, _velocity, _enemyAgent.remainingDistance / _enemyAgent.stoppingDistance);
        }

        bool shouldMove = _velocity.magnitude > 0.5f && _enemyAgent.remainingDistance > _enemyAgent.stoppingDistance;

        float deltaMagnitube = worldPos.magnitude;
        if(deltaMagnitube > _enemyAgent.radius / 2f)
        {
            transform.position = Vector3.Lerp(_enemyAnim.rootPosition, _enemyAgent.nextPosition, smooth);
        }

        _enemyAnim.SetFloat("Vertical", _velocity.magnitude);
        _enemyAnim.SetBool("isWalking", shouldMove);
        _enemyAnim.SetBool("isRunning", _playerDetected);
    }

    private void OnAnimatorMove()
    {
        Vector3 rootPos = _enemyAnim.rootPosition;
        rootPos.y = _enemyAgent.nextPosition.y;
        transform.position = rootPos;
        _enemyAgent.nextPosition = rootPos;
    }
    #endregion

    #region -Coroutines-
    private IEnumerator UpdateTargetPoint()
    {
        if (_deloyTargetUpdate == false || _playerDetected) yield break;

        yield return new WaitForSeconds(_stayTime);
        //������������
        _iTarget = Random.Range(0, _waypoints.Count);
        _targetTransform.position = _waypoints[_iTarget].position;

        _deloyTargetUpdate = false;
    }

    private IEnumerator FollowThePlayerDeloy()
    {
        if(_playerDetected) yield break;
        _followDeloy = true;
        yield return new WaitForSeconds(_followTime);
        _followDeloy = false;
        _followPlayer = false;
    }

    #endregion

    #region -AiFov-
    private IEnumerator FOVRputine()
    {
        float delay = 0.2f;
        WaitForSeconds wait = new WaitForSeconds(delay);
        while (true)
        {
            yield return wait;
            FovCheck();
        }
    }

    private void FovCheck()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(_fovTarget.position, _fovRadius, _targetMask);

        if (rangeChecks.Length != 0)
        {
            Transform target = rangeChecks[0].transform;
            Vector3 directionalToTarget = (target.position - _fovTarget.position).normalized;

            if (Vector3.Angle(_fovTarget.forward, directionalToTarget) < _fovAngle / 2)
            {
                float distanceToTarget = Vector3.Distance(_fovTarget.position, target.position);

                if (!Physics.Raycast(_fovTarget.position, directionalToTarget, distanceToTarget, _otherMask))
                {
                    _followPlayer = true;
                    _playerDetected = true;
                }
                else
                {
                    _playerDetected = false;
                }
            }
            else
            {
                _playerDetected = false;
            }
        }
        else if (_playerDetected)
        {
            _playerDetected = false;
        }
    }
    #endregion

    #endregion
}