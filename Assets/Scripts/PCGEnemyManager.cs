using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PCGEnemyManager : MonoBehaviour
{

    [SerializeField] private int maxAnnealingIterations = 200;
    
    // Lista de los prefabs de distintos tipos de enemigos (p.e. elites, jackals, brutes, etc.)
    [SerializeField]
    private List<PCGEnemy> enemyDefinitionsList = new List<PCGEnemy>();
    
    // Los enemigos que ya se spawnearon en escena.
    private List<PCGEnemy> _existingEnemies = new List<PCGEnemy>();
    

    //  los valores específicos de enemigos que ya se guardaron como buenos.
    // private List<PCGEnemyStats> _existinEnemyCreations = new List<PCGEnemyStats>();

    private Dictionary<string, List<PCGEnemyStats>> _existingEnemyCreationsDictionary =
        new Dictionary<string, List<PCGEnemyStats>>();

    

    bool TryAddNewEnemy(PCGEnemyStats enemy, string enemyType)
    {
        // si no contiene una Key con este nombre de enemigo, entonces esa lista está vacía.
        if (!_existingEnemyCreationsDictionary.ContainsKey(enemyType))
        {
            _existingEnemyCreationsDictionary[enemyType] = new List<PCGEnemyStats> { enemy };
            return true;
        }
        
        // Checa si este enemigo es demasiado parecido a uno ya guardado o si es en promedio muy parecido a todos los demás.
        float totalDifference = 0;
        float []enemyFeaturesVec = enemy.GetFeaturesVectorNorm();
        int size = enemyFeaturesVec.Length;
        foreach (var pcgEnemy in _existingEnemyCreationsDictionary[enemyType])
        {
            // calcular diferencia contra enemy
            float diff = Utilities.Difference(enemyFeaturesVec, pcgEnemy.GetFeaturesVectorNorm(), size);
            // si tiene solo 10% o menos de diferencia contra este enemy. Ese 0.1 es un umbral 
            if (diff < 0.3f)
            {
                // si es menor, entonces no lo queremos, es demasiado parecido a uno ya existente.
                return false; // salimos de la función.
            }
            totalDifference += diff;
        }
        
        totalDifference /= _existingEnemyCreationsDictionary[enemyType].Count;
        if(totalDifference < 0.2) // comparamos contra un “umbral”, en este caso 0.2
        {
            return false; // Entonces es demasiado parecido a varios enemigos existentes
        }
        _existingEnemyCreationsDictionary[enemyType].Add(enemy); // si pasó las dos verificaciones anteriores, entonces sí vale la pena añadirlo.
        
        return true;
    }
    
    void CreateEnemy()
    {

        for(int i = 0; i < 30; i++)
        {

            // Ahorita instancia un enemigo de una de las posibles definiciones en enemyDefinitionsList al azar. 
            PCGEnemy newEnemy = Instantiate(enemyDefinitionsList[Random.Range(0, enemyDefinitionsList.Count)]);
            Debug.Log("El enemigo creado fue: " + newEnemy.name);
    
    
            // Se inicializa usando los rangos del PCGonfig del tipo de enemigo que sea newEnemy.
            PCGEnemyStats newStats = new PCGEnemyStats(newEnemy.GetConfig());
            Debug.Log("Los stats antes de ser mejorado fueron: ");
            newStats.PrintStats();
    
            // // Quiero la diferencia entre dos enemigos
            // float[] enemy1 = newStats.GetFeaturesVectorNorm();
            // float[] enemySAME = newStats.GetFeaturesVectorNorm();
            // float[] enemyMIN = new float[5] { 0, 0, 0, 0, 0 }; // para demostración nada más
            // float[] enemyMAX = new float[5] { 1, 1, 1, 1, 1 }; // para demostración nada más
            //
            // float diff = Difference(enemy1, enemySAME, enemy1.Length);
            // Debug.Log($"la difference entre enemy1 y enemySAME es: {diff}");
            //
            // diff = Difference(enemy1, enemyMIN, enemy1.Length);
            // Debug.Log($"la difference entre enemy1 y enemyMIN es: {diff}");
            //
            // diff = Difference(enemy1, enemyMAX, enemy1.Length);
            // Debug.Log($"la difference entre enemy1 y enemyMAX es: {diff}");
            //
            //
            // diff = Difference(enemyMIN, enemyMAX, enemy1.Length);
            // Debug.Log($"la difference entre enemyMIN y enemyMAX es: {diff}");
            //
            // float[] enemyElite = new float[5] {0.7f,0.6f,0.5f,0.8f,0.8f}; // para demostración nada más
            // float[] enemyHunter = new float[5] {0.9f,0.9f,0.1f,0.1f,0}; // para demostración nada más
            //
            // diff = Difference(enemyElite, enemyHunter, enemyElite.Length);
            // Debug.Log($"la difference entre enemyElite y enemyHunter es: {diff}");
    
            // mejoramos esos stats antes de asignarlos al enemigo.
            // newStats = GreedySearch(newStats, 0.25f, 50);

            
            float rateOfDecay = 1.0f / maxAnnealingIterations;
            // si el rate of decay es muy alto, entonces te puedes quedar atorado en máximos locales muy rápido.
            // si el rate of decay es muy bajo, entonces puedes que pierdas buenos puntos para el greedy search solo porque el random lo dijo.
            newStats = SimulatedAnnealing(newStats, 0.25f, 200, 1.0f, rateOfDecay);

    
            // Si no se añadió a los enemigos que son distintos, no se spawnea.
            if (!TryAddNewEnemy(newStats, newEnemy.gameObject.name))
            {
                Debug.LogWarning("el enemigo que se intentó crear fue rechazado por ser demasiado parecido a otro.");
                // esto de aquí+ solo destruye el script de tipo PCGEnemy que le pertenece a un GameObject en escena,
                // que no es lo que queríamos en esta situación
                // Destroy(newEnemy); 
                Destroy(newEnemy.gameObject); // destruye al GameObject que es dueño de este script de tipo PCGEnemy en la escena.
                continue;
            }
    
            // Si sí se logró añadir ese enemigo nuevo, entonces le asignamos sus valores reales de HP, damage, etc.
            // es decir, los no normalizados
            newEnemy.SetStats(newStats);
            Debug.Log("Los stats después de ser mejorado fueron: ");
            newStats.PrintStats();
    
            
            // creamos un PCGEnemy que dentro tiene su PCGEnemyStats
            _existingEnemies.Add(newEnemy);
        }
    }
    
    PCGEnemyStats GreedySearch( PCGEnemyStats origin, float difficultyWeight, int maxIterations )
    {
        // origin es nuestra posición en el espacio inicial aleatoria.
        PCGEnemyStats currentEntity = origin;

        float difficultyScore = currentEntity.GetDifficultyV2(); // da la dificultad normalizada.
        float balanceScore = currentEntity.GetBalance(); // da el balance normalizado.
        float currentTotalScore = balanceScore*(1-difficultyWeight) + difficultyScore*difficultyWeight; // total debe dar 1.
        
        bool changed = false;
        // mientras que la función de fitness evaluando al actual no supere el threshold, continua.
        // ahorita, DifficultyThreshold debe tomar en cuenta la configValues del enemigo que se está creando.
        for (int i = 0; i < maxIterations; i++) // TENGAN CUIDADO CON ESTE DifficultyThreshold EN SU ENTREGA.
        {
            changed = false;
            
            // obtenemos a los vecinos de la entidad actual.
            List<PCGEnemyStats> neighbors = currentEntity.GetNeighbors();
            // checamos si alguno de estos vecinos tiene un mejor de la fitness function que CurrentEntity.
            foreach (var neighbor in neighbors)
            {
                float neighborTotalScore = neighbor.GetDifficultyV2() * difficultyWeight +
                                           neighbor.GetBalance() * (1 - difficultyWeight);
                if (currentTotalScore < neighborTotalScore)
                {
                    // entonces este neighbor es una mejor entidad que currentEntity.
                    currentEntity = neighbor;
                    changed = true;
                    currentTotalScore = neighborTotalScore; // actualizamos la total score.
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
    
    
    PCGEnemyStats SimulatedAnnealing( PCGEnemyStats origin, float difficultyWeight, int maxIterations, float initialValue, float rateOfDecay )
    {
        // initialValue es el valor inicial que se tiene que tunear para el algoritmo
        float initialMinusDecay = initialValue;
        
        // origin es nuestra posición en el espacio inicial aleatoria.
        PCGEnemyStats currentEntity = origin;

        float difficultyScore = currentEntity.GetDifficultyV2(); // da la dificultad normalizada.
        float balanceScore = currentEntity.GetBalance(); // da el balance normalizado.
        float currentTotalScore = balanceScore*(1-difficultyWeight) + difficultyScore*difficultyWeight; // total debe dar 1.
        
        bool changed = false;
        // mientras que la función de fitness evaluando al actual no supere el threshold, continua.
        // ahorita, DifficultyThreshold debe tomar en cuenta la configValues del enemigo que se está creando.
        for (int i = 0; i < maxIterations; i++) // TENGAN CUIDADO CON ESTE DifficultyThreshold EN SU ENTREGA.
        {
            if (currentTotalScore > 0.85f) // 0.85 es un threshold de que si ya encontramos algo bueno, se salga
            {
                return currentEntity; // regresamos a esta buena unidad.
            }
            
            // changed = false;
            initialMinusDecay -= rateOfDecay; // reducimos esto cada iteración para que vaya aumentando la probabilidad
                                              // de irse por un vecino random, en vez del mejor. 
            
            // obtenemos a los vecinos de la entidad actual.
            List<PCGEnemyStats> neighbors = currentEntity.GetNeighbors();
            // checamos si alguno de estos vecinos tiene un mejor de la fitness function que CurrentEntity.
            foreach (var neighbor in neighbors)
            {
                float neighborTotalScore = neighbor.GetDifficultyV2() * difficultyWeight +
                                           neighbor.GetBalance() * (1 - difficultyWeight);
                if (initialMinusDecay > Random.Range(0.0f, 1.0f))
                {
                    currentEntity = neighbor;
                    // changed = true;
                    currentTotalScore = neighborTotalScore; // actualizamos la total score.
                    continue; // no necesitamos ejecutar lo demás porque de todos modos ya current es el vecino.
                }
                
                if (currentTotalScore < neighborTotalScore)
                {
                    // entonces este neighbor es una mejor entidad que currentEntity.
                    currentEntity = neighbor;
                    // changed = true;
                    currentTotalScore = neighborTotalScore; // actualizamos la total score.
                }
            }

            // esta condición de aquí evita que se cicle para siempre cuando no hay un vecino mejor ni se supera el threshold.
            // if (changed == false) 
            // {
            //     Debug.LogWarning("ya no se encontró a nadie mejor que esta unidad");
            //     break;
            // }
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
        while (currentEntity.GetDifficultyV2() < DifficultyThreshold &&  iterationCount < 100 /*&& openList.Count() > 0*/)
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
                if (currentEntity.GetDifficultyV2() < neighbor.GetDifficultyV2())
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
