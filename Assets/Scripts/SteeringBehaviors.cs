using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/*#define SWITCH_OFF 0
#define SWITCH_ON 1
#define SWITCH_MEDIUM 2*/

public class SteeringBehaviors : MonoBehaviour
{
    //public bool OnOrOff = false;
    //public int SwitchState = (int)SwitchStatus.Off;

    //public string SwitchStateString = "Off";

    //public enum SwitchStatus : byte
    //{
    //    On,
    //    Off,
    //    Medium,
    //    NotSoMuchButSomething
    //}

    /*public enum bitMask
    {
        mask1 = 1,
        mask2 = 2,
        mask3 = 4,
        mask4 = 8,
        mask5 = 16,
        mask6 = 32,
        mask7 = 64,
        mask8 = 128,
    >>
    <<
    }*/

    long Value;

    // VENTAJAS DE LOS ENUMS
    // Legibilidad del c�digo
    // No hardcodear
    // optimizaci�n

    // Uso de un enum

    //public enum SteeringBehaviorEnum
    //{
    //    Seek,
    //    Flee,
    //    Pursuit,
    //    Evade
    //}

    //public SteeringBehaviorEnum currentSteeringBehavior;

    public enum SteeringAction
    {
        Approach,  // seek y pursuit
        Escape   // flee y evade
    }

    public SteeringAction currentSteeringAction = SteeringAction.Approach;

    // Posici�n
    // ya la tenemos a trav�s del transform.position

    // velocidad, cu�nto cambia la posici�n cada X tiempo. // Tiene direcci�n y magnitud
    protected Vector3 currentVelocity = Vector3.zero;

    // para limitar la velocidad de este agente en cada cuadro.
    [SerializeField]
    protected float maxVelocity = 10.0f;

    // Aceleraci�n, cu�nto cambia la velocidad cada X tiempo. // Tiene direcci�n y magnitud
    // protected Vector3 currentAcceleration = new Vector3();
    [SerializeField]
    protected float maxForce = 2.0f;


    // [SerializeField]
    // protected PredictableMovement ReferenciaEnemigo;
    protected GameObject ReferenciaObjetivo;
    protected Rigidbody targetRB;

    // si queda tiempo vemos c�mo quedar�a con esta forma de implementarlo.
    // protected PlayerControllerRef = null; 

    public List<GameObject> obstacleList = new List<GameObject>();

    public float RepelRadius = 3;
    public float MaxRepelForce = 3;


    public void SetEnemyReference(GameObject enemyRef)
    { 
        ReferenciaObjetivo = enemyRef;
        // Debug.Log($"{name} tiene como objetivo a: {enemyRef.name}");

        // tenemos que checar si hay un rigidbody o no.
        if(ReferenciaObjetivo != null)
        { 
            targetRB = ReferenciaObjetivo.GetComponent<Rigidbody>();
            if(targetRB == null)
            {
                Debug.Log("El enemigo referenciado actualmente no tiene Rigidbody. �As� deber�a ser?");
            }
        }
        // si NO hay un ReferenciaObjetivo, entonces le decimos que el targetRB es null tambi�n
        else
        {
            targetRB = null;
        }
    }

    //public void SetEnemyReference(PredictableMovement enemy)
    //{
    //    ReferenciaEnemigo = enemy;
    //}

    [SerializeField]
    protected Rigidbody rb;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    protected Vector3 Seek(Vector3 targetPosition)
    {
        Vector3 steeringForce = Vector3.zero;

        // La flecha que me lleva hacia mi objetivo lo m�s r�pido que yo podr�a ir.
        // el m�todo punta menos cola nos da la flecha hacia el objetivo.
        Vector3 desiredDirection = targetPosition - transform.position;

        Vector3 desiredDirectionNormalized = desiredDirection.normalized;

        //                      // la pura direcci�n hacia objetivo * mi m�xima velocidad posible
        Vector3 desiredVelocity = desiredDirectionNormalized        * maxVelocity;

        // Steering = velocidad deseada � velocidad actual
        steeringForce = desiredVelocity - rb.linearVelocity; // currentVelocity;

        return steeringForce;
    }

