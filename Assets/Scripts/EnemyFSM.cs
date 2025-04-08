using UnityEngine;

public class EnemyFSM : BaseFSM
{
    // RECORDATORIO: Las FSM que heredan de BaseFSM no deben tener ni un Update() ni un Start().
    // siempre tiene que sobreescribir GetInitialState

    // Caminar random mientras no hayas detectado al player
        // Lo que va a hacer tu entidad mientras no haya detectado al player, va a ser lo que diga IdleState
    // detectaste al player, moverte hacia él
        // Hunt State nos dice cómo se va a comportar mientras se intenta acercar al player.
    // llegaste a cierto rango válido para ti? atacar al player
        // AttackState nos dice cómo va a atacar una vez que esté dentro del rango de ataque.
    // 

    [SerializeField]
    private GameObject playerRef;

    [SerializeField]
    private BossEnemy enemyOwner;
    
    // CONTEXT
    // private 
    
    private IdleState _idleState;

    private MeleeState _meleeState;
    private RangedState _rangedState;
    // private InvincibleState _invincibleState;

    public IdleState GetIdleState()
    {
        return _idleState;
    }
    
    public MeleeState GetMeleeState()
    {
        return _meleeState;
    }

    public RangedState GetRangedState()
    {
        return _rangedState;
    }
    
    // En las clases hijas de BaseFSM siempre se manda a llamar el Initialize justo dentro del Start
    protected override void Initialize()
    {
        // aquí ya hacemos lo que haríamos en el Start de esta clase.
        
        // // Le decimos al GameObject dueño de esta FSM que nos preste el script de BossEnemy que tiene asignado.
        // _enemyOwner = gameObject.GetComponent<BossEnemy>();
        // if (_enemyOwner == null)
        // {
        //     Debug.LogError("No hay script de BossEnemy asignado a este gameObject con la FSM. Error grave.");
        //     return;
        // }
        // // Estas líneas comentadas son exactamente lo mismo que lo de arriba, pero un poquito mejor en cuestiones de performance.
        // // if ( ! TryGetComponent<BossEnemy>(out _enemyOwner))
        // // {
        // //     Debug.LogError("No hay script de BossEnemy asignado a este gameObject con la FSM. Error grave.");
        // //     return;
        // // }
            
        
        _meleeState = gameObject.AddComponent<MeleeState>();
        _meleeState.Initialize(this, enemyOwner, playerRef);
        
        _rangedState = gameObject.AddComponent<RangedState>();
        _rangedState.Initialize(this, enemyOwner, playerRef);
    }

    protected override BaseState GetInitialState()
    {
        _idleState = gameObject.AddComponent<AlternativeIdleState>();
        _idleState.Initialize(this, enemyOwner, playerRef);
        return _idleState;
    }
    
    
}
