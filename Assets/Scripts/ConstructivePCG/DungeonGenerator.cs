using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] protected int width = 6;
    [SerializeField] protected int height = 6;

    [SerializeField] private float roomGenerationProbability = 0.5f;

    [SerializeField] private bool useNonCreatedRooms = false;
    
    // Necesitamos una cuadrícula que represente el espacio posible de nuestro calabozo.
    // ¿Qué va a haber en cada uno de esos espacios de la cuadrícula? Pues cuartos del calabozo.
    public Room[][] _roomsGrid = null;

    // cuarto desde el cual se iniciará la generación del calabozo.
    public int _initialRoomX;
    public int _initialRoomY;
    
    // Se le suma a la probabilidad de sí spawnear un cuarto al iniciar el proceso.
    [Range(-1.0f, 1.0f)]
    [SerializeField] private float initialRoomGenerationProbabilityBoost = 0.25f;
    
    [Range(-1.0f, 1.0f)]
    [SerializeField] private float targetRoomGenerationProbabilityBoost = 0.25f;


    // la usamos para reducir initialRoomGenerationProbabilityBoost linealmente hasta 0. 
    [SerializeField] private int roundsBeforeTargetProbabilityBoost = 5;
    private float _currentRoomGenerationProbabilityBoost;


    [SerializeField] private float cosineProbabilityModifierMagnitude = 0.4f;
    [SerializeField] private float cosineFrequency = 0.5f;
    

    public Vector3 cubeSize = new Vector3(0.5f, 0.5f, 0.5f);

    // La lista de Rooms que nos falta por explorar/expandir en el algoritmo de generación
    [SerializeField] private readonly Queue<Room> _openQueue = new Queue<Room>();
    
    protected void GenerateTileMap()
    {
        _roomsGrid = new Room[height][];

        for(byte j = 0; j < height; j++)
        {
            _roomsGrid[j] = new Room[width];
            for (byte i = 0; i < width; i++)
            {
                // Lo primero sería instanciarlos.
                _roomsGrid[j][i] = new Room();
                // Esto nos permite comunicar a los Rooms con el tilemap para preguntar qué rooms hay alrededor.
                _roomsGrid[j][i].Setup(i, j); 
            }
        }
    }

    protected virtual void GenerateDungeon()
    {
        // iniciamos este valor como el valor inicial, para poder irlo decrementando.
        _currentRoomGenerationProbabilityBoost = initialRoomGenerationProbabilityBoost ;
        
        // extraer las coordenadas del cuarto inicial
        int x = _initialRoomX;
        int y = _initialRoomY;
        _roomsGrid[y][x].SetAsInitialRoom(); // importante setear al inicial, si no, no sabe que ya está ocupado.
        
        _openQueue.Enqueue(_roomsGrid[y][x]);
        Room currentRoom = null;
        while(_openQueue.Count > 0)
        {
            currentRoom = _openQueue.Dequeue();
            // Checar si se van a crear cuartos aledaños.
            GenerateNeighborRooms(currentRoom.XPos, currentRoom.YPos);
        }
    }

    // Esta la función de "Expandir"
    void GenerateNeighborRooms(int x, int y)
    {
        Room currentRoom = _roomsGrid[y][x];
        // De este currentRoom, checamos sus 4 vecinos (arriba, abajo, izquierda derecha)
        if (currentRoom.YPos > 0) // checamos al vecino de arriba
        {
            ConnectRoomsWithProbability(currentRoom, 0, -1);
        }
        if (currentRoom.YPos < height - 1) // checamos al vecino de abajo
        {
            ConnectRoomsWithProbability(currentRoom, 0, 1);
        }
        
        if (currentRoom.XPos > 0) // checamos al vecino de izquierda
        {
            ConnectRoomsWithProbability(currentRoom, -1, 0);
        }
        if (currentRoom.XPos < width - 1) // checamos al vecino de derecha
        {
            ConnectRoomsWithProbability(currentRoom, 1, 0);
        }

        // Todos los vecinos que se generen aquí, se añaden a una "lista abierta" de nodos que faltan por expandir. 
    }

    protected void ConnectRoomsWithProbability(Room currentRoom, int  xOffset, int yOffset)
    {
        Room roomToConnect = _roomsGrid[currentRoom.YPos + yOffset][currentRoom.XPos + xOffset];
        // le preguntamos a la cuadrícula _roomsGrid si esa coordenada ya está ocupada Y que no esté como no-creado.
        if (roomToConnect.Occupied == Room.RoomStatus.NonOccupied && 
            roomToConnect.Occupied != Room.RoomStatus.NonCreated )
        {
            // interpolación lineal.
            // _currentRoomGenerationProbabilityBoost= math.lerp(initialRoomGenerationProbabilityBoost, 
            //                                                 targetRoomGenerationProbabilityBoost,
            //                                                 currentRoom.generation / (float)roundsBeforeTargetProbabilityBoost);

            // manejo con función coseno.
            _currentRoomGenerationProbabilityBoost =
                math.cos(currentRoom.generation*math.PI*cosineFrequency) * cosineProbabilityModifierMagnitude;
            
            // si sí, no hacemos nada en esa dirección.
            // si no, intentamos ver si se crea un cuarto.
            // intentamos crear un cuarto en esta dirección, con cierta probabilidad.
            if (Random.value < roomGenerationProbability + _currentRoomGenerationProbabilityBoost)
            {
                // entonces sí lo generamos
                roomToConnect.Initialize(currentRoom);
                Debug.Log($"Se generó el cuarto con coordenadas: X: {currentRoom.XPos + xOffset}, Y: {currentRoom.YPos + yOffset}");
                // le decimos al nodo que vamos a encolar que él pertenece a la generación siguiente de la se papá.
                roomToConnect.generation = currentRoom.generation;
                roomToConnect.generation += 1;
                _openQueue.Enqueue(roomToConnect); // lo metemos en la fila de cuartos por expandir. 
            }
            else if (useNonCreatedRooms)
            {
                roomToConnect.SetAsNonCreated();
            }
        }
    }

    protected void ConnectRooms(Room currentRoom, int xOffset, int yOffset)
    {
        Room roomToConnect = _roomsGrid[currentRoom.YPos + yOffset][currentRoom.XPos + xOffset];
        // le preguntamos a la cuadrícula _roomsGrid si esa coordenada ya está ocupada Y que no esté como no-creado.
        if (roomToConnect.Occupied == Room.RoomStatus.NonOccupied && 
            roomToConnect.Occupied != Room.RoomStatus.NonCreated )
        {
            // entonces sí lo generamos
            roomToConnect.Initialize(currentRoom);
            Debug.Log($"Se generó el cuarto con coordenadas: X: {currentRoom.XPos + xOffset}, Y: {currentRoom.YPos + yOffset}");
            // le decimos al nodo que vamos a encolar que él pertenece a la generación siguiente de la se papá.
            roomToConnect.generation = currentRoom.generation;
            roomToConnect.generation += 1;
            _openQueue.Enqueue(roomToConnect); // lo metemos en la fila de cuartos por expandir. 
        }
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateTileMap();
        GenerateDungeon();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        // como la cuadrícula solo se inicializa una vez que se le da play, hay que evitar que esto se 
        // ejecute antes de dar play.
        if (_roomsGrid == null)
            return;

        
        
        // vamos a dibujar toda la cuadrícula, y poner dónde sí está ocupado y dónde no.
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Donde está ocupado vamos a dibujar un cubito
                if (_roomsGrid[y][x].Occupied == Room.RoomStatus.Occupied)
                {
                    Gizmos.DrawCube(new Vector3(x, y, 0.0f), cubeSize);
                    
                    // vamos a aprovechar la referencia de cada Room a su parent, para 
                    // dibujar las conexiones entre ellos.
                    Gizmos.DrawLine(new Vector3(x, y, 0.0f), 
                        new Vector3(_roomsGrid[y][x].Parent.XPos, _roomsGrid[y][x].Parent.YPos, 0.0f));
                    
                }
                else if(_roomsGrid[y][x].Occupied == Room.RoomStatus.NonOccupied)
                {
                    // donde no, dibujamos una esfera
                    Gizmos.DrawSphere(new Vector3(x, y, 0), 0.25f);
                }
                else // los Non-created
                {
                    Gizmos.DrawWireSphere(new Vector3(x, y, 0), 0.25f);
                }
                

                
            }
        }
        
        Gizmos.color = Color.green;
        Gizmos.DrawCube(new Vector3(_initialRoomX, _initialRoomY, 0.0f), new Vector3(0.75f,0.75f,0.75f ));


        
    }
}
