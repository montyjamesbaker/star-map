using System.Collections.Generic;
using UnityEngine;
public class GalaxyCreatorV1 : MonoBehaviour {
    [SerializeField] int seed;
    [SerializeField] bool randomizeSeed;
    [SerializeField] int numberOfStars;
    [SerializeField] float galaxyRadius;
    [SerializeField] float minDistance;
    [SerializeField] float maxDistance;
    [SerializeField] bool generateNewGalaxy;
    [SerializeField] bool isGalaxyFlat;
    [SerializeField][Range(0.001f, Mathf.Infinity)] float flatGalaxyHeight;
    [SerializeField] bool showGizmos;
    [SerializeField] GameObject starShape;
    [SerializeField] List<Star> starsInGalaxy;
    // Start is called before the first frame update
    void Start() {
        if (randomizeSeed) {
            seed = Random.Range(0, 1000000);
        }
        Random.InitState(seed);
        GenerateGalaxy();
    }
    private void Update() {
        if (generateNewGalaxy) {
            generateNewGalaxy = false;
            if (randomizeSeed) {
                seed = Random.Range(0, 1000000);
            }
            Random.InitState(seed);
            GenerateGalaxy();
        }
    }
    void GenerateGalaxy() {
        foreach (Star star in starsInGalaxy) {
            Destroy(star.gameObject);
        }
        starsInGalaxy = new List<Star>();
        while (starsInGalaxy.Count < numberOfStars) {
            Star newStar = GenerateNewStar();
            if (newStar != null) {
                starsInGalaxy.Add(newStar);
            }
        }
    }
    Star GenerateNewStar() {
        bool posGenerated = false;
        Vector3 starPos = GenerateStarPosition();
        posGenerated = true;
        Collider[] colliders = Physics.OverlapSphere(starPos, minDistance);
        foreach (Collider col in colliders) {
            if (col.CompareTag("Star")) {
                posGenerated = false;
                break;
            }
        }
        if (posGenerated) {
            Star newStar = Instantiate(starShape, starPos, Quaternion.identity).GetComponent<Star>();
            return newStar;
        }
        return null;
    }
    Vector3 GenerateStarPosition() {
        Vector3 position;
        switch (isGalaxyFlat) {
            case true:
                position = Random.insideUnitCircle * galaxyRadius;
                position.z = position.y;
                position.y = Random.Range(0, flatGalaxyHeight) - flatGalaxyHeight / 2;
                break;
            case false:
                position = Random.insideUnitSphere * galaxyRadius;
                break;
        }
        return position;
    }
    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        if (!isGalaxyFlat) {
            Gizmos.DrawWireSphere(transform.position, galaxyRadius);
        }
        else {
            UnityEditor.Handles.DrawWireDisc(transform.position + Vector3.down * flatGalaxyHeight / 2, Vector3.up, galaxyRadius);
            UnityEditor.Handles.DrawWireDisc(transform.position + Vector3.up * flatGalaxyHeight / 2, Vector3.up, galaxyRadius);
        }
        if (showGizmos) {
            foreach (Star star in starsInGalaxy) {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(star.transform.position, minDistance);
                //Gizmos.color = Color.green;
                //Gizmos.DrawWireSphere(star.transform.position, maxDistance);
            }
        }
    }
}