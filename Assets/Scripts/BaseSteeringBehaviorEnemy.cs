using UnityEngine;

public class BaseSteeringBehaviorEnemy : BaseEnemy
{
    // Radio de detecci�n 
    [SerializeField]
    protected Senses detectionSenses;

    // velocidad de movimiento
    [SerializeField]
    protected SteeringBehaviors steeringBehaviors;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }


    private void FixedUpdate()
    {
        // Si el script de Senses ya detect� a alguien.
        // if(detectionSenses.IsEnemyDetected())
        {
            // entonces podemos setearlo en el script de steering behaviors.
            steeringBehaviors.SetEnemyReference(detectionSenses.GetDetectedEnemyRef());
            steeringBehaviors.obstacleList = detectionSenses.GetDetectedObstacles();
        }
    }
}
