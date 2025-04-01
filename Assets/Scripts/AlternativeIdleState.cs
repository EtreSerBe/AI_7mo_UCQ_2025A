using UnityEngine;

public class AlternativeIdleState : IdleState
{
    
    
    void Start()
    {
        StateName = "Alternative Idle";
    }
    
    public override void OnUpdate()
    {
        // Debug.Log("hola, soy el puro update de  Alternative Idle State"); 
        // base.OnUpdate();

        // Ejemplo de Casteo de variables (casting).
        // int myNumber = 65;
        // char myChar = (char)myNumber;

        if (Vector3.Distance(transform.position, _playerRef.transform.position) < 10)
        {
            EnemyFSM enemyFsm = (EnemyFSM)OwnerFSMRef;
            OwnerFSMRef.ChangeState(enemyFsm.GetMeleeState());
            return; // SIEMPRE que hagamos un cambio de estado tiene que ir seguido de un return.
            // si no hacemos return corremos el riesgo de que se ejecute código de este estado del cual -se supone-
            // acabamos de salir.
            Debug.LogWarning("Yo soy del alternative idle state.");
        }
        

    }
}
