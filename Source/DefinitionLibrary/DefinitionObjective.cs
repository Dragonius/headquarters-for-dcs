﻿/*
==========================================================================
This file is part of Headquarters for DCS World (HQ4DCS), a mission generator for
Eagle Dynamics' DCS World flight simulator.

HQ4DCS was created by Ambroise Garel (@akaAgar).
You can find more information about the project on its GitHub page,
https://akaAgar.github.io/headquarters-for-dcs

HQ4DCS is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

HQ4DCS is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with HQ4DCS. If not, see https://www.gnu.org/licenses/
==========================================================================
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Headquarters4DCS.DefinitionLibrary
{
    /// <summary>
    /// Definition of a mission objective.
    /// </summary>
    public sealed class DefinitionObjective : Definition
    {
        /// <summary>
        /// Valid spawn point types for this objective.
        /// </summary>
        public TheaterLocationSpawnPointType[] SpawnPointType { get; private set; }

        /// <summary>
        /// How far the waypoint will be from the spawned units, in nautical miles.
        /// </summary>
        public MinMaxD WaypointInaccuracy { get; private set; }

        /// <summary>
        /// Should the waypoint be set on the ground?
        /// </summary>
        public bool WaypointOnGround { get; private set; }

        /// <summary>
        /// Scripts to include only once.
        /// </summary>
        public string[] ScriptsOnce { get; private set; }

        /// <summary>
        /// Scripts to incude once per objective.
        /// </summary>
        public string[] ScriptsEachObjective { get; private set; }

        /// <summary>
        /// OGG files to include.
        /// </summary>
        public string[] OGGFiles { get; private set; }

        /// <summary>
        /// Unit groups to spawn for this mission objective.
        /// </summary>
        public DefinitionObjectiveUnitGroup[] Groups { get; private set; }

        protected override bool OnLoad(string path)
        {
            using (INIFile ini = new INIFile(path))
            {
                ScriptsEachObjective = ini.GetValueArray<string>("Objective", "Scripts.EachObjective");
                ScriptsOnce = ini.GetValueArray<string>("Objective", "Scripts.Once");

                SpawnPointType = ini.GetValueArray<TheaterLocationSpawnPointType>("Objective", "SpawnPoint.Type");
                if (SpawnPointType.Length == 0) SpawnPointType = new TheaterLocationSpawnPointType[] { TheaterLocationSpawnPointType.LandMedium, TheaterLocationSpawnPointType.LandLarge };

                WaypointInaccuracy = ini.GetValue<MinMaxD>("Objective", "Waypoint.Inaccuracy");
                WaypointOnGround = ini.GetValue<bool>("Objective", "Waypoint.OnGround");

                // [Groups] section
                Groups = new DefinitionObjectiveUnitGroup[ini.GetTopLevelKeysInSection("UnitGroups").Length];
                int i = 0;
                foreach (string k in ini.GetTopLevelKeysInSection("UnitGroups"))
                { Groups[i] = new DefinitionObjectiveUnitGroup(ini, k); i++; }
            }

            return true;
        }
    }
}
