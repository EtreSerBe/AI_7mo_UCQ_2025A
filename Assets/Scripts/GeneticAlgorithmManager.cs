using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;


class DescendingComparer<T> : IComparer<T> where T : IComparable<T> {
    public int Compare(T x, T y) {
        return y.CompareTo(x);
    }
}


public class GeneticAlgorithmManager : MonoBehaviour
{
    private enum ECrossoverAlgorithm : byte
    {
        RandomCrossover,
        HalfAndHalfParentCrossover,
    }

    [SerializeField] private ECrossoverAlgorithm selectedCrossoverAlgorithm = ECrossoverAlgorithm.RandomCrossover;

    private enum EMutationAlgorithm : byte
    {
        RandomValue,
        ControllableValue
    }

    [SerializeField] private EMutationAlgorithm selectedMutationAlgorithm = EMutationAlgorithm.ControllableValue;
    [SerializeField] private float controllableMutationStepDistance = 0.1f;
    
    
    [SerializeField] private int geneticAlgorithmIterations = 30;
    
    [SerializeField]
    private float difficultyScoreWeight = 0.5f;

    // cuántos elementos van a generarse con el algoritmo genético
    [SerializeField] private int populationSize = 20;
    
    // Lista de los prefabs de distintos tipos de enemigos (p.e. elites, jackals, brutes, etc.)
    [SerializeField]
    private List<PCGEnemy> enemyDefinitionsList = new List<PCGEnemy>();

    [SerializeField] private List<PCGEnemyStats> finalEntitiesList;
    
    private List<PCGEnemyStats> GeneticAlgorithmInitialization( PCGEnemy enemyDefinition, int numberOfEntities )
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

    private List<PCGEnemyStats> GeneticAlgorithmFitnessAssignment(List<PCGEnemyStats> entitiesList )
    {
        // Simplemente tomamos todas las entidades que se tengan y las ordenamos por su fitness usando esta estructura
        // de datos llamada sorted dictionary.
        SortedDictionary<float, PCGEnemyStats> sortedEntities = new SortedDictionary<float, PCGEnemyStats>(new DescendingComparer<float>());
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
        
        // SOLO PARA MOTIVOS DE DEBUG.
        // foreach (var entity in result)
        // {
        //     float totalScore = GetTotalScore(entity);
        //     Debug.Log($"las entidades tras ser ordenadas son: {totalScore}");
        // }
        return result;
    }

    private List<PCGEnemyStats> GeneticAlgorithmSelection(List<PCGEnemyStats> entitiesByFitness, int topNElements)
    {
        List<PCGEnemyStats> selectedEntities = entitiesByFitness.GetRange(0, topNElements); 
        // solamente tomamos los N-mejores elementos de todos los que teníamos y ya.
        return selectedEntities;
    }

    // NOTA: Por el momento crossover y mutación se hacen ambas aquí dentro.
    private List<PCGEnemyStats> GeneticAlgorithmCrossover(List<PCGEnemyStats> topNEntities)
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
    
    // NOTA: Por el momento crossover y mutación se hacen ambas aquí dentro. Esta versión usa la versión de 
    // Crossover(PCGEnemyStats parent1, PCGEnemyStats parent2, bool useRandomCrossover = true) pero con el 
    // parámetro de useRandomCrossover en false para generarlos de manera distinta.
    private List<PCGEnemyStats> GeneticAlgorithmCrossoverHalfAndHalf(List<PCGEnemyStats> topNEntities)
    {
        // copiamos los valores que ya trae el parámetro de entrada porque los vamos a necesitar. 
        List<PCGEnemyStats> resultingEntities = new List<PCGEnemyStats>(topNEntities);
        
        // Vamos a hacer X entidades nuevas haciendo cruzas entre pares de los topNEntities.
        // En este caso, cada par de padres van a generar 2 hijos, 
        // los hijos de Parent1 y Parent2 van a tener los siguientes hijos:
        // HijoA {HP1, Damage1, Rate1, Range2, MovementSpeed2}
        // HijoB {HP2, Damage2, Rate2, Range1, MovementSpeed1}
        
        for (int i = 0; i < topNEntities.Count; i += 2)
        {
            // tomamos al padre i y padre i+1:
            PCGEnemyStats parentI = topNEntities[i];
            PCGEnemyStats parentIPlusOne = topNEntities[i+1];
            // hacemos el crossover
            // Este sería el HijoA del ejemplo de arriba
            PCGEnemyStats newChild = Crossover(parentI, parentIPlusOne, false);
            newChild = Mutation(newChild);
            resultingEntities.Add(newChild);
            
            // Este sería el HijoB del ejemplo de arriba, por eso la mandamos a llamar con parentI y parentIPlusOne invertidos.
            PCGEnemyStats newChild2 = Crossover(parentIPlusOne, parentI, false);
            newChild2 = Mutation(newChild2);
            resultingEntities.Add(newChild2);
        }

        return resultingEntities;
    }
    

