using UnityEngine;

public class RangedState : BaseState
{
    [SerializeField] private GameObject _playerRef;

    // El GameObject que es dueño de la máquina de estados que es dueña de este estado.
    // A través de esta referencia nosotros podemos leer o cambiar las variables necesarias de nuestro dueño. 
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
        StateName = "RangedState";
    }
    
    public void BeginMeleeBasicAttackActiveFrames()
    {
        Debug.LogWarning("BeginMeleeBasicAttackActiveFrames pero en RangedState");
    }
    
    public void EndMeleeBasicAttackActiveFrames()
    {
        Debug.LogWarning("EndMeleeBasicAttackActiveFrames pero en RangedState");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
