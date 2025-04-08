using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class PriorityQueue
{
    private LinkedList<Node> nodes;

    public PriorityQueue()
    {
        nodes = new LinkedList<Node>();
    }
    
    // mete al elemento dado conforme a la prioridad que tenga.
    public void Enqueue(Node node, float priority)
    {
        // vamos a checar todos los nodos desde el inicio, hasta encontrar uno que tenga una prioridad mayor que priority
        LinkedListNode<Node> currentNode = nodes.First;
        while(currentNode != null)
        {
            // checa cuál es su prioridad. Si la del current node es mayor que la del que estamos tratando de insertar
            // entonces ponemos al nuevo antes que este currentNode.
            if(currentNode.Value.priority > priority)
            {
                nodes.AddBefore(currentNode, node);
                return;
            }

            // si la prioridad del current no fue mayor que priority, 
            // entonces pasamos currentNode al siguiente nodo.
            currentNode = currentNode.Next;
        }

        // en este punto el nodo nuevo es el menos prioritario, y por lo tanto se añade al final de la queue.
        nodes.AddLast(node);
    }

    public bool Remove(Node node)
    {
        return nodes.Remove(node);
    }

    public Node Dequeue()
    {
        Node outNode = nodes.First.Value;
        nodes.RemoveFirst();
        return outNode;
    }

    public int Count()
    {
        return nodes.Count;
    }

    public void PrintElements()
    {
        string message = string.Empty;
        foreach (Node node in nodes)
        {
            message += $"X{node.x}, Y{node.y} prio = {node.priority}; ";
        }
        Debug.Log(message);
    }
}
