using UnityEngine;
using System.Collections.Generic;

public class PerkSelectorUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField]
    public GameObject panelContenedor; // El objeto padre que activas/desactivas
    public Transform contenedorDeCartas; // Donde se instancian los botones (Grid Layout Group)
    public BotonVentajaUI prefabCarta;   // El prefab que tiene el script de arriba
    
    [SerializeField]
    public Canvas canvasVentajas; // Canvas que contiene la UI de ventajas
    // Variables internas para saber quién es quién en este momento
    private int idPerdedorActual;
    private int idGanadorActual;

    // --- LLAMADO POR EL GAMEMANAGER AL FINAL DE LA RONDA ---
    // RECORDAR SIEMPRE GENERAR EN UNA ESCENA PRIMERO UN CANVAS Y LUEGO EL PERKMANAGER ******
    public void InicializarSeleccion(int idPerdedor, int idGanador)
    {
        idPerdedorActual = idPerdedor;
        idGanadorActual = idGanador;
        if (canvasVentajas == null)
        {
            Debug.LogError("Canvas de Ventajas no asignado en PerkSelectorUI.");
            return;
        }

         if (idPerdedor == 1)
        {
            Debug.Log("Asignando camara jugador 1 al canvas de ventajas CamaraJugador1 -> " + GameManager.Instance.playerCamera1.name +" canvasVentajas -> " + canvasVentajas.name);
            canvasVentajas.worldCamera = GameManager.Instance.playerCamera1;
            canvasVentajas.planeDistance = 0.5f; // Asegura que el canvas esté frente a la cámara
        }
        else
        {     Debug.Log("Asignando camara jugador 2 al canvas de ventajas CamaraJugador2 -> " + GameManager.Instance.playerCamera2.name +" canvasVentajas -> " + canvasVentajas.name);
            canvasVentajas.worldCamera = GameManager.Instance.playerCamera2;
            canvasVentajas.planeDistance = 0.5f; // Asegura que el canvas esté frente a la cámara
        }

        panelContenedor.SetActive(true);
       
        GenerarCartas();
    }

    private void GenerarCartas()
    {
        // 1. Limpiar cartas anteriores
        foreach (Transform hijo in contenedorDeCartas)
        {
            Destroy(hijo.gameObject);
        }

        // 2. Pedir 3 ventajas aleatorias al PerkManager
        // (Asegúrate de tener el método ObtenerOpcionesAleatorias en tu PerkManager)
        List<Ventajas> opciones = PerkManager.Instance.ObtenerOpcionesAleatorias(3);

        // 3. Crear los botones
        foreach (Ventajas ventaja in opciones)
        {
            Debug.Log("Generando carta para ventaja: " + ventaja.nombreVentaja);
            //****** Falta Crear un Prefab de BotonVentajaUI y asignarlo en el inspector
            BotonVentajaUI nuevaCarta = Instantiate(prefabCarta, contenedorDeCartas);
            
            // AQUÍ OCURRE LA MAGIA:
            // Le pasamos una "Función Anónima" (Lambda) que define qué pasa al hacer click.
            nuevaCarta.ConfigurarCarta(ventaja, () => ConfirmarEleccion(ventaja));
        }
    }

    private void ConfirmarEleccion(Ventajas ventajaElegida)
    {
        // 1. Comunicar la decisión al PerkManager
        // Nota: Le pasamos los IDs para que él decida si es Buff (para perdedor) o Debuff (para ganador)
        PerkManager.Instance.AplicarVentajaSeleccionada(ventajaElegida, idPerdedorActual, idGanadorActual);

        // 2. Cerrar la UI
        panelContenedor.SetActive(false);
    }
}