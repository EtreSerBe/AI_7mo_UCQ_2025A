using System.Collections.Generic;
using System.Data;
using System.Drawing;
using Unity.Hierarchy;
using Unity.Mathematics;
using UnityEngine;
using Color = UnityEngine.Color;

public class Node
{
    public Node()
    { 
        parentRef = null;
    }

    public Node(int x, int y)
    {
        this.x = x;
        this.y = y;
        isWalkable = true;
        parentRef = null;
        terrain_cost = 1;
        accumulated_cost = 0; // empieza en 0 porque al fin y al cabo siempre se lo asigna su primer padre.
        final_cost = float.PositiveInfinity;
    }

    // Qué necesitan tener nuestros nodos.
    // saber su posición o coordenadas X Y, estos valores también van a ser su ID.
    public int x;
    public int y;
    public bool isWalkable;
    public bool partOfRoute = false;
    public float priority = 0;
    public float terrain_cost; // terrain cost
    public float accumulated_cost; // costo acumulado del padre.
    public float heuristic_cost; // costo asociado a la heurística
    public float final_cost; // g_cost + f_cost, es decir, accumulated_cost + heuristic_cost.

    // la referencia al nodo padre en el árbol generado durante el proceso de pathfinding.
    public Node parentRef;

    // Saber quiénes son sus vecinos (aristas hacia vecinos)
    // por simplicidad vamos que sus nodos de izquierda, derecha, arriba y abajo son sus vecinos.
    /* Esto de tener una referencia por vecino no lo vamos a hacer en este caso de la cuadrícula, 
     * porque tomaría muchos recursos y lo podemos sustituir a través de sumar y restar posiciones en el array de la cuadrícula.
     * Esta ventaja tiene el costo de tener que checar que no nos salgamos de la cuadrícula 
     * (tener cuidado con las posiciones 0s en X y Y, y en las del final del array en X y Y).
     * 
     * public Node up;
    public Node right;
    public Node left;
    public Node down;*/
    // teniendo lo anterior en cuenta, sus vecinos están implícitos en la posición de cada nodo.
    // Arriba: [x][y-1]
    // Abajo: [x][y+1]
    // Derecha: [x+1][y]
    // Izquierda: [x-1][y]

    // ejemplo, posición X2, Y2
    // tu vecino de arriba cuál es? le sumas uno a la coordenada en Y
    // cuál sería tu vecino de abajo? pues le restas 1 en Y
    // para la derecha? sumar 1 en X
    // para la izquierda? restar 1 en X

    // Ejemplo #2: posición X0, Y0
    // si nos intentamos ir hacia la izquierda o hacia arriba, estaríamos yendo de 0 a -1, la cual no es
    // una posición válida en un array, y eso sería un error de access violation (es un error muy grave).



    // más tarde: peso de este nodo.

}

public class TileGrid : MonoBehaviour
{
    // va a tener una cuadrícula de width*height nodos 
    [SerializeField]
    protected int width = 5;
    [SerializeField]
    protected int height = 5;


    [SerializeField]
    protected int2 beginNodePos = new int2(0, 0);

    [SerializeField]
    protected int2 goalNodePos = new int2(1, 1);


    // es mejor que el primer [] sean las Y, y el segundo [] sean las X.
    // esto es mejor para el performance porque permite acceso secuencial a la memoria.
    protected Node[][] nodeGrid; 


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // hay que verificar que beginNodePos y goalNodePos sean válidas respecto a nuestro width y height.
        if(beginNodePos.x < 0 || beginNodePos.y < 0 || beginNodePos.x >= width || beginNodePos.y >= height )
        {
            Debug.LogError("posición de beginNodePos es inválido porque no está dentro de los límites del nodeGrid");
            return; // nos salimos de la función porque si no ejecutaría lo de más abajo y tronaría.
        }
        if (goalNodePos.x < 0 || goalNodePos.y < 0 || goalNodePos.x >= width || goalNodePos.y >= height)
        {
            Debug.LogError("posición de goalNodePos es inválido porque no está dentro de los límites del nodeGrid");
            return; // nos salimos de la función porque si no ejecutaría lo de más abajo y tronaría.
        }

        InitializeGrid();

        Debug.Log("antes de llamar a DFS recursivo");

        Node beginNode = nodeGrid[beginNodePos.y][beginNodePos.x];
        beginNode.isWalkable = true;
        Node goalNode = nodeGrid[goalNodePos.y][goalNodePos.x];
        goalNode.isWalkable = true;

        // IMPORTANTE: le ponemos al parent de nodeGrid que es igual a él mismo, porque
        // si no, su padre es null y entonces hace un paso equivocado en el pathfinding.
        beginNode.parentRef = beginNode;

        // bool DFSResult = DepthFirstSearchRecursive(beginNode, goalNode);
        // bool DFSResult = DepthFirstSearch(beginNode, goalNode);

        // bool BestFirstSearchResult = BestFirstSearch(beginNode, goalNode);
        bool SearchResult = DjikstraSearch(beginNode, goalNode);
        //bool AStarSearchResult = AStarSearch(beginNode, goalNode);


        if (SearchResult)
        {
            Debug.Log("sí hubo camino");

            Node backtrackingNode = goalNode;
            // haciendo backtracking:
            while (backtrackingNode.parentRef != backtrackingNode)
            {
                Debug.Log($"el nodo X{backtrackingNode.x}, Y{backtrackingNode.y} fue parte del verdadero camino.");
                backtrackingNode.partOfRoute = true;
                backtrackingNode = backtrackingNode.parentRef;
                
            }
        }
        else
        {
            Debug.Log("NO hubo camino");
        }


    }

    void InitializeGrid()
    {
        // hay que pedir memoria para nuestro nodeGrid.
        nodeGrid = new Node[height][];

        for (int y = 0; y < height; y++)
        {
            nodeGrid[y] = new Node[width]; // pedimos memoria para toda la fila

            for (int x = 0; x < width; x++)
            {
                nodeGrid[y][x] = new Node(x, y);
                // Ponemos randoms de caminable o no.
                float rand = UnityEngine.Random.Range(0, 1.0f);
                if(rand < 0.1f)
                {
                    nodeGrid[y][x].isWalkable = false;
                }
            }
        }

        Debug.Log("node grid inicializado");
    }

    bool EnqueueNodeRecursive(Node enqueuedNode, Node currentNode, Node goalNode)
    {
        // cada que intentes poner un nodo como abierto/conocido, hay que checar que su padre sea null.
        if (enqueuedNode.parentRef == null && enqueuedNode.isWalkable == true)
        {
            Debug.Log($" El nodo X{enqueuedNode.x} Y{enqueuedNode.y} ya está siendo abierto/conocido.");

            // le asignamos que el currentNode es su padre.
            enqueuedNode.parentRef = currentNode;

            // entonces sí podemos checar a este vecino.
            bool dfsResult = DepthFirstSearchRecursive(enqueuedNode, goalNode);
            if (dfsResult)
            {
                // quiere decir que el hijo de currentNode llegó a la meta, y por lo tanto, también currentNode
                // le dice a su papá que él también llegó a la meta.
                return true;
            }
        }

        // si no se pudo encolar ni encontró el camino, retorna falso.
        return false;
    }

    // Esta función NO es recursiva.
    bool EnqueueNode(Node enqueuedNode, Node currentNode, Node goalNode, ref Stack<Node> openNodes)
    {
        // cada que intentes poner un nodo como abierto/conocido, hay que checar que su padre sea null.
        if (enqueuedNode.parentRef == null && enqueuedNode.isWalkable == true)
        {
            Debug.Log($" El nodo X{enqueuedNode.x} Y{enqueuedNode.y} ya está siendo abierto/conocido.");

            // le asignamos que el currentNode es su padre.
            enqueuedNode.parentRef = currentNode;

            // entonces sí podemos checar a este vecino.
            // en vez de la recursión, tenemos la pila/stack.
            openNodes.Push(enqueuedNode);
            return true;
        }

        // si no se pudo encolar ni encontró el camino, retorna falso.
        return false;
    }

    bool EnqueueNode(Node enqueuedNode, Node currentNode, Node goalNode, ref Queue<Node> openNodes)
    {
        // cada que intentes poner un nodo como abierto/conocido, hay que checar que su padre sea null.
        if (enqueuedNode.parentRef == null && enqueuedNode.isWalkable == true)
        {
            Debug.Log($" El nodo X{enqueuedNode.x} Y{enqueuedNode.y} ya está siendo abierto/conocido.");

            // le asignamos que el currentNode es su padre.
            enqueuedNode.parentRef = currentNode;

            // entonces sí podemos checar a este vecino.
            // en vez de la recursión, tenemos la pila/stack.
            openNodes.Enqueue(enqueuedNode);
            return true;
        }

        // si no se pudo encolar ni encontró el camino, retorna falso.
        return false;
    }

    void EnqueueNode(Node enqueuedNode, Node currentNode, Node goalNode, ref PriorityQueue openNodes)
    {
        // cada que intentes poner un nodo como abierto/conocido, hay que checar que su padre sea null.
        if (enqueuedNode.parentRef == null && enqueuedNode.isWalkable == true)
        {
            // le asignamos que el currentNode es su padre.
            enqueuedNode.parentRef = currentNode;

            // Al encolar un nodo de prioridad hay que calcular esa prioridad
            // usamos la distancia euclidiana como la prioridad de los nodos. Usamos teorema de Pitágoras
            float priority = math.sqrt( math.square(goalNode.x - enqueuedNode.x) + math.square(goalNode.y - enqueuedNode.y) );
            enqueuedNode.priority = priority;

            // entonces sí podemos checar a este vecino.
            // en vez de la recursión, tenemos la pila/stack.
            openNodes.Enqueue(enqueuedNode, enqueuedNode.priority);
        }
    }

    void DijkstraEnqueueNode(Node enqueuedNode, Node currentNode, Node goalNode, ref PriorityQueue openNodes)
    {
        // cada que intentes poner un nodo como abierto/conocido, hay que checar que su padre sea null.
        if (enqueuedNode.isWalkable == true)
        {
            // primero tenemos que checar si ya tiene padre o no.
            // si no tiene, pues este currentNode ahora es su nuevo padre.
            if(enqueuedNode.parentRef == null)
            {
                enqueuedNode.parentRef = currentNode;
                // el peso de mis antepasados + el peso propio de pasar por este nodo EnqueuedNode
                enqueuedNode.accumulated_cost = currentNode.accumulated_cost + enqueuedNode.terrain_cost;
                enqueuedNode.priority = enqueuedNode.accumulated_cost;
                // finalmente, lo metemos en la fila de prioridad.
                openNodes.Enqueue(enqueuedNode, enqueuedNode.priority);
            }
            // si sí tiene padre, entonces checamos para ver si currentNode sería un padre más barato que el que ya tiene.
            else if(enqueuedNode.parentRef.accumulated_cost >  currentNode.accumulated_cost) 
            {
                // entonces lo tenemos que eliminar de la lista abierta (openNodes) en la posición que tiene ahorita,
                // y lo metemos en la nueva posición que le corresponda.
                openNodes.Remove(enqueuedNode);

                // entonces currentNode sería un padre más barato.
                enqueuedNode.parentRef = currentNode;
                // el peso de mis antepasados + el peso propio de pasar por este nodo EnqueuedNode
                enqueuedNode.accumulated_cost = currentNode.accumulated_cost + enqueuedNode.terrain_cost;
                enqueuedNode.priority = enqueuedNode.accumulated_cost;

                // finalmente, lo metemos en la fila de prioridad.
                openNodes.Enqueue(enqueuedNode, enqueuedNode.priority);
            }
        }
    }

    void AStarEnqueueNode(Node enqueuedNode, Node currentNode, Node goalNode, ref PriorityQueue openNodes)
    {
        // cada que intentes poner un nodo como abierto/conocido, hay que checar que su padre sea null.
        if (enqueuedNode.isWalkable == true)
        {
            // primero tenemos que checar si ya tiene padre o no.
            // si no tiene, pues este currentNode ahora es su nuevo padre.
            if (enqueuedNode.parentRef == null)
            {
                enqueuedNode.parentRef = currentNode;
                // el peso de mis antepasados + el peso propio de pasar por este nodo EnqueuedNode
                enqueuedNode.accumulated_cost = currentNode.accumulated_cost + enqueuedNode.terrain_cost;
                // Sacamos su distancia hacia el nodo objetivo.
                enqueuedNode.heuristic_cost = math.sqrt(math.square(goalNode.x - enqueuedNode.x) 
                    + math.square(goalNode.y - enqueuedNode.y));

                enqueuedNode.final_cost = enqueuedNode.priority = enqueuedNode.accumulated_cost + enqueuedNode.heuristic_cost;
                // finalmente, lo metemos en la fila de prioridad.
                openNodes.Enqueue(enqueuedNode, enqueuedNode.priority);
            }
            // si sí tiene padre, entonces checamos para ver si currentNode sería un padre más barato que el que ya tiene.
            else if (enqueuedNode.parentRef.accumulated_cost > currentNode.accumulated_cost)
            {
                // entonces lo tenemos que eliminar de la lista abierta (openNodes) en la posición que tiene ahorita,
                // y lo metemos en la nueva posición que le corresponda.
                openNodes.Remove(enqueuedNode);

                // entonces currentNode sería un padre más barato.
                enqueuedNode.parentRef = currentNode;
                // el peso de mis antepasados + el peso propio de pasar por este nodo EnqueuedNode
                enqueuedNode.accumulated_cost = currentNode.accumulated_cost + enqueuedNode.terrain_cost;
                // Sacamos su distancia hacia el nodo objetivo.
                enqueuedNode.heuristic_cost = math.sqrt(math.square(goalNode.x - enqueuedNode.x)
                    + math.square(goalNode.y - enqueuedNode.y));

                enqueuedNode.final_cost = enqueuedNode.priority = enqueuedNode.accumulated_cost + enqueuedNode.heuristic_cost;

                // finalmente, lo metemos en la fila de prioridad.
                openNodes.Enqueue(enqueuedNode, enqueuedNode.priority);
            }
        }
    }

    // NOTA GRAN NOTA: Según yo se necesita que chequemos y asignar el parent antes de mandar DFS otra vez, porque 
    // si no se cicla infinitamente.
    bool DepthFirstSearchRecursive(Node currentNode, Node goalNode)
    {
        // checamos si ya llegamos a la meta.
        if (currentNode == goalNode)
        {
            Debug.Log("sí hubo camino");
            // aquí empezaríamos el backtracking (fin exitoso del la recursión)
            return true; // regresamos true porque sí llegamos a la meta. Estamos parados en la meta actualmente.
        }

        // exploramos todos los vecinos y aplicamos DFS sobre cada uno de ellos.
        int x = currentNode.x;
        int y = currentNode.y;
        // checamos los 4 vecinos.
        // VECINO DE ARRIBA (y-1)
        // primero tenemos que checar que y-1 sea una posición válida en el array. 
        // nos basta con que sea mayor que 0, porque si le restas 1 a 1 o más, entonces va a ser 0 o más.
        if (y < height - 1)
        {
            bool dfsResult = EnqueueNodeRecursive(nodeGrid[y + 1][x], currentNode, goalNode);
            if (dfsResult)
                if (dfsResult)
                {
                    nodeGrid[y + 1][x].partOfRoute = true;
                    Debug.Log($"sí hubo camino, y el nodo: X{x}, Y{y + 1} fue parte del camino");
                    return true;
                }
        }

        // si nuestro arreglo fuera Array[height=5] entonces va del 0 al 4,
        // si le vamos a sumar 1 y queremos no salirnos del array, debemos checar que el current
        // sea de -2 que el límite de nuestro arreglo.

        // VECINO DERECHA
        if (x < width - 1)
        {
            bool dfsResult = EnqueueNodeRecursive(nodeGrid[y][x + 1], currentNode, goalNode);
            if (dfsResult)
                if (dfsResult)
                {
                    nodeGrid[y][x + 1].partOfRoute = true;
                    Debug.Log($"sí hubo camino, y el nodo: X{x + 1}, Y{y}, fue parte del camino");
                    return true;
                }
        }

        // VECINO DE ABAJO (y+1)
        if (y > 0)
        {
            bool dfsResult = EnqueueNodeRecursive(nodeGrid[y - 1][x], currentNode, goalNode);
            if (dfsResult)
            {
                nodeGrid[y - 1][x].partOfRoute = true;
                Debug.Log($"sí hubo camino, y el nodo: X{x}, Y{y - 1} fue parte del camino");
                return true;
            }
        }

        // VECINO IZQUIERDA
        if (x > 0)
        {
            bool dfsResult = EnqueueNodeRecursive(nodeGrid[y][x - 1], currentNode, goalNode);
            if (dfsResult)
                if (dfsResult)
                {
                    nodeGrid[y][x - 1].partOfRoute = true;
                    Debug.Log($"sí hubo camino, y el nodo: X{x - 1}, Y{y} fue parte del camino");
                    return true;
                }
        }

        Debug.Log($" El nodo X{x} Y{y} ya está cerrado.");

        // en este camino (ninguno de sus hijos) no se encontró el goal, así que vamos hacia atrás/arriba.
        return false;

    }

    bool DepthFirstSearch(Node origin, Node goal)
    {
        origin.parentRef = origin;

        // La primera condición de terminación de nuestro ciclo es:
        // si ya llegué a la meta, termino y retorno verdadero de que sí llegué a la meta.

        // la otra condición de terminación del ciclo es:
        // si ya no hay acciones por realizar, es decir: si ya no hay más nodos abiertos que visitar.
        // vamos a guardar nuestros nodos abiertos en una Stack (Pila).
        Stack<Node> openNodes = new Stack<Node>();
        // los nodos que ya no les que
        // Es decir, cuando sacas un nodo de la openStack lo pasas a los nodos cerrados.
        // los nodos cerrados únicamente sirven para saber que ya están cerrados.
        HashSet<Node> closedNodes = new HashSet<Node>();

        // Necesitamos meter al primer nodo a nuestro conjunto de nodos abiertos antes de inicial el while.
        openNodes.Push(origin);

        Node currentNode = null;

        while(currentNode != goal && openNodes.Count > 0)
        {
            // current va a ser el nodo que esté hasta arriba de la pila en este momento.
            currentNode = openNodes.Peek();

            // exploramos todos los vecinos y aplicamos DFS sobre cada uno de ellos.
            int x = currentNode.x;
            int y = currentNode.y;
            // checamos los 4 vecinos.
            // VECINO DE ARRIBA (y-1)
            // primero tenemos que checar que y-1 sea una posición válida en el array. 
            // nos basta con que sea mayor que 0, porque si le restas 1 a 1 o más, entonces va a ser 0 o más.
            if (y < height - 1)
            {
                bool dfsResult = EnqueueNode(nodeGrid[y + 1][x], currentNode, goal, ref openNodes);
                if (dfsResult)
                {
                    continue; // si sí se metió un nodo al tope de la pila, hay que iniciar otra vez el while.
                }
            }

            // si nuestro arreglo fuera Array[height=5] entonces va del 0 al 4,
            // si le vamos a sumar 1 y queremos no salirnos del array, debemos checar que el current
            // sea de -2 que el límite de nuestro arreglo.

            // VECINO DERECHA
            if (x < width - 1)
            {
                bool dfsResult = EnqueueNode(nodeGrid[y][x + 1], currentNode, goal, ref openNodes);
                if (dfsResult)
                {
                    continue; // si sí se metió un nodo al tope de la pila, hay que iniciar otra vez el while.
                }
            }

            // VECINO DE ABAJO (y+1)
            if (y > 0)
            {
                bool dfsResult = EnqueueNode(nodeGrid[y - 1][x], currentNode, goal, ref openNodes);
                if (dfsResult)
                {
                    continue; // si sí se metió un nodo al tope de la pila, hay que iniciar otra vez el while.
                }
            }

            // VECINO IZQUIERDA
            if (x > 0)
            {
                bool dfsResult = EnqueueNode(nodeGrid[y][x - 1], currentNode, goal, ref openNodes);
                if (dfsResult)
                {
                    continue; // si sí se metió un nodo al tope de la pila, hay que iniciar otra vez el while.
                }
            }

            Debug.Log($" El nodo X{x} Y{y} ya está cerrado.");

            // Cuando ya llegamos aquí es que el currentNode ya no tiene más acciones disponibles
            // entonces pasa a estar cerrado
            Node closedNode = openNodes.Pop();
            closedNodes.Add( closedNode );  // este nodo cerrado ya nunca se tiene que modificar.
        }

        if(currentNode == goal)
        {
            Debug.Log("Sí hubo camino de manera iterativa");
            return true;
        }
        else
        {
            Debug.Log("NO hubo camino de manera iterativa");
        }

        return false;
    }


    bool BestFirstSearch(Node origin, Node goal)
    {
        origin.parentRef = origin;

        // Nuestros nodos abiertos los vamos a guardar en estructura de datos llamada Fila de Prioridad
        // PriorityQueue.
        PriorityQueue openNodes = new PriorityQueue();
        HashSet<Node> closedNodes = new HashSet<Node>();
        // Necesitamos meter al primer nodo a nuestro conjunto de nodos abiertos antes de inicial el while.
        openNodes.Enqueue(origin, 0.0f);

        Node currentNode = null;
        while (openNodes.Count() > 0)
        {
            // atendemos al de hasta adelante de la fila de prioridad.
            currentNode = openNodes.Dequeue();
            // como ya lo estamos atendiendo, lo pasamos a la lista de nodos cerrados.
            closedNodes.Add(currentNode);

            // si ya encontramos la meta, nos salimos del ciclo y de esta función.
            if(currentNode == goal)
            {
                Debug.Log("Sí hubo camino de manera iterativa");
                return true;
            }

            // Después checamos a sus 4 vecinos.
            int x = currentNode.x;
            int y = currentNode.y;



            // ARRIBA
            if (y < height - 1)
            {
                EnqueueNode(nodeGrid[y + 1][x], currentNode, goal, ref openNodes);
            }

            // DERECHA
            if (x < width - 1)
            {
                EnqueueNode(nodeGrid[y][x + 1], currentNode, goal, ref openNodes);
            }

            // ABAJO
            if ( y > 0)
            {
                EnqueueNode(nodeGrid[y - 1][x], currentNode, goal, ref openNodes);
            }

            // izquierda
            if (x < 0)
            {
                EnqueueNode(nodeGrid[y][x - 1], currentNode, goal, ref openNodes);
            }

            // Imprimimos cuál cerramos, y después cómo quedó la lista abierta tras tratar de encolar a sus vecinos.
            Debug.Log($" El nodo X{x} Y{y} ya está cerrado. La lista abierta quedó como muestra abajo: ");
            openNodes.PrintElements();
        }

        // con el ciclo de esta manera, si llegas a esta parte de la función es porque se te 
        // acabaron los nodos abiertos y no llegaste a la meta, por lo que no encontraste un camino.
        Debug.Log("NO hubo camino.");

        return false;
    }

    bool DjikstraSearch(Node origin, Node goal)
    {
        origin.parentRef = origin;

        // Nuestros nodos abiertos los vamos a guardar en estructura de datos llamada Fila de Prioridad
        // PriorityQueue.
        PriorityQueue openNodes = new PriorityQueue();
        HashSet<Node> closedNodes = new HashSet<Node>();
        // Necesitamos meter al primer nodo a nuestro conjunto de nodos abiertos antes de inicial el while.
        openNodes.Enqueue(origin, 0.0f);

        Node currentNode = null;
        while (openNodes.Count() > 0)
        {
            // atendemos al de hasta adelante de la fila de prioridad.
            currentNode = openNodes.Dequeue();
            // como ya lo estamos atendiendo, lo pasamos a la lista de nodos cerrados.
            closedNodes.Add(currentNode);

            // si ya encontramos la meta, nos salimos del ciclo y de esta función.
            if (currentNode == goal)
            {
                Debug.Log("Sí hubo camino en Djikstra");
                return true;
            }

            // Después checamos a sus 4 vecinos.
            int x = currentNode.x;
            int y = currentNode.y;

            // ARRIBA
            if (y < height - 1)
            {
                DijkstraEnqueueNode(nodeGrid[y + 1][x], currentNode, goal, ref openNodes);
            }

            // DERECHA
            if (x < width - 1)
            {
                DijkstraEnqueueNode(nodeGrid[y][x + 1], currentNode, goal, ref openNodes);
            }

            // ABAJO
            if (y > 0)
            {
                DijkstraEnqueueNode(nodeGrid[y - 1][x], currentNode, goal, ref openNodes);
            }

            // izquierda
            if (x > 0)
            {
                DijkstraEnqueueNode(nodeGrid[y][x - 1], currentNode, goal, ref openNodes);
            }

            // Imprimimos cuál cerramos, y después cómo quedó la lista abierta tras tratar de encolar a sus vecinos.
            Debug.Log($" El nodo X{x} Y{y} ya está cerrado. La lista abierta quedó como muestra abajo: ");
            openNodes.PrintElements();
        }

        // con el ciclo de esta manera, si llegas a esta parte de la función es porque se te 
        // acabaron los nodos abiertos y no llegaste a la meta, por lo que no encontraste un camino.
        Debug.Log("NO hubo camino.");

        return false;
    }

    bool AStarSearch(Node origin, Node goal)
    {
        origin.parentRef = origin;

        // Nuestros nodos abiertos los vamos a guardar en estructura de datos llamada Fila de Prioridad
        // PriorityQueue.
        PriorityQueue openNodes = new PriorityQueue();
        HashSet<Node> closedNodes = new HashSet<Node>();
        // Necesitamos meter al primer nodo a nuestro conjunto de nodos abiertos antes de inicial el while.
        openNodes.Enqueue(origin, 0.0f);

        Node currentNode = null;
        while (openNodes.Count() > 0)
        {
            // atendemos al de hasta adelante de la fila de prioridad.
            currentNode = openNodes.Dequeue();
            // como ya lo estamos atendiendo, lo pasamos a la lista de nodos cerrados.
            closedNodes.Add(currentNode);

            // si ya encontramos la meta, nos salimos del ciclo y de esta función.
            if (currentNode == goal)
            {
                Debug.Log("Sí hubo camino en Djikstra");
                return true;
            }

            // Después checamos a sus 4 vecinos.
            int x = currentNode.x;
            int y = currentNode.y;

            // ARRIBA
            if (y < height - 1)
            {
                AStarEnqueueNode(nodeGrid[y + 1][x], currentNode, goal, ref openNodes);
            }

            // DERECHA
            if (x < width - 1)
            {
                AStarEnqueueNode(nodeGrid[y][x + 1], currentNode, goal, ref openNodes);
            }

            // ABAJO
            if (y > 0)
            {
                AStarEnqueueNode(nodeGrid[y - 1][x], currentNode, goal, ref openNodes);
            }

            // izquierda
            if (x > 0)
            {
                AStarEnqueueNode(nodeGrid[y][x - 1], currentNode, goal, ref openNodes);
            }

            // Imprimimos cuál cerramos, y después cómo quedó la lista abierta tras tratar de encolar a sus vecinos.
            Debug.Log($" El nodo X{x} Y{y} ya está cerrado. La lista abierta quedó como muestra abajo: ");
            openNodes.PrintElements();
        }

        // con el ciclo de esta manera, si llegas a esta parte de la función es porque se te 
        // acabaron los nodos abiertos y no llegaste a la meta, por lo que no encontraste un camino.
        Debug.Log("NO hubo camino.");

        return false;
    }

    bool BreadthFirstSearch(Node origin, Node goal)
    {
        origin.parentRef = origin;

        // La primera condición de terminación de nuestro ciclo es:
        // si ya llegué a la meta, termino y retorno verdadero de que sí llegué a la meta.

        // la otra condición de terminación del ciclo es:
        // si ya no hay acciones por realizar, es decir: si ya no hay más nodos abiertos que visitar.
        // vamos a guardar nuestros nodos abiertos en una Stack (Pila).
        Queue<Node> openNodes = new Queue<Node>();
        // los nodos que ya no les que
        // Es decir, cuando sacas un nodo de la openStack lo pasas a los nodos cerrados.
        // los nodos cerrados únicamente sirven para saber que ya están cerrados.
        HashSet<Node> closedNodes = new HashSet<Node>();

        // Necesitamos meter al primer nodo a nuestro conjunto de nodos abiertos antes de inicial el while.
        openNodes.Enqueue(origin);

        Node currentNode = null;

        while (currentNode != goal && openNodes.Count > 0)
        {
            // current va a ser el nodo que esté hasta arriba de la pila en este momento.
            currentNode = openNodes.Dequeue();

            // Cuando ya llegamos aquí es que el currentNode ya no tiene más acciones disponibles
            // entonces pasa a estar cerrado
            closedNodes.Add(currentNode);  // este nodo cerrado ya nunca se tiene que modificar.

            // exploramos todos los vecinos y aplicamos DFS sobre cada uno de ellos.
            int x = currentNode.x;
            int y = currentNode.y;
            // checamos los 4 vecinos.
            // VECINO DE ARRIBA (y-1)
            // primero tenemos que checar que y-1 sea una posición válida en el array. 
            // nos basta con que sea mayor que 0, porque si le restas 1 a 1 o más, entonces va a ser 0 o más.
            if (y < height - 1)
            {
                EnqueueNode(nodeGrid[y + 1][x], currentNode, goal, ref openNodes);
            }

            // si nuestro arreglo fuera Array[height=5] entonces va del 0 al 4,
            // si le vamos a sumar 1 y queremos no salirnos del array, debemos checar que el current
            // sea de -2 que el límite de nuestro arreglo.

            // VECINO DERECHA
            if (x < width - 1)
            {
                EnqueueNode(nodeGrid[y][x + 1], currentNode, goal, ref openNodes);
            }

            // VECINO DE ABAJO (y+1)
            if (y > 0)
            {
                EnqueueNode(nodeGrid[y - 1][x], currentNode, goal, ref openNodes);
            }

            // VECINO IZQUIERDA
            if (x > 0)
            {
                EnqueueNode(nodeGrid[y][x - 1], currentNode, goal, ref openNodes);
            }

            Debug.Log($" El nodo X{x} Y{y} ya está cerrado.");
        }

        if (currentNode == goal)
        {
            Debug.Log("Sí hubo camino de manera iterativa");
            return true;
        }
        else
        {
            Debug.Log("NO hubo camino de manera iterativa");
        }

        return false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        // tenemos que checar que ya hayamos pedido memoria para la cuadrícula de nodos.
        // si no, entonces es null y nos salimos de esta función.
        if(nodeGrid == null) return;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (nodeGrid[y][x].isWalkable)
                {
                    Gizmos.color = Color.white;

                    if (nodeGrid[y][x].partOfRoute == true)
                    {
                        Gizmos.DrawSphere(new Vector3(x, y, 0.0f), 0.5f);
                    }
                    Gizmos.DrawCube(new Vector3(x, y, 0.0f), Vector3.one * 0.5f);

                    // ahora dibujemos una línea de padre a hijo.
                    if (nodeGrid[y][x].parentRef != null)
                    {
                        Gizmos.color = Color.red;
                        Vector3 parentPos = new Vector3(nodeGrid[y][x].parentRef.x, nodeGrid[y][x].parentRef.y, 0);
                        Vector3 currentPos = new Vector3(x, y, 0.0f);
                        Gizmos.DrawLine(parentPos, currentPos);
                    }
                }
                else
                {
                    Gizmos.color = Color.black;
                    // si no es caminable lo dibujamos como una esfera.
                    Gizmos.DrawWireSphere(new Vector3(x, y, 0.0f), 0.5f);
                }
            }
        }

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(new Vector3(beginNodePos.x, beginNodePos.y, 0.0f), 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(new Vector3(goalNodePos.x, goalNodePos.y, 0.0f), 0.5f);

    }
}