    protected Vector3 Flee(Vector3 targetPosition)
    {
        // Flee hace lo mismo que Seek pero en el sentido opuesto.
        // Lo hacemos del sentido opuesto usando el signo de menos '-'.
        return -Seek(targetPosition);
    }

    // Para pursuit necesitamos conocer la velocidad de nuestro objetivo.
    Vector3 Pursuit(Vector3 targetPosition, Vector3 targetCurrentVelocity)
    {
        // Cu�nta distancia hay entre mi objetivo y yo, dividida entre mi m�xima velocidad posible.
        float LookAheadTime = (transform.position - targetPosition).magnitude / maxVelocity;

        Vector3 predictedPosition = targetPosition + targetCurrentVelocity * LookAheadTime;

        return Seek(predictedPosition);
    }

    Vector3 Evade(Vector3 targetPosition, Vector3 targetCurrentVelocity)
    {
        // Cu�nta distancia hay entre mi objetivo y yo, dividida entre mi m�xima velocidad posible.
        float LookAheadTime = (transform.position - targetPosition).magnitude / maxVelocity;

        Vector3 predictedPosition = targetPosition + targetCurrentVelocity * LookAheadTime;

        return -Seek(predictedPosition);
    }

    Vector3 ObstacleAvoidance(Vector3 obstaclePosition, float RepelRadius, float MaxRepelForce)
    {
        Vector3 outVector = Vector3.zero;

        // Devuelve una fuerza proporcional a la posici�n de nuestro personaje con la de nuestro obst�culo.
        // La fuerza va a ir desde el obst�culo hacia nuestro personaje.
        Vector3 PuntaMenosCola = transform.position - obstaclePosition;
        // Qyueremos la direcci�n de esa flecha.
        Vector3 DireccionPuntaMenosCola = PuntaMenosCola.normalized;
        // Usamos la distancia entre nuestro personaje y el obst�culo para ver con qu� tanta fuerza lo va a repeler.
        float distance = PuntaMenosCola.magnitude;
        // Entre m�s grande sea la distancia con respecto al radio de repelencia, menos fuerza nos va a aplicar.
        // si nuestra distancia fuera 0, nos aplica fuerza m�xima.

        if(distance - RepelRadius >= 0)
        {
            // entonces no me aplica nada de fuerza.
        }
        // de lo contrario, estamos dentro del radio de repelencia.
        // el valor de la resta nos dice qu� tan dentro del c�rculo estamos.
        float intersectionDistance = RepelRadius - distance;
        // si es muy negativo quiere decir que estamos muy dentro,
        float intersectionPercentage = intersectionDistance / RepelRadius; // esto lo deja en el rango de 0 a 1.

        outVector = DireccionPuntaMenosCola * intersectionPercentage * MaxRepelForce;

        return outVector;
    }

