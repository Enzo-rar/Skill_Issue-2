using UnityEngine;

public enum VentajaFavorable
{
    Ninguna,
    DobleSalto,
    WallRun,
    MasVida,
    Velocidad,
    MunicionInfinita
}

public enum VentajaDebuff
{
    Ninguna,
    Flashbang,
    ReduccionVida,
    Lentitud,
    RoboDeArma,
    Confusion

}

public enum Rareza
{
    Comun,
    Raro,
    Epico

}

[CreateAssetMenu(fileName = "Ventajas", menuName = "Scriptable Objects/Ventajas")]public class Ventajas : ScriptableObject
{
    public string nombreVentaja;
    [TextArea] public string descripcion;
    public Sprite icono;
    public Rareza rareza;
    
    
    public VentajaFavorable tipoFavorable; // Si es Ninguna, es un Debuff
    public VentajaDebuff tipoDebuff;  // Si es Ninguna, es un Buff

    // Helper para saber si es Buff o Debuff rápidamente
    public bool EsDebuff()
    {
        return tipoDebuff != VentajaDebuff.Ninguna;
    }
}