using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using Random = UnityEngine.Random;

// importante que NO herede de Monobehavior para que podamos hacerle New.
[Serializable]
public class PCGEnemyStats
{
    public float HP;
    public float Damage;
    public float AttackRate; // lo mínimo es 1 vez cada 5 segundos y lo máximo es 10 por segundo.
    public float AttackRange;
    public float MovementSpeed;

    public float[] FeaturesAsVectorNorm = new float[5];

    public float HPNorm;
    public float DamageNorm;
    public float AttackRateNorm; // lo mínimo es 1 vez cada 5 segundos y lo máximo es 10 por segundo.
    public float AttackRangeNorm;
    public float MovementSpeedNorm;
    
    // Idealmente, si se pudiera hacer una Union como en C/C++ así FeaturesAsVector y las 5 variables de arriba 
    // ocuparían exactamente las mismas direcciones de memoria y, por lo tanto, no desperdiciaríamos nada.
    // public List<float> FeaturesAsVector = new List<float>();
    
    
    
    // union
    // {
    // public float HP;
    // public float Damage;
    // public float AttackRate; // lo mínimo es 1 vez cada 5 segundos y lo máximo es 10 por segundo.
    // public float AttackRange;
    // public float MovementSpeed;
    //}
    // {public float [5] features; }

    // union
    // {
    //  float x, y, z;   
    // }
    // {
    //  float xyz[3];
    // }
    // myUnion.x
    // myUnion.xyz[0];
    
    
    //
    

    private PCGConfigValuesScriptableObject _configValuesScriptableObject;

    public PCGEnemyStats(PCGConfigValuesScriptableObject configValuesScriptableObject)
    {
        _configValuesScriptableObject = configValuesScriptableObject;
        HP = Random.Range(_configValuesScriptableObject.MinHp, _configValuesScriptableObject.MaxHp);
        Damage = Random.Range(_configValuesScriptableObject.MinDamage, _configValuesScriptableObject.MaxDamage);
        AttackRate = Random.Range(_configValuesScriptableObject.MinAttackRate, _configValuesScriptableObject.MaxAttackRate);
        AttackRange = Random.Range(_configValuesScriptableObject.MinAttackRange, _configValuesScriptableObject.MaxAttackRange);
        MovementSpeed = Random.Range(_configValuesScriptableObject.MinMovementSpeed, _configValuesScriptableObject.MaxMovementSpeed);

        HPNorm = (HP - _configValuesScriptableObject.MinHp) / (_configValuesScriptableObject.MaxHp - _configValuesScriptableObject.MinHp);
        DamageNorm = (Damage - _configValuesScriptableObject.MinDamage) / (_configValuesScriptableObject.MaxDamage - _configValuesScriptableObject.MinDamage);
        AttackRateNorm = (AttackRate - _configValuesScriptableObject.MinAttackRate) / (_configValuesScriptableObject.MaxAttackRate - _configValuesScriptableObject.MinAttackRate);
        AttackRangeNorm = (AttackRange - _configValuesScriptableObject.MinAttackRange) / (_configValuesScriptableObject.MaxAttackRange - _configValuesScriptableObject.MinAttackRange);
        MovementSpeedNorm = (MovementSpeed - _configValuesScriptableObject.MinMovementSpeed) / (_configValuesScriptableObject.MaxMovementSpeed - _configValuesScriptableObject.MinMovementSpeed);
        
        FeaturesAsVectorNorm = new float [5] {HPNorm, DamageNorm, AttackRateNorm, AttackRangeNorm, MovementSpeedNorm};
    }

    // Es lo opuesto a GetStoredVector: no usa memoria extra pero pide memoria dinámica cada vez que se manda a llamar,
    // lo cual es malo para el performance.
    public List<float> GetAsVector()
    {
        List<float> result = new List<float>() {HP, Damage, AttackRate, AttackRange, MovementSpeed};
        return result;
    }

    // Ventaja de esta forma es que no necesitamos estar pidiendo memoria dinámica cada vez que se llama.
    // Desventaja, usamos el doble de memoria para guardar este features as Vector Y si llegara a cambiar algunos de los valores 
    // nosotros tenemos que actualizar FeaturesAsVector también.
    public float[] GetFeaturesVectorNorm()
    {
        return FeaturesAsVectorNorm;
    }

    public void SetFeaturesAsNormVector(float[] features)
    {
        HPNorm = features[0];
        DamageNorm = features[1];
        AttackRateNorm = features[2];
        AttackRangeNorm = features[3];
        MovementSpeedNorm = features[4];
    }
    
