﻿using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using PhotonMMORPG.Common;
using PhotonMMORPG.Common.CustomEventArgs;
using UnityEngine;

public class PhotonServer : MonoBehaviour, IPhotonPeerListener
{
    private const string CONNECTION_STRING = "localhost:5055";
    private const string APP_NAME = "MyCoolServer";

    private static PhotonServer _instance;
    public static PhotonServer Instance
    {
        get { return _instance; }
    }

    public List<Player> Players = new List<Player>();

    private PhotonPeer PhotonPeer { get; set; }

    public string CharacterName { get; set; }

    public event EventHandler<LoginEventArgs> OnLoginResponse;
    public event EventHandler<ChatMessageEventArgs> OnReceiveChatMessage;

    void Awake()
    {
        if (Instance != null)
        {
            DestroyObject(gameObject);
            return;
        }


        DontDestroyOnLoad(gameObject);

        Application.runInBackground = true;

        _instance = this;
    }
    // Use this for initialization
    void Start()
    {

        PhotonPeer = new PhotonPeer(this, ConnectionProtocol.Udp);
        if (!PhotonPeer.RegisterType(typeof(Vector3Net), (byte)200, Vector3Net.Serialize, Vector3Net.Deserialize))
        {
            throw new Exception("not working...");
        }
        Connect();
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonPeer != null)
            PhotonPeer.Service();
    }

    void OnApplicationQuit()
    {
        WorldExitOperation();
        Disconnect();
    }

    private void Connect()
    {
        if (PhotonPeer != null)
            PhotonPeer.Connect(CONNECTION_STRING, APP_NAME);
    }

    private void Disconnect()
    {
        if (PhotonPeer != null)
            PhotonPeer.Disconnect();
    }

    public void DebugReturn(DebugLevel level, string message)
    {
        Debug.Log("DebugReturn level:" + level.ToString());
    }

    public void OnOperationResponse(OperationResponse operationResponse)
    {
        switch (operationResponse.OperationCode)
        {
            case (byte)OperationCode.Login:
                LoginHandler(operationResponse);
                break;
            case (byte)OperationCode.ListPlayers:
                ListPlayersHandler(operationResponse);
                break;
            default:
                Debug.Log("Unknown OperationResponse:" + operationResponse.OperationCode);
                break;
        }
    }

    public void OnEvent(EventData eventData)
    {
        switch (eventData.Code)
        {
            case (byte)EventCode.ChatMessage:
                ChatMessageHandler(eventData);
                break;
            case (byte)EventCode.Move:
                MoveHandler(eventData);
                break;
            case (byte)EventCode.WorldEnter:
                WorldEnterHandler(eventData);
                break;
            case (byte)EventCode.WorldExit:
                WorldExitHandler(eventData);
                break;
            case (byte)EventCode.UpdateAnimator:
                UpdateAnimatorHandle(eventData);
                break;
            default:
                Debug.Log("Unknown Event:" + eventData.Code);
                break;
        }
    }

    public void OnStatusChanged(StatusCode statusCode)
    {
        switch (statusCode)
        {
            case StatusCode.Connect:
                Debug.Log("Connected to server!");
                break;
            case StatusCode.Disconnect:
                Debug.Log("Disconnected from server!");
                break;
            case StatusCode.TimeoutDisconnect:
                Debug.Log("TimeoutDisconnected from server!");
                break;
            case StatusCode.DisconnectByServer:
                Debug.Log("DisconnectedByServer from server!");
                break;
            case StatusCode.DisconnectByServerUserLimit:
                Debug.Log("DisconnectedByLimit from server!");
                break;
            case StatusCode.DisconnectByServerLogic:
                Debug.Log("DisconnectedByLogic from server!");
                break;
            case StatusCode.EncryptionEstablished:
                break;
            case StatusCode.EncryptionFailedToEstablish:
                break;
            default:
                Debug.Log("Unknown status:" + statusCode.ToString());
                break;
        }
    }

    #region handlers for response

    private void LoginHandler(OperationResponse operationResponse)
    {
        if (operationResponse.ReturnCode != 0)
        {
            ErrorCode errorCode = (ErrorCode)operationResponse.ReturnCode;
            switch (errorCode)
            {
                case ErrorCode.NameExists:
                    if (OnLoginResponse != null)
                        OnLoginResponse(this, new LoginEventArgs(ErrorCode.NameExists));
                    break;
                default:
                    Debug.Log("Error Login returnCode:" + operationResponse.ReturnCode);
                    break;
            }

            return;
        }

        if (OnLoginResponse != null)
            OnLoginResponse(this, new LoginEventArgs(ErrorCode.Ok));
    }

    private void ChatMessageHandler(EventData eventData)
    {
        string message = (string)eventData.Parameters[(byte)ParameterCode.ChatMessage];

        if (OnReceiveChatMessage != null)
            OnReceiveChatMessage(this, new ChatMessageEventArgs(message));
    }

    private void MoveHandler(EventData eventData)
    {
        string characterName = (string)eventData.Parameters[(byte)ParameterCode.CharacterName];
        float posX = (float)eventData.Parameters[(byte)ParameterCode.PosX];
        float posY = (float)eventData.Parameters[(byte)ParameterCode.PosY];
        float posZ = (float)eventData.Parameters[(byte)ParameterCode.PosZ];

        float rotX = (float)eventData.Parameters[(byte)ParameterCode.RotX];
        float rotY = (float)eventData.Parameters[(byte)ParameterCode.RotY];
        float rotZ = (float)eventData.Parameters[(byte)ParameterCode.RotZ];


        var client = Players.FirstOrDefault(n => n.CharacterName.Equals(characterName));
        if (client == null)
        {
            Debug.Log("Move:not client");
            return;
        }


        client.NewPosition = new Vector3(posX, posY, posZ);
        client.NewRotation = Quaternion.Euler(rotX, rotY, rotZ);

        //Debug.Log("MoveHandler");
    }

    private void UpdateAnimatorHandle(EventData eventData)
    {
        string characterName = (string)eventData.Parameters[(byte)ParameterCode.CharacterName];
        float speed = (float)eventData.Parameters[(byte)ParameterCode.Speed];
        bool jump = (bool)eventData.Parameters[(byte)ParameterCode.Jump];
        bool die = (bool)eventData.Parameters[(byte)ParameterCode.Die];

        bool respawn = (bool)eventData.Parameters[(byte)ParameterCode.Respawn];
        bool attack = (bool)eventData.Parameters[(byte)ParameterCode.Attack];

        var client = Players.FirstOrDefault(n => n.CharacterName.Equals(characterName));
        if (client == null)
        {
            Debug.Log("UpdateAnimator:not client");
            return;
        }

        client.speed = speed;
        client.die = die;
        client.jump = jump;
        client.respawn = respawn;
        client.attack = attack;
    }

    private void WorldEnterHandler(EventData eventData)
    {
        string characterName = (string)eventData.Parameters[(byte)ParameterCode.CharacterName];
        float posX = (float)eventData.Parameters[(byte)ParameterCode.PosX];
        float posZ = (float)eventData.Parameters[(byte)ParameterCode.PosZ];
        float posY = Terrain.activeTerrain.terrainData.GetHeight((int)posX, (int)posZ);

        Debug.Log(posX + " " + posY + " " + posZ);
        Vector3 characterPosition = new Vector3(posX, posY, posZ);
        characterPosition.y = Terrain.activeTerrain.SampleHeight(characterPosition);

        var obj = Instantiate(Resources.Load("Hammer Warrior")) as GameObject;
        obj.transform.position = new Vector3(posX, posY, posZ);

        var player = obj.AddComponent<Player>();
        player.CharacterName = characterName;

        Players.Add(player);

        Debug.Log("WorldEnterHandler charName:" + characterName);
    }

    private void WorldExitHandler(EventData eventData)
    {
        string characterName = (string)eventData.Parameters[(byte)ParameterCode.CharacterName];

        var client = Players.FirstOrDefault(n => n.CharacterName.Equals(characterName));
        if (client == null)
            return;
        Players.Remove(client);
        DestroyObject(client.gameObject);

        Debug.Log("WorldExitHandler charName:" + characterName);
    }

    private void ListPlayersHandler(OperationResponse operationResponse)
    {
        var dicPlayer = operationResponse.Parameters[(byte)ParameterCode.ListPlayers] as Dictionary<string, object[]>;
        if (dicPlayer == null)
        {
            Debug.Log("ListPlayers null!");
            return;
        }

        foreach (var p in dicPlayer)
        {
            string charName = p.Key;
            object[] pos = p.Value;
            var obj = Instantiate(Resources.Load("Hammer Warrior")) as GameObject;

            obj.transform.position = new Vector3((float)pos[0], (float)pos[1], (float)pos[2]);

            var player = obj.AddComponent<Player>();
            player.CharacterName = charName;

            Players.Add(player);

            Debug.Log("Create player from list charName:" + charName);
        }

        Debug.Log("ListPlayersHandler ");
    }
    #endregion


    #region Up-level API

    public void SendLoginOperation(string name)
    {
        PhotonPeer.OpCustom((byte)OperationCode.Login,
                            new Dictionary<byte, object> { { (byte)ParameterCode.CharacterName, name } }, true);
    }

    public void SendChatMessage(string message)
    {
        PhotonPeer.OpCustom((byte)OperationCode.SendChatMessage,
                           new Dictionary<byte, object> { { (byte)ParameterCode.ChatMessage, message } }, true);
    }

    public void GetRecentChatMessage()
    {
        PhotonPeer.OpCustom((byte)OperationCode.GetRecentChatMessages, new Dictionary<byte, object>(), true);
    }

    public void MoveOperation(float x, float y, float z, float _x, float _y, float _z)
    {
        PhotonPeer.OpCustom((byte)OperationCode.Move,
                            new Dictionary<byte, object>
                                {
                                    {(byte) ParameterCode.PosX, x},
                                    {(byte) ParameterCode.PosY, y},
                                    {(byte) ParameterCode.PosZ, z},
                                    {(byte) ParameterCode.RotX, _x},
                                    {(byte) ParameterCode.RotY, _y},
                                    {(byte) ParameterCode.RotZ, _z},

    
                                }, false);
    }

    public void AnimatorOperations(float speed,bool jump, bool die, bool respawn, bool attack)
    {
        PhotonPeer.OpCustom((byte)OperationCode.UpdateAnimator,
            new Dictionary<byte, object>
            {
                 {(byte) ParameterCode.Speed, speed},
                 {(byte) ParameterCode.Jump, jump},
                 {(byte) ParameterCode.Die, die},
                 {(byte) ParameterCode.Respawn, respawn},
                 {(byte) ParameterCode.Attack, attack},

            }, false);
    }


    public void WorldEnterOperation()
    {
        PhotonPeer.OpCustom((byte)OperationCode.WorldEnter, new Dictionary<byte, object>(), true);
    }

    public void WorldExitOperation()
    {
        PhotonPeer.OpCustom((byte)OperationCode.WorldExit, new Dictionary<byte, object>(), true);
    }

    public void ListPlayersOperation()
    {
        PhotonPeer.OpCustom((byte)OperationCode.ListPlayers, new Dictionary<byte, object>(), true);
    }
    #endregion
}
