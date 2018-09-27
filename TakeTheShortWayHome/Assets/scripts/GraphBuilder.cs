using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds the graph
/// </summary>
public class GraphBuilder : MonoBehaviour
{
    static Graph<Waypoint> graph;

    static Waypoint start;
    static Waypoint end;

    private List<Waypoint> waypoints;

    /// <summary>
    /// Awake is called before Start
    /// </summary>
    void Awake()
    {
        graph = new Graph<Waypoint>();
        waypoints = new List<Waypoint>();

        // first get all waypoint gameobjects
        GameObject startGO = GameObject.FindGameObjectWithTag("Start");
        GameObject[] wayPointGOs = GameObject.FindGameObjectsWithTag("Waypoint");
        GameObject endGO = GameObject.FindGameObjectWithTag("End");

        // add nodes (all waypoints, including start and end) to graph

        // Add start waypoint to graph
        start = startGO.GetComponent<Waypoint>();
        graph.AddNode(start);
        waypoints.Add(start);

        // Add intermediate waypoints to graph
        for (int i = 0; i < wayPointGOs.Length;i++)
        {
            Waypoint waypoint = wayPointGOs[i].GetComponent<Waypoint>();
            graph.AddNode(waypoint);
            waypoints.Add(waypoint);
        }

        // Add end waypoint to graph
        end = endGO.GetComponent<Waypoint>();
        graph.AddNode(end);
        waypoints.Add(end);

        // add edges to graph
        foreach (Waypoint waypoint1 in waypoints)
        {
            Vector2 position1 = waypoint1.Position;

            foreach (Waypoint waypoint2 in waypoints)
            {
                Vector2 position2 = waypoint2.Position;

                float diffX = Mathf.Abs(position1.x - position2.x);
                float diffY = Mathf.Abs(position1.y - position2.y);

                float distance = Mathf.Sqrt(Mathf.Pow(diffX, 2) + Mathf.Pow(diffY, 2));

                if (diffX <= 3.0f && diffY <= 3.5f)
                {
                    graph.AddEdge(waypoint1, waypoint2, distance);
                }
            }
        }
    }

    /// <summary>
    /// Gets the graph
    /// </summary>
    /// <value>graph</value>
    public static Graph<Waypoint> Graph
    {
        get { return graph; }
    }

    /// <summary>
    /// Gets the start point of the graph
    /// </summary>
    /// <value>start point</value>
    public static Waypoint Start
    {
        get { return start; }
    }

    /// <summary>
    /// Gets the end point of the graph
    /// </summary>
    /// <value>end point</value>
    public static Waypoint End
    {
        get { return end; }
    }
}
