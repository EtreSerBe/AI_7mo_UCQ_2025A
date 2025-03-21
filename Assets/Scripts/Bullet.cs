using UnityEngine;

/*
 
 bullet1
    damage=5, mask=X, 18 variables m�s

bullet2 
    damage=5, mask=X, 18 variables m�s

bullet3
    damage=5, mask=X
 
*/

/*

bulletScriptableObject 
    damage=5, mask=X, 18 variables m�s

 
bullet1
    ScriptableObjectRef = bulletScriptableObject;
    ScriptableObjectRef.damage
    ScriptableObjectRef.mask

bullet2
    ScriptableObjectRef = bulletScriptableObject:
 */


public class Bullet : MonoBehaviour
{
    [SerializeField]
    protected LayerMask mask;

    // qu� tanto da�o va a hacer esta bala al colisionar contra algo.
    [SerializeField]
    protected float damage;

    public float GetDamage() { return damage; }

    // lo �nico que necesita saber una bala es saber cu�ndo choca.
    private void OnTriggerEnter(Collider other)
    {
        // queremos que s� choque contra Enemigos (Enemy), Paredes (Wall), Obst�culos (Obstacle)
        var maskValue = 1 << other.gameObject.layer;
        var maskANDmaskValue = (maskValue & mask.value);

        // esto es una sola comprobaci�n para filtrar todas las capas que no nos interesan.
        if (maskANDmaskValue > 0)  
        {
            // Debug.Log("Choque con algo en la capa" + LayerMask.LayerToName(other.gameObject.layer) );

            // Vamos a destruir nuestra bala, porque la mayor�a de las balas se destruyen al tocar algo.
            // Si necesit�ramos una bala que se comporte distinto, le podemos hacer override a OnTriggerEnter
            // en la clase espec�fica de esa bala.
            Destroy(gameObject);
        }

    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    Debug.Log("colisi�n con algo en la capa" + LayerMask.LayerToName(collision.gameObject.layer));

    //    // queremos que s� choque contra Enemigos (Enemy), Paredes (Wall), Obst�culos (Obstacle)
    //    var maskValue = 1 << collision.gameObject.layer;
    //    if (~(maskValue & mask.value) == 1)
    //    {
    //        Debug.Log("Choque con algo en la capa" + LayerMask.LayerToName(collision.gameObject.layer));
    //    }
    //}

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
