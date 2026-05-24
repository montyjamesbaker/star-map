using System.Collections.Generic;
using UnityEngine;
public class Star : MonoBehaviour {
    [SerializeField] private GameObject connectionRendererPrefab;
    public Dictionary<Star, float> connections = new Dictionary<Star, float>();
    public static float tempScaleMult = 1;
    private static GUIScript guiScript;
    [SerializeField] private StarType starType;
    private float originalScale;
    public enum StarType {
        RedDwarf,
        SunLike,
        HotBlue,
        WhiteDwarf,
        RedGiant,
        BlackHole,
        None
    }
    private void OnMouseDown() {
        guiScript.SelectStar(this);
    }
    private void Awake() {
        transform.localScale *= tempScaleMult;
        starType = StarType.None;
        guiScript = GameObject.Find("GUICanvas").GetComponent<GUIScript>();
    }
    public void InitialiseStar(StarType type, float scale) {
        if (starType == StarType.None && type != StarType.None) {
            starType = type;
            originalScale = scale;
            transform.localScale *= scale;
            GetComponent<MeshRenderer>().material = (Material)Resources.Load("StarMaterials/" + starType.ToString());
            return;
        }
        // Debug code
        else if (starType != StarType.None) {
            Debug.LogWarning("Tried to set the starType of " + gameObject.name + " despite it already having one");
        }
        else if (type == StarType.None) {
            Debug.LogError("Tried to set the starType of " + gameObject.name + " to None");
        }
    }
    public void AddConnection(Star star, float distance, bool draw = false) {
        connections.Add(star, distance);
        if (draw) {
            GameObject connectionRenderer = Instantiate(connectionRendererPrefab);
            connectionRenderer.transform.SetParent(transform);
            connectionRenderer.GetComponent<ConnectionRendererScript>().SetStars(this, star);
        }
    }
    public void OnGalaxyCreated() {
        transform.localScale = new Vector3(originalScale, originalScale, originalScale);
        foreach (var connection in connections) {
            Debug.Log(connection.Key.gameObject.name + connection.Value);
        }
    }
    public void OnDestroyStars() {
        Destroy(gameObject);
    }
    public StarType GetStarType() {
        return starType;
    }
}