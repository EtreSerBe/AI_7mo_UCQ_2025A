using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    protected LayerMask mask;

    // qué tanto daño va a hacer esta bala al colisionar contra algo.
    [SerializeField]
    protected float damage;

    public float GetDamage() { return damage; }

    // lo único que necesita saber una bala es saber cuándo choca.
    private void OnTriggerEnter(Collider other)
    {
        // queremos que sí choque contra Enemigos (Enemy), Paredes (Wall), Obstáculos (Obstacle)
        var maskValue = 1 << other.gameObject.layer;
        var maskANDmaskValue = (maskValue & mask.value);

        // esto es una sola comprobación para filtrar todas las capas que no nos interesan.
        if (maskANDmaskValue > 0)  
        {
            // Debug.Log("Choque con algo en la capa" + LayerMask.LayerToName(other.gameObject.layer) );

            // Vamos a destruir nuestra bala, porque la mayoría de las balas se destruyen al tocar algo.
            // Si necesitáramos una bala que se comporte distinto, le podemos hacer override a OnTriggerEnter
            // en la clase específica de esa bala.
            Destroy(gameObject);
        }

    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    Debug.Log("colisión con algo en la capa" + LayerMask.LayerToName(collision.gameObject.layer));

    //    // queremos que sí choque contra Enemigos (Enemy), Paredes (Wall), Obstáculos (Obstacle)
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
