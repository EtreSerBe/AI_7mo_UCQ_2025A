using System;
using System.Collections;
using UnityEngine;

public class CoroutineExample : MonoBehaviour
{
    // Corrutinas

    public float accumulatedTime = 0;
    public float interval = 1.0f;

    public float lastTime = 0;

    // que cada 1 segundo me imprima Hola

    public int contadorCorrutina = 0;

    private Coroutine printSecondCoroutine;

    private IEnumerator PrintSecond()
    {
        int counter = 0;
        while (counter > 10)
        {
            yield return new WaitForSeconds(interval);

            Debug.Log("holi desde la corrutina");
            counter++;
        }
    }

    private IEnumerator Example()
    {
        yield return new WaitForSeconds(1);

        float timeCounter = 0;
        // se 
        while (timeCounter < 3.0f)
        {
            transform.position += Vector3.one* Time.deltaTime;

            timeCounter += Time.deltaTime;

            yield return null;
        }

        Debug.Log("Gruñido del jefe");
        // Activar animación.

        int counter = 0;
        while (counter > 10)
        {
            yield return new WaitForSeconds(interval);

            Debug.Log("holi desde la corrutina");
            counter++;
        }
    }


    public IEnumerator AnimalitoAgeOfEmpires()
    {
        // repetir
        while (true)
        {
            // caminar poquito en una dirección aleatoria
            Debug.Log("estoy caminando poquito");

            // esperar x segundos
            yield return new WaitForSeconds(2);
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lastTime = Time.time;

        printSecondCoroutine = StartCoroutine(PrintSecond());
    }

    // Update is called once per frame
    void Update()
    {
        accumulatedTime += Time.deltaTime; // ir acumulando los delta time en esta variable de accumulatedTime
        if (accumulatedTime > interval)
        {
            Debug.Log("Holiwis");
            accumulatedTime = 0;
        }

        if (Time.time - lastTime > 1.0f)
        {
            Debug.Log("Holis con datetime");
            lastTime = Time.time;
        }
    }
}
