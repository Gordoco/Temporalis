using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Assertions;

[RequireComponent(typeof(CharacterController))]
public class NetworkedMovement : NetworkBehaviour
{
    private CharacterController CC;

    [SyncVar (hook = "ClientPositionLerp")] private Vector3 ServerPosition;

    private void Awake()
    {
        //Required Component
        CC = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) 
        {
            Move(transform.position + (transform.forward * 5));
        }
    }

    /// <summary>
    /// Network agnostic movement smoothly over network to specified position
    /// </summary>
    /// <param name="Position"></param>
    public void Move(Vector3 Position)
    {
        //if (!isOwned) return;
        if (isClient && !isServer) ClientMoveHandler(Position);
        if (isServer) ServerMoveHandler(Position);
    }

    /// <summary>
    /// Client-side handling of a movement request
    /// </summary>
    private void ClientMoveHandler(Vector3 Position)
    {
        Assert.IsTrue(isClient);
        Debug.Log("[Client Movement] - Moved to " + Position);
        CC.Move(Position);
        if (!isServer) ServerMoveCommand(Position);
    }

    private void ClientPositionLerp(Vector3 oldValue, Vector3 newValue)
    {
        if (!isClient || isServer) return;
        if (transform.position != ServerPosition)
        {
            //CC.Move(ServerPosition);
            StartCoroutine(ClientPositionLerpCoroutine(oldValue, newValue));
        }
    }

    private IEnumerator ClientPositionLerpCoroutine(Vector3 oldPosition, Vector3 Position)
    {
        const float INTERVAL = 0.1f;
        double percent = 0;
        while (Position == ServerPosition && transform.position != Position)
        {
            yield return new WaitForSeconds(INTERVAL);
            transform.position = new Vector3(Mathf.Lerp(oldPosition.x, Position.x, (float)percent),
                Mathf.Lerp(oldPosition.y, Position.y, (float)percent),
                Mathf.Lerp(oldPosition.z, Position.z, (float)percent));
            percent += INTERVAL*2;
        }
    }

    [Command]
    private void ServerMoveCommand(Vector3 Position)
    {
        Assert.IsTrue(isServer);
        Debug.Log("[Command Movement] - Moved to " + Position);
        CC.Move(Position);
        ServerPosition = Position;
    }

    /// <summary>
    /// Server-side handling of a movement request
    /// </summary>
    private void ServerMoveHandler(Vector3 Position)
    {
        Assert.IsTrue(isServer);
        Debug.Log("[Server Movement] - Moved to " + Position);
        CC.Move(Position);
        ServerPosition = Position;
        //ServerMoveRPC(Position);
    }

    [ClientRpc]
    private void ServerMoveRPC(Vector3 Position)
    {
        if (isOwned) return;
        Debug.Log("[Replicated Movement] - Moved to " + Position);
        CC.Move(Position);
    }
}
