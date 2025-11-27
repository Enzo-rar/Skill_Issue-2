using UnityEngine;
using UnityEngine.UI;
using TMPro; // Usando TextMeshPro para mejor calidad de texto

public class BotonVentajaUI : MonoBehaviour
{
    [Header("Referencias Visuales")]
    public Image iconoImg;
    public TextMeshProUGUI nombreTxt;
    public TextMeshProUGUI descripcionTxt;
    public Image fondoRareza; // Para cambiar el color según la rareza
    public Button botonComponente;

    [Header("Resaltado Visual")]
    public GameObject bordeResaltado; // Un GameObject hijo que es el borde resaltado
    public Vector3 escalaSeleccionada = new Vector3(1.1f, 1.1f, 1.1f);

    // Colores para las rarezas configurables desde el inspector
    [Header("Configuración de Colores")]
    public Color colorComun = new Color(173f/255f, 203f/255f, 47f/255f); // Verde
    public Color colorRaro = new Color(65f/255f, 136f/255f, 197f/255f); // Azul
    public Color colorEpico = new Color(169f/255f, 52f/255f, 180f/255f); // Morado

    // Método de configuración que llamará el PerkSelectorUI
    public void ConfigurarCarta(Ventajas data, System.Action alHacerClick)
    {
        Debug.Log("Configurando carta de ventaja: " + data.nombreVentaja);
        Debug.Log("Descripcion de la ventaja: " + data.descripcion);
        // 1. Asignar textos e imagenes
        nombreTxt.text = data.nombreVentaja;
        descripcionTxt.text = data.descripcion;
        iconoImg.sprite = data.icono;

        // 2. Asignar color según rareza
        switch (data.rareza)
        {
            case Rareza.Comun: fondoRareza.color = colorComun; break;
            case Rareza.Raro: fondoRareza.color = colorRaro; break;
            case Rareza.Epico: fondoRareza.color = colorEpico; break;
        }

        // 3. Configurar el evento de click
        botonComponente.onClick.RemoveAllListeners(); // Limpieza por si acaso
        botonComponente.onClick.AddListener(() => alHacerClick());
    }
}