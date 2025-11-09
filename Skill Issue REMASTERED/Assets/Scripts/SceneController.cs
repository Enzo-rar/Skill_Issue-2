using UnityEngine;

public class SceneController : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    private GameObject _enemy;

    [SerializeField] private Transform[] spawnPoints; 

    void Update()
    {
        if (_enemy == null)
        {   // ¡OJO!
            _enemy = Instantiate<GameObject>(enemyPrefab); // instanciar si no había un enemigo

            var index = Random.Range(0, spawnPoints.Length);
            var pos = spawnPoints[index].position;
            
            _enemy.transform.position = pos;
            float angle = Random.Range(0, 360);
            _enemy.transform.Rotate(0, angle, 0);
        }
    }
}

