using System.Collections.Generic;
using UnityEngine;

public class PCGEnemyManager : MonoBehaviour
{

    // Lista de los prefabs de distintos tipos de enemigos (p.e. elites, jackals, brutes, etc.)
    [SerializeField]
    private List<PCGEnemy> enemyDefinitionsList = new List<PCGEnemy>();
    
    // Los enemigos que ya se spawnearon en escena.
    private List<PCGEnemy> _existingEnemies = new List<PCGEnemy>();
    
    
    void CreateEnemy()
    {
        // Ahorita instancia un enemigo de una de las posibles definiciones en enemyDefinitionsList al azar. 
        PCGEnemy newEnemy = Instantiate(enemyDefinitionsList[Random.Range(0, enemyDefinitionsList.Count)]);
        Debug.Log("El enemigo creado fue: " + newEnemy.name);

        
        // Se inicializa usando los rangos del PCGonfig del tipo de enemigo que sea newEnemy.
        PCGEnemyStats newStats = new PCGEnemyStats(newEnemy.GetConfig() ); 
        Debug.Log("Los stats antes de ser mejorado fueron: ");
        newStats.PrintStats();

        
        // mejoramos esos stats antes de asignarlos al enemigo.
        newStats = GreedySearch(newStats, 300f);
        
        newEnemy.SetStats(newStats);
        Debug.Log("Los stats después de ser mejorado fueron: ");
        newStats.PrintStats();
        
        // creamos un PCGEnemy que dentro tiene su PCGEnemyStats
        _existingEnemies.Add(newEnemy);
    }
    
    PCGEnemyStats GreedySearch( PCGEnemyStats origin, float DifficultyThreshold )
    {
        // origin es nuestra posición en el espacio inicial aleatoria.
        PCGEnemyStats currentEntity = origin;

        bool changed = false;
        // mientras que la función de fitness evaluando al actual no supere el threshold, continua.
        // ahorita, DifficultyThreshold debe tomar en cuenta la configValues del enemigo que se está creando.
        while (currentEntity.GetDifficultyV1() < DifficultyThreshold ) // TENGAN CUIDADO CON ESTE DifficultyThreshold EN SU ENTREGA.
        {
            changed = false;
            
            // obtenemos a los vecinos de la entidad actual.
            List<PCGEnemyStats> neighbors = currentEntity.GetNeighbors();
            // checamos si alguno de estos vecinos tiene un mejor de la fitness function que CurrentEntity.
            foreach (var neighbor in neighbors)
            {
                if (currentEntity.GetDifficultyV1() < neighbor.GetDifficultyV1())
                {
                    // entonces este neighbor es una mejor entidad que currentEntity.
                    currentEntity = neighbor;
                    changed = true;
                }
            }

            // esta condición de aquí evita que se cicle para siempre cuando no hay un vecino mejor ni se supera el threshold.
            if (changed == false) 
            {
                Debug.LogWarning("ya no se encontró a nadie mejor que esta unidad");
                break;
            }
        }

        // regresamos al mejor que se encontró.
        return currentEntity;
    }
    
    PCGEnemyStats GreedyWithListSearch( PCGEnemyStats origin, float DifficultyThreshold )
    {
        // origin es nuestra posición en el espacio inicial aleatoria.
        PCGEnemyStats currentEntity = origin;

        // HashSet<PCGEnemyStats> closedList() = new HashSet<PCGEnemyStats>();
        // PriorityQueue openList = new PriorityQueue();
        // openList.Enqueue(currentEntity);
        
        PCGEnemyStats currentBest = origin;
        
        int iterationCount = 0;
        // mientras que la función de fitness evaluando al actual no supere el threshold,
        // o no hayas hecho suficientes iteraciones o todavía haya más vecinos a quienes visitar, entonces continua.
        while (currentEntity.GetDifficultyV1() < DifficultyThreshold &&  iterationCount < 100 /*&& openList.Count() > 0*/)
        {
            iterationCount++;

            // current entity es el que tiene la mejor fitness ahorita DE los que falta por visitar.
            // currentEntity = openList.Dequeue(); 
            // lo metemos a la lista cerrada
            // closedList.Add(currentEntity);
            
            // obtenemos a los vecinos de la entidad actual.
            List<PCGEnemyStats> neighbors = currentEntity.GetNeighbors();
            // checamos si alguno de estos vecinos tiene un mejor de la fitness function que CurrentEntity.
            foreach (var neighbor in neighbors)
            {
                if (currentEntity.GetDifficultyV1() < neighbor.GetDifficultyV1())
                {
                    // entonces este neighbor es una mejor entidad que currentEntity.
                    currentBest = neighbor;
                }
            }
        }

        // regresamos al mejor que se encontró.
        return currentEntity;
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Hago esto aquí para que solo se calcule una vez por cada definición de enemigo distinto.
        foreach (var pcgEnemy in enemyDefinitionsList)
        {
            pcgEnemy.GetConfig().Initialize();
        }
        
        CreateEnemy();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
