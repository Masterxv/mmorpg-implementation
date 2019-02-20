﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCommon
{
    public enum MessageSubCode : int
    {
        LoginUserPass = 1,
        LoginNewAccount,
        CharacterList,
        SelectCharacter,
        CreateCharacter,
        EnterRegion,
        EnterWorld,
        RequestRegion
    }
}