    private void FixedUpdate()
    {
        Vector3 steeringForce = Vector3.zero;



        // Vector3 steeringForce = Seek(ReferenciaEnemigo.transform.position);


        // Vector3 steeringForce = Pursuit(ReferenciaEnemigo.transform.position, ReferenciaEnemigo.rb.linearVelocity );

        // Solo aplicamos Pursuit si el objetivo que estamos persiguiendo tiene un Rigidbody.
        if (ReferenciaObjetivo != null)
        {

            if (targetRB != null)
            {
                switch (currentSteeringAction)
                {
                    case SteeringAction.Approach:
                        steeringForce = Pursuit(ReferenciaObjetivo.transform.position, targetRB.linearVelocity);
                        break;
                    case SteeringAction.Escape:
                        steeringForce = Evade(ReferenciaObjetivo.transform.position, targetRB.linearVelocity);
                        break;
                }
            }
            else if (ReferenciaObjetivo != null)
            {
                switch (currentSteeringAction)
                {
                    case SteeringAction.Approach:
                        steeringForce = Seek(ReferenciaObjetivo.transform.position);
                        break;
                    case SteeringAction.Escape:
                        steeringForce = Flee(ReferenciaObjetivo.transform.position);
                        break;
                }
            }
            else
            {
                rb.linearVelocity = Vector3.zero;
            }

            foreach(var obstacle in obstacleList)
            {
                steeringForce += ObstacleAvoidance(obstacle.transform.position, RepelRadius, MaxRepelForce);
            }


            // Deber�a estar aqu� pero ahorita no hace nada, seg�n yo.
            steeringForce = Vector3.ClampMagnitude(steeringForce, maxForce);

            rb.AddForce(steeringForce, ForceMode.Acceleration);

            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxVelocity);

            if (rb.linearVelocity.magnitude > maxVelocity)
                Debug.LogWarning(rb.linearVelocity);
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 targetPosition = Vector3.zero;
        Vector3 targetCurrentVelocity = Vector3.zero;

        if (ReferenciaObjetivo != null)
        {
            targetPosition = ReferenciaObjetivo.transform.position;
            if (targetRB != null)
            { targetCurrentVelocity = targetRB.linearVelocity; }
        }

        if (targetRB != null)  // solo dibujar lo del pursuit/evade si el objetivo tiene un rigidbody con velocidad.
        {

            Debug.Log(targetRB.gameObject.name);
            if(ReferenciaObjetivo == null)
            {
                Debug.LogError("referencia objetivoi es null");
            }

            float LookAheadTime = (transform.position - targetPosition).magnitude / maxVelocity;

            Vector3 predictedPosition = targetPosition + targetCurrentVelocity * LookAheadTime;

            Gizmos.DrawCube(predictedPosition, Vector3.one);

            Gizmos.DrawLine(transform.position, predictedPosition);
        }
        else if (ReferenciaObjetivo != null)
        {
            // Si s� hay un objetivo, entonces dibujamos una l�nea hacia ese objetivo.
            Gizmos.DrawLine(transform.position, ReferenciaObjetivo.transform.position);
        }

        //Gizmos.color = Color.red;
        //// Hacemos una l�nea de la velocidad que tiene este agente ahorita.
        //Gizmos.DrawLine (transform.position, transform.position + rb.linearVelocity.normalized * 3);

        //// Dibujamos las fuerzas.
        //Gizmos.color = Color.green;

        //Vector3 steeringForce = Vector3.zero;

        //if (targetRB != null && ReferenciaObjetivo != null)
        //    steeringForce = Pursuit(ReferenciaObjetivo.transform.position, targetCurrentVelocity);

        //steeringForce = Vector3.ClampMagnitude(steeringForce, maxForce);

        //Gizmos.DrawLine(transform.position, transform.position + steeringForce);

    }

    // Update is called once per frame
    /* void Update()
    {
        // Para saber hacia d�nde ir, aplicamos el m�todo punta menos cola, 
        // ReferenciaEnemigo va a ser la punta.
        // La posici�n del due�o de este script va a ser la Cola del vector.
        // Vector3 Difference = ReferenciaEnemigo.transform.position - transform.position;

        // le aplicamos la fuerza del seek a nuestro agente.
        // con esto, no siempre vamos a acelerar la m�xima cantidad.
        Vector3 accelerationVector = Seek(ReferenciaEnemigo.transform.position);

        // nuestra velocidad debe de incrementar de acuerdo a nuestra aceleraci�n.
        currentVelocity += accelerationVector * Time.deltaTime;


        // Queremos obtener velocidad hacia el objetivo.
        // currentVelocity += Difference;
        Debug.Log($"currentVelocity antes de limitarla {currentVelocity}");

        // c�mo limitan el valor de una variable?
        if (currentVelocity.magnitude < maxVelocity)
        {
            // entonces la velocidad se queda como est�, porque no es mayor que max velocity.
        }
        else
        {
            // si no, haces que la velocidad actual sea igual que la velocidad m�xima.
            currentVelocity = currentVelocity.normalized * maxVelocity;
            Debug.Log($"currentVelocity despu�s de limitarla {currentVelocity}");
        }

        //if(Difference.magnitude < DetectionDistance)
        //{
        //    // ya lo detectaste.
        //}



        // Ahora hacemos que la velocidad cambie nuestra posici�n conforme al paso del tiempo.
        transform.position += currentVelocity * Time.deltaTime;
    }*/
}
