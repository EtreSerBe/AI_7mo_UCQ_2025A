using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;




public class Room
{
    public enum RoomStatus : byte
    {
        NonOccupied, 
        Occupied, 
        NonCreated
    }
    
    // Posición X, Y en la cuadrícula del calabozo. // Los puse como byte en lugar de int o short para que no ocupen tanta memoria.
    private byte _xPos;
    private byte _yPos;
    public byte generation = 0; // en qué paso del proceso de generación se creó este room.

    
    public byte XPos => _xPos;
    public byte YPos => _yPos;
    
    

    // Se vuelve occupied cuando se conecta una puerta hacia él (o es el cuarto inicial).
    private RoomStatus _occupied = RoomStatus.NonOccupied; // cuando esté occupied, ya no se puede conectar hacia este cuarto.
    public RoomStatus Occupied => _occupied;

    public void SetAsInitialRoom()
    {
        _occupied = RoomStatus.Occupied;
        _parent = this; // él es su propio padre, para propósitos de que no truenen los gizmos.
    }

    public void SetAsNonCreated()
    {
        _occupied = RoomStatus.NonCreated;
    }

    private Room _parent;

    public Room Parent => _parent;
    
    public void Initialize(Room parentRoom)
    {
        // Este nodo parentRoom es el que te creó,
        _parent = parentRoom;
        // y estás ocupado en el grid.
        _occupied = RoomStatus.Occupied;

        Door newDoor = new Door(parentRoom, this);
        
        // creamos la puerta entre este room y el parentRoom, y ambos deben conocer la puerta.
        _doors.Add(newDoor);
        parentRoom._doors.Add(newDoor);
    }
    
    // eventualmente necesitaremos un parent, un parent direction, el número de iteración de la wave, entre otras.
    
    // Un cuarto debe tener entre 1 y 4 puertas.
    private List<Door> _doors = new List<Door>(); // lo dejo como una list para que sea un poco más flexible que tener solo 4 puertas, por si se quiere cambiar luego.
    
    // Un room puede tener un grid propio de X*Y unidades. Alternativamente, podríamos poner puntos en este room
    // donde se pueden colocar otros elementos, por ejemplo, enemy spawners, escaleras, cofres, etc.
    // Con este ejemplo, vamos a poner X spawn points en el editor y ya.
    [SerializeField] private List<GameObject> _spawnPoints = new List<GameObject>();

    // Que en cada punto del grid o punto de spawn solo haya un elemento.
    // (para este caso en específico, esto se podría cambiar para otro juego)
    
    public void Setup(byte x, byte y) 
    { 
        _xPos = x;
        _yPos = y;
    }


    // computacionalmente costosa, en especial entre más generaciones hay.
    public int GetNumberOfGeneration()
    {
        Room currentRoom = this;
        int counter = 0;
        while (currentRoom._parent != currentRoom)
        {
            currentRoom = currentRoom._parent;
            counter++;
        }

        return counter;
    }
    
}
