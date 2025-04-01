using UnityEngine;

public class IdleState : BaseState
{
    [SerializeField]
    protected GameObject _playerRef;

    private BossEnemy _enemyOwner;
    
    public void Initialize(BaseFSM ownerFSM, BossEnemy enemyOwner, GameObject playerRef)
    {
        OwnerFSMRef = ownerFSM;
        _enemyOwner = enemyOwner;
        _playerRef = playerRef;
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StateName = "Idle";
    }

    public override void OnUpdate()
    {
        // Debug.Log("hola, soy el puro update de Idle"); 
    }

}
