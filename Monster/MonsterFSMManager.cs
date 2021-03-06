﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MonsterState
{
    IDLE = 0,
    PATROL,
    CHASE,
    ATTACK,
    RAGEATTACK,
    RUNAWAY,
    REST,
    DEAD
}

[RequireComponent(typeof(MonsterStat))]
[ExecuteInEditMode]
public class MonsterFSMManager : MonoBehaviour, IFSMManager
{
    private bool _isinit = false;
    public MonsterState startState = MonsterState.IDLE;
    private Dictionary<MonsterState, MonsterFSMState> _states = new Dictionary<MonsterState, MonsterFSMState>();

    [SerializeField]
    private MonsterState _currentState;
    public MonsterState CurrentState
    {
        get
        {
            return _currentState;
        }
    }

    public MonsterFSMState CurrentStateComponent
    {
        get { return _states[_currentState]; }
    }

    private CharacterController _cc;
    public CharacterController CC { get { return _cc; } }

    private CharacterController _playercc;
    public CharacterController PlayerCC { get { return _playercc; } }

    private Transform _playerTransform;
    public Transform PlayerTransform { get { return _playerTransform; } }

    private MonsterStat _stat;
    public MonsterStat Stat { get { return _stat; } }

    private Animator _anim;
    public Animator Anim { get { return _anim; } }

    private Camera _sight;
    public Camera Sight { get { return _sight; } }

    private Transform _target;
    public Transform Target { get { return _target; } }

    private Transform _restPos;
    public Transform RestPos { get { return _restPos; } }

    public int sightAspect = 3;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _stat = GetComponent<MonsterStat>();
        _anim = GetComponentInChildren<Animator>();
        _sight = GetComponentInChildren<Camera>();

        _playercc = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterController>();
        _playerTransform = _playercc.transform;

        _target = GameObject.FindGameObjectWithTag("Player").transform;

        _restPos = GameObject.FindGameObjectWithTag("RestPos").transform;
        MonsterState[] stateValues = (MonsterState[])System.Enum.GetValues(typeof(MonsterState));
        foreach (MonsterState s in stateValues)
        {
            System.Type FSMType = System.Type.GetType("Monster" + s.ToString());
            MonsterFSMState state = (MonsterFSMState)GetComponent(FSMType);
            if (null == state)
            {
                state = (MonsterFSMState)gameObject.AddComponent(FSMType);
            }

            _states.Add(s, state);
            state.enabled = false;
        }

    }

    public void SetState(MonsterState newState)
    {
        if (_isinit)
        {
            _states[_currentState].enabled = false;
            _states[_currentState].EndState();
        }
        _currentState = newState;
        _states[_currentState].BeginState();
        _states[_currentState].enabled = true;
        _anim.SetInteger("CurrentState", (int)_currentState);
    }

    private void Start()
    {
        SetState(startState);
        _isinit = true;
    }

    private void OnDrawGizmos()
    {
        if (_sight != null)
        {
            Gizmos.color = Color.red;
            Matrix4x4 temp = Gizmos.matrix;

            Gizmos.matrix = Matrix4x4.TRS(
                _sight.transform.position,
                _sight.transform.rotation,
                Vector3.one
                );

            Gizmos.DrawFrustum(
                _sight.transform.position,
                _sight.fieldOfView,
                _sight.farClipPlane,
                _sight.nearClipPlane,
                _sight.aspect
                );

            Gizmos.matrix = temp;
        }
    }

    public void NotifyTargetKilled()
    {
        SetState(MonsterState.IDLE);
    }

    public void SetDeadState()
    {
        SetState(MonsterState.DEAD);
    }

    public void SetRunawayState()
    {
        SetState(MonsterState.RUNAWAY);
    }
}
