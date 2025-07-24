using System;
using System.Collections.Generic;
using UnityEngine;


public enum EDirection 
{
    Up = 0, 
    Right = 1,
    Down = 2,
    Left = 3, 
}


public class RuleBasedDungeonGenerator : DungeonGenerator
{
    // private Queue<Delegate> rulesOpenQueue = new Queue<Delegate>();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateTileMap();
        GenerateDungeon();
    }

    protected override void GenerateDungeon()
    {
        // extraer las coordenadas del cuarto inicial
        int x = _initialRoomX;
        int y = _initialRoomY;
        _roomsGrid[y][x].SetAsInitialRoom(); // importante setear al inicial, si no, no sabe que ya está ocupado.

        // necesitamos una "lista abierta" de reglas por ejecutar.

        FibonacciRule(1, 1, _roomsGrid[y][x], EDirection.Up);
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public bool FibonacciRule(int a, int b, Room room, EDirection direction)
    {
        // recibir el cuarto desde el cual partir, 

        // obtenemos los offsets de X y Y según sea la direction.
        int xOffset = direction == EDirection.Left ? -1 : direction == EDirection.Right ? 1 : 0;
        int yOffset = direction == EDirection.Up ? -1 : direction == EDirection.Down ? 1 : 0;

        
        Room currentRoom = room;
        // generar a+b cantidad de cuartos en línea recta en la dirección "direction".
        // le restamos uno porque es el que ya se generó a la derecha del último
        for (int i = 0; i < a + b-1; i++)
        {
            if (currentRoom.YPos + yOffset < 0 
                || currentRoom.YPos + yOffset >= height 
                || currentRoom.XPos + xOffset < 0
                || currentRoom.XPos + xOffset >= width) // checamos si se saldría de la cuadrícula.
            {
                return false;
            }

            // generamos un cuarto, lo conectamos con el currentRoom 
            ConnectRooms(currentRoom, xOffset, yOffset);
            currentRoom = _roomsGrid[currentRoom.YPos + yOffset][currentRoom.XPos + xOffset];
        }

        direction = (EDirection)(((int)direction + 1)%4);
        
        // obtenemos los offsets de X y Y según sea la direction.
        xOffset = direction == EDirection.Left ? -1 : direction == EDirection.Right ? 1 : 0;
        yOffset = direction == EDirection.Up ? -1 : direction == EDirection.Down ? 1 : 0;

            
        // después, generar un cuarto a la derecha.
        ConnectRooms(currentRoom, xOffset, yOffset);
        currentRoom = _roomsGrid[currentRoom.YPos + yOffset][currentRoom.XPos + xOffset];


        return FibonacciRule(b, a + b, currentRoom, direction);
    }
    
}
