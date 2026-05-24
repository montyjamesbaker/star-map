using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class GalaxyCreator : MonoBehaviour {
    [SerializeField] private GameObject starPrefab;
    [SerializeField] public bool flatGalaxy;
    [SerializeField] public int numberOfStars;
    [SerializeField] public int maxConnectionsPerStar;
    [SerializeField] public bool forceConnectAllStars;
    [SerializeField] public float minDist;
    [SerializeField] public float maxDist;
    [SerializeField] public float distScaling; // Multiplier for minDist and maxDist based on how large a star is (larger stars can be connected to further ones, but not closer ones)
    [SerializeField] public int seed;
    [SerializeField] public bool randomizeSeed;
    private Dictionary<Star.StarType, int> starTypeChances = new Dictionary<Star.StarType, int>(); // All of the values should always add up to 100
    private Dictionary<Star.StarType, float[]> starScaleRanges = new Dictionary<Star.StarType, float[]>();
    private Dictionary<Star, List<Star>> connectionsInGalaxy = new Dictionary<Star, List<Star>>();
    public List<Star> starsInGalaxy = new List<Star>();
    private void Awake() {
        Screen.SetResolution(1920, 1080, true);
        // Give TextHandler the StarNames text file so that it can get star names from it later on
        TextHandler.ReadFile("Assets/Resources/StarNames.txt");
        Star.tempScaleMult = distScaling;
        // Temp method of setting starTypeChances values, replace later
        starTypeChances.Add(Star.StarType.RedDwarf, 40);
        starTypeChances.Add(Star.StarType.SunLike, 30);
        starTypeChances.Add(Star.StarType.HotBlue, 10);
        starTypeChances.Add(Star.StarType.WhiteDwarf, 10);
        starTypeChances.Add(Star.StarType.RedGiant, 5);
        starTypeChances.Add(Star.StarType.BlackHole, 5);
        // Temp method of setting starScaleRanges values, replace later
        starScaleRanges.Add(Star.StarType.RedDwarf, new float[] { 1, 1.5f });
        starScaleRanges.Add(Star.StarType.SunLike, new float[] { 1.5f, 2.5f });
        starScaleRanges.Add(Star.StarType.HotBlue, new float[] { 3, 4 });
        starScaleRanges.Add(Star.StarType.WhiteDwarf, new float[] { 0.5f, 1 });
        starScaleRanges.Add(Star.StarType.RedGiant, new float[] { 6, 7.5f });
        starScaleRanges.Add(Star.StarType.BlackHole, new float[] { 2.5f, 5 });
        starScaleRanges.Add(Star.StarType.None, new float[] { 1, 1 }); // these shouldnt exist  
    }
    public void GenerateNewGalaxy() {
        StopAllCoroutines();
        BroadcastMessage("OnDestroyStars");
        starsInGalaxy = new List<Star>();
        if (randomizeSeed) {
            seed = Random.Range(0, 1000000);
        }
        StartCoroutine(GenerateGalaxy());
    }
    IEnumerator GenerateGalaxy() {
        // This is an IEnumerator so that it can happen over time, since its satisfying to watch
        // Declaring variables for galaxy creation
        Star star;
        float newStarScale;
        Vector3 newStarPos;
        Star.StarType newStarType;
        // Set the seed
        Random.InitState(seed);
        // Create the first star as a starting point for the rest to build from
        newStarType = GetRandomStarType();
        newStarScale = GetRandomStarScale(newStarType);
        GenerateStar(Vector3.zero, newStarType, newStarScale);
        newStarType = GetRandomStarType();
        newStarScale = GetRandomStarScale(newStarType);
        while (starsInGalaxy.Count < numberOfStars) {
            // Get the position of a random star
            star = starsInGalaxy[Random.Range(0, starsInGalaxy.Count)];
            if (!(star.connections.Count >= maxConnectionsPerStar)) {
                // Get a position between the min and max distance from star
                newStarPos = GetPointInRange(star, newStarScale);
                if (IsValidStarPosition(newStarPos)) {
                    // If newStarPos is a valid position to place the current star in, then make a star there
                    Star newStar = GenerateStar(newStarPos, newStarType, newStarScale);
                    newStarType = GetRandomStarType();
                    newStarScale = GetRandomStarScale(newStarType);
                    if (forceConnectAllStars && IsConnectionValid(star, newStar)) {
                        // forceConnectAllStars being on results in the galaxy being a minimum spanning tree, with additional random connections between stars
                        ConnectStars(star, newStar);
                    }
                }
                yield return null;
            }
        }
        StartCoroutine(ConnectStarsInGalaxy());
    }
    IEnumerator ConnectStarsInGalaxy() {
        SortStarsInGalaxy(); // Sort starsInGalaxy by the stars scales
        Star star; // The current star
        Star star1; // The star that we will attempt to connect the current star to
        for (int i = 0; i < starsInGalaxy.Count; i++) {
            star = starsInGalaxy[i];
            List<Collider> colliders = Physics.OverlapSphere(star.transform.position, maxDist + star.transform.localScale.x + distScaling * star.transform.localScale.x).ToList<Collider>();
            for (int j = -2; j < colliders.Count; j++) {
                if (colliders.Count == 0) {
                    break;
                }
                if (j >= 0) {
                    star1 = colliders[j].GetComponent<Star>();
                }
                else if (j == -2) {
                    Collider closestCollider = GetClosestCollider(colliders, star.transform.position);
                    star1 = closestCollider.GetComponent<Star>();
                    colliders.Remove(closestCollider);
                }
                else {
                    Collider furthestCollider = GetFurthestCollider(colliders, star.transform.position);
                    star1 = furthestCollider.GetComponent<Star>();
                    colliders.Remove(furthestCollider);
                }
                if (star1 != null) {
                    if (star1.connections.Count < maxConnectionsPerStar && star.connections.Count < maxConnectionsPerStar && star != star1 && !star1.connections.ContainsKey(star) && IsConnectionValid(star, star1)) {
                        ConnectStars(star, star1);
                        if (star.connections.Count >= maxConnectionsPerStar) {
                            break;
                        }
                        yield return null;
                    }
                }
            }
        }
        BroadcastMessage("OnGalaxyCreated");
    }
    Vector3 GetPointInRange(Star startStar, float newStarScale) {
        float scaleMult;
        // If the new star is larger than the current star, use its scale as a multiplier instead
        if (startStar.transform.localScale.x > newStarScale) {
            scaleMult = startStar.transform.localScale.x;
        }
        else {
            scaleMult = newStarScale;
        }
        float distance = Random.Range(minDist, maxDist) + (startStar.transform.localScale.x + newStarScale) / 2 + distScaling * scaleMult; // x y and z scale of stars are equal, so just use x for simplicity
        if (flatGalaxy) {
            Vector2 flatPos = Random.insideUnitCircle.normalized * distance;
            return startStar.transform.position + new Vector3(flatPos.x, 0, flatPos.y);
        }
        return startStar.transform.position + Random.onUnitSphere * distance;
    }
    bool IsValidStarPosition(Vector3 position) {
        bool valid = true;
        Collider[] colliders = Physics.OverlapSphere(position, minDist);
        foreach (Collider col in colliders) {
            if (col.CompareTag("Star")) {
                valid = false;
                break;
            }
        }
        return valid;
    }
    bool IsConnectionValid(Star star1, Star star2) {
        float scaleMult;
        // If the new star is larger than the current star, use its scale as a multiplier instead
        if (star1.transform.localScale.x > star2.transform.localScale.x) {
            scaleMult = star1.transform.localScale.x;
        }
        else {
            scaleMult = star2.transform.localScale.x;
        }
        float distance = Random.Range(minDist, maxDist) + (star1.transform.localScale.x + star2.transform.localScale.x) / 2 + distScaling * scaleMult;
        if (Physics.Raycast(star1.transform.position, star2.transform.position - star1.transform.position, distance)) {
            return false;
        }
        if (star1.connections.ContainsKey(star2)) {
            return false;
        }
        if (star2.connections.ContainsKey(star1)) {
            return false;
        }
        return true;
    }
    Star GenerateStar(Vector3 position, Star.StarType type, float scale) {
        GameObject starObject = Instantiate(starPrefab, transform);
        starObject.transform.position = position;
        Star star = starObject.GetComponent<Star>();
        star.InitialiseStar(type, scale);
        connectionsInGalaxy.Add(star, new List<Star>());
        starsInGalaxy.Add(star);
        star.gameObject.name = TextHandler.ReadLine(Mathf.FloorToInt(Random.Range(0,TextHandler.GetLineCount())));
        return star;
    }
    Star.StarType GetRandomStarType() {
        // Generate a random star type based on the probabilities listed in starTypeChances
        int randomValue = Mathf.RoundToInt(Random.Range(0, 100));
        foreach (var starTypeChance in starTypeChances) {
            if (starTypeChance.Value >= randomValue) {
                return starTypeChance.Key;
            }
            randomValue -= starTypeChance.Value;
        }
        return Star.StarType.None;
    }
    float GetRandomStarScale(Star.StarType starType) {
        float[] scaleRange = starScaleRanges[starType];
        return Random.Range(scaleRange[0], scaleRange[1]);
    }
    void SortStarsInGalaxy() {
        for (int i = 0; i < starsInGalaxy.Count - 1; i++) {
            for (int j = i + 1; j > 0; j--) {
                if (starsInGalaxy[j - 1].transform.localScale.x > starsInGalaxy[j].transform.localScale.x) {
                    Star temp = starsInGalaxy[j - 1];
                    starsInGalaxy[j - 1] = starsInGalaxy[j];
                    starsInGalaxy[j] = temp;
                }
            }
        }
    }
    Collider GetClosestCollider(List<Collider> colliders, Vector3 position) {
        if (colliders.Count == 0) {
            return null;
        }
        Collider closestCollider = colliders[0];
        foreach (Collider col in colliders) {
            if (Vector3.Distance(col.transform.position, position) < Vector3.Distance(closestCollider.transform.position, position)) {
                closestCollider = col;
            }
        }
        return closestCollider;
    }
    Collider GetFurthestCollider(List<Collider> colliders, Vector3 position) {
        if (colliders.Count == 0) {
            return null;
        }
        Collider furthestCollider = colliders[0];
        foreach (Collider col in colliders) {
            if (Vector3.Distance(col.transform.position, position) > Vector3.Distance(furthestCollider.transform.position, position)) {
                furthestCollider = col;
            }
        }
        return furthestCollider;
    }
    void ConnectStars(Star star1, Star star2) {
        const float inverseGravityBias = 3; // The closer to 0 this is, the more effect the stars scales have on the weight of their connections
        float starScale1 = star1.transform.localScale.x;
        float starScale2 = star2.transform.localScale.x;
        float distance = Vector3.Distance(star1.transform.position, star2.transform.position);
        float starDistance1 = (starScale1 + inverseGravityBias) / (starScale2 + inverseGravityBias) * distance;
        float starDistance2 = (starScale2 + inverseGravityBias) / (starScale1 + inverseGravityBias) * distance;
        star1.AddConnection(star2, starDistance1, true);
        star2.AddConnection(star1, starDistance2);
        if (!connectionsInGalaxy[star1].Contains(star2) && !connectionsInGalaxy[star2].Contains(star1)) {
            connectionsInGalaxy[star1].Add(star2);
        }
    }
    public void UnhighlightConnections() {
        BroadcastMessage("Unhighlight");
    }
}