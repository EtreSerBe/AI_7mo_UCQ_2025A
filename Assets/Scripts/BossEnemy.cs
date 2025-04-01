using UnityEngine;
using UnityEngine.AI;

public class BossEnemy : BaseEnemy
{
    // En mi caso, mi BaseEnemy no tenía navMeshAgent, por eso se lo puse yo aquí. Si su clase BaseEnemy 
    // ya tiene NavMeshAgent dentro, no necesitan redeclararlo aquí.
    private NavMeshAgent _navMeshAgent;

    public NavMeshAgent GetNavMeshAgent()
    {
        return _navMeshAgent;
    }

    [SerializeField]
    private float _visionRadius = 15.0f;

    private float _meleeAttackDamage = 3.0f;
    private float _meleeAttackRange = 5.0f;
    private float _meleeAttackRate = 2.0f; // te puede atacar máximo una vez cada dos segundos.

    public float GetMeleeAttackRange()
    {
        return _meleeAttackRange;
    }
    public float GetMeleeAttackRate()
    {
        return _meleeAttackRate;
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
