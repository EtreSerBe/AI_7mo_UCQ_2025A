using NUnit.Framework;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PathfollowSteering : SteeringBehaviors
{

    // Una referencia a nuestro pathfinding, porque a �l le vamos a pedir que encuentre un camino 
    // y que nos los devuelva.
    TileGrid _tileGrid;

    [SerializeField]
    int2 goalNodePos = new int2(1, 1);

    List<Node> pathToGoal;

    // Nos dice a  cu�l waypoint se va a dirigir actualmente.
    private int currentTargetWaypoint = 0;

    [SerializeField]
    private float acceptanceRadius = 3;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {


        if( ! TryGetComponent<TileGrid>(out _tileGrid))
        {
            // entonces NO estuvo ese componente e imprimimos un error.
            Debug.LogError($"{name} este gameobject deb�a tener un TileGrid pero no lo tiene, favor de verificar.");
        }
    }

    

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            currentTargetWaypoint = 0;

            // cuando se presione spacebar haga el pathfinding.
            _tileGrid.InitializeGrid();

            // tomar la posici�n inicial de nuestro personaje como la posici�n inicial del pathfinding.
            // obtener el cuadro del grid que est� m�s cerca de nuestro personaje.
            Node nodeClosestToCharacter = _tileGrid.NearestNode(transform.position.x, transform.position.y);
            int2 beginNodePos = new int2(nodeClosestToCharacter.x, nodeClosestToCharacter.y);

            Debug.Log($"la posici�n en la cuadr�cula m�s cercana al personaje es: X{beginNodePos.x} Y{beginNodePos.y}");

            _tileGrid.SetupGrid(beginNodePos, goalNodePos, out Node beginNode, out Node goalNode);

            // Solo por motivos de debug, le modifico su start y goal position al tilegrid.
            _tileGrid.SetBeginPos(beginNodePos.x, beginNodePos.y);
            _tileGrid.SetGoalPos(goalNodePos.x, goalNodePos.y);


            // poner como posici�n objetivo la que nosotros le digamos a trav�s de goalNodePos.
            if (_tileGrid.AStarSearch(beginNode, goalNode))
            {
                Debug.Log("El personaje encontr� un camino.");
                // si s� hubo camino, entonces queremos saber cu�l fue ese camino.
                // guardamos ese camino porque es el que vamos a seguir con nuestro steering behavior.
                pathToGoal = _tileGrid.RouteToGoal(goalNode);
            }
            else
            {
                Debug.Log("El personaje NO encontr� un camino.");
            }
        }
    }

    private void FixedUpdate()
    {
        if (pathToGoal == null)
            return; // Si no hay un camino puesto, no hacer nada.

        Vector3 waypointPosition = new Vector3(pathToGoal[currentTargetWaypoint].x,
            pathToGoal[currentTargetWaypoint].y, 0.0f);

        if ((transform.position - waypointPosition).magnitude <
            acceptanceRadius)
        {
            // Entonces ya lleg� y ahora se mueve hacia el siguiente waypoint.
            currentTargetWaypoint++;
            if (currentTargetWaypoint >= pathToGoal.Count)
            {
                // currentTargetWaypoint = 0;
                currentTargetWaypoint = pathToGoal.Count - 1;
                rb.linearVelocity = Vector3.zero;
                return;
            }
            // currentTargetWaypoint = currentTargetWaypoint % waypoints.Length; // dar�a lo mismo que el if de arriba
            //

            // Gizmos
            // .
        }

        // Hacemos seek hacia el waypoint que actualmente queremos llegar.
        Vector3 steeringForce = Seek(waypointPosition);

        rb.AddForce(steeringForce, ForceMode.Acceleration);
    }
}
