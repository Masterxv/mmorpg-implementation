﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameCommon;
using Servers.Models.Interfaces;
using Servers.Models.Templates;

namespace Servers.Models
{
    public class NpcCharacter : ICharacter
    {
        public NpcTemplate NpcTemplate { get; set; }
        public Vector Position { get; set; }
        public Vector StartPosition { get; set; }
        public Vector Rotation { get; set; }
        public Vector StartRotation { get; set; }
        public Guid Identifier { get; set; }
    }
}
