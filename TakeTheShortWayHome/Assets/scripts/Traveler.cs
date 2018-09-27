using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A traveler
/// </summary>
public class Traveler : MonoBehaviour
{
    // events fired by class
    PathFoundEvent pathFoundEvent = new PathFoundEvent();
    PathTraversalCompleteEvent pathTraversalCompleteEvent = new PathTraversalCompleteEvent();

    // targets
    private Waypoint start;
    private Waypoint end;
    private LinkedList<Waypoint> waypoints;
    private Waypoint targetWaypoint;

    // movement control
    Rigidbody2D rb2d;
    const float BaseImpulseForceMagnitude = 2.0f;

    // circling behaviour
    private bool circling;
    private float rotateSpeed = 5f;
    private float radius = 0.01f;
    private Vector2 centre;
    private float angle;

    /// <summary>
    /// Use this for initialization
    /// </summary>
    void Start()
	{
        EventManager.AddPathFoundInvoker(this);
        EventManager.AddPathTraversalCompleteInvoker(this);

        EventManager.AddPathFoundListener(SetHUDText);


        // save reference for efficiency
        rb2d = GetComponent<Rigidbody2D>();

        waypoints = doDijkstraSearch();
        SetTarget(start);
    }

    /// <summary>
    /// This is not the correct way to do this, however
    /// I cannot get the HUD to respond to the PathFoundEvent
    /// no matter what I try...
    /// </summary>
    /// <param name="distance">Distance.</param>
    void SetHUDText(float distance)
    {
        GameObject hudGO = GameObject.FindGameObjectWithTag("HUD");
        HUD hud = hudGO.GetComponent<HUD>();
        hud.SetPathLength(distance);
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
	{
        if (circling)
        {
            transform.position = getCirclingPosition();
        }
    }
	
    /// <summary>
    /// Adds the given listener for the PathFoundEvent
    /// </summary>
    /// <param name="listener">listener</param>
    public void AddPathFoundListener(UnityAction<float> listener)
    {
        pathFoundEvent.AddListener(listener);
    }

    /// <summary>
    /// Adds the given listener for the PathTraversalCompleteEvent
    /// </summary>
    /// <param name="listener">listener</param>
    public void AddPathTraversalCompleteListener(UnityAction listener)
    {
        pathTraversalCompleteEvent.AddListener(listener);
    }

    /// <summary>
    /// Called when another object is within a trigger collider
    /// attached to this object
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerStay2D(Collider2D other)
    {
        if (waypoints.Count > 0)
        {
            // only respond if the collision is with the target pickup
            if (other.gameObject == targetWaypoint.gameObject)
            {
                // remove visited waypoint from list of waypoints and game
                waypoints.RemoveFirst();

                // go to next target if there is one
                if (waypoints.Count > 0)
                {
                    SetTarget(waypoints.First.Value);
                }
                else
                {
                    startCircling(); // 'cos why not?

                    // hide all edges
                    Camera.main.GetComponent<EdgeRenderer>().StopDrawingEdges();

                    // destroy all waypoints (using explosion)
                    pathTraversalCompleteEvent.Invoke();
                }
            }
        }
        else
        {
            SetTarget(null);
        }
    }

    /// <summary>
    /// Sets the target waypoint to the provided waypoint
    /// </summary>
    /// <param name="waypoint">waypoint.</param>
    void SetTarget(Waypoint waypoint)
    {
        targetWaypoint = waypoint;

        if (targetWaypoint != null)
            GoToTargetWaypoint();
    }

    /// <summary>
    /// Starts the traveler moving toward the target waypoint
    /// </summary>
    void GoToTargetWaypoint()
    {
        if (targetWaypoint != null)
        {
            // calculate direction to target waypoint and start moving toward it
            Vector2 direction = new Vector2(
                targetWaypoint.gameObject.transform.position.x - transform.position.x,
                targetWaypoint.gameObject.transform.position.y - transform.position.y);
            direction.Normalize();
            rb2d.velocity = Vector2.zero;
            Vector2 force = direction * BaseImpulseForceMagnitude;
            rb2d.AddForce(force,
                ForceMode2D.Impulse);
        }
    }

    private void startCircling()
    {
        centre = transform.position;
        circling = true;
        radius = 0.01f;
    }

    private Vector3 getCirclingPosition()
    {
        angle += rotateSpeed * Time.deltaTime;

        var offset = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * radius;
        return centre + offset;
    }

    void FixedUpdate()
    {
        if (radius < 0.5f)
            radius += 0.001f;
    }


    private LinkedList<Waypoint> doDijkstraSearch()
    {
        Graph<Waypoint> graph = GraphBuilder.Graph;

        //Create a search list (a sorted linked list) of search nodes (I provided a SearchNode class, which you should instantiate with Waypoint. I also provided a SortedLinkedList class)
        SortedLinkedList<SearchNode<Waypoint>> searchList = new SortedLinkedList<SearchNode<Waypoint>>();

        //Create a dictionary of search nodes keyed by the corresponding graph node.This dictionary gives us a very fast way to determine if the search node corresponding to a graph node is still in the search list
        Dictionary<GraphNode<Waypoint>, SearchNode<Waypoint>> searchNodeMap = new Dictionary<GraphNode<Waypoint>, SearchNode<Waypoint>>();

        //Save references to the start and end graph nodes in variables
        start = GraphBuilder.Start;
        end = GraphBuilder.End;

        GraphNode<Waypoint> startGraphNode = graph.Find(start);
        GraphNode<Waypoint> endGraphNode = graph.Find(end);

        //For each graph node in the graph
        foreach (GraphNode<Waypoint> graphNode in graph.Nodes)
        {
            //Create a search node for the graph node (the constructor I provided in the SearchNode class initializes distance to the max float value and previous to null)
            SearchNode<Waypoint> searchNode = new SearchNode<Waypoint>(graphNode);

            //If the graph node is the start node
            if (graphNode == startGraphNode)
                // Set the distance for the search node to 0
                searchNode.Distance = 0;

            //Add the search node to the search list
            searchList.Add(searchNode);
            //Add the search node to the dictionary keyed by the graph node
            searchNodeMap.Add(graphNode, searchNode);
        }

        //While the search list isn't empty
        while (searchList.Count > 0)
        {
            //Save a reference to the current search node(the first search node in the search list) in a variable
            SearchNode<Waypoint> currentSearchNode = searchList.First.Value;
            //Remove the first search node from the search list
            searchList.RemoveFirst();
            //Save a reference to the current graph node for the current search node in a variable
            GraphNode<Waypoint> currentGraphNode = currentSearchNode.GraphNode;
            //Remove the search node from the dictionary(because it's no longer in the search list)
            searchNodeMap.Remove(currentGraphNode);

            //If the current graph node is the end node
            if (currentGraphNode == endGraphNode)
            {
                //Display the distance for the current search node as the path length in the scene(Hint: I used the HUD and the event system to do this)
                pathFoundEvent.Invoke(currentSearchNode.Distance);

                //Return a linked list of the waypoints from the start node to the end node(Hint: The lecture code for the Searching a Graph lecture builds a linked list for a path in the ConvertPathToString method)
                LinkedList<Waypoint> pathLinkedList = new LinkedList<Waypoint>();
                while(currentSearchNode != null)
                {
                    GraphNode<Waypoint> graphNode = currentSearchNode.GraphNode;
                    Waypoint waypoint = graphNode.Value;
                    pathLinkedList.AddFirst(waypoint);
                    currentSearchNode = currentSearchNode.Previous;
                }
                return pathLinkedList;
            }

            //For each of the current graph node's neighbors
            foreach(GraphNode<Waypoint> neighbour in currentGraphNode.Neighbors)
            {
                //If the neighbor is still in the search list(use the dictionary to check this)
                SearchNode<Waypoint> neighbourSearchNode = null;
                if (searchNodeMap.TryGetValue(neighbour, out neighbourSearchNode))
                {
                    //Save the distance for the current graph node + the weight of the edge from the current graph node to the current neighbor in a variable
                    float distanceToStart = currentSearchNode.Distance + currentGraphNode.GetEdgeWeight(neighbour);
                    //If the distance you just calculated is less than the current distance for the neighbor search node(Hint: You can retrieve the neighbor search node from the dictionary using the neighbor graph node)
                    if (distanceToStart < neighbourSearchNode.Distance)
                    {
                        //Set the distance for the neighbor search node to the new distance
                        neighbourSearchNode.Distance = distanceToStart;
                        //Set the previous node for the neighbor search node to the current search node
                        neighbourSearchNode.Previous = currentSearchNode;
                        //Tell the search list to Reposition the neighbor search node. We need to do this because the change to the distance for the neighbor search node could have moved it forward in the search list
                        searchList.Reposition(neighbourSearchNode);
                    }
                }
            }
        }

        return null;
    }
}
