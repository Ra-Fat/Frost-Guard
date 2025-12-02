using UnityEngine;

public class Path : MonoBehaviour
{
    public Transform[] points;
    public Path[] nextPaths;  // assign next possible paths in inspector

    void Awake()
    {
        points = new Transform[transform.childCount];
        for (int i = 0; i < points.Length; i++)
            points[i] = transform.GetChild(i);
    }
}
