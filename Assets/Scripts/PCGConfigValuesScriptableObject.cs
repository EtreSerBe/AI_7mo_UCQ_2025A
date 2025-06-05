using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PCGConfigValuesScriptableObject", order = 1)]
public class PCGConfigValuesScriptableObject : ScriptableObject
{
    public float MinHp = 1;
    public float MaxHp = 1000;
    public float MinDamage = 0.1f;
    public float MaxDamage = 1000;
    public float MinAttackRate = 10;
    public float MaxAttackRate = 0.05f;
    public float MinAttackRange = 0.1f;
    public float MaxAttackRange = 100;
    public float MinMovementSpeed = 0.0f;
    public float MaxMovementSpeed = 100;

    public float HpRange;
    public float DamageRange;
    public float AttackRateRange;
    public float AttackRangeRange;
    public float MovementSpeedRange;


    public int StepCount = 1; // podríamos tener un step size o step count por cada eje, ahorita lo pondré igual para todos.

    public float HpStepDistance;
    public float DamageStepDistance;
    public float AttackRateStepDistance;
    public float AttackRangeStepDistance;
    public float MovementSpeedStepDistance;



    public void Initialize()
    {
        HpRange = MaxHp - MinHp;
        DamageRange = MaxDamage - MinDamage;
        AttackRateRange = MaxAttackRate - MinAttackRate;
        AttackRangeRange = MaxAttackRange - MinAttackRange;
        MovementSpeedRange = MaxMovementSpeed - MinMovementSpeed;

        HpStepDistance = HpRange / StepCount;
        DamageStepDistance = DamageRange / StepCount;
        AttackRateStepDistance = AttackRateRange / StepCount;
        AttackRangeStepDistance = AttackRangeRange / StepCount;
        MovementSpeedStepDistance = MovementSpeedRange / StepCount;
    }
    


}
