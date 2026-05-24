using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GUIScript : MonoBehaviour {
    [SerializeField] GalaxyCreator galaxyCreator;
    [SerializeField] GameObject GenerationPanel;
    [SerializeField] GameObject PathfindingPanel;
    // Generation fields
    [SerializeField] TMP_InputField seedField;
    bool randomSeed = true;
    bool flatGalaxy = false;
    [SerializeField] TMP_InputField starCountField;
    [SerializeField] TMP_InputField maxConnectionsField;
    bool forceConnect = true;
    [SerializeField] TMP_InputField minDistanceField;
    [SerializeField] TMP_InputField maxDistanceField;
    [SerializeField] TMP_InputField distanceScalingField;
    // Pathfinding fields
    [SerializeField] TMP_InputField selectedStarField;
    [SerializeField] TMP_InputField selectedTypeField;
    [SerializeField] TMP_InputField selectedMassField;
    [SerializeField] TMP_InputField startStarField;
    [SerializeField] TMP_InputField endStarField;
    [SerializeField] GameObject pathTextParent;
    Star selectedStar;
    Star startStar;
    Star endStar;
    public void OnGenerateGalaxyButtonPressed() {
        galaxyCreator.flatGalaxy = flatGalaxy;
        galaxyCreator.numberOfStars = int.Parse(starCountField.text);
        galaxyCreator.maxConnectionsPerStar = int.Parse(maxConnectionsField.text);
        galaxyCreator.forceConnectAllStars = forceConnect;
        galaxyCreator.minDist = float.Parse(minDistanceField.text);
        galaxyCreator.maxDist = float.Parse(maxDistanceField.text);
        galaxyCreator.distScaling = float.Parse(distanceScalingField.text);
        galaxyCreator.randomizeSeed = randomSeed;
        galaxyCreator.seed = int.Parse(seedField.text);
        galaxyCreator.GenerateNewGalaxy();
    }
    public void OnRandomSeedToggled(bool newRandomSeed) {
        randomSeed = newRandomSeed;
    }
    public void OnFlatGalaxyToggled(bool newFlatGalaxy) {
        flatGalaxy = newFlatGalaxy;
    }
    public void OnForceConnectAllStarsToggled(bool newForceConnect) {
        forceConnect = newForceConnect;
    }
    public void OnPathfindButtonPressed() {
        // Pathfind
        PathScript.SetStarList(GameObject.Find("GalaxyCreator").GetComponent<GalaxyCreator>().starsInGalaxy);
        List<Star> starList = PathScript.FindPath(startStar, endStar);
        // Unhighlight the previously made path
        galaxyCreator.UnhighlightConnections();
        // Highlight the traversed connections
        for (int i = 0; i < starList.Count; i++) {
            ConnectionRendererScript[] connectionRendererScripts = starList[i].GetComponentsInChildren<ConnectionRendererScript>();
            foreach (ConnectionRendererScript connectionRendererScript in connectionRendererScripts) {
                if (i < starList.Count - 1) {
                    if (connectionRendererScript.GetStars()[1] == starList[i + 1]) {
                        connectionRendererScript.SetHighlighted();
                    }
                }
                if (i > 0) {
                    if (connectionRendererScript.GetStars()[1] == starList[i - 1]) {
                        connectionRendererScript.SetHighlighted();
                    }
                }
            }
        }
        // Display the path on the GUI
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(270, 42);
        foreach (Transform child in pathTextParent.transform) {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < starList.Count; i++) {
            rectTransform.sizeDelta += new Vector2(0, 42);
            GameObject newText = new GameObject(starList[i].name + "StarNameText");
            newText.AddComponent<TextMeshProUGUI>();
            newText.GetComponent<TextMeshProUGUI>().text = starList[i].name;
            newText.transform.SetParent(pathTextParent.transform);
        }
    }
    public void SelectStar(Star star) {
        if (star != null) {
            selectedStar = star;
            selectedStarField.text = star.name;
            selectedTypeField.text = star.GetStarType().ToString();
            selectedMassField.text = (Mathf.Round(star.transform.localScale.x * 100f) / 100).ToString();
        }
    }
    public void SetStartStar() {
        if (selectedStar != null) {
            startStar = selectedStar;
            startStarField.text = startStar.name;
        }
    }
    public void SetEndStar() {
        if (selectedStar != null) {
            endStar = selectedStar;
            endStarField.text = endStar.name;
        }
    }
    public void OnGenerationPanelButtonPressed() {
        PathfindingPanel.SetActive(false);
        GenerationPanel.SetActive(true);
    }
    public void OnPathfindingPanelButtonPressed() {
        PathfindingPanel.SetActive(true);
        GenerationPanel.SetActive(false);
    }
}