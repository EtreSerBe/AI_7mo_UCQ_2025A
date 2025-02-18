using UnityEngine;

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
    }

    // Qu� necesitan tener nuestros nodos.
    // saber su posici�n o coordenadas X Y, estos valores tambi�n van a ser su ID.
    public int x;
    public int y;
    public bool isWalkable;

    // la referencia al nodo padre en el �rbol generado durante el proceso de pathfinding.
    public Node parentRef;

    // Saber qui�nes son sus vecinos (aristas hacia vecinos)
    // por simplicidad vamos que sus nodos de izquierda, derecha, arriba y abajo son sus vecinos.
    /* Esto de tener una referencia por vecino no lo vamos a hacer en este caso de la cuadr�cula, 
     * porque tomar�a muchos recursos y lo podemos sustituir a trav�s de sumar y restar posiciones en el array de la cuadr�cula.
     * Esta ventaja tiene el costo de tener que checar que no nos salgamos de la cuadr�cula 
     * (tener cuidado con las posiciones 0s en X y Y, y en las del final del array en X y Y).
     * 
     * public Node up;
    public Node right;
    public Node left;
    public Node down;*/
    // teniendo lo anterior en cuenta, sus vecinos est�n impl�citos en la posici�n de cada nodo.
    // Arriba: [x][y-1]
    // Abajo: [x][y+1]
    // Derecha: [x+1][y]
    // Izquierda: [x-1][y]

    // ejemplo, posici�n X2, Y2
    // tu vecino de arriba cu�l es? le sumas uno a la coordenada en Y
    // cu�l ser�a tu vecino de abajo? pues le restas 1 en Y
    // para la derecha? sumar 1 en X
    // para la izquierda? restar 1 en X

    // Ejemplo #2: posici�n X0, Y0
    // si nos intentamos ir hacia la izquierda o hacia arriba, estar�amos yendo de 0 a -1, la cual no es
    // una posici�n v�lida en un array, y eso ser�a un error de access violation (es un error muy grave).



    // m�s tarde: peso de este nodo.

}

public class Grid : MonoBehaviour
{
    // va a tener una cuadr�cula de width*height nodos 
    [SerializeField]
    protected int width = 5;
    [SerializeField]
    protected int height = 5;

    // es mejor que el primer [] sean las Y, y el segundo [] sean las X.
    // esto es mejor para el performance porque permite acceso secuencial a la memoria.
    protected Node[][] nodeGrid; 


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // hay que pedir memoria para nuestro nodeGrid.
        nodeGrid = new Node[height][];

        for(int y = 0; y < height; y++)
        {
            nodeGrid[y] = new Node[width]; // pedimos memoria para toda la fila

            for (int x = 0; x < width; x++)
            {
                nodeGrid[y][x] = new Node(x, y);
            }
        }

        Debug.Log("node grid inicializado");
    }

    // NOTA GRAN NOTA: Seg�n yo se necesita que chequemos y asignar el parent antes de mandar DFS otra vez, porque 
    // si no se cicla infinitamente.
    bool DepthFirstSearchRecursive(Node currentNode, Node goalNode)
    {
        // checamos si ya llegamos a la meta.
        if (currentNode == goalNode)
        {
            // aqu� empezar�amos el backtracking (fin exitoso del la recursi�n)
            return true; // regresamos true porque s� llegamos a la meta. Estamos parados en la meta actualmente.
        }

        // exploramos todos los vecinos y aplicamos DFS sobre cada uno de ellos.
        int x = currentNode.x;
        int y = currentNode.y;
        // checamos los 4 vecinos.
        // VECINO DE ARRIBA (y-1)
        // primero tenemos que checar que y-1 sea una posici�n v�lida en el array. 
        // nos basta con que sea mayor que 0, porque si le restas 1 a 1 o m�s, entonces va a ser 0 o m�s.
        if(y > 0)
        {
            // entonces s� podemos checar a este vecino.
            bool dfsResult = DepthFirstSearchRecursive(nodeGrid[y - 1][x], goalNode);
            if(dfsResult)
            {
                nodeGrid[y - 1][x].parentRef = currentNode;
                // quiere decir que el hijo de currentNode lleg� a la meta, y por lo tanto, tambi�n currentNode
                // le dice a su pap� que �l tambi�n lleg� a la meta.
                return true;
            }
        }

        // si nuestro arreglo fuera Array[height=5] entonces va del 0 al 4,
        // si le vamos a sumar 1 y queremos no salirnos del array, debemos checar que el current
        // sea de -2 que el l�mite de nuestro arreglo.

        // VECINO DE ABAJO (y+1)
        if (y < height-1)
        {
            // entonces s� podemos checar a este vecino.
            bool dfsResult = DepthFirstSearchRecursive(nodeGrid[y + 1][x], goalNode);
            if (dfsResult)
            {
                nodeGrid[y + 1][x].parentRef = currentNode;
                // quiere decir que el hijo de currentNode lleg� a la meta, y por lo tanto, tambi�n currentNode
                // le dice a su pap� que �l tambi�n lleg� a la meta.
                return true;
            }
        }

        // VECINO DERECHA
        if (x < width - 1)
        {
            // entonces s� podemos checar a este vecino.
            bool dfsResult = DepthFirstSearchRecursive(nodeGrid[y][x + 1], goalNode);
            if (dfsResult)
            {
                nodeGrid[y][x + 1].parentRef = currentNode;
                // quiere decir que el hijo de currentNode lleg� a la meta, y por lo tanto, tambi�n currentNode
                // le dice a su pap� que �l tambi�n lleg� a la meta.
                return true;
            }
        }

        // VECINO IZQUIERDA
        if (x > 0)
        {
            // entonces s� podemos checar a este vecino.
            bool dfsResult = DepthFirstSearchRecursive(nodeGrid[y][x - 1], goalNode);
            if (dfsResult)
            {
                nodeGrid[y][x - 1].parentRef = currentNode;
                // quiere decir que el hijo de currentNode lleg� a la meta, y por lo tanto, tambi�n currentNode
                // le dice a su pap� que �l tambi�n lleg� a la meta.
                return true;
            }
        }

        // en este camino (ninguno de sus hijos) no se encontr� el goal, as� que vamos hacia atr�s/arriba.
        return false;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        // tenemos que checar que ya hayamos pedido memoria para la cuadr�cula de nodos.
        // si no, entonces es null y nos salimos de esta funci�n.
        if(nodeGrid == null) return;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (nodeGrid[y][x].isWalkable)
                    Gizmos.DrawCube(new Vector3(x, y, 0.0f), Vector3.one * 0.8f);
                else 
                {
                    // si no es caminable lo dibujamos como una esfera.
                    Gizmos.DrawSphere(new Vector3(x, y, 0.0f), 0.8f);
                }
            }
        }
    }
}
