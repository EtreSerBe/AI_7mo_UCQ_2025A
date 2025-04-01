using JetBrains.Annotations;
using UnityEngine;

public class BaseFSM : MonoBehaviour
{
    // Variable donde se guarda referencia al estado actual
    private BaseState _currentState = null;
    
    // Update
    // Únicamente mandar a llamar el OnUpdate del estado actual.
    void Update()
    {
        _currentState.OnUpdate();
    }
    
    // Initialize
    // Nos dice que la máquina debe entrar al estado inicial (ejecutar su función OnEnter).
    protected virtual void Initialize()
    {

        
        
    }

    // NOTA: El Start de la Base FSM no se debe de modificar en las clases hijas. 
    // Modifique el método Initialize en su lugar.
    void Start()
    {
        // Nuestro estado actual es el estado inicial.
        _currentState = GetInitialState();
        if (_currentState == null)
        {
            Debug.LogError("_currentState es null, ¿olvidaste sobreescribir GetInitialState en " +
                           "esta clase hija de BaseFSM? Saliendo de la función");
            return;
        }
        
        // Entramos a nuestro estado actual (OnEnter)
        _currentState.OnEnter();
        Initialize();
    }
    // https://youtu.be/-VkezxxjsSE?si=XSjs7K7ViQRl-mJJ&t=848
    
    // Este de aquí es clave porque no queremos sobreescribir el Start de BaseFSM.
    protected virtual BaseState GetInitialState()
    {
        return null;
    }
    
    
    // ChangeState
    public void ChangeState(BaseState newState)
    {
        // Sale del estado actual 
        _currentState.OnExit();
    
        // pone que el estado actual es ahora el nuevo estado deseado
        _currentState = newState;
        
        // entra al nuevo estado actual
        _currentState.OnEnter();
    }
    
    


    // Update is called once per frame

}
