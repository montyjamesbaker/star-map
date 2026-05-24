using UnityEngine;
public class ConnectionRendererScript : MonoBehaviour {
    [SerializeField] Material defaultMaterial;
    [SerializeField] Material highlightedMaterial;
    private LineRenderer lineRenderer;
    private Star[] stars = new Star[2];
    public void SetStars(Star star1, Star star2) {
        lineRenderer = GetComponent<LineRenderer>();
        stars[0] = star1;
        stars[1] = star2;
        lineRenderer.SetPosition(0, star1.transform.position);
        lineRenderer.SetPosition(1, star2.transform.position);
    }
    public void SetHighlighted() {
        lineRenderer.material = highlightedMaterial;
    }
    public void Unhighlight() {
        lineRenderer.material = defaultMaterial;
    }
    public Star[] GetStars() {
        return stars;
    }
}