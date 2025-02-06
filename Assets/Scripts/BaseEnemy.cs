using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    [SerializeField]
    protected int currentHP;

    [SerializeField]
    protected int maxHP;

    // Radio de detección 
    [SerializeField]
    protected Senses detectionSenses;

    // velocidad de movimiento
    [SerializeField]
    protected SteeringBehaviors steeringBehaviors;

    // MeshRenderer meshRenderer;

    [SerializeField]
    protected int attackDamage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        // Si el script de Senses ya detectó a alguien.
        // if(detectionSenses.IsEnemyDetected())
        {
            // entonces podemos setearlo en el script de steering behaviors.
            steeringBehaviors.SetEnemyReference(detectionSenses.GetDetectedEnemyRef());
            steeringBehaviors.obstacleList = detectionSenses.GetDetectedObstacles();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
