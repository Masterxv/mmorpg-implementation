﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExitGames.Logging;
using GameCommon;
using MultiplayerGameFramework.Implementation.Messaging;
using MultiplayerGameFramework.Interfaces.Client;
using MultiplayerGameFramework.Interfaces.Messaging;
using MultiplayerGameFramework.Interfaces.Server;
using Servers.Data.Client;
using Servers.Models;
using Servers.Services.Interfaces;
using ServiceStack.Redis;
using ServiceStack.Text;

namespace Servers.Handlers.World
{
    public class ClientEnterWorld : IHandler<IServerPeer>
    {
        private ILogger Log { get; set; }
        private IWorldService WorldService { get; set; }
        private IConnectionCollection<IClientPeer> ConnectionCollection { get; set; }
        private IRedisClientsManager ClientsManager { get; set; }
        private IRedisPubSubServer RedisPubSub { get; set; }
             
        public ClientEnterWorld(ILogger log, IWorldService worldService, IConnectionCollection<IClientPeer> connectionCollection,
            IRedisClientsManager clientsManager, IRedisPubSubServer redisPubSubServer)
        {
            Log = log;
            WorldService = worldService;
            ConnectionCollection = connectionCollection;
            ClientsManager = clientsManager;
            RedisPubSub = redisPubSubServer;
        }

        public MessageType Type => MessageType.Request;

        public byte Code => (byte)MessageOperationCode.World;

        public int? SubCode => (int?)MessageSubCode.EnterWorld;

        public bool HandleMessage(IMessage message, IServerPeer peer)
        {
            var playerData = MessageSerializerService.DeserializeObjectOfType<CharacterData>(message.Parameters[(byte)MessageParameterCode.Object]);

            Response response;
            if (playerData != null)
            {
                var player = new Player()
                {
                    UserId = playerData.UserId,
                    ServerPeer = peer,
                    CharacterName = playerData.SelectedCharacter.Name,
                    Client = ConnectionCollection.GetPeers<IClientPeer>().FirstOrDefault(clientPeer =>
                        clientPeer.PeerId == (Guid)message.Parameters[(byte)MessageParameterCode.PeerId])
                };
                Log.DebugFormat("On Client EnterWorld:    New player added to world server {0}", player.CharacterName);

                using (IRedisClient redis = ClientsManager.GetClient())
                {
                    Log.DebugFormat("The redis client is working here on world server");
                    //TO DO
                }

                var returnCode = WorldService.AddNewPlayerToWorld(player);

                if (returnCode == ReturnCode.WorldAddedNewPlayer)
                {
                    response = new Response(Code, SubCode, new Dictionary<byte, object>() { { (byte)MessageParameterCode.SubCodeParameterCode, SubCode }, { (byte)MessageParameterCode.PeerId, message.Parameters[(byte)MessageParameterCode.PeerId] } }, "New player on world", (short)returnCode);
                }
                else
                {
                    response = new Response(Code, SubCode, new Dictionary<byte, object>() { { (byte)MessageParameterCode.SubCodeParameterCode, SubCode }, { (byte)MessageParameterCode.PeerId, message.Parameters[(byte)MessageParameterCode.PeerId] } }, "Player is already in world", (short)returnCode);
                }
            }
            else
            {
                response = new Response(Code, SubCode, new Dictionary<byte, object>() { { (byte)MessageParameterCode.SubCodeParameterCode, SubCode }, { (byte)MessageParameterCode.PeerId, message.Parameters[(byte)MessageParameterCode.PeerId] } }, "Invalid operation", (short)ReturnCode.OperationInvalid);
            }

            peer.SendMessage(response);

            return true;
        }
    }
}
