﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameCommon;
using MultiplayerGameFramework.Interfaces.Messaging;
using MultiplayerGameFramework.Interfaces.Support;

namespace Servers.Support
{
    public class ClientCodeRemove:IClientCodeRemover
    {
        public void RemoveCodes(IMessage message)
        {
            //Do not remove any codes yet, nothing tor
            message.Parameters.Remove((byte) MessageParameterCode.PeerId);
        }
    }
}
