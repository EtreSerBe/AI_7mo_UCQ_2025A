using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    // Diferencia como distancia euclidiana entre estas dos entidades. Alias, teorema de Pitágoras.
    public static float Difference(float[] entity1, float[] entity2, int size)
    {
        float result = 0;
        for (int i = 0; i < size; i++)
        {
            result += Mathf.Pow(entity1[i] - entity2[i], 2);
        }
        return Mathf.Sqrt(result)/Mathf.Sqrt(size);
    }
    
    // Esta es la resta de dos vectores tal cual, la vamos a usar más adelante.
    public static float[] Subtract(float[] entity1, float[] entity2, int size)
    {
        float[] result = new float[size];
        for (int i = 0; i < size; i++)
        {
            result[i] = entity1[i] - entity2[i];
        }
        return result;
    }
    
    public static bool TryAddNewEnemy(ref Dictionary<string, List<PCGEnemyStats>>  existingEnemyCreationsDictionary, 
        PCGEnemyStats enemy, string enemyType, float individualDifferenceThreshold, float averageDifferenceThreshold, 
        out float averageDifference)
    {

        // si no contiene una Key con este nombre de enemigo, entonces esa lista está vacía.
        if (!existingEnemyCreationsDictionary.ContainsKey(enemyType))
        {
            existingEnemyCreationsDictionary[enemyType] = new List<PCGEnemyStats> { enemy };
            averageDifference = 1.0f; // máxima diferencia porque pues es el único.
            return true;
        }
        
        // Checa si este enemigo es demasiado parecido a uno ya guardado o si es en promedio muy parecido a todos los demás.
        float totalDifference = 0;
        float []enemyFeaturesVec = enemy.GetFeaturesVectorNorm();
        int size = enemyFeaturesVec.Length;
        foreach (var pcgEnemy in existingEnemyCreationsDictionary[enemyType])
        {
            // calcular diferencia contra enemy
            float diff = Utilities.Difference(enemyFeaturesVec, pcgEnemy.GetFeaturesVectorNorm(), size);
            // si tiene solo 10% o menos de diferencia contra este enemy. Ese 0.1 es un umbral 
            if (diff < individualDifferenceThreshold)
            {
                averageDifference = -1.0f; // Regresamos -1.0f que significa que NO se añadió a la conexión.
                // si es menor, entonces no lo queremos, es demasiado parecido a uno ya existente.
                return false; // salimos de la función.
            }
            totalDifference += diff;
        }
        
        averageDifference = totalDifference / existingEnemyCreationsDictionary[enemyType].Count;
        if(totalDifference < averageDifferenceThreshold) // comparamos contra un “umbral”, en este caso 0.2
        {
            averageDifference = -1.0f; // Regresamos -1.0f que significa que NO se añadió a la conexión.
            return false; // Entonces es demasiado parecido a varios enemigos existentes
        }
        existingEnemyCreationsDictionary[enemyType].Add(enemy); // si pasó las dos verificaciones anteriores, entonces sí vale la pena añadirlo.

        // aquí ya está asignado el valor de averageDifference para evaluarse.
        return true;
    }


    
}
