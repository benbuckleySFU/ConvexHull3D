using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Diagnostics;
using System.IO;
using UnityEngine.SocialPlatforms;
using System.Text.RegularExpressions;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Windows;
using System.Runtime.CompilerServices;

[ExecuteAlways]
[ExecuteInEditMode]
public class GenerateWalks : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject baseObject;
    public GameObject partitionParent;
    public GameObject hullVertexObject;
    public Material baseMaterial;
    public Material hullVertexMaterial;
    public GameObject parent;
    private float scale = 1;
    private Vector3 position = Vector3.zero;
    private Color stepColour = new Color(126 * 1.0f / 255, 126 * 1.0f / 255, 0 * 1.0f / 255, 1.0f);

    // Variables needed to show the number of steps in each walk
    public GameObject titleParent;
    private GameObject numStepsTextParent;
    public GameObject numStepsTextCanvas;
    public TextMeshProUGUI numStepsText;

    // The variables needed for doing random generation:
    private System.Random random = new System.Random();

    // Keep track of current partition. Dimension of vector should be set to whatever the size of the largest vector is.
    private List<Vector3> currentPointList = new List<Vector3>();
    private int vectorDimension = 0;
    private int maxIntSize = 0;
    private int maxNumParts = 0;

    // Keep track of current set of objects
    private List<GameObject> currentPointObjects = new List<GameObject>();

    // Variables for importing partitions
    public TMP_InputField importInputField;
    public TMP_InputField maxNumPointsInputField;
    public TMP_InputField maxDistanceInputField;

    // Variable for displaying text for current displayed partition
    public TMP_Text currentPartitionText;

    // Variables for dealing with the hull mesh
    private Mesh hullMesh = null;

    void Start()
    {
        //baseObject.transform.localScale = new Vector3(scale, scale, scale);
        // Create stepset
        UnityEngine.Debug.Log("Scale: " + scale);

        for (int i = 0; i < 10; i++)
        {
            string testPartition = generateRandomPointSetString(10, 3);
            UnityEngine.Debug.Log(testPartition);
        }

        //GameObject newObject = Instantiate(hullVertexObject, Vector3.zero, Quaternion.identity, partitionParent.transform);

    }

    // Update is called once per frame
    void Update()
    {
    }

    /*
     * Mesh intMesh(int i, int j)
    {
        // Generate the mesh to visualize the integer in currentPartition[i][j]
        //float length = (float)Math.Sqrt(1.0 * currentPartition[i][j]);
        //float theta = (float) Math.PI * 2 * 1/vectorDimension;
        //float length = (float)Math.Sqrt(currentPartition[i][j] / (2 * Math.Cos(theta/2) * Math.Sin(theta/2)));
        float length = (float)currentPartition[i][j];

        int yValue = -i;
        if (length < 1 || vectorDimension < 1)
        {
            return new Mesh();
        }
        else if (vectorDimension == 1 || vectorDimension == 2)
        {
            
            // Need to generate a rectangle
            // Eight vertices
            int[] triangles;

            // If vectorDimension == 2 and j == 1, then we want to flip on the x axis.
            
            // It turns out, we need to DUPLICATE the vertices to make sure this renders properly.

            if (j == 1)
            {
                length = -length;
            }

            List<int> trianglesList = new List<int>();
            Vector3[] vertices = new Vector3[24];
            // Top face
            vertices[0] = new Vector3(0, yValue, 0);
            vertices[1] = new Vector3(0, yValue, 1);
            vertices[2] = new Vector3(length, yValue, 1);
            vertices[3] = new Vector3(length, yValue, 0);
            trianglesList.AddRange(new List<int> { 0, 1, 2, 0, 2, 3 });
            // Bottom face
            vertices[4] = new Vector3(0, yValue - 1, 0);
            vertices[5] = new Vector3(0, yValue - 1, 1);
            vertices[6] = new Vector3(length, yValue - 1, 1);
            vertices[7] = new Vector3(length, yValue - 1, 0);
            trianglesList.AddRange(new List<int> { 4, 6, 5, 4, 7, 6 });
            // Z+ Face
            vertices[8] = new Vector3(0, yValue, 1);
            vertices[9] = new Vector3(length, yValue, 1);
            vertices[10] = new Vector3(0, yValue - 1, 1);
            vertices[11] = new Vector3(length, yValue - 1, 1);
            trianglesList.AddRange(new List<int> { 8, 10, 9, 9, 10, 11 });
            // Z- Face
            vertices[12] = new Vector3(0, yValue, 0);
            vertices[13] = new Vector3(length, yValue, 0);
            vertices[14] = new Vector3(0, yValue - 1, 0);
            vertices[15] = new Vector3(length, yValue - 1, 0);
            trianglesList.AddRange(new List<int> { 12, 13, 14, 13, 15, 14 });
            // X+ Face
            vertices[16] = new Vector3(length, yValue, 0);
            vertices[17] = new Vector3(length, yValue, 1);
            vertices[18] = new Vector3(length, yValue - 1, 0);
            vertices[19] = new Vector3(length, yValue - 1, 1);
            trianglesList.AddRange(new List<int> { 16, 17, 18, 17, 19, 18 });
            // X- Face
            vertices[20] = new Vector3(0, yValue, 0);
            vertices[21] = new Vector3(0, yValue, 1);
            vertices[22] = new Vector3(0, yValue - 1, 0);
            vertices[23] = new Vector3(0, yValue - 1, 1);
            trianglesList.AddRange(new List<int> { 20, 22, 21, 21, 22, 23 });

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            if (j == 0)
            {
                mesh.triangles = trianglesList.ToArray();
            }
            else
            {
                trianglesList.Reverse();
                mesh.triangles = trianglesList.ToArray();
            }
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            return mesh;
        }
        else // vectorDimension > 2
        {
            float theta = (float)Math.PI * 2 * 1 / vectorDimension;
            length = (float)Math.Sqrt(currentPartition[i][j] / (2 * Math.Cos(theta / 2) * Math.Sin(theta / 2)));
            // Need to generate wedge
            // Think in terms of an ANGLE. We want to divide 0 to 2pi by vectorDimension
            //float startAngle = (float) Math.PI * 2 * j / vectorDimension;
            float startAngle = j * theta;
            float endAngle = (j + 1) * theta;
            //float endAngle = (float)Math.PI * 2 * (j+1) / vectorDimension;
            float startX = length * (float)Math.Cos(startAngle);
            float startZ = length * (float)Math.Sin(startAngle);
            float endX = length * (float)Math.Cos(endAngle);
            float endZ = length * (float)Math.Sin(endAngle);


            // It turns out I need to use duplicate vertices to make sure this renders properly.
            List<int> trianglesList = new List<int>();
            Vector3[] vertices = new Vector3[18];
            // Top Face
            vertices[0] = new Vector3(0, yValue, 0);
            vertices[1] = new Vector3(endX, yValue, endZ);
            vertices[2] = new Vector3(startX, yValue, startZ);
            trianglesList.AddRange(new List<int> { 0,1,2 });
            // Bottom Face
            vertices[3] = new Vector3(0, yValue - 1, 0);
            vertices[4] = new Vector3(endX, yValue - 1, endZ);
            vertices[5] = new Vector3(startX, yValue - 1, startZ);
            trianglesList.AddRange(new List<int> { 3,5,4 });
            // Outer Face
            vertices[6] = new Vector3(startX, yValue, startZ);
            vertices[7] = new Vector3(endX, yValue, endZ);
            vertices[8] = new Vector3(startX, yValue - 1, startZ);
            vertices[9] = new Vector3(endX, yValue - 1, endZ);
            trianglesList.AddRange(new List<int> { 6,7,8,7,9,8 });
            // Clockwise Face
            vertices[10] = new Vector3(0, yValue, 0);
            vertices[11] = new Vector3(startX, yValue, startZ);
            vertices[12] = new Vector3(0, yValue - 1, 0);
            vertices[13] = new Vector3(startX, yValue - 1, startZ);
            trianglesList.AddRange(new List<int> { 10,11,12,11,13,12 });
            // Counterclockwise Face
            vertices[14] = new Vector3(0, yValue, 0);
            vertices[15] = new Vector3(endX, yValue, endZ);
            vertices[16] = new Vector3(0, yValue - 1, 0);
            vertices[17] = new Vector3(endX, yValue - 1, endZ);
            trianglesList.AddRange(new List<int> { 14,16,15,15,16,17 });

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = trianglesList.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            return mesh;
        }
    }
    */

    void OnDisable()
    {
        GetComponent<MeshFilter>().sharedMesh = null;
    }

    void OnApplicationQuit()
    {
        GetComponent<MeshFilter>().sharedMesh = null;
    }

    public void importPointList()
    {
        currentPointList = new List<Vector3>();
        vectorDimension = 3;

        string partitionText = Regex.Replace(importInputField.text, @"\s", ""); // Get rid of spaces
        // Format: A series of sets of parentheses with numbers in them separated by commas
        string vecPattern = @"(\([^\(\)]*\))";

        Regex rxGetVectors = new Regex(vecPattern, RegexOptions.Compiled);
        MatchCollection matches = rxGetVectors.Matches(partitionText);

        foreach (Match match in matches)
        {
            string intListString = Regex.Replace(match.Groups[1].Value, @"[\(\)]", "");
            UnityEngine.Debug.Log("intListString: " + intListString);
            // Now try to make an integer list.
            string[] intStrings = intListString.Split(",");

            List<int> newIntList = new List<int>();
            for (int i = 0; i < intStrings.Length; i++)
            {
                int currentInt = 0;
                //UnityEngine.Debug.Log(i + ": " + intStrings[i]);
                bool isInt = int.TryParse(intStrings[i], out currentInt);
                //UnityEngine.Debug.Log(i + ": " + currentInt);
                newIntList.Add(currentInt);
            }
            // If newIntList has less than 3 elements, fill it with zeroes. Otherwise, take the first three elements.
            while (newIntList.Count < 3)
            {
                newIntList.Add(0);
            }
            currentPointList.Add(new Vector3((float)newIntList[0], (float)newIntList[1], (float)newIntList[2]));
        }

        // Remove duplicates!
        currentPointList = new List<Vector3>(new HashSet<Vector3>(currentPointList));

        // Print list:
        for (int i = 0; i < currentPointList.Count; i++)
        {
            UnityEngine.Debug.Log("<x,y,z> = (" + String.Join(",", currentPointList[i]) + ")");
        }

        // Now must visualize the convex hull:
        displayPoints();

        // Try to display the mesh?
        //GetComponent<MeshFilter>().mesh = hullMesh;
    }

    private void displayPoints()
    {
        for (int i = 0; i < currentPointObjects.Count; i++)
        {
            //currentPartitionObjects[i];
            DestroyImmediate(currentPointObjects[i]);
        }
        GameObject displayObject = Instantiate(baseObject);

        for (int i = 0; i < currentPointList.Count; i++)
        {
            GameObject newObject = Instantiate(displayObject, currentPointList[i], Quaternion.identity, partitionParent.transform);
            currentPointObjects.Add(newObject);
        }
        DestroyImmediate(displayObject);
        List<Vector3> convexHull = findConvexHull();
        GameObject newDisplayObject = Instantiate(hullVertexObject);

        for (int i = 0; i < convexHull.Count; i++)
        {
            UnityEngine.Debug.Log(@"convexHull[" + i + "] = " + convexHull[i]);
            GameObject newObject = Instantiate(newDisplayObject, convexHull[i], Quaternion.identity, partitionParent.transform);
            currentPointObjects.Add(newObject);

        }
        DestroyImmediate(newDisplayObject);

        GetComponent<MeshFilter>().mesh = hullMesh;

    }

    private List<Vector3> findConvexHull()
    {
        hullMesh = new Mesh();
        // First, need to check if all the points are on the same line.
        // Point sets to test: (0,0,0)
        // (0,0,0), (0,2,1)
        // (1,1,1), (0,0,0), (2,2,2), (5,5,5), (-1,-1,-1)
        // (1,1,1), (0,0,0), (2,2,2), (5,5,5), (-1,-1,-1), (0,0,1)
        if (currentPointList.Count < 3)
        {
            UnityEngine.Debug.Log("Point or simple linear hull");
            return currentPointList; // Either a point or a line
        }
        bool couldBeLinear = true;
        // Keep track of the two points farthest from each other -- these form the hull for a straight line
        Vector3 point1 = currentPointList[0];
        Vector3 point2 = currentPointList[1];
        Vector3 point3 = new Vector3();
        // If we do find a non-linear plane, it will be useful for the next step.
        List<Vector3> currentPlane = new List<Vector3>();
        // Use currentIndex to keep track of which points we've checked so far
        int currentIndex = 0;
        for (int i = 2; i < currentPointList.Count && couldBeLinear; i++)
        {
            point3 = currentPointList[i];
            if (!threePointsLinear(point1, point2, point3))
            {
                couldBeLinear = false;
                currentIndex = i + 1;
            }
            else
            {
                // Check if point3 is at the outside of the hull so far
                float dist12 = (point2 - point1).sqrMagnitude;
                float dist23 = (point3 - point2).sqrMagnitude;
                float dist13 = (point3 - point1).sqrMagnitude;
                // If dist23 and dist13 < dist 12, then point1 and point2 are still the best hull.
                if (dist12 < dist13)
                {
                    // point3 should be part of the new hull. But which point should be reassigned?
                    if (dist23 < dist13)
                    {
                        // point2 is in the middle and should be replaced with point3
                        point2 = point3;
                    }
                    else // dist23 > dist13
                    {
                        // point1 is in the middle and should be replaced with point3
                        point1 = point3;
                    }
                }
                else // dist12 > dist13
                {
                    // point3 might still be part of the new hull if point1 is in the middle
                    if (dist12 < dist23)
                    {
                        // point1 is in the middle and should be replaced with point3
                        point1 = point3;

                    }
                    else // dist12 > dist23
                    {
                        // point3 is in the middle. Do nothing
                    }
                }
            }
        }
        if (couldBeLinear)
        {
            UnityEngine.Debug.Log("Linear hull: (" + String.Join(", ", point1) + "), (" + String.Join(", ", point2) + ")");
            return new List<Vector3> { point1, point2 };
        }
        // If we've gotten this far, we know the hull is not linear. Then points 1, 2 and 3 must form a plane (that isn't a line).
        currentPlane = new List<Vector3> { point1, point2, point3 };

        // To do the incremental hull algorithm, we need to think in terms of facets.
        // For 2D hulls, a facet is a line with two points.
        // In 2D, we don't have to worry too much about clockwise or counterclockwiseness.
        // Assume hull is 2D until we find a facet that isn't on the plane.
        List<Vector3> facet1 = new List<Vector3> { point1, point2 };
        List<Vector3> facet2 = new List<Vector3> { point2, point3 };
        List<Vector3> facet3 = new List<Vector3> { point3, point1 };

        List<List<Vector3>> facetList = new List<List<Vector3>> { facet1, facet2, facet3 };

        // The incremental hull algorithm involves determining whether new points are "above" or "below" the facets.
        // In 2D, this means determining whether the point is on the same side of the facet as the centre of the polytope.
        // We can calculate a "centre" now. It will always be inside the polytope, since we will expand it further to find the hull.
        Vector3 centre = (point1 + point2 + point3) * 1.0f / 3.0f;

        // Next, need to check if all the points are on the same plane
        // Note that we conveniently have the list of points currentPlane to help us.
        // Also, we know that every point with an index lower than currentIndex is either in currentPlane, or is definitely not part of the convex hull.

        // Test: // (1,1,1), (0,0,0), (2,2,2), (5,5,5), (-1,-1,-1), (0,0,1)
        // (0,0,0), (0, 1, 1), (0, 0, 1), (0, 1, 0), (0, -1, 0)
        // (0,0,0), (0, 1, 1), (0, 0, 1), (1,0,1)
        bool isPlanar = true;

        Vector3 point4 = new Vector3();
        // We'll eventually need a set of vertices in the hull, and we'll make it into a list
        HashSet<Vector3> hullSet = new HashSet<Vector3>();
        List<Vector3> hullList = hullSet.ToList();

        // We'll also need to consider the mesh:
        List<Vector3> hullVertices = new List<Vector3>();
        List<int> hullTriangles = new List<int>();

        for (int i = currentIndex; i < currentPointList.Count && isPlanar; i++)
        {
            UnityEngine.Debug.Log("isPlanar: " + isPlanar);
            point4 = currentPointList[i];
            if (!fourPointsPlanar(point1, point2, point3, point4))
            {
                isPlanar = false;
                // currentIndex = i + 1;
            }
            else
            {
                // Check whether point4 is "above" any of the current facets.
                // If it is, remove those facets and add new facets that include point4
                List<Vector3> visiblePoints = new List<Vector3>();
                List<int> toRemove = new List<int>();
                for (int j = 0; j < facetList.Count; j++)
                {
                    // Check whether point4 is above facet j. Do this with a cross product.
                    Vector3 AB = facetList[j][1] - facetList[j][0];
                    Vector3 AC = centre - facetList[j][0];
                    Vector3 AP = point4 - facetList[j][0];

                    // If the centre and point4 are on opposite sides of the line, then:
                    // The cross product AB x AC will point in the opposite direction of AB X AP
                    // In which case, (AB x AC) dot (AB x AP) will be negative.
                    float dotProduct = Vector3.Dot(Vector3.Cross(AB, AC), Vector3.Cross(AB, AP));
                    if (dotProduct < 0)
                    {
                        // The points in this facet are visible to point4.
                        visiblePoints.Add(facetList[j][0]);
                        visiblePoints.Add(facetList[j][1]);
                        // Mark that this facet must be removed from facetList
                        toRemove.Add(j);
                    }
                }
                // Remove unnecesary facets from facetList.
                for (int j = toRemove.Count - 1; j >= 0 ; j--)
                {
                    facetList.RemoveAt(toRemove[j]);
                }
                // Now I have a list of points visible to point4.
                // In principle, if a point appears in the list twice, then it is part of a CORNER that is visible to point4, and should not be used.
                visiblePoints = RemoveAllDuplicates(visiblePoints);
                // Then we will be left with two points.
                UnityEngine.Debug.Log("visiblePoints.Count = " + visiblePoints.Count);
                facetList.Add(new List<Vector3> { visiblePoints[0], point4 });
                facetList.Add(new List<Vector3> { point4, visiblePoints[1] });
            }
        }
        if (isPlanar)
        {
            UnityEngine.Debug.Log("Planar hull!");
            UnityEngine.Debug.Log("facetList.Count: " + facetList.Count);
            // Flatten out facetList
            int triangleCount = 0;
            for (int i = 0; i < facetList.Count; i++)
            {
                for (int j = 0; j < facetList[i].Count; j++)
                {
                    hullSet.Add(facetList[i][j]);
                    hullVertices.Add(facetList[i][j]);
                    hullTriangles.Add(triangleCount++);
                }
                hullVertices.Add(centre);
                hullTriangles.Add(triangleCount++);
            }
            hullList = hullSet.ToList();
            for (int i = 0; i < hullList.Count; i++)
            {
                UnityEngine.Debug.Log("hullList[" + i + "] = (" + String.Join(", ", hullList[i]) + ")");
            }
            int numVertices2D = hullVertices.Count;
            for (int i = 0; i < numVertices2D; i++)
            {
                //hullVertices.Add(hullVertices[i] + new Vector3(0.001f, 0.001f, 0.001f));
                hullTriangles.Add(numVertices2D - 1 - i);
            }
            UnityEngine.Debug.Log("hullVertices.Count: " + hullVertices.Count);
            UnityEngine.Debug.Log("hullTriangles.Count: " + hullTriangles.Count);

            hullMesh = new Mesh();
            hullMesh.vertices = hullVertices.ToArray();
            hullMesh.triangles = hullTriangles.ToArray();
            

            return hullList;
        }

        // If we've gotten this far, we know that there is a 3D convex hull. We also have an initial simplex:
        List<Vector3> currentHull = new List<Vector3> { point1, point2, point3, point4 };

        // We start with a polytope with 4 faces. Picture a stretched-out tetrahedron.
        facet1 = new List<Vector3> { point1, point2, point3 };
        facet2 = new List<Vector3> { point1, point2, point4 };
        facet3 = new List<Vector3> { point1, point3, point4 };
        List<Vector3> facet4 = new List<Vector3> { point2, point3, point4 };
        facetList = new List<List<Vector3>> { facet1, facet2, facet3, facet4 };

        // Again, we need to find the centre of these four points:
        centre = (point1 + point2 + point3 + point4) * 1.0f / 4.0f;
        Vector3 point5 = new Vector3();
        // Any points we've looked at are already in the current hull, or have been rejected.
        for (int i = currentIndex; i < currentPointList.Count; i++)
        {
            point5 = currentPointList[i];
            UnityEngine.Debug.Log("3D hull testing current point: " + point5);
            // At this point, we know all the points are in a 3D space (and not 4D space), so we don't have to check that.
            // For each point, we just have to go through the current list of facets and see if point5 is "above" any of them.
            // We DO have to keep track of visible edges. We can represent each edge as a set, since we don't need to care about direction.
            List<List<Vector3>> visibleEdges = new List<List<Vector3>>();
            // We also have to keep track of which facets to remove.
            List<int> toRemove = new List<int>();
            for (int j = 0; j < facetList.Count; j++)
            {
                //
                Vector3 AB = facetList[j][1] - facetList[j][0];
                Vector3 AC = facetList[j][2] - facetList[j][0];
                // Find the normal vector to the facet. It doesn't matter whether it's inward or outward.
                Vector3 N = Vector3.Cross(AB, AC);
                // Find vectors from any point on the plane (say, facetList[j][0]) to point5 and to centre.
                Vector3 PA = point5 - facetList[j][0];
                Vector3 DA = centre - facetList[j][0];
                // If they are on opposite sides of the plane, then their projections onto the normal vector will point in opposite directions.
                // Then the product of the following two dot products will be less than zero:
                float PA_n = Vector3.Dot(PA, N);
                float DA_n = Vector3.Dot(DA, N);
                if (PA_n * DA_n < 0.0f)
                {
                    UnityEngine.Debug.Log("Facet (" + String.Join(", ", facetList[j]) + ") is below the current point!");
                    // Then the current facet is below point5.
                    // All 3 edges on the current facet are visible to point5:
                    visibleEdges.Add(new List<Vector3> { facetList[j][0], facetList[j][1] });
                    visibleEdges.Add(new List<Vector3> { facetList[j][0], facetList[j][2] });
                    visibleEdges.Add(new List<Vector3> { facetList[j][1], facetList[j][2] });
                    // Note that we must remove this facet.
                    toRemove.Add(j);
                }

            }
            // Remove unnecesary facets from facetList.
            for (int j = toRemove.Count - 1; j >= 0; j--)
            {
                facetList.RemoveAt(toRemove[j]);
            }
            // Remove any edge that is visible to point5 in more than one facet, i.e. appears in visibleEdges twice.
            visibleEdges = RemoveAllDuplicateEdges(visibleEdges);
            // Create new facets composed from the remaining edges and point5
            for (int j = 0; j < visibleEdges.Count; j++)
            {
                List<Vector3> newFacet = visibleEdges[j];
                newFacet.Add(point5);
                facetList.Add(newFacet);
            }

        }
        UnityEngine.Debug.Log("3D HULL!!!!");
        UnityEngine.Debug.Log("facetList.Count: " + facetList.Count);
        // Flatten out facetList
        //hullSet = new HashSet<Vector3>();
        for (int i = 0; i < facetList.Count; i++)
        {
            for (int j = 0; j < facetList[i].Count; j++)
            {
                hullSet.Add(facetList[i][j]);
                hullVertices.Add(facetList[i][j]);
                hullTriangles.Add(i * facetList[i].Count + j);
            }
        }
        hullList = hullSet.ToList();
        for (int i = 0; i < hullList.Count; i++)
        {
            UnityEngine.Debug.Log("hullList[" + i + "] = (" + String.Join(", ", hullList[i]) + ")");
        }

        // Add extra faces so visible from both sides:
        int numVertices = hullVertices.Count;
        for (int i = 0; i < numVertices; i++)
        {
            //hullVertices.Add(hullVertices[i] + new Vector3(0.001f, 0.001f, 0.001f));
            hullTriangles.Add(numVertices - 1 - i);
        }


        hullMesh = new Mesh();
        hullMesh.vertices = hullVertices.ToArray();
        hullMesh.triangles = hullTriangles.ToArray();
        
        return hullList;

    }

    static List<T> RemoveAllDuplicates<T>(List<T> inputList)
    {
        // Find all items that occur more than once
        var duplicates = inputList.GroupBy(item => item).Where(group => group.Count() > 1).SelectMany(group => group);

        // Remove all instances of duplicates from the list
        inputList.RemoveAll(item => duplicates.Contains(item));

        return inputList;
    }

    static List<List<Vector3>> RemoveAllDuplicateEdges(List<List<Vector3>> inputList)
    {
        /* 
         * List<HashSet<Vector3>> newList = new List<HashSet<Vector3>>();
        for (int i = 0; i < inputList.Count; i++)
        {
            HashSet<Vector3> currentItem = inputList[i];
            // Assume we haven't found a duplicate of the current item until we've found one.
            bool duplicateFound = false;
            for (int j = i + 1; j < inputList.Count && !duplicateFound; j++)
            {

            }
        }
        */
        List<List<Vector3>> inputListCopy = inputList;
        List<List<Vector3>> toReturn = new List<List<Vector3>>();
        while (inputListCopy.Count > 0)
        {
            // The idea is this: If an item is a duplicate, remove ALL the duplicates.
            // If an item is not a duplicate, add it to toReturn
            List<Vector3> currentItem = inputListCopy[0];
            List<Vector3> edge1 = inputListCopy[0].ToList();
            // Assume we haven't found a duplicate of the current item until we've found one.
            bool duplicateFound = false;
            List<int> toRemove = new List<int>();
            for (int i = 1; i < inputListCopy.Count; i++)
            {
                // Check if inputListCopy[i] is equal to inputListCopy[0].
                // In this program, we know they'll both be sets of 2 vectors.
                List<Vector3> edge2 = inputListCopy[i];
                if (  ((edge1[0] - edge2[0]).sqrMagnitude < 0.000001 && (edge1[1] - edge2[1]).sqrMagnitude < 0.000001) ||
                      ((edge1[0] - edge2[1]).sqrMagnitude < 0.000001 && (edge1[1] - edge2[0]).sqrMagnitude < 0.000001) )
                {
                    // The edges are equal
                    duplicateFound = true;
                    toRemove.Add(i);
                }
            }
            // Remove duplicates from the list if necessary
            for (int i = toRemove.Count - 1; i >= 0; i--)
            {
                inputListCopy.RemoveAt(toRemove[i]);
            }
            // If no duplicate has been found, the edge can be added to the returned list.
            if (!duplicateFound)
            {
                toReturn.Add(currentItem);
            }
            // Either way, we remove the first element of inputListCopy
            inputListCopy.RemoveAt(0);
        }
        return toReturn;
    }

    private bool threePointsLinear(Vector3 point1, Vector3 point2, Vector3 point3)
    {
        Vector3 crossProduct = Vector3.Cross(point2 - point1, point3 - point1);
        // For our purposes, since the coordinates are integers, epsilon doesn't have to be too small
        // In this context, we only care about sqrMagnitude
        if (crossProduct.sqrMagnitude < 0.000001)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool fourPointsPlanar(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4)
    {
        Vector3 crossProduct = Vector3.Cross(point2 - point1, point3 - point1);
        float dotProduct = Vector3.Dot(point4 - point1, crossProduct);
        UnityEngine.Debug.Log("dotProduct = " + dotProduct);
        if (Math.Abs(dotProduct) < 0.000001)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /*
     * private void displayPartition()
    {
        UnityEngine.Debug.Log("Size of CurrentPartitionObjects: " + currentPartitionObjects.Count);
        for (int i = 0; i < currentPartitionObjects.Count; i++)
        {
            //currentPartitionObjects[i];
            DestroyImmediate(currentPartitionObjects[i]);
        }
        currentPartitionObjects = new List<GameObject>();

        Material material = Instantiate(baseMaterial);
        GameObject displayObject = Instantiate(baseObject);
        Renderer renderer = displayObject.GetComponent<Renderer>();
  
        Color nextColor = Color.HSVToRGB(0.0f, 1.0f, 1.0f);
        material.color = nextColor;
        renderer.material = material;
        position = Vector3.zero;

        // Calculate the total vector for the partition
        int[] currentPartitionTotal = new int[vectorDimension];
        int currentPartitionSize = 0;

        for (int i = 0; i < currentPointList.Count; i++)
        {
            for (int j = 0; j < currentPointList[i].Count; j++)
            {
                nextColor = Color.HSVToRGB(1.0f * j / vectorDimension, 1.0f, 1.0f);
                
                // Update currentPartitionTotal
                currentPartitionSize += currentPointList[i][j];
                currentPartitionTotal[j] += currentPointList[i][j];

                GameObject newObject = Instantiate(displayObject, Vector3.zero, Quaternion.identity, partitionParent.transform);
                currentPartitionObjects.Add(newObject);
                //newObject.GetComponent<MeshRenderer>().sharedMaterial = baseMaterial;
                newObject.GetComponent<MeshFilter>().sharedMesh = intMesh(i,j);
                newObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(nextColor.r, nextColor.g, nextColor.b));


                position.x = 0;
                position = position + new Vector3(0, 0, 1);
                
            }
            position.x = 0;
            position.z = 0;
            position = position + new Vector3(0, -1, 0);
            //nextColor = Color.HSVToRGB(0.0f, 1.0f, 1.0f);

        }
        string toDisplay = "Current partition total: (" + string.Join(", ", currentPartitionTotal) + ") (Size: " + currentPartitionSize + ")";
        currentPartitionText.SetText(toDisplay);

    }
    */


    public string generateRandomPointSetString(int numPoints, int maxDistance)
    {
        HashSet<Vector3> newPointSet = new HashSet<Vector3>();
        while (newPointSet.Count < numPoints)
        {
            newPointSet.Add(new Vector3(random.Next(2 * (maxDistance + 1)) - maxDistance, random.Next(2 * (maxDistance + 1)) - maxDistance, random.Next(2 * (maxDistance + 1)) - maxDistance));
        }
        List<Vector3> newPointList = newPointSet.ToList();

        // Create the string
        string toReturn = "";
        for (int i = 0; i < newPointList.Count; i++)
        {
            toReturn += "(";
            for (int j = 0; j < 3; j++)
            {
                toReturn += (int)newPointList[i][j];
                if (j < 2)
                {
                    toReturn += ",";
                }
            }

            toReturn += ")";
        }

        return toReturn;
    }

    public void generateRandomPartition()
    {
        string numPointsInput = maxNumPointsInputField.text;
        int newNumPoints = 0;
        bool isInt = int.TryParse(numPointsInput, out newNumPoints);

        string maxDistanceInput = maxDistanceInputField.text;
        int newMaxDistance = 0;
        isInt = int.TryParse(maxDistanceInput, out newMaxDistance);

        string toOutput = generateRandomPointSetString(newNumPoints, newMaxDistance);
        importInputField.SetTextWithoutNotify(toOutput);
    }

    /*
    public void generateRandomPartitionOld()
    {
        string dimInput = dimensionInputField.text;
        string maxIntSizeInput = maxIntSizeInputField.text;
        string maxNumPartsInput = maxNumPartsInputField.text;

        int newVectorDimension = 5;
        int newMaxIntSize = 100;
        int newMaxNumparts = 100;

        bool isInt = int.TryParse(dimInput, out newVectorDimension);
        isInt = int.TryParse(maxIntSizeInput, out newMaxIntSize);
        isInt = int.TryParse(maxNumPartsInput, out newMaxNumparts);

        string toOutput = "";

        // Now that we have the dimension, generate a bunch of random partitions.
        // NOTE: THIS IS ACTUALLY NOT CORRECT FOR UNIFORM RANDOM GENERATION! For now, just a prototype.
        for (int i = 0; i < newMaxNumparts; i++)
        {
            // Generate a new part
            List<int> newPart = new List<int>();
            for (int j = 0; j < newVectorDimension; j++)
            {
                newPart.Add(random.Next(newMaxIntSize + 1));
            }
            // Add it to the string
            toOutput = toOutput + "(" + String.Join(",", newPart) + ")";

        }
        UnityEngine.Debug.Log(toOutput);
        importInputField.SetTextWithoutNotify(toOutput);
    }
    */

    private int compareParts(List<int> part1, List<int> part2)
    {
        // First, try the sum. If sum of part1 > sum of part2, then part1 precedes part2.
        if (part1.Sum() > part2.Sum())
        {
            return -1;
        }
        else if (part1.Sum() < part2.Sum())
        {
            return 1;
        }
        else // Sums are the same. Must look at lexicographic ordering.
        {
            for (int i = 0; i < vectorDimension; i++)
            {
                if (part1[i] > part2[i])
                {
                    return -1;
                }
                else if (part1[i] < part2[i])
                {
                    return 1;
                }
            }
            return 0;
        }
    }

}