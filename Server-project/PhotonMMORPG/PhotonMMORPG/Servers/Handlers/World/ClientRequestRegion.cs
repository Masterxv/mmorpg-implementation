﻿using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Logging;
using GameCommon;
using MultiplayerGameFramework.Implementation.Messaging;
using MultiplayerGameFramework.Interfaces.Client;
using MultiplayerGameFramework.Interfaces.Config;
using MultiplayerGameFramework.Interfaces.Messaging;
using MultiplayerGameFramework.Interfaces.Server;
using Servers.Data.Client;
using Servers.Models;
using Servers.Services.Interfaces;

namespace Servers.Handlers.World
{
    public class ClientRequestRegion : IHandler<IServerPeer>
    {
        private ILogger Log { get; set; }
        private IWorldService WorldService { get; set; }
        private IConnectionCollection<IClientPeer> ConnectionCollection { get; set; }
        private IServerConnectionCollection<IServerType, IServerPeer> ServerConnectionCollection { get; set; }

        public ClientRequestRegion(ILogger log, IWorldService worldService,
            IServerConnectionCollection<IServerType, IServerPeer> serverConnectionCollection,
            IConnectionCollection<IClientPeer> connectionCollection)
        {
            Log = log;
            WorldService = worldService;
            ConnectionCollection = connectionCollection;
            ServerConnectionCollection = serverConnectionCollection;
        }

        public MessageType Type => MessageType.Request;

        public byte Code => (byte)MessageOperationCode.World;

        public int? SubCode => (int?)MessageSubCode.RequestRegion;

        public bool HandleMessage(IMessage message, IServerPeer peer)
        {
            var playerData = MessageSerializerService.DeserializeObjectOfType<CharacterData>(message.Parameters[(byte)MessageParameterCode.Object]);
            Log.DebugFormat("OnClientRequestRegion: Client {0} request region", playerData.SelectedCharacter.Name);

            var clientPeerGuid = new Guid((byte[])message.Parameters[(byte)MessageParameterCode.PeerId]);
            
            var player = new Player()
            {
                UserId = playerData.UserId,
                ServerPeer = peer,
                World = WorldService.GetWorld(),
                Name = playerData.SelectedCharacter.Name,
                ClientPeerId = clientPeerGuid
            };

            var region = WorldService.GetRegionForPlayer(player);

            ConnectionCollection.GetPeers<IClientPeer>().FirstOrDefault(x => x.PeerId == clientPeerGuid)
                .ClientData<CharacterData>().Region = new GameCommon.SerializedObjects.Region()
            {
                Name = region.Name,
                ApplicationServerName =  region.ApplicationServerName
            };

            var response = region == null
                ? new Response(Code, SubCode, new Dictionary<byte, object>() { { (byte)MessageParameterCode.SubCodeParameterCode, SubCode }, { (byte)MessageParameterCode.PeerId, message.Parameters[(byte)MessageParameterCode.PeerId] } }, "AreaRegion can't be determined ", (short)ReturnCode.NoRegion)
                : new Response(Code, SubCode, new Dictionary<byte, object>() { { (byte)MessageParameterCode.SubCodeParameterCode, SubCode }, { (byte)MessageParameterCode.PeerId, message.Parameters[(byte)MessageParameterCode.PeerId] }, { (byte)MessageParameterCode.Object, MessageSerializerService.SerializeObjectOfType(region) } }, "AreaRegion of character", (short)ReturnCode.Ok);
            
            peer.SendMessage(response);

            return true;
        }
    }
}
