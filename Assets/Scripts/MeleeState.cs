using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class MeleeState : BaseState
{
    [SerializeField] private GameObject _playerRef;

    // El GameObject que es dueño de la máquina de estados que es dueña de este estado.
    // A través de esta referencia nosotros podemos leer o cambiar las variables necesarias de nuestro dueño. 
    private BossEnemy _enemyOwner;

    private float _lastBasicAttackTime;
    private float _lastAreaAttackTime;
    private float _lastDashAttackTime;
    private float _lastUltimateAttackTime;


    private int _meleeBasicAttackAnimatorHash;
    private int _meleeDashAttackAnimatorHash;
    private int _meleeAreaAttackAnimatorHash;
    private int _meleeUltimateAttackAnimatorHash;
    
    // Cuáles acciones tiene disponibles este estado?
    public enum MeleeActions
    {
        BasicMeleeAttack,
        DashMeleeAttack,
        AreaMeleeAttack,
        MeleeUltimateAttack
    }

    private List<MeleeActions> _pastActions = new List<MeleeActions>();

    public void Initialize(BaseFSM ownerFSM, BossEnemy enemyOwner, GameObject playerRef)
    {
        OwnerFSMRef = ownerFSM;
        _enemyOwner = enemyOwner;
        _playerRef = playerRef;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StateName = "MeleeState";
        _meleeBasicAttackAnimatorHash = Animator.StringToHash("BasicAttack");
        _meleeDashAttackAnimatorHash = Animator.StringToHash("DashAttack");
        _meleeAreaAttackAnimatorHash = Animator.StringToHash("AreaAttack");
        _meleeUltimateAttackAnimatorHash = Animator.StringToHash("UltimateAttack");
    }

    public override void OnUpdate()
    {
        // Acercarnos al target de nuestro dueño y atacarlo cuando estemos en un rango válido.
        // A nuestro dueño le decimos que se acerque a su Player objetivo.
        _enemyOwner.GetNavMeshAgent().destination = _playerRef.transform.position;

        // EJEMPLOTE:
        // Si después de todas las acciones de un combo X, el player no sufrió nada de daño
        // entonces, aplicar ataque especial Y.
        // if (_pastActions.Contains(MeleeActions.BasicMeleeAttack) &&
        //     _pastActions.Contains(MeleeActions.DashMeleeAttack) &&
        //     _pastActions.Contains(MeleeActions.AreaMeleeAttack) &&
        //     _pastActions.Contains(MeleeActions.MeleeUltimateAttack))
        // {
        //     if (_playerRef.hp == playerHpBeforeComboStart)
        //     {
        //         // entonces, aplicar ataque especial MegaUltimateWow
        //         if (tryMegaUltimateWowAttack())
        //             return;
        //     }
        // }
        
        
        // Primero tenemos que checar que SÍ haya una acción pasada antes de checar si la acción pasada fue un melee básico.
        if (_pastActions.Count > 0)
        {
            if (_pastActions.Contains(MeleeActions.AreaMeleeAttack) &&
                _pastActions.Contains(MeleeActions.DashMeleeAttack))
            {
                // entonces ya podemos hacer el ultimate.
                if (TryUltimateAttack())
                {
                    // La clave para que esto funcione bien es que esta transición se haga DESPUÉS de terminar el 
                    // ultimate.
                    
                    // Después de hacer el ultimate, hacemos el cambio de estado al estado Ranged.
                    EnemyFSM ownerFsm = (EnemyFSM)OwnerFSMRef;
                    OwnerFSMRef.ChangeState(ownerFsm.GetRangedState());
                    return;
                }
            }
            
            if (_pastActions[_pastActions.Count - 1] == MeleeActions.BasicMeleeAttack)
            {
                // Tenemos 2 casos
                // Caso 1) ya se hizo el dash o area hace 2 acciones
                if (_pastActions.Count > 1)
                {
                    // ya con esto, podemos decidir si hacer un dash o uno de área
                    // (_pastActions.FindLastIndex(is MeleeActions.DashMeleeAttack) <=  _pastActions.Count - 2) ) // esta es más flexible
                    if (_pastActions[_pastActions.Count - 2] ==  MeleeActions.DashMeleeAttack)
                    {
                        // Entonces hacemos el de área
                        if (TryAreaAttack())
                            return;
                    }
                    else if (_pastActions[_pastActions.Count - 2] == MeleeActions.AreaMeleeAttack)
                    {
                        // entonces hacemos el dash   
                        if (TryDashAttack())
                            return;
                    }
                }
                // Caso 2) no se ha usado ni el Dash ni el de área, entonces ambos tienen un 50% de probabilidad.
                else
                {
                    // hacemos un random de 50/50 para elegir si se hace un ataque de área o un dash.
                    int rand = Random.Range(0, 2); // random entre 0 y 1
                    if (rand == 1)
                    {
                        // Si sí se ejecutó esta acción, salir del método OnUpdate para no poder ejecutar ninguna otra acción.
                        if (TryAreaAttack())
                            return;
                    }

                    if (rand == 0) // ataque dash
                    {
                        if (TryDashAttack())
                            return;
                    }
                }
            }
        }

        if (TryBasicMeleeAttack())
            return;

        // Debug.Log(" " + 1);
        // toma ese '1' y lo convierte en un caracter temporalmente, y así lo puede interpretar de otra manera.

        // Debug.Log("Update melee state");

        if (Vector3.Distance(transform.position, _playerRef.transform.position) >= 10)
        {
            EnemyFSM enemyFsm = (EnemyFSM)OwnerFSMRef;
            OwnerFSMRef.ChangeState(enemyFsm.GetIdleState());
            return; // La regla es: tú pones change state? la siguiente línea debe ser return.
        }

    }

    public void BeginMeleeBasicAttackActiveFrames()
    {
        Debug.LogWarning("BeginMeleeBasicAttackActiveFrames");
    }
    
    public void EndMeleeBasicAttackActiveFrames()
    {
        Debug.LogWarning("EndMeleeBasicAttackActiveFrames");
    }

    // Tiempo en que se usó este ataque la última vez.
    // la cadencia/ritmo en que se puede hacer este ataque
    private bool TryAttack(ref float lastAttackTime, float attackRate, float attackRange, MeleeActions actionType, 
        int attackTriggerHash)
    {
        // checar si el ataque ya se puede volver a realizar.
        if (lastAttackTime + attackRate < Time.time)
        {
            // Hay que checar si ya estamos en rango válido para hacer este ataque.
            // checamos si la distancia entre este agente y el player es menor o igual que el rango de nuestro ataque.
            if (Vector3.Distance(transform.position, _playerRef.transform.position) <=
                attackRange)
            {
                // si esa condición se cumple, entonces puedo atacar con ese ataque melee a ese player.
                Debug.Log(
                    $"el agente: {gameObject.name} atacó al player {_playerRef.name} con un ataque melee de {actionType}.");

                _enemyOwner.GetAnimator().SetTrigger(attackTriggerHash);
                
                // Añadimos esta acción al registro de acciones pasadas.
                _pastActions.Add(actionType);

                for (int i = 0; i < _pastActions.Count; i++)
                {
                    Debug.Log($"action of time: {i} was: {_pastActions[i]}");
                }

                // Después de lanzar el golpe, activamos el timer de "ya no puedes volver a atacar hasta después de cierto tiempo"
                lastAttackTime = Time.time;
                return true; // sí se ejecutó la acción.
            }
        }

        return false;
    }

    private bool TryBasicMeleeAttack()
    {
        return TryAttack(ref _lastBasicAttackTime, _enemyOwner.GetMeleeBasicAttackRate(),
            _enemyOwner.GetMeleeBasicAttackRange(), MeleeActions.BasicMeleeAttack, _meleeBasicAttackAnimatorHash);
    }

    private bool TryAreaAttack()
    {
        return TryAttack(ref _lastAreaAttackTime, _enemyOwner.GetMeleeAreaAttackRate(),
            _enemyOwner.GetMeleeAreaAttackRange(), MeleeActions.AreaMeleeAttack, _meleeAreaAttackAnimatorHash);
    }

    private bool TryDashAttack()
    {
        return TryAttack(ref _lastDashAttackTime, _enemyOwner.GetMeleeDashAttackRate(),
            _enemyOwner.GetMeleeDashAttackRange(), MeleeActions.DashMeleeAttack, _meleeDashAttackAnimatorHash);
    }
    
    private bool TryUltimateAttack()
    {
        return TryAttack(ref _lastUltimateAttackTime, _enemyOwner.GetMeleeUltimateAttackRate(),
            _enemyOwner.GetMeleeUltimateAttackRange(), MeleeActions.MeleeUltimateAttack, _meleeUltimateAttackAnimatorHash);
    }

    public override void OnExit()
    {
        // olvidar todas las secuencias de acciones anteriores que se realizaron en este estado.
        _pastActions.Clear();
    }
}