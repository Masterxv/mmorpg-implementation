﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameCommon;
using MultiplayerGameFramework.Interfaces.Server;

namespace Servers.Models.Interfaces
{
    public interface IAreaRegion
    {
        string Name { get; }
        Guid ZoneId { get; }
        IWorld World { get; }
        int GameTick { get; }
        string ApplicationServerName { get; }

        void AddNpcCharacter(Vector pos, NpcCharacter obj);
        void RemoveNpcCharacter(Vector pos, NpcCharacter obj);

        void SpawnMobs(Vector spawnPosition);
    }
}
