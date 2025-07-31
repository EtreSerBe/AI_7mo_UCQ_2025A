using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;


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

            if (i > (a + b - 1) / 2)
            {
                if(direction == EDirection.Down || direction == EDirection.Up)
                    CreateCross(currentRoom, 0, (a + b - 1) / 2, direction);
                else 
                    CreateCross(currentRoom, 2, (a + b - 1) / 2, direction);
            }
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
    
    // una función que checa hacia atrás en el grid, y si están ocupados N bloques, hace algo.
    public bool CheckConsecutiveBlocks(Room room, EDirection directionToCheck, int numberOfBlocks)
    {
        // obtenemos los offsets de X y Y según sea la direction.
        int xOffset = directionToCheck == EDirection.Left ? -1 : directionToCheck == EDirection.Right ? 1 : 0;
        int yOffset = directionToCheck == EDirection.Up ? -1 : directionToCheck == EDirection.Down ? 1 : 0;
        
        // checamos que no nos salgamos de la cuadrícula, si sí nos salimos, no tiene caso checar más.
        if (room.YPos + yOffset * numberOfBlocks < 0 
            || room.YPos + yOffset * numberOfBlocks >= height 
            || room.XPos + xOffset * numberOfBlocks < 0
            || room.XPos + xOffset * numberOfBlocks >= width) // checamos si se saldría de la cuadrícula.
        {
            return false; // no es posible tener N bloques consecutivos.
        }
        
        // ahora hacemos un for para checar numberOfBlocks estén ocupados o no.
        for (int i = 0; i < numberOfBlocks; i++)
        {
            // si no está ocupado, entonces no hay N bloques consecutivos en esta dirección.
            if (_roomsGrid[room.YPos + yOffset * i][room.XPos + xOffset * i].Occupied != Room.RoomStatus.Occupied)
                return false; 
        }

        return true;
    }

    // crea una "cruz" de cuartos, con verticalSpan hacia arriba y hacia abajo, y horizontalSpan hacia la derecha y hacia la izquierda
    public Room CreateCross(Room room, int verticalSpan, int horizontalSpan, EDirection direction)
    {
        Room currentRoom = room;
        for (int i = 1; i < verticalSpan; i++)
        {
            if (currentRoom.YPos - i >= 0)
            {
                ConnectRooms( _roomsGrid[currentRoom.YPos -(i - 1)][currentRoom.XPos], 0, -1);
            }
            if(currentRoom.YPos + i < height ) // crear cuarto hacia arriba
            {
                ConnectRooms( _roomsGrid[currentRoom.YPos +(i - 1)][currentRoom.XPos], 0, 1);
            }
        }
        
        // otro for igualito, pero para horizontal
        for (int i = 1; i < horizontalSpan; i++)
        {
            if (currentRoom.XPos - i >= 0)
            {
                ConnectRooms( _roomsGrid[currentRoom.YPos][currentRoom.XPos -(i - 1)], -1, 0);
            }
            if(currentRoom.XPos + i < width ) // crear cuarto hacia arriba
            {
                ConnectRooms( _roomsGrid[currentRoom.YPos][currentRoom.XPos +(i - 1)], 1, 0);
            }
        }
        
        // regresamos el último cuarto creado en la dirección deseada
        switch (direction)
        {
            case EDirection.Up:
            {
                int maxHeight = math.min(height-1, currentRoom.YPos + verticalSpan);
                return _roomsGrid[maxHeight][currentRoom.XPos];
            }
            case EDirection.Down:
            {
                int minHeight = math.max(0, currentRoom.YPos - verticalSpan);
                return _roomsGrid[minHeight][currentRoom.XPos];
            }
            case EDirection.Right:
            {
                int maxWidth = math.min(currentRoom.XPos + horizontalSpan, width-1);
                return _roomsGrid[currentRoom.YPos][maxWidth];
            }
            case EDirection.Left:
            {
                int minWidth = math.max(0, currentRoom.XPos - horizontalSpan);
                return _roomsGrid[currentRoom.YPos][minWidth];
            }
            default:
                Debug.LogError("No Direction received.");
                return null;
        }
        
    }

    public bool CheckFlowerPlacement(Room room)
    {
        // primero checamos que no estemos hasta arriba del grid, porque si no, al hacer -1 nos saldríamos.
        if (room.YPos - 1 < 0)
            return true; // si no puede haber nadie encima de la flor, entonces no puede haber tierra,
                         // y sí se puede colocar la flor.
        
        // si el room de arriba es una tierra, entonces no podemos poner la flor aquí.
        if (_roomsGrid[room.YPos - 1][room.XPos].currentItemType == Room.EItemType.Dirt)
            return false;
        
        return true;
    }

    public void PlaceFlower(Room room)
    {
        // le decimos al room que ahora tiene una flor.
        room.currentItemType = Room.EItemType.Flower;
        
        // VAMOS A MODIFICAR UN POCO EL EJEMPLO, LA FLOR SOLO VA A IMPLICAR LO DE ABAJO SI ES POSIBLE, NO 
        // VA A CHECAR SI EL DE LA DERECHA TENDRÍA TIERRA ARRIBA.
        // SI EL COLOCAMIENTO INICIAL FUE VÁLIDO, SE SIGUE CON LAS IMPLICACIONES HASTA QUE DEJE DE
        // SER VÁLIDA LA COLOCACIÓN DE LAS IMPLICACIONES
        
        // si la flor nos implicara tener una tierra debajo
        // 1: checamos si debajo de esta planta ya hay una tierra. Si sí, ya no haces nada más.
        // 2: si no la tienes (y por tanto no hay nada debajo) entonces la pones.
        
        // se puede poner 1 de 2, o una flor a la derecha, o un cielo a la derecha.
        if (Random.value > 0.5f)
        {
            if(room.XPos + 1 < width)
            {
                // entonces ponemos flor.
                if (CheckFlowerPlacement(_roomsGrid[room.YPos][room.XPos + 1]))
                {
                    PlaceFlower(_roomsGrid[room.YPos][room.XPos + 1]);
                }
            }
        }
        else
        {
            // si no, ponemos cielo.
            if(room.XPos + 1 < width)
            {
                // entonces ponemos flor.
                if (CheckSkyPlacement(_roomsGrid[room.YPos][room.XPos + 1]))
                {
                    PlaceSky(_roomsGrid[room.YPos][room.XPos + 1]);
                }
            }
        }
        
    }
    
    public void PlaceSky(Room room)
    {
        // le decimos al room que ahora tiene una flor.
        room.currentItemType = Room.EItemType.Sky;
        
        // este de aquí no tiene ninguna implicación, solo se pone.
    }

    
    public bool CheckSkyPlacement(Room room)
    {
        // primero checamos que no estemos hasta arriba del grid, porque si no, al hacer -1 nos saldríamos.
        if (room.YPos - 1 < 0)
            return true; // si no puede haber nadie encima del cielo, entonces no puede haber tierra,
        // y sí se puede colocar el cielo.
        
        // si el room de arriba es una tierra, entonces no podemos poner cielo aquí.
        if (_roomsGrid[room.YPos - 1][room.XPos].currentItemType == Room.EItemType.Dirt)
            return false;
        
        return true;
    }
    
}