    private PCGEnemyStats Crossover(PCGEnemyStats parent1, PCGEnemyStats parent2, bool useRandomCrossover = true)
    {
        // le pasamos true solo porque no queremos tener un constructor sin parámetros en PCGEnemyStats.
        PCGEnemyStats child = new PCGEnemyStats(true);
        
        // para cada característica de nuestra entidad hijo, hacemos random para ver si se le pone el valor
        // de parent1 o el de parent2.
        float []features= child.GetFeaturesVectorNorm();
        float []featuresParent1 = parent1.GetFeaturesVectorNorm();
        float []featuresParent2 = parent2.GetFeaturesVectorNorm();
        
        if(useRandomCrossover)
        {
            // OPCIÓN 1 DE CROSSOVER: 100% random
            for (int i = 0; i < features.Length; i++)
            {
                int selectedParent = Random.Range(0, 2); // random de 0 a 1
                features[i] = selectedParent == 0 ? featuresParent1[i] : featuresParent2[i];
            }
        }
        else
        {
            // OPCIÓN 2 DE CROSSOVER: MITAD DEL PARENT1 Y MITAD DEL PARENT2
            // por ejemplo, HP, Damage y AttackRate del parent1, y AttackRange y MovementSpeed del parent2.  
            for (int i = 0; i < features.Length/2; i++)
            {
                features[i] = featuresParent1[i];
            }
            for (int i = features.Length/2; i < features.Length; i++)
            {
                features[i] = featuresParent2[i];
            }
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
        
        switch (selectedMutationAlgorithm)
        {
            case EMutationAlgorithm.RandomValue:
                // y a la que tomamos le damos un valor al azar.
                features[randomFeature] = Random.Range(0, 1.0f);
                break;
            case EMutationAlgorithm.ControllableValue:
            {
                int positiveOrNegativeChange = Random.Range(0, 2);
                if (positiveOrNegativeChange == 0)
                {
                    features[randomFeature] += controllableMutationStepDistance;
                }
                else
                {
                    features[randomFeature] -= controllableMutationStepDistance;
                }

                // lo limitamos al rango [0,1] porque si no ya no estaría normalizado.
                features[randomFeature] = Mathf.Clamp01(features[randomFeature]);               
                break;
            }
            default:
                break;
        }
        
        // y a la que tomamos le damos un valor al azar.
        features[randomFeature] = Random.Range(0, 1.0f);
        
        // nos falta decirle a newStats que sus valores cambiaron. Si no, nunca se iba a guardar ese cambio hecho.
        newStats.SetFeaturesAsNormVector(features);
        
        return newStats;
    }

    
    float GetTotalScore( PCGEnemyStats entity )
    {

        // if (entity.Name == desiredClassName)
        // {
        //     return entity.GetBalance()*(1.0f-difficultyScoreWeight) + entity.GetDifficultyV2()*difficultyScoreWeight  + 0.10f;
        // }
        
        // Difficulty debe acercarse 
        return entity.GetBalance()*(1.0f-difficultyScoreWeight) + entity.GetDifficultyV2()*difficultyScoreWeight ;
    }
    
    // float GetDifferenceScore()
    

    void RunGeneticAlgorithm()
    {
        PCGEnemy newEnemy = Instantiate(enemyDefinitionsList[Random.Range(0, enemyDefinitionsList.Count)]);

        // Inicialización del tipo de enemigo que caiga (Elite o Brute por ahora), y cuántos queremos en la población (20).
        List<PCGEnemyStats> initialEntities = GeneticAlgorithmInitialization(newEnemy, populationSize);

        List<PCGEnemyStats> evolvedEntities = initialEntities;

        int iterationCounter = 0;
        while (iterationCounter < geneticAlgorithmIterations)
        {
            iterationCounter++;
            List<PCGEnemyStats> sortedEntities = GeneticAlgorithmFitnessAssignment(evolvedEntities);

            
            // que me dé la mejor mitad de la población del algoritmo genético.
            List<PCGEnemyStats> selectedEntities = GeneticAlgorithmSelection(sortedEntities, populationSize/2);

            switch (selectedCrossoverAlgorithm)
            {
                case ECrossoverAlgorithm.RandomCrossover:
                    evolvedEntities = GeneticAlgorithmCrossover(selectedEntities);
                    break;
                case ECrossoverAlgorithm.HalfAndHalfParentCrossover:
                    evolvedEntities = GeneticAlgorithmCrossoverHalfAndHalf(selectedEntities);
                    break;
                default:
                    break;
            }
        }

        // Llamamos a GeneticAlgorithmFitnessAssignment para que nos ordene nuestras entidades
        finalEntitiesList = GeneticAlgorithmFitnessAssignment(evolvedEntities); // asignamos a la finalEntitiesList con los resultados finales.

        Debug.Log($"las entidades creadas por el algoritmo genético fueron: ");
        foreach (var entity in finalEntitiesList)
        {
            float difficultyScore = entity.GetDifficultyV2();
            float balanceScore = entity.GetBalance();
            float totalScore = GetTotalScore(entity);
            Debug.Log($"TotalScore: {totalScore}; de los cuales, " +
                      $"DifficultyScore aportó: {difficultyScore * difficultyScoreWeight}; y " +
                      $" BalanceScore aportó: {balanceScore * (1.0f - difficultyScoreWeight)}. " +
                      $"Difficulty pura fue: {difficultyScore}; Balance pura fue: {balanceScore}");
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
