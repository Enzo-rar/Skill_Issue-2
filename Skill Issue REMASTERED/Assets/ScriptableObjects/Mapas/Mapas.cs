#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "Mapas", menuName = "Scriptable Objects/Mapas")]
public class Mapas : ScriptableObject
{  
    #if UNITY_EDITOR
    public SceneAsset sceneAsset;
    #endif
    public string scenePath;


    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (sceneAsset != null)
        {
            scenePath = AssetDatabase.GetAssetPath(sceneAsset);
        }
    }
#endif


}
