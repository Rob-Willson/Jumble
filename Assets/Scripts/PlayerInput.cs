﻿using System;
using UnityEngine;
using Mirror;

public class PlayerInput : NetworkBehaviour
{
    public static Action<Vector3> TileDragOngoing;
    public static Action TileDragEnd;

    private Camera mainCamera = null;
    private Camera MainCamera
    {
        get
        {
            if(mainCamera == null)
            {
                mainCamera = Camera.main;
            }
            return mainCamera;
        }
    }

    [SerializeField] private Tile currentlySelectedTile;
    private bool hasTile
    {
        get
        {
            return currentlySelectedTile == null ? false : true;
        }
    }

    private void Update()
    {
        if(isServerOnly)
        {
            return;
        }
        if(!hasAuthority)
        {
            return;
        }
        LocalClientUpdate();
    }

    [Client]
    private void LocalClientUpdate ()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            Debug.Log("Mouse0 down");

            if(hasTile)
            {
                Debug.LogError("Already had tile picked up but trying to mouse down. Why wasn't tile dropped?");
                return;
            }

            if(Physics.Raycast(MainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100))
            {
                Tile hitTile = hit.collider.gameObject.GetComponent<Tile>();
                if(hitTile == null)
                {
                    return;
                }
                CmdPickUpTile(hitTile.netIdentity);
            }

        }

        if(Input.GetKeyUp(KeyCode.Mouse0))
        {
            if(hasTile)
            {
                RequestDropTile();
            }
        }

        if(Input.GetKey(KeyCode.Mouse0))
        {
            if(!hasTile)
            {
                return;
            }

            Vector3 mousePositionInWorld = GetMouseWorldCoordinate();
            TileDragOngoing?.Invoke(mousePositionInWorld);
        }
    }

    [Command]
    private void CmdPickUpTile (NetworkIdentity tileNetworkIdentity)
    {
        // TODO: Validate
        // ...

        TargetDoPickUpTile(base.connectionToClient, tileNetworkIdentity);
    }

    [TargetRpc]
    private void TargetDoPickUpTile(NetworkConnection connectionToTargetClient, NetworkIdentity tileNetworkIdentity)
    {
        Tile tile = tileNetworkIdentity.GetComponent<Tile>();
        if(tile == null)
        {
            Debug.LogError("FAIL: Tile component wasn't found on passed NetworkIdentity.");
            return;
        }

        currentlySelectedTile = tile;
    }

    [Client]
    private void RequestDropTile ()
    {
        Vector3 dropPosition = GetMouseWorldCoordinate();
        Vector3Int dropPositionSnapped = new Vector3Int(Mathf.RoundToInt(dropPosition.x), 0, Mathf.RoundToInt(dropPosition.z));

        CmdDropTile(currentlySelectedTile.netIdentity, dropPositionSnapped);
    }

    [Command]
    private void CmdDropTile (NetworkIdentity tileNetworkIdentity, Vector3Int requestedDropPosition)
    {
        // TODO: Validate
        // ...

        tileNetworkIdentity.transform.localPosition = requestedDropPosition;
        TargetDoDropTile(base.connectionToClient);
    }

    [TargetRpc]
    private void TargetDoDropTile(NetworkConnection connectionToTargetClient)
    {
        currentlySelectedTile = null;
        TileDragEnd?.Invoke();
    }

    /// <summary>
    /// Raycast down from camera through the mouse position, returning the coordinate on the floor plane.
    /// </summary>
    [Client]
    private Vector3 GetMouseWorldCoordinate ()
    {
        return GetWorldCoordinateFromScreenPosition(Input.mousePosition);
    }

    /// <summary>
    /// Raycast down from camera through a given position in the screen, returning the coordinate on the floor plane.
    /// </summary>
    [Client]
    private Vector3 GetWorldCoordinateFromScreenPosition (Vector2 screenPosition)
    {
        Ray ray = MainCamera.ScreenPointToRay(screenPosition);
        float distanceToDrawPlane = (0f - ray.origin.y) / ray.direction.y;
        Vector3 screenPositionInWorld = ray.GetPoint(distanceToDrawPlane);
        return screenPositionInWorld;
    }

}