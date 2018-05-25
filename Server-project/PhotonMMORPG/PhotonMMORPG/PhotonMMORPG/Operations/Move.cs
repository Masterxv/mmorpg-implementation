﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Photon.SocketServer;
using Photon.SocketServer.Rpc;
using PhotonMMORPG.Common;

namespace PhotonMMORPG.Operations
{
    class Move : BaseOperation
    {
        public Move(IRpcProtocol protocol, OperationRequest request) : base(protocol, request)
        {
        }

        [DataMember(Code = (byte)ParameterCode.PosX)]
        public float X { get; set; }

        [DataMember(Code = (byte)ParameterCode.PosY)]
        public float Y { get; set; }

        [DataMember(Code = (byte)ParameterCode.PosZ)]
        public float Z { get; set; }


        [DataMember(Code = (byte)ParameterCode.RotX)]
        public float _X { get; set; }

        [DataMember(Code = (byte)ParameterCode.RotY)]
        public float _Y { get; set; }

        [DataMember(Code = (byte)ParameterCode.RotZ)]
        public float _Z { get; set; }
    }
}
