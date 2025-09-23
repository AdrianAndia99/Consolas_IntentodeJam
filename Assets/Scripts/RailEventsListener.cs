using UnityEngine;
using System;

public class RailEventsListener : MonoBehaviour
{
    void OnEnable()
    {
        PlayerControl.OnReachedWaypoint += HandleReachedWaypoint;
        PlayerControl.OnPathCompleted += HandlePathCompleted;
    }

    void OnDisable()
    {
        PlayerControl.OnReachedWaypoint -= HandleReachedWaypoint;
        PlayerControl.OnPathCompleted -= HandlePathCompleted;
    }

    private void HandleReachedWaypoint(PlayerControl pc, int index)
    {
        Debug.Log($"Se alcanzó el waypoint {index} por {pc.name}.");
    }

    private void HandlePathCompleted(PlayerControl pc)
    {
        Debug.Log($"Se completó el recorrido de {pc.name}.");
    }
}