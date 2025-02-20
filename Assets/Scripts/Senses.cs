using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Senses : MonoBehaviour
{

    protected List<GameObject> refEnemigosDetectados = new List<GameObject>();

    protected List<GameObject> refObstaculosDetectados = new List<GameObject>();

    public List<GameObject> GetDetectedObstacles() { return refObstaculosDetectados; }

    // protected MonoBehaviour 
    //public LayerMask layerMask;



    // Manera #1
    // radio alrededor del dueño de este script en el cual se detectarán gameObjects.
    [SerializeField]
    protected float DetectionRadius = 12.5f;

    // Manera #2
    // detectar a través de colliders.
    protected SphereCollider visionColliderSphere;


    protected bool isEnemyDetected = false;

    protected GameObject detectedEnemy = null;

    public GameObject GetDetectedEnemyRef()
    {
        return detectedEnemy;
    }

    public bool IsEnemyDetected() { return isEnemyDetected; }

    private Coroutine CorrutinaDesalertar;

    bool IsAlerted = false;

    private IEnumerator Desalertar()
    {
        yield return new WaitForSeconds(5);

        Debug.LogWarning("Pasamos a desalerta");
        IsAlerted = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        visionColliderSphere = GetComponent<SphereCollider>();
        if (visionColliderSphere != null)
        {
            visionColliderSphere.radius = DetectionRadius;
        }
        else // nos faltaba hacerle saber al usuario que le falta asignar esto o hubo un error en esto y debería hacer algo al respecto
        {
            Debug.LogError("visionColliderSphere es null, ¿olvidaste asignar el SphereCollider a este gameobject?");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"entró a colisión con: {other.gameObject.name}");
        // si alguien choca contra nuestro visionColliderSphere,
        // entonces alguien acaba de entrar a nuestro rango de visión.

        if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            //Debug.Log(layerMask.ToString());
            //Debug.Log( LayerMask.GetMask(layerMask.ToString()));
            // si quien chocó conmigo es un player, lo añado a las entidades que están en mi rango de visión.
            refEnemigosDetectados.Add(other.gameObject);
            // Si detectamos de nuevo al player, pues le decimos que cancele el desalertar
            IsAlerted = true;
            StopCoroutine( CorrutinaDesalertar ); 
        }
        // tenerlo como else hace que le dé prioridad al player.
        else if(other.gameObject.layer == LayerMask.NameToLayer("Waypoints")) 
        {
            refEnemigosDetectados.Add(other.gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            refObstaculosDetectados.Add(other.gameObject);
        }

    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"Salió de colisión con: {other.gameObject.name}");

        // si alguien deja de chocar contra nuestro visionColliderSphere,
        // entonces ya lo vamos a quitar de nuestros enemigos conocidos.
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // si quien chocó conmigo es un player, lo añado a las entidades que están en mi rango de visión.
            refEnemigosDetectados.Remove(other.gameObject);
            CorrutinaDesalertar = StartCoroutine(Desalertar());
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Waypoints"))
        {
            refEnemigosDetectados.Remove(other.gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            refObstaculosDetectados.Remove(other.gameObject);
        }
    }

    public bool IsInsideRadius(Vector3 pos1, Vector3 pos2, float radius)
    {
        if ((pos1 - pos2).magnitude <= radius)
            return true;

        return false;
    }

    // Qué es una sobrecarga de función? (function overload)
    public bool IsInsideRadius(GameObject pos1, GameObject pos2, float radius)
    {
        return IsInsideRadius(pos1.transform.position, pos2.transform.position, radius);
    }

    public List<GameObject> GetNearbyObjects(Vector3 originPosition, float radius)
    {
        List<GameObject> nearbyObjects = new List<GameObject> ();

        GameObject[] foundObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.InstanceID);

        foreach (GameObject obj in foundObjects)
        {
            if (obj == this.gameObject) // ignorar al dueño de este script, para que no se encuentre a sí mismo.
                continue;

            // Si este encontrado está suficientemente cerca, pues lo meto en la lista
            if(IsInsideRadius(originPosition, obj.transform.position, radius) )
            {
                nearbyObjects.Add(obj);
            }
            // si no, pues no hacemos nada con él.
        }

        return nearbyObjects;
        
    }

    private void FixedUpdate()
    {

        // refEnemigosDetectados = GetNearbyObjects(transform.position, DetectionRadius);

        // que nos ordene los objetivos encontrados por algún parámetro, por ejemplo, la distancia de menor a mayor.
        // refEnemigosDetectados.Sort()
        float bestDistance = float.MaxValue;
        GameObject nearestGameObj = null;
        foreach ( GameObject obj in refEnemigosDetectados)
        {
            float currentDistance = (transform.position - obj.transform.position).magnitude;
            if (currentDistance < bestDistance)
            {
                // esta es nuestra nueva mejor distancia, y guardamos a cuál Objeto se refiere.
                bestDistance = currentDistance;
                nearestGameObj = obj;
            }
        }

        if(nearestGameObj != null)
        {
            isEnemyDetected = true;
        }
        else
        {
            isEnemyDetected = false;
        }
        detectedEnemy = nearestGameObj;


        //// Queremos saber la distancia entre el GameObject dueño de este script y el Enemigo.
        //// if ((gameObject.transform.position - ReferenciaEnemigo.transform.position).magnitude
        ////    <= DetectionRadius)
        //if (IsInsideRadius(gameObject, ReferenciaEnemigo, DetectionRadius))
        //{
        //    isEnemyDetected = true;
        //    detectedEnemy = ReferenciaEnemigo.gameObject;
        //    // Debug.Log("Hora de correr");
        //    Debug.Log("Enemigo está dentro del radio de detección.");
        //}
        //else
        //{
        //    isEnemyDetected = false;
        //}
    }

    private void OnDrawGizmos()
    {
        // lo tenemos que dibujar incluso aunque aún no hayamos detectado al enemigo, 
        // para poder visualizar mejor ese radio.
        Gizmos.color = Color.green;

        if (detectedEnemy != null)
        {        // Queremos saber la distancia entre el GameObject dueño de este script y el Enemigo.
            if (IsInsideRadius(gameObject, detectedEnemy, DetectionRadius))
            {
                Gizmos.color = Color.red;
            }
        }

        // haya detectado o no al enemigo debe dibujar la esfera de detección.
        Gizmos.DrawWireSphere(transform.position, DetectionRadius);

    }


    // Update is called once per frame
    void Update()
    {


    }
}
