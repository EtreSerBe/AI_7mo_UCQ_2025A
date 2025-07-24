using UnityEngine;

public class Door
{
    // a lo mejor poner su renderer, su animator para que se haga la animación de abrir cuando se abra, etc. 
    
    /*
     * Puede estar abierto.
        Puede estar cerrado con llave o no. Y si la llave es específico o genérica.
        OPCIONAL Puede estar cerrado de un lado pero no del otro.
        Tiene que conectar dos cuartos

     */
    public Door(Room a, Room b)
    {
        _roomA = a;
        _roomB = b;
    }

    // private bool _locked = false;
    // en vez de solo guardar si está cerrado o no, podemos guardar de una vez el ¿con qué está cerrado?
    // en lockId el 0 representa abierto, y los demás valores representan el ID de una llave en específico.
    private short _lockId= 0;
    // otras alternativas serían usar un enum, o strings, u otras cosas.
    private Room _roomA;
    private Room _roomB;
}
