using UnityEngine;

public class BaseState : MonoBehaviour
{
    // Cada estado está identificado por un nombre
    protected string StateName = "Base";
    protected BaseFSM OwnerFSMRef;

    public void Initialize(BaseFSM FSMRef)
    {
        OwnerFSMRef = FSMRef;
    }
    
    public virtual void OnEnter()
    {
        Debug.Log($"On enter del estado: {StateName}");
    }

    public virtual void OnUpdate()
    {
        // Debug.Log($"On Update del estado: {StateName}");
    }

    public virtual void OnExit()
    {
        Debug.Log($"On Exit del estado: {StateName}");
    }
    
    // no usar el Update normal del monobehavior nunca.
    
}
