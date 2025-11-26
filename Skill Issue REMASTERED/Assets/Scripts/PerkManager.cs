using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Necesario para filtrar listas

public class PerkManager : MonoBehaviour
{
    public static PerkManager Instance;
    
    [Header("Base de Datos")]
    public List<Ventajas> todasLasVentajas;
    // Aquí se podrian cargar las ventajas desde Resources para evitar asignarlas manualmente
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

 // Método llamado desde la UI cuando el perdedor hace click en una carta

public void AplicarVentajaSeleccionada(Ventajas ventaja, int idPerdedor, int idGanador)
{
    // El Manager solo decide A QUIÉN se lo manda, no QUÉ hace.
    if (ventaja.EsDebuff())
    {
        // Si es malo, se lo mandamos al ganador (enemigo)
        ObtenerJugador(idGanador).ActivarVentaja(ventaja);
    }
    else
    {
        // Si es bueno, se lo queda el perdedor
        ObtenerJugador(idPerdedor).ActivarVentaja(ventaja);
    }
}

    // Sistema de Probabilidad por Rareza para generar las 3 opciones
    public List<Ventajas> ObtenerOpcionesAleatorias(int cantidad = 3)
    {
        List<Ventajas> seleccionadas = new List<Ventajas>();
        
        for (int i = 0; i < cantidad; i++)
        {
            Ventajas v = ObtenerVentajaPonderada();
            // Evitar duplicados en la misma mano
            while(seleccionadas.Contains(v)) 
            {
                v = ObtenerVentajaPonderada();
            }
            seleccionadas.Add(v);
        }
        return seleccionadas;
    }

    private Ventajas ObtenerVentajaPonderada()
    {
        // Ejemplo simple de probabilidad:
        // Comun: 60%, Raro: 30%, Epico: 10%
        float roll = Random.Range(0f, 100f);
        Rareza rarezaBuscada;

        if (roll < 60) rarezaBuscada = Rareza.Comun;
        else if (roll < 90) rarezaBuscada = Rareza.Raro;
        else rarezaBuscada = Rareza.Epico;

        // Filtrar la lista global por la rareza que salió
        List<Ventajas> pool = todasLasVentajas.Where(x => x.rareza == rarezaBuscada).ToList();
        
        // Si no hay cartas de esa rareza, devolvemos una cualquiera para evitar errores
        if (pool.Count == 0) return todasLasVentajas[Random.Range(0, todasLasVentajas.Count)];

        return pool[Random.Range(0, pool.Count)];
    }

    private PlayerCharacter ObtenerJugador(int id)
    {   //Esta funcion probablemente cambie para comunicarse con el GameManager, de manera que un jugador se registre en el GameManager al iniciar la partida
        //Luego esta funcion preguntara por ID al GameManager para devolver la referencia correcta
        //lógica para buscar al jugador por ID
        return FindObjectsByType<PlayerCharacter>(FindObjectsSortMode.None).FirstOrDefault(p => p.playerId == id);
    }
}