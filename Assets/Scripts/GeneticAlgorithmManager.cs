using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class GeneticAlgorithmManager : MonoBehaviour
{
    [SerializeField]
    private float difficultyScoreWeight = 0.85f;

    // cuántos elementos van a generarse con el algoritmo genético
    [SerializeField] private int populationSize = 20;
    
    // Lista de los prefabs de distintos tipos de enemigos (p.e. elites, jackals, brutes, etc.)
    [SerializeField]
    private List<PCGEnemy> enemyDefinitionsList = new List<PCGEnemy>();

    [SerializeField] private List<PCGEnemyStats> finalEntitiesList;
    
    List<PCGEnemyStats> GeneticAlgorithmInitialization( PCGEnemy enemyDefinition, int numberOfEntities )
    {
        // vamos a crear N entidades y las vamos a guardar en un contenedor (estructura de datos)
        List<PCGEnemyStats> initialEntities = new List<PCGEnemyStats>();

        // mete N entidades random al arreglo de initialEntities.
        for(int i =0 ; i < numberOfEntities; i++)
        {
            // esto de aquí ya le asigna sus randoms en los rangos adecuados, tanto normalizados como no-normalizados.
            initialEntities.Add( new PCGEnemyStats(enemyDefinition.GetConfig()) );
        }

        return initialEntities;
    }

    List<PCGEnemyStats> GeneticAlgorithmFitnessAssignment(List<PCGEnemyStats> entitiesList )
    {
        // simplemente tomamos todas las entidades que se tengan y las ordenamos por su fitness usando esta estructura
        // de datos llamada sorted dictionary.
        SortedDictionary<float, PCGEnemyStats> sortedEntities = new SortedDictionary<float, PCGEnemyStats>();
        foreach (var entity in entitiesList)
        {
            float totalScore = GetTotalScore(entity);
            if (!sortedEntities.TryAdd(totalScore, entity))
            {
                Debug.LogWarning($"two entities had the exact same total score. Not possible to add them. total score: {totalScore}");
            }
        }

        // obtenemos las entidades ordenadas pero ya solo como una lista normalita.
        List<PCGEnemyStats> result = new List<PCGEnemyStats>(sortedEntities.Values.ToArray());
        
        foreach (var entity in result)
        {
            float totalScore = GetTotalScore(entity);
            Debug.Log($"las entidades tras ser ordenadas son: {totalScore}");
        }
        return result;
    }

    List<PCGEnemyStats> GeneticAlgorithmSelection(List<PCGEnemyStats> entitiesByFitness, int topNElements)
    {
        List<PCGEnemyStats> selectedEntities = entitiesByFitness.GetRange(0, topNElements); 
        // solamente tomamos los N-mejores elementos de todos los que teníamos y ya.
        return selectedEntities;
    }

    // NOTA: Por el momento crossover y mutación se hacen ambas aquí dentro.
    List<PCGEnemyStats> GeneticAlgorithmCrossover(List<PCGEnemyStats> topNEntities)
    {
        // copiamos los valores que ya trae el parámetro de entrada porque los vamos a necesitar. 
        List<PCGEnemyStats> resultingEntities = new List<PCGEnemyStats>(topNEntities);
        
        // vamos a hacer X entidades nuevas haciendo cruzas entre pares de los topNEntities.
        
        // versión 1.A)
        for (int i = 0; i < topNEntities.Count; i += 2)
        {
            // tomamos al padre i y padre i+1:
            PCGEnemyStats parentI = topNEntities[i];
            PCGEnemyStats parentIPlusOne = topNEntities[i+1];
            // hacemos el crossover
            PCGEnemyStats newChild = Crossover(parentI, parentIPlusOne);
            newChild = Mutation(newChild);
            resultingEntities.Add(newChild);
        }
        // ya que tenemos hijos de los N mejores, hacemos copias de los N/2 mejores pero con mutaciones.
        for (int i = 0; i < topNEntities.Count / 2; i++)
        {
            PCGEnemyStats bestI = topNEntities[i];
            // hacemos una copia de él (ya se hace dentro de mutation) y luego le cambiamos una feature al azar por
            // un valor al azar.
            PCGEnemyStats bestIMutation = Mutation(bestI);
            resultingEntities.Add(bestIMutation);
        }

        return resultingEntities;

        // Hay dos maneras:
        // 1) ordenada: 1er con 2do mejor; 3er con 4to mejor; 5to con 6to mejor, etc.
        // empezams con 20 entidades,
        // después, nos quedamos con las mejores 10,
        // si esos 10 padres solo tienen un hijo cada pareja, entonces solo nos darían 15 hijos.
        // de dónde salen los 5 que nos faltan?
        // posibilidad A) hacer una copia de los 5 mejores (o de 5 al azar de los mejores)
        // y cambiarles aleatoriamente una propiedad. Esto se conoce como una mutación
        // posibilidad B) hacer otros 5 hijos pero hacerle mutaciones a cada uno.

        // 2) todos contra todos (puede haber repetición): random 1 con random 2.
        // por ejemplo, hijo1 = padre1 con padre6; hijo2 es padre6 con padre3;

        // 3) todos contra todos sin repetición: es decir, random 1 con random 2, pero después de usarlos los quitas.

    }

    PCGEnemyStats Crossover(PCGEnemyStats parent1, PCGEnemyStats parent2)
    {
        // le pasamos true solo porque no queremos tener un constructor sin parámetros en PCGEnemyStats.
        PCGEnemyStats child = new PCGEnemyStats(true);
        
        // para cada característica de nuestra entidad hijo, hacemos random para ver si se le pone el valor
        // de parent1 o el de parent2.
        float []features= child.GetFeaturesVectorNorm();
        float []featuresParent1 = parent1.GetFeaturesVectorNorm();
        float []featuresParent2 = parent2.GetFeaturesVectorNorm();

        
        for (int i = 0; i < features.Length; i++)
        {
            int selectedParent = Random.Range(0, 2); // random de 0 a 1
            features[i] = selectedParent == 0 ? featuresParent1[i] : featuresParent2[i];
        }

        child.SetFeaturesAsNormVector(features);
        return child;
    }

    PCGEnemyStats Mutation(PCGEnemyStats toCopyAndThenMutate)
    {
        // primero hacemos una copia:
        PCGEnemyStats newStats = new PCGEnemyStats(toCopyAndThenMutate);
        // dame cuántas características tiene
        float []features= newStats.GetFeaturesVectorNorm();
        
        // después, tomamos una de ellas al azar.
        int randomFeature = Random.Range(0, features.Length);
        // y a la que tomamos le damos un valor al azar.
        features[randomFeature] = Random.Range(0, 1.0f);
        
        // nos falta decirle a newStats que sus valores cambiaron. Si no, nunca se iba a guardar ese cambio hecho.
        newStats.SetFeaturesAsNormVector(features);
        
        return newStats;
    }

    float GetTotalScore( PCGEnemyStats entity)
    {
        return entity.GetBalance()*(1.0f-difficultyScoreWeight) + entity.GetDifficultyV2()*difficultyScoreWeight;
    }

    void RunGeneticAlgorithm()
    {
        PCGEnemy newEnemy = Instantiate(enemyDefinitionsList[Random.Range(0, enemyDefinitionsList.Count)]);

        // Inicialización del tipo de enemigo que caiga (Elite o Brute por ahora), y cuántos queremos en la población (20).
        List<PCGEnemyStats> initialEntities = GeneticAlgorithmInitialization(newEnemy, populationSize);

        List<PCGEnemyStats> evolvedEntities = initialEntities;

        int counter = 0;
        while (counter < 300)
        {
            counter++;
            List<PCGEnemyStats> sortedEntities = GeneticAlgorithmFitnessAssignment(evolvedEntities);

            
            // que me dé la mejor mitad de la población del algoritmo genético.
            List<PCGEnemyStats> selectedEntities = GeneticAlgorithmSelection(sortedEntities, populationSize/2);

            evolvedEntities = GeneticAlgorithmCrossover(selectedEntities);
            finalEntitiesList = evolvedEntities; // NO IDEAL, HAY QUE CAMBIARLO.
        }

        foreach (var entity in finalEntitiesList)
        {
            float totalScore = GetTotalScore(entity);
            Debug.Log($"las entidades creadas son: {totalScore}");
        }
    }
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RunGeneticAlgorithm();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
