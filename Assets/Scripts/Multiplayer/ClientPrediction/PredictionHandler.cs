using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Unity.VisualScripting;
using UnityEngine.Windows;

public class PredictionHandler : NetworkBehaviour
{
    //Shared
    private float timer;
    private int currentTick;
    private float minTimeBetweenTicks;
    [SerializeField] private float SERVER_TICK_RATE = 45f;
    [SerializeField] private bool ROTATION_ONLY = false;
    [SerializeField] private bool LOCAL_SPACE = false;
    private const int BUFFER_SIZE = 1024;

    //Client specific
    private StatePayload[] clientStateBuffer;
    private InputPayload[] inputBuffer;
    private StatePayload latestServerState;
    private StatePayload lastProcessedState;
    private Vector3 inputLocation;
    private Vector3 inputScale;
    private Quaternion inputRotation;

    //Server Specific
    private StatePayload[] serverStateBuffer;
    private Queue<InputPayload> inputQueue;

    [Server]
    public void OnClientInput(InputPayload inputPayload)
    {
        inputQueue.Enqueue(inputPayload);
    }

    [Client]
    public void OnServerMovementState(StatePayload serverState) 
    {
        latestServerState = serverState;
    }

    [Client]
    public void ProcessTranslation(Vector3 inLoc)
    {
        if (!ROTATION_ONLY) 
            inputLocation = inLoc;
        else
            Debug.LogError("[ ERROR: PredictionHandler.cs - Attempting to process a translation on a rotation locked PredictionHandler ]");
    }

    [Client]
    public void ProcessScaling(Vector3 inScale)
    {
        inputScale = inScale;
    }

    [Client]
    public void ProcessRotation(Quaternion inRot)
    {
        inputRotation = inRot;
    }

    /// <summary>
    /// Equivalent of delta time for the prediction behavior
    /// </summary>
    /// <returns></returns>
    public float GetMinTimeBetweenTicks()
    {
        return minTimeBetweenTicks;
    }

    void Start()
    {
        inputLocation = LOCAL_SPACE ? transform.localPosition : transform.position;
        inputRotation = LOCAL_SPACE ? transform.localRotation : transform.rotation;
        inputScale = transform.localScale;

        if (transform.root.name == "LocalGamePlayer") transform.name = "GamePlayerCharacter";

        minTimeBetweenTicks = 1f / SERVER_TICK_RATE;

        if (isClient)
        {
            clientStateBuffer = new StatePayload[BUFFER_SIZE];
            inputBuffer = new InputPayload[BUFFER_SIZE];
        }
        if (isServer)
        {
            serverStateBuffer = new StatePayload[BUFFER_SIZE];
            inputQueue = new Queue<InputPayload>();
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        while (timer >= minTimeBetweenTicks)
        {
            timer -= minTimeBetweenTicks;
            if (isClient && isOwned) ClientHandleTick();
            if (isServer) ServerHandleTick();
            currentTick++;
        }
    }

    [Client]
    void ClientHandleTick()
    {
        if (!isServer && !latestServerState.Equals(default(StatePayload)) &&
            (lastProcessedState.Equals(default(StatePayload)) ||
            !latestServerState.Equals(lastProcessedState)))
        {
            HandleServerReconciliation();
        }

        int bufferIndex = currentTick % BUFFER_SIZE;

        // Add payload to inputBuffer
        InputPayload inputPayload = new InputPayload();
        inputPayload.tick = currentTick;
        if (!ROTATION_ONLY) inputPayload.inputVector = inputLocation;
        inputPayload.inputScale = inputScale;
        inputPayload.inputRot = inputRotation;
        inputBuffer[bufferIndex] = inputPayload;

        clientStateBuffer[bufferIndex] = ProcessMovement(inputPayload);

        //Send input to Server
        Vector3 inputPos = LOCAL_SPACE ? transform.localPosition : transform.position;
        Quaternion inputRot = LOCAL_SPACE ? transform.localRotation : transform.rotation;
        Vector3 inputScale = transform.localScale;

        if (!isServer) SendToServer(inputPayload);
        else ReplicateToClientsDirectly(inputPos, inputScale, inputRot);
    }

    [ClientRpc]
    void ReplicateToClientsDirectly(Vector3 pos, Vector3 scale, Quaternion rot)
    {
        if (!ROTATION_ONLY)
        {
            if (LOCAL_SPACE) transform.localPosition = pos;
            else transform.position = pos;
        }

        if (LOCAL_SPACE) transform.localRotation = rot;
        else transform.rotation = rot;

        transform.localScale = scale;
    }

    [Server]
    void ServerHandleTick()
    {
        int bufferIndex = -1;
        while (inputQueue.Count > 0)
        {
            InputPayload inputPayload = inputQueue.Dequeue();

            bufferIndex = inputPayload.tick % BUFFER_SIZE;

            StatePayload statePayload = ProcessMovement(inputPayload);
            serverStateBuffer[bufferIndex] = statePayload;
        }

        if (bufferIndex != -1)
        {
            SendToClients(serverStateBuffer[bufferIndex]);
        }
    }

    [Client]
    void HandleServerReconciliation()
    {
        lastProcessedState = latestServerState;

        int serverStateBufferIndex = latestServerState.tick % BUFFER_SIZE;
        float positionError = Vector3.Distance(latestServerState.position, clientStateBuffer[serverStateBufferIndex].position);

        if (positionError > 0.001f)
        {

            Debug.Log("RECONCILING");

            //Rewind and Replay
            if (!ROTATION_ONLY)
            {
                if (LOCAL_SPACE) transform.localPosition = latestServerState.position;
                else transform.position = latestServerState.position;
            }
            
            if (LOCAL_SPACE) transform.localRotation = latestServerState.rotation;
            else transform.rotation = latestServerState.rotation;

            transform.localScale = latestServerState.scale;

            //Update buffer at index of latest server state
            clientStateBuffer[serverStateBufferIndex] = latestServerState;

            //Resimulate the rest of the ticks up to the current client tick
            int tickToProcess = (latestServerState.tick) + 1;

            while (tickToProcess < currentTick)
            {
                int bufferIndex = tickToProcess % BUFFER_SIZE;

                //Process new movement with reconciled state
                StatePayload statePayload = ProcessMovement(inputBuffer[bufferIndex]);

                //Update buffer with recalculated state
                clientStateBuffer[bufferIndex] = statePayload;

                tickToProcess++;
            }
        }

    }

    StatePayload ProcessMovement(InputPayload input)
    {
        if (!ROTATION_ONLY)
        {
            if (LOCAL_SPACE)
            {
                transform.localPosition = input.inputVector;
            }
            else
            {
                transform.position += input.inputVector * minTimeBetweenTicks;
            }
        }

        if (LOCAL_SPACE) transform.localRotation = input.inputRot;
        else transform.rotation = input.inputRot;

        transform.localScale = input.inputScale;

        return !LOCAL_SPACE ? 
        new StatePayload()
        {
            tick = input.tick,
            position = transform.position,
            rotation = transform.rotation,
            scale = transform.localScale,
        }
        :
        new StatePayload()
        {
            tick = input.tick,
            position = transform.localPosition,
            rotation = transform.localRotation,
            scale = transform.localScale
        };
    }

    [Command]
    void SendToServer(InputPayload input)
    {
        OnClientInput(input);
    }

    [ClientRpc]
    void SendToClients(StatePayload payload)
    {
        OnServerMovementState(payload);
    }
}
