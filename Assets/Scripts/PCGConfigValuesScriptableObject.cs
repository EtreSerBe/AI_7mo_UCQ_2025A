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

    [HideInInspector]
    public float HpRange;
    [HideInInspector]
    public float DamageRange;
    [HideInInspector]
    public float AttackRateRange;
    [HideInInspector]
    public float AttackRangeRange;
    [HideInInspector]
    public float MovementSpeedRange;


    public int StepCount = 1; // podríamos tener un step size o step count por cada eje, ahorita lo pondré igual para todos.

    [HideInInspector]
    public float HpStepDistance;
    [HideInInspector]
    public float DamageStepDistance;
    [HideInInspector]
    public float AttackRateStepDistance;
    [HideInInspector]
    public float AttackRangeStepDistance;
    [HideInInspector]
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
