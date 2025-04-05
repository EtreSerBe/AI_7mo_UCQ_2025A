using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossEnemy : BaseEnemy
{
    // // Cuáles acciones tiene disponibles este personaje para todos sus estados?
    // public enum EnemyActions
    // {
    //     BasicMeleeAttack,
    //     DashMeleeAttack,
    //     AreaMeleeAttack,
    //     MeleeUltimateAttack,
    //     BasicRangedAttack,
    //     DashRangedAttack,
    //     AreaRangedAttack,
    //     RangedUltimateAttack,
    //
    // }
    // private List<EnemyActions> _pastActions = new List<EnemyActions>();
    
    // En mi caso, mi BaseEnemy no tenía navMeshAgent, por eso se lo puse yo aquí. Si su clase BaseEnemy 
    // ya tiene NavMeshAgent dentro, no necesitan redeclararlo aquí.
    private NavMeshAgent _navMeshAgent;

    public NavMeshAgent GetNavMeshAgent()
    {
        return _navMeshAgent;
    }

    [SerializeField]
    private float _visionRadius = 15.0f;

    [SerializeField] 
    private float _meleeBasicAttackDamage = 3.0f;
    [SerializeField] 
    private float _meleeBasicAttackRange = 5.0f;
    [SerializeField] 
    private float _meleeBasicAttackRate = 2.0f; // te puede atacar máximo una vez cada dos segundos.

    public float GetMeleeBasicAttackRange()
    {
        return _meleeBasicAttackRange;
    }
    public float GetMeleeBasicAttackRate()
    {
        return _meleeBasicAttackRate;
    }
    
    [SerializeField] 
    private float _meleeAreaAttackDamage = 8.0f;
    [SerializeField] 
    private float _meleeAreaAttackRange = 10.0f;
    [SerializeField] 
    private float _meleeAreaAttackRate = 5.0f; // te puede atacar máximo una vez cada dos segundos.

    public float GetMeleeAreaAttackRange()
    {
        return _meleeAreaAttackRange;
    }
    public float GetMeleeAreaAttackRate()
    {
        return _meleeAreaAttackRate;
    }
    
    
    private float _rangedAttackDamage = 1.0f;
    private float _rangedAttackRange = 10.0f;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Le decimos que nos preste el componente de NavMeshAgent que debe tener este gameObject.
        _navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
        if (_navMeshAgent == null)
        {
            Debug.LogError("No hay Componente de NavMeshAgent asignado a este gameObject. Error grave.");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
