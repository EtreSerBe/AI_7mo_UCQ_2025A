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

    private MeleeActions _currentMeleeAction = MeleeActions.None;

    [SerializeField] private LayerMask _meleeAttackLayerMask;
    
    private float _lastBasicAttackTime;
    private float _lastAreaAttackTime;
    private float _lastDashAttackTime;
    private float _lastUltimateAttackTime;


    private int _meleeBasicAttackAnimatorHash;
    private int _meleeDashAttackAnimatorHash;
    private int _meleeAreaAttackAnimatorHash;
    private int _meleeUltimateAttackAnimatorHash;
    private int _animatorMovementSpeedHash;
    
    // Cuáles acciones tiene disponibles este estado?
    public enum MeleeActions
    {
        None,
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
        _animatorMovementSpeedHash = Animator.StringToHash("MovementSpeed");
        _meleeAttackLayerMask += LayerMask.NameToLayer("Player"); // quiero colisionar contra el player, así que añado esto.
    }

    public override void OnUpdate()
    {
        // Acercarnos al target de nuestro dueño y atacarlo cuando estemos en un rango válido.
        // A nuestro dueño le decimos que se acerque a su Player objetivo.
        _enemyOwner.GetNavMeshAgent().destination = _playerRef.transform.position;

        Vector2 horizontalVelocity = new Vector2(_enemyOwner.GetNavMeshAgent().velocity.x,
            _enemyOwner.GetNavMeshAgent().velocity.z); 
        _enemyOwner.GetAnimator().SetFloat(_animatorMovementSpeedHash, horizontalVelocity.magnitude);
        
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

        // No permitimos que se ejecute ningún otro ataque mientras no se haya terminado la animación del ataque actual.
        // Esto porque en mi diseño yo decidí que los ataques de MeleeActions son mutuamente excluyentes, es decir, 
        // solo pueda estar activo uno de ellos a la vez. Si ustedes tienen algún comportamiento que no es mutuamente 
        // excluyente, entonces ese probablemente debería ir antes de este if-return.
        if (_currentMeleeAction != MeleeActions.None)
            return;
        
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
                    // ultimate. Para ello, hice una función específica adicional que se manda a llamar con un 
                    // animation event de la animación del Ultimate.
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


        // float accumulatedTime += Time.deltaTime;
        // float rand = Random.Range(0, 1);
        // if (rand + accumulatedTime > RushProbability )
        // {
        //     // haces el rush
        //     accumulatedTime = 0;
        // }
        //
        // rushDelayTime = 1.0;
        // rand = Random.Range(0, rushDelayTime);
        // Invoke(RushCoroutine, rand);
        // StartCoroutine(RushCoroutine, ren)
    }

    // Esta función se llama como un animation event en las animaciones de ataques. Representa el momento en que 
    // se encienden los elementos que detectan colisión para hacer daño.
    public void BeginAttackActiveFrames(string attackName)
    {
        Debug.LogWarning($"BeginAttackActiveFrames for attack: {attackName}");
        // Activa 
        _enemyOwner.ToggleSwordCollider(true);
    }
    
    // Esta función se llama como un animation event en las animaciones de ataques. Representa el momento en que 
    // se desactivan los elementos que detectan colisión para hacer daño, y por lo tanto ya no hace daño.
    public void EndAttackActiveFrames(string attackName)
    {
        Debug.LogWarning($"EndAttackActiveFrames for attack: {attackName}");
        _enemyOwner.ToggleSwordCollider(false);
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
                _currentMeleeAction = actionType; // IMPORTANTE: setear que actualmente se está ejecutando esta acción.
                
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
    
    // This method is called in animation events of attack animations.
    public void FinishedAttackEvent(string attackName)
    {
        Debug.Log($"finished the animation for attack: {attackName}");
        // quitamos la variable de que se está ejecutando dicha acción. Con eso ya se podrá elegir una nueva acción.
        _currentMeleeAction = MeleeActions.None; 
    }

    // No recibe nada, pues ahorita únicamente la necesito para salir del estado melee y entrar al Ranged.
    public void FinishedUltimateAttackEvent()
    {
        // Después de hacer el ultimate, hacemos el cambio de estado al estado Ranged.
        EnemyFSM ownerFsm = (EnemyFSM)OwnerFSMRef;
        OwnerFSMRef.ChangeState(ownerFsm.GetRangedState());
    }

    public override void OnExit()
    {
        // olvidar todas las secuencias de acciones anteriores que se realizaron en este estado.
        _pastActions.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        var maskValue = 1 << other.gameObject.layer;
        var maskANDmaskValue = (maskValue & _meleeAttackLayerMask.value);

        // esto es una sola comprobaci�n para filtrar todas las capas que no nos interesan.
        if (maskANDmaskValue > 0)
        {
            // Hacemos lo que corresponda según la layer a que golpeamos.
            // Si sí fue el player, le hacemos daño.
            Debug.Log($"Hicimos daño al player con el ataque: {nameof(_currentMeleeAction)}");

            switch (_currentMeleeAction)
            {
                case MeleeActions.AreaMeleeAttack:
                    Debug.Log($"Hicimos 20 de daño al player con el ataque: {nameof(_currentMeleeAction)}");
                    break;
                case MeleeActions.BasicMeleeAttack:
                    Debug.Log($"Hicimos 5 de daño al player con el ataque: {nameof(_currentMeleeAction)}");
                    break;
                case MeleeActions.DashMeleeAttack:
                    Debug.Log($"Hicimos 12 daño al player con el ataque: {nameof(_currentMeleeAction)}");
                    break;
                case MeleeActions.MeleeUltimateAttack:
                    Debug.Log($"Hicimos 45 daño al player con el ataque: {nameof(_currentMeleeAction)}");
                    break;
            }
        }
    }
}