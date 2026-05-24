using System.Collections.Generic;
public class PathScript {
    // This script is basically a copy of "DijkstraSimplified.cs", I made it simply to understand the script better.
    private static List<Star> starsInGalaxy;
    public static List<Star> FindPath(Star startStar, Star endStar) {
        
        List<StarPath> priorityList = new List<StarPath>();

        foreach (Star star in starsInGalaxy) {
            StarPath newPath = new StarPath(star, null);
            if (star == startStar) {
                newPath.previousStar = newPath;
                newPath.distanceToStart = 0.0f;
            }
            priorityList.Add(newPath);
        }
        // make sure that startStar is the first in the priority list
        priorityList = SortPriorityList(priorityList);

        while (priorityList.Count > 0) {
            StarPath currentStarNode = priorityList[0];
            foreach (StarPath nextStar in GetStarPathsFromConnections(currentStarNode.star.connections, priorityList)) {
                // calculate the distance to the starting star
                float distance = nextStar.star.connections[currentStarNode.star] + currentStarNode.distanceToStart;

                // check if this new paths distance is shorter than the current path to this star from the start node
                if (distance < nextStar.distanceToStart) {
                    // if it is, update nextStars distance accordingly
                    nextStar.distanceToStart = distance;
                    nextStar.previousStar = currentStarNode;
                }
            }
            // currentStarNode has been fully checked, so can be removed (equivalent to marking it as "checked" on paper)
            priorityList.Remove(currentStarNode);

            // reorder the priority list for the next loop
            if (priorityList.Count > 0) {
                priorityList = SortPriorityList(priorityList);
                // if the highest priority star is the end star, either the shortest path has been found, or there is no path
                if (priorityList[0].star == endStar && priorityList[0].distanceToStart != float.MaxValue) {
                    // if the end stars distance to the start isnt infinite, then the shortest path has been found
                    List<Star> pathToEnd = new List<Star>();
                    StarPath backtrackStar = priorityList[0]; //This is currently the end goal star.
                    pathToEnd.Add(backtrackStar.star);

                    while (backtrackStar.star != startStar) {
                        // backtrack through the StarPaths using their previousStar to eventually reach the start star
                        backtrackStar = backtrackStar.previousStar;
                        pathToEnd.Add(backtrackStar.star);
                    }
                    // the path is currently backwards, so reverse it
                    pathToEnd.Reverse();
                    // return the finished path
                    return pathToEnd;
                }
            }
        }
        // if this is reached, then no path to the end exists, since every possible connection has been checked
        return new List<Star>();
    }
    private static List<StarPath> GetStarPathsFromConnections(Dictionary<Star, float> starConnections, List<StarPath> priorityList) {
        // given a stars "connections" dictionary, return a list of StarPaths of the connections within it
        List<StarPath> result = new List<StarPath>();
        foreach (KeyValuePair<Star, float> connection in starConnections) {
            Star star = connection.Key;
            for (int i = 0; i < priorityList.Count; i++) {
                if (priorityList[i].star == star) {
                    result.Add(priorityList[i]);
                    break;
                }
            }
        }
        return result;
    }
    private static List<StarPath> SortPriorityList(List<StarPath> pathList) {
        // bubble sort pathList based on their distanceToStart attribute
        // NOTE: this could be where A* can be introduced, since this determines the next star to be checked
        for (int i = 0; i < pathList.Count; i++) {
            for (int j = 0; j < pathList.Count - 1; j++) {
                StarPath first = pathList[j];
                StarPath second = pathList[j + 1];
                if (first.distanceToStart > second.distanceToStart) {
                    pathList[j] = second;
                    pathList[j + 1] = first;
                }
            }
        }
        return pathList;
    }
    public static void SetStarList(List<Star> stars) {
        starsInGalaxy = new List<Star>(stars);
    }
    public class StarPath {
        public Star star;
        public StarPath previousStar;
        public float distanceToStart;
        public StarPath(Star currentStar, StarPath lastStar) {
            star = currentStar;
            previousStar = lastStar;
            distanceToStart = float.MaxValue;
        }
    }
}