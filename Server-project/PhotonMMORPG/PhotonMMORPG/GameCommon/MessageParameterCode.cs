﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCommon
{
    public enum MessageParameterCode : byte
    {
        SubCodeParameterCode = 0,// Match the same value in the config
        PeerId,
        LoginName,
        Password,
        UserId,
        Object
    }
}
