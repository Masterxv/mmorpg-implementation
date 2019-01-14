﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameCommon;
using GameCommon.SerializedObjects;
using MGF.Mappers;
using MultiplayerGameFramework.Implementation.Messaging;
using MultiplayerGameFramework.Interfaces.Messaging;
using MultiplayerGameFramework.Interfaces.Server;

namespace Servers.Handlers.Login
{
    public class LoginCharacterListCharacterHandler : IHandler<IServerPeer>
    {
        public MessageType Type => MessageType.Request;

        public byte Code => (byte)MessageOperationCode.Login;

        public int? SubCode => (int?)MessageSubCode.CharacterList;

        public bool HandleMessage(IMessage message, IServerPeer peer)
        {
            //how to prevent hacking? => Proxy server removed all special codes from the client and uses its own when forwarding data
            var charList = new CharacterMapper().LoadByUserId((int)message.Parameters[(byte)MessageParameterCode.UserId]).Select(x => new Character
            {
                CharacterId = x.Id,
                Name = x.Name
            });
            //We have a list of characters, added into a serializable object
            var returnResponse = new Response(Code, SubCode, new Dictionary<byte, object>()
            {
                { (byte)MessageParameterCode.PeerId, message.Parameters[(byte)MessageParameterCode.PeerId]},
                { (byte)MessageParameterCode.Object, MessageSerializerService.SerializeObjectOfType(charList)},
                { (byte)MessageParameterCode.SubCodeParameterCode, SubCode}
            });

            peer.SendMessage(returnResponse);
            return true;
        }
    }
}
