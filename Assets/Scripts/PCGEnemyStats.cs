using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using Random = UnityEngine.Random;

// importante que NO herede de Monobehavior para que podamos hacerle New.
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
    
    public void PrintStats()
    {
        Debug.Log($"los stats de esta unidad son: HP = {HP}, Damage: {Damage}, AttackRate: {AttackRate}, AttackRange: {AttackRange}, MovementSpeed: {MovementSpeed}" );
    }
    
    // Constructor para copiar valores
    public PCGEnemyStats(PCGEnemyStats entity)
    {
        HP = entity.HP;
        Damage = entity.Damage;
        AttackRate = entity.AttackRate;
        AttackRange = entity.AttackRange;
        MovementSpeed = entity.MovementSpeed;
        _configValuesScriptableObject = entity._configValuesScriptableObject;
        
        HPNorm = entity.HPNorm;
        DamageNorm = entity.DamageNorm;
        AttackRateNorm = entity.AttackRateNorm;
        AttackRangeNorm = entity.AttackRangeNorm;
        MovementSpeedNorm = entity.MovementSpeedNorm;
        
        FeaturesAsVectorNorm = new float [5] {HPNorm, DamageNorm, AttackRateNorm, AttackRangeNorm, MovementSpeedNorm};
    }
    
    // tipo de movimiento, por ejemplo, que te persiga, que huya, que no se mueva, que se mueva en zig-zag, que patrulle un área, etc.
    // enum de tipo de movimiento

    // efecto especial que te aplica al golpearte, por ejemplo, que te stunee, que te queme, envenene, paralice, etc.
    // enum de tipo de efecto especial.

    public float GetDifficultyV1()
    {
        return HP + Damage + AttackRate + AttackRange + MovementSpeed;
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
        neighborHPMinus.HP = math.max(neighborHPMinus.HP - _configValuesScriptableObject.HpStepDistance, _configValuesScriptableObject.MinHp); // que lo mínimo que pueda tener sea el mínimo del rango.
        result.Add(neighborHPMinus);
        
        PCGEnemyStats neighborHPPlus = new PCGEnemyStats(this); // va a tener lo mismo en todas las características, excepto en HP.
        neighborHPPlus.HP = math.min(neighborHPPlus.HP + _configValuesScriptableObject.HpStepDistance , _configValuesScriptableObject.MaxHp);
        result.Add(neighborHPPlus);
        
        // si nos fuéramos a salir del rango de ese eje, hay de dos: o lo limitas al rango o lo descartas
        // ahorita vamos a limitarlo al rango.
        
        // Eje Damage.
        PCGEnemyStats neighborDamageMinus = new PCGEnemyStats(this); // va a tener lo mismo en todas las características, excepto en HP.
        neighborDamageMinus.Damage = math.max(neighborDamageMinus.Damage - _configValuesScriptableObject.DamageStepDistance, _configValuesScriptableObject.MinDamage); // que lo mínimo que pueda tener sea el mínimo del rango.
        result.Add(neighborDamageMinus);
        
        PCGEnemyStats neighborDamagePlus = new PCGEnemyStats(this); // va a tener lo mismo en todas las características, excepto en HP.
        neighborDamagePlus.Damage = math.min(neighborDamagePlus.Damage + _configValuesScriptableObject.DamageStepDistance , _configValuesScriptableObject.MaxDamage);
        result.Add(neighborDamagePlus);

        // Eje Attack Rate.
        PCGEnemyStats neighborAttackRateMinus = new PCGEnemyStats(this); // va a tener lo mismo en todas las características, excepto en HP.
        neighborAttackRateMinus.AttackRate = math.max(neighborAttackRateMinus.AttackRate - _configValuesScriptableObject.AttackRateStepDistance, _configValuesScriptableObject.MinAttackRate); // que lo mínimo que pueda tener sea el mínimo del rango.
        result.Add(neighborAttackRateMinus);
        
        PCGEnemyStats neighborAttackRatePlus = new PCGEnemyStats(this); // va a tener lo mismo en todas las características, excepto en HP.
        neighborAttackRatePlus.AttackRate = math.min(neighborAttackRatePlus.AttackRate + _configValuesScriptableObject.AttackRateStepDistance , _configValuesScriptableObject.MaxAttackRate);
        result.Add(neighborAttackRatePlus);
        

        // FALTAN ATTACK RANGE Y MOVEMENT SPEED.
        
        
        
        return result;

    }
    
    
}