    public void PrintStats()
    {
        Debug.Log($"los stats de esta unidad son: HP = {HP}, Damage: {Damage}, AttackRate: {AttackRate}, AttackRange: {AttackRange}, MovementSpeed: {MovementSpeed}" );
    }

    // nada más le pongo ese parámetro porque no quiero que haya constructor vacío.
    public PCGEnemyStats(bool IsForCrossover)
    {
        
    }

    
    // Constructor para copiar valores
    public PCGEnemyStats(PCGEnemyStats entity)
    {
        // LE PONGO QUE NO LOS COPIE PORQUE AL FINAL DEL PROCESO GENERATIVO ES CUANDO SE LE DARÁN ESOS VALORES
        // REALMENTE, CON BASE EN LOS VALORES NORMALIZADOS.
        // HP = entity.HP;
        // Damage = entity.Damage;
        // AttackRate = entity.AttackRate;
        // AttackRange = entity.AttackRange;
        // MovementSpeed = entity.MovementSpeed;
        _configValuesScriptableObject = entity._configValuesScriptableObject;
        
        HPNorm = entity.HPNorm;
        DamageNorm = entity.DamageNorm;
        AttackRateNorm = entity.AttackRateNorm;
        AttackRangeNorm = entity.AttackRangeNorm;
        MovementSpeedNorm = entity.MovementSpeedNorm;
        
        FeaturesAsVectorNorm = new float [5] {HPNorm, DamageNorm, AttackRateNorm, AttackRangeNorm, MovementSpeedNorm};
    }

    // Recalculamos sus características des-normalizándolas, para que tengan los valores útiles para el gameplay.
    public void UpdateStatsBasedOnNormalized()
    {
        HP = HPNorm * (_configValuesScriptableObject.MaxHp - _configValuesScriptableObject.MinHp) +
             _configValuesScriptableObject.MinHp;

        Damage = DamageNorm * (_configValuesScriptableObject.MaxDamage - _configValuesScriptableObject.MinDamage) +
             _configValuesScriptableObject.MinDamage;
        AttackRate = AttackRateNorm * (_configValuesScriptableObject.MaxAttackRate - _configValuesScriptableObject.MinAttackRate) +
             _configValuesScriptableObject.MinAttackRate;
        AttackRange = AttackRangeNorm * (_configValuesScriptableObject.MaxAttackRange - _configValuesScriptableObject.MinAttackRange) +
             _configValuesScriptableObject.MinAttackRange;
        MovementSpeed = MovementSpeedNorm * (_configValuesScriptableObject.MaxMovementSpeed - _configValuesScriptableObject.MinMovementSpeed) +
                        _configValuesScriptableObject.MinMovementSpeed;
    }
    
    // tipo de movimiento, por ejemplo, que te persiga, que huya, que no se mueva, que se mueva en zig-zag, que patrulle un área, etc.
    // enum de tipo de movimiento

    // efecto especial que te aplica al golpearte, por ejemplo, que te stunee, que te queme, envenene, paralice, etc.
    // enum de tipo de efecto especial.

    public float GetDifficultyV1()
    {
        return HP + Damage + AttackRate + AttackRange + MovementSpeed;
    }
    
    public float GetDifficultyV2()
    {
        return (HPNorm + DamageNorm + AttackRateNorm + AttackRangeNorm + MovementSpeedNorm)/5.0f;
    }

    public float GetBalance()
    {
        float functionRange = 0.5f * 5; 
        // NOTA: Me faltaba normalizar antes de hacer el Clamp.
        float value = (HPNorm - 0.5f + DamageNorm - 0.5f + AttackRateNorm - 0.5f + AttackRangeNorm - 0.5f +
            MovementSpeedNorm - 0.5f) / functionRange;
        return 1.0f - Mathf.Abs(Mathf.Clamp(value, -1, 1));
    }
    
    

