using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine;

[CreateAssetMenu(fileName = "Mapas", menuName = "Scriptable Objects/Mapas")]
public class Mapas : ScriptableObject
{
    public SceneAsset sceneAsset;
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
