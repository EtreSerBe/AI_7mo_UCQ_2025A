using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    protected float currentHP;

    [SerializeField]
    protected float maxHP;



    // MeshRenderer meshRenderer;

    [SerializeField]
    protected int attackDamage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHP = maxHP;
    }

    private void FixedUpdate()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        // si contra quien choc� este enemigo es algo de la capa de Balas del jugador, este enemigo debe de tomar da�o.
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerBullet"))
        {
            // obtenemos el script de bullet de ese gameobject que nos choc�, 
            Bullet collidingBullet = other.GetComponent<Bullet>();
            if(collidingBullet == null)
            {
                // si no tiene un script de Bullet, entonces no hay nada que hacer,
                // probablemente a ese "other" le falta que se le asigne el script de bullet.
                Debug.LogError("error, alguien en la capa PlayerBullet no tiene script de Bullet.");
                return;
            }

            // y nos restamos la vida en la cantidad que Bullet nos diga.
            currentHP -= collidingBullet.GetDamage();


            Debug.Log($"perd� {collidingBullet.GetDamage()} de vida, mi vida ahora es: {currentHP}");


            // si tu vida llega a 0 o menos, te mueres.
            if(currentHP <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

}