    // Vamos a poner que hay 10 pasos por eje.
    public List<PCGEnemyStats> GetNeighbors()
    {
        List<PCGEnemyStats> result = new List<PCGEnemyStats>();
        // metemos en result los vecinos de cada eje de nuestro enemigo.

        // todo el rango de HP se divide entre el número de pasos a dar en ese eje.
        // ahorita es del 1 al 100
        // si nuestro original ahorita está en 50 de HP., entonces los vecinos sería 59.9 y 40.1.
        
        // Eje HP.
        PCGEnemyStats neighborHPMinus = new PCGEnemyStats(this); // va a tener lo mismo en todas las características, excepto en HP.
        neighborHPMinus.HPNorm = math.max(neighborHPMinus.HPNorm - _configValuesScriptableObject.HpStepDistanceNorm, 0.0f); // que lo mínimo que pueda tener sea el mínimo del rango.
        result.Add(neighborHPMinus);
        
        PCGEnemyStats neighborHPPlus = new PCGEnemyStats(this); // va a tener lo mismo en todas las características, excepto en HP.
        neighborHPPlus.HPNorm = math.min(neighborHPPlus.HPNorm + _configValuesScriptableObject.HpStepDistanceNorm , 1.0f);
        result.Add(neighborHPPlus);
        
        // si nos fuéramos a salir del rango de ese eje, hay de dos: o lo limitas al rango o lo descartas
        // ahorita vamos a limitarlo al rango.
        
        // Eje Damage.
        PCGEnemyStats neighborDamageMinus = new PCGEnemyStats(this); // va a tener lo mismo en todas las características, excepto en Damage.
        neighborDamageMinus.DamageNorm = math.max(neighborDamageMinus.DamageNorm - _configValuesScriptableObject.DamageStepDistanceNorm, 0.0f); // que lo mínimo que pueda tener sea el mínimo del rango.
        result.Add(neighborDamageMinus);
        
        PCGEnemyStats neighborDamagePlus = new PCGEnemyStats(this); // va a tener lo mismo en todas las características, excepto en Damage.
        neighborDamagePlus.DamageNorm = math.min(neighborDamagePlus.DamageNorm + _configValuesScriptableObject.DamageStepDistanceNorm , 1.0f);
        result.Add(neighborDamagePlus);

        // Eje Attack Rate.
        PCGEnemyStats neighborAttackRateMinus = new PCGEnemyStats(this); // va a tener lo mismo en todas las características, excepto en Attack Rate.
        neighborAttackRateMinus.AttackRateNorm = math.max(neighborAttackRateMinus.AttackRateNorm - _configValuesScriptableObject.AttackRateStepDistanceNorm, 0.0f); // que lo mínimo que pueda tener sea el mínimo del rango.
        result.Add(neighborAttackRateMinus);
        
        PCGEnemyStats neighborAttackRatePlus = new PCGEnemyStats(this); // va a tener lo mismo en todas las características, excepto en Attack Rate.
        neighborAttackRatePlus.AttackRateNorm = math.min(neighborAttackRatePlus.AttackRateNorm + _configValuesScriptableObject.AttackRateStepDistanceNorm, 1.0f);
        result.Add(neighborAttackRatePlus);
        

        // ATTACK RANGE 
        PCGEnemyStats neighborAttackRangeMinus = new PCGEnemyStats(this); // va a tener lo mismo en todas las características, excepto en Attack Range.
        neighborAttackRangeMinus.AttackRangeNorm = math.max(neighborAttackRangeMinus.AttackRangeNorm - _configValuesScriptableObject.AttackRangeStepDistanceNorm, 0.0f); // que lo mínimo que pueda tener sea el mínimo del rango.
        result.Add(neighborAttackRangeMinus);
        
        PCGEnemyStats neighborAttackRangePlus = new PCGEnemyStats(this); // va a tener lo mismo en todas las características, excepto en Attack Range.
        neighborAttackRangePlus.AttackRangeNorm = math.min(neighborAttackRangePlus.AttackRangeNorm + _configValuesScriptableObject.AttackRangeStepDistanceNorm , 1.0f);
        result.Add(neighborAttackRangePlus);
        
        // MOVEMENT SPEED
        PCGEnemyStats neighborMoveSpeedMinus = new PCGEnemyStats(this); // va a tener lo mismo en todas las características, excepto en Movement Speed.
        neighborMoveSpeedMinus.MovementSpeedNorm = math.max(neighborMoveSpeedMinus.MovementSpeedNorm - _configValuesScriptableObject.MovementSpeedStepDistanceNorm, 0.0f); // que lo mínimo que pueda tener sea el mínimo del rango.
        result.Add(neighborMoveSpeedMinus);
        
        PCGEnemyStats neighborMoveSpeedPlus = new PCGEnemyStats(this); // va a tener lo mismo en todas las características, excepto en Movement speed.
        neighborMoveSpeedPlus.MovementSpeedNorm = math.min(neighborMoveSpeedPlus.MovementSpeedNorm + _configValuesScriptableObject.MovementSpeedStepDistanceNorm , 1.0f);
        result.Add(neighborMoveSpeedPlus);
        
        
        return result;

    }
    
    
}
