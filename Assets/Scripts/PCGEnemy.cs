using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

// PCG enemy es la clase de la cual vamos a hacer prefabs de nuestros distintos tipos de enemigos.
// para que tengan geometrías, animators, VFX, SFX distintos pero dentro de esa misma caterogía de enemigos.
// Por ejemplo, en Halo, un prefab sería de Elites, otros Jackals, otros Grunts, otros Brute, etc. 
public class PCGEnemy : MonoBehaviour
{
    private NavMeshAgent _navMeshAgent;
    private Animator _animator;
    
    // el prefab además deberá traer el mesh, los colliders, etc. que necesite.
    
    // también necesitaríamos aquello con lo que te van a atacar. Por ejemplo, las pistolas en halo borderlands, las balas en binding of isaac, etc.
    
    // todos aquellos que puedan necesitar de estos stats, los accederían de aquí.
    
    [SerializeField]
    private PCGEnemyStats _stats;

    // _configValues sería los rangos válidos de configuración para la categoría de enemigos en cuestión.
    [FormerlySerializedAs("_configValues")] [SerializeField]
    private PCGConfigValuesScriptableObject configValuesScriptableObject;

    public PCGConfigValuesScriptableObject GetConfig()
    {
        return configValuesScriptableObject;
    }
    
    public void SetStats(PCGEnemyStats stats)
    {
        stats.UpdateStatsBasedOnNormalized();
        _stats = stats;
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
