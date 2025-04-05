using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class MeleeState : BaseState
{
    [SerializeField] private GameObject _playerRef;

    // El GameObject que es dueño de la máquina de estados que es dueña de este estado.
    // A través de esta referencia nosotros podemos leer o cambiar las variables necesarias de nuestro dueño. 
    private BossEnemy _enemyOwner;

    private float _lastBasicAttackTime;
    private float _lastAreaAttackTime;

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
    }

    public override void OnUpdate()
    {
        // Acercarnos al target de nuestro dueño y atacarlo cuando estemos en un rango válido.
        // A nuestro dueño le decimos que se acerque a su Player objetivo.
        _enemyOwner.GetNavMeshAgent().destination = _playerRef.transform.position;

        // Primero tenemos que checar que SÍ haya una acción pasada antes de checar si la acción pasada fue un melee básico.
        if (_pastActions.Count > 0)
        {
            if (_pastActions[_pastActions.Count - 1] == MeleeActions.BasicMeleeAttack)
            {
                // Area attack
                if (_lastAreaAttackTime + _enemyOwner.GetMeleeAreaAttackRate() < Time.time)
                {
                    // Hay que checar si ya estamos en rango válido para hacer un ataque melee de área.
                    // checamos si la distancia entre este agente y el player es menor o igual que el rango de nuestro ataque melee.
                    if (Vector3.Distance(transform.position, _playerRef.transform.position) <=
                        _enemyOwner.GetMeleeAreaAttackRange())
                    {
                        // si esa condición se cumple, entonces puedo atacar con ese ataque melee a ese player.
                        Debug.Log(
                            $"el agente: {gameObject.name} atacó al player {_playerRef.name} con un ataque melee de area.");

                        // Añadimos esta acción al registro de acciones pasadas.
                        _pastActions.Add(MeleeActions.AreaMeleeAttack);

                        for (int i = 0; i < _pastActions.Count; i++)
                        {
                            Debug.Log($"action of time: {i} was: {_pastActions[i]}");
                        }

                        // Después de lanzar el golpe, activamos el timer de "ya no puedes volver a atacar hasta después de cierto tiempo"
                        _lastAreaAttackTime = Time.time;
                        return;
                    }
                }
            }
        }

        // Antes de atacar, tenemos que checar si sí podemos atacar por el tiempo de espera entre ataques.
        // ejemplo: _lastAttackTime = 5 ; _enemyOwner.GetMeleeAttackRate() = 2.0f; Time.time = 6.9
        // con esos datos del ejemplo, no podríamos atacar, porque nuestro siguiente ataque podría ser a partir del 
        // tiempo 7.0, y 6.9 es menor que 7, entonces no podemos atacar.
        if (_lastBasicAttackTime + _enemyOwner.GetMeleeBasicAttackRate() < Time.time)
        {
            // Hay que checar si ya estamos en rango válido para hacer un ataque melee.
            // checamos si la distancia entre este agente y el player es menor o igual que el rango de nuestro ataque melee.
            if (Vector3.Distance(transform.position, _playerRef.transform.position) <=
                _enemyOwner.GetMeleeBasicAttackRange())
            {
                // si esa condición se cumple, entonces puedo atacar con ese ataque melee a ese player.
                Debug.Log($"el agente: {gameObject.name} atacó al player {_playerRef.name} con un ataque melee.");

                // Añadimos esta acción al registro de acciones pasadas.
                _pastActions.Add(MeleeActions.BasicMeleeAttack);

                for (int i = 0; i < _pastActions.Count; i++)
                {
                    Debug.Log($"action of time: {i} was: {_pastActions[i]}");
                }

                // Después de lanzar el golpe, activamos el timer de "ya no puedes volver a atacar hasta después de cierto tiempo"
                _lastBasicAttackTime = Time.time;
                return;
            }
        }


        // Debug.Log(" " + 1);
        // toma ese '1' y lo convierte en un caracter temporalmente, y así lo puede interpretar de otra manera.

        // Debug.Log("Update melee state");

        if (Vector3.Distance(transform.position, _playerRef.transform.position) >= 10)
        {
            EnemyFSM enemyFsm = (EnemyFSM)OwnerFSMRef;
            OwnerFSMRef.ChangeState(enemyFsm.GetIdleState());
            return; // La regla es: tú pones change state? la siguiente línea debe ser return.
        }


        // Action myAction = pastActions.Last;
        // While(myAction is not DashAttack && is not AreaAttack)
        // {
        //      myAction = previousAction;
        // }
        // if(myAction is DashAttack)
        //{
        //  myNewAction = AreaAttack;
        //}
        // else
        // {
        //  myNewAction = DashAttack;
        // }
    }

    public override void OnExit()
    {
        // olvidar todas las secuencias de acciones anteriores que se realizaron en este estado.
        _pastActions.Clear();
    }
}