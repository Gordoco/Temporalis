using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PredictionHandler : NetworkBehaviour
{
    //Shared
    private float timer;
    private int currentTick;
    private float minTimeBetweenTicks;
    private const float SERVER_TICK_RATE = 45f;
    private const int BUFFER_SIZE = 1024;

    //Client specific
    private StatePayload[] clientStateBuffer;
    private InputPayload[] inputBuffer;
    private StatePayload latestServerState;
    private StatePayload lastProcessedState;
    private float horizontalInput;
    private float verticalInput;

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

    private void Awake()
    {
        if (transform.root.name != "LocalGamePlayer") enabled = false;
    }

    void Start()
    {
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
        if (isClient)
        {
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");
        }

        timer += Time.deltaTime;
        while (timer >= minTimeBetweenTicks)
        {
            timer -= minTimeBetweenTicks;
            if (isClient) ClientHandleTick();
            if (isServer) ServerHandleTick();
            currentTick++;
        }

        Debug.Log(transform.position);
    }

    [Client]
    void ClientHandleTick()
    {
        if (!latestServerState.Equals(default(StatePayload)) &&
            (lastProcessedState.Equals(default(StatePayload)) ||
            !latestServerState.Equals(lastProcessedState)))
        {
            HandleServerReconciliation();
        }

        int bufferIndex = currentTick % BUFFER_SIZE;

        // Add payload to inputBuffer
        InputPayload inputPayload = new InputPayload();
        inputPayload.tick = currentTick;
        inputPayload.inputVector = new Vector3(horizontalInput, 0, verticalInput);
        inputBuffer[bufferIndex] = inputPayload;

        clientStateBuffer[bufferIndex] = ProcessMovement(inputPayload);

        //Send input to Server
        SendToServer(inputPayload);
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
            Debug.Log("[ TickHandler.cs - FIXING POSITION ]");

            //Rewind and Replay
            transform.position = latestServerState.position;

            //Update buffer at index of latest server state
            clientStateBuffer[serverStateBufferIndex] = latestServerState;

            //Resimulate the rest of the ticks up to the current client tick
            int tickToProcess = latestServerState.tick + 1;

            while (tickToProcess < currentTick)
            {
                //Process new movement with reconciled state
                StatePayload statePayload = ProcessMovement(inputBuffer[tickToProcess]);

                //Update buffer with recalculated state
                int bufferIndex = tickToProcess % BUFFER_SIZE;
                clientStateBuffer[bufferIndex] = statePayload;

                tickToProcess++;
            }
        }

    }

    StatePayload ProcessMovement(InputPayload input)
    {
        transform.position += input.inputVector * 5f * minTimeBetweenTicks;

        return new StatePayload()
        {
            tick = input.tick,
            position = transform.position,
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
