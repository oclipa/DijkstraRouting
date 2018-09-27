using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A waypoint
/// </summary>
public class Waypoint : MonoBehaviour
{
    [SerializeField]
    int id;

    [SerializeField]
    Explosion prefabExplosion;

    private bool visited;

    /// <summary>
    /// Use this for initialization
    /// </summary>
    void Start()
    {
        EventManager.AddPathTraversalCompleteListener(Explode);
    }

    /// <summary>
    /// Changes waypoint to green
    /// </summary>
    /// <param name="other">other collider</param>
    void OnTriggerEnter2D(Collider2D other)
    {
        GetComponent<SpriteRenderer>().color = Color.green;
        visited = true;
    }

    /// <summary>
    /// Gets the position of the waypoint
    /// </summary>
    /// <value>position</value>
    public Vector2 Position
    {
        get { return transform.position; }
    }

    /// <summary>
    /// Gets the unique id for the waypoint
    /// </summary>
    /// <value>unique id</value>
    public int Id
    {
        get { return id; }
    }

    void Explode()
    {
        if (visited && prefabExplosion != null)
        {
            Instantiate<Explosion>(prefabExplosion, transform.position, Quaternion.identity);
            Destroy(this.gameObject);
        }
    }
}
