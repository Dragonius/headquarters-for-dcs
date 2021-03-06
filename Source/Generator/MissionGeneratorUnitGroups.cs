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

using Headquarters4DCS.DefinitionLibrary;
using Headquarters4DCS.Mission;
using Headquarters4DCS.Template;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Headquarters4DCS.Generator
{
    public sealed class MissionGeneratorUnitGroups : IDisposable
    {
        private const string TANKER_TACAN = "40X"; // FIXME: load from file

        private readonly DefinitionLanguage Language;
        private readonly MissionGeneratorCallsign CSGenerator;

        private Dictionary<int, int> AirdromeParkingNumber = new Dictionary<int, int>();

        private List<int> UsedOnboardNumbers = new List<int>();
        private List<int> UsedFakeGroupNamesNumbers = new List<int>();

        /// <summary>
        /// Unique ID of the next group to spawn. Only for "non-critical" groups.
        /// (objective "waypoint" groups have an ID of 1000*(objectiveIndex + 1), objective "center" group has an ID of 10000)
        /// </summary>
        private int LastGroupID;

        public MissionGeneratorUnitGroups(DefinitionLanguage language, MissionGeneratorCallsign csGenerator)
        {
            Language = language;
            LastGroupID = 1;
            CSGenerator = csGenerator;
            //TotalAAValue = 0;
        }

        /// <summary>
        /// IDispose implementation.
        /// </summary>
        public void Dispose() { }

        //public void GeneratePlayerFlightGroups(HQMission mission, MissionTemplate template, DefinitionMissionObjective missionObjective)
        //{
        //    HQDebugLog.Instance.Log("Generating player flight groups...");

        //    foreach (MissionTemplatePlayerFlightGroup pfg in template.FlightGroupsPlayers)
        //    {
        //        DefinitionUnit aircraftDefinition = Library.GetDefinition<DefinitionUnit>(pfg.Aircraft);

        //        if ((aircraftDefinition == null) || !aircraftDefinition.AircraftPlayerControllable)
        //            throw new Exception($"Player aircraft \"{pfg.Aircraft}\" not found.");

        //        HQMissionUnitGroup uGroup = new HQMissionUnitGroup(
        //            "GroupAircraftPlayer", "UnitAircraft", aircraftDefinition.Category,
        //            GroupID, template.PlayerCoalition, mission.Airbases[1].Coordinates,
        //            Enumerable.Repeat(aircraftDefinition.ID, pfg.GroupSize).ToArray());

        //        SetupAircraftGroup(
        //            uGroup, mission, CallsignFamily.Aircraft, true,
        //            aircraftDefinition, missionObjective.MissionPayload, mission.Airbases[0].DCSID);

        //        if (pfg.AIWingmen)
        //        {
        //            // set group AI to default allied AI, and add a special flag so the first unit of the group will be a client
        //            uGroup.UnitsSkill = HQSkillToDCSSkill(template.AlliedSkill);
        //            uGroup.Flags.Add(UnitGroupFlag.FirstUnitIsClient);
        //        }
        //        else // No AI wingmen: make all units clients
        //            uGroup.UnitsSkill = DCSSkillLevel.Client;

        //        uGroup.CustomValues.Add("AirdromeID", HQTools.ValToString(mission.Airbases[0].DCSID));
        //        uGroup.CustomValues.Add("FinalWPIndex", HQTools.ValToString(mission.Waypoints.Count + 2));
        //        uGroup.CustomValues.Add("FinalX", HQTools.ValToString(mission.Airbases[1].Coordinates.X));
        //        uGroup.CustomValues.Add("FinalY", HQTools.ValToString(mission.Airbases[1].Coordinates.Y));
        //        uGroup.CustomValues.Add("FinalAirdromeID", HQTools.ValToString(mission.Airbases[1].DCSID));
        //        uGroup.CustomValues.Add("DCSTask", MGTools.GetDCSTaskNameString(missionObjective.DCSTask));
        //        uGroup.CustomValues.Add("DCSTaskTasks", MGTools.GetDCSTaskAdditionalTasksString(missionObjective.DCSTask, 2));

        //        uGroup.CustomValues.Add("TakeoffAltitude", "13"); // FIXME: fixed value in the Lua file
        //        uGroup.CustomValues.Add("TakeOffAltitudeType", "BARO"); // FIXME: fixed value in the Lua file
        //        uGroup.CustomValues.Add("TakeOffSpeed", "130.0"); // FIXME: fixed value in the Lua file

        //        switch (pfg.StartFrom)
        //        {
        //            default: // PlayerFlightGroupStartLocation.InAir
        //                uGroup.CustomValues.Add("TakeoffAltitude", HQTools.ValToString(20000 * HQTools.FEET_TO_METERS * HQTools.RandomDouble(0.9, 1.1))); // FIXME: altitude by aircraft
        //                uGroup.CustomValues.Add("TakeOffSpeed", HQTools.ValToString(300.0 * HQTools.KNOTS_TO_METERSPERSECOND)); // FIXME: speed by aircraft
        //                uGroup.CustomValues.Add("TakeOffType", "BARO");
        //                uGroup.CustomValues.Add("TakeOffAction", "Turning Point");
        //                uGroup.CustomValues.Add("TakeOffType", "Turning Point");
        //                break;
        //            case PlayerFlightGroupStartLocation.FromRunway:
        //                uGroup.CustomValues.Add("TakeOffAction", "From Runway");
        //                uGroup.CustomValues.Add("TakeOffType", "TakeOff");
        //                break;
        //            case PlayerFlightGroupStartLocation.FromParking:
        //                uGroup.CustomValues.Add("TakeOffAction", "From Parking Area");
        //                uGroup.CustomValues.Add("TakeOffType", "TakeOffParking");
        //                break;
        //            case PlayerFlightGroupStartLocation.FromParkingHot:
        //                uGroup.CustomValues.Add("TakeOffAction", "From Parking Area Hot");
        //                uGroup.CustomValues.Add("TakeOffType", "TakeOffParkingHot");
        //                break;
        //        }

        //        foreach (HQMissionWaypoint wp in mission.Waypoints)
        //            uGroup.Waypoints.Add(
        //                new HQMissionWaypoint(
        //                    wp.Coordinates, wp.Name, wp.AltitudeMultiplier * HQTools.RandomDouble(0.9, 1.1), HQTools.RandomDouble(0.9, 1.1)));

        //        mission.BriefingFlightPackage.Add(
        //        new HQMissionBriefingFlightGroup(
        //            uGroup.Name, uGroup.Units[0], DCSAircraftTask.CAS, // FIXME: tasking
        //            uGroup.UnitCount, uGroup.RadioFrequency)); // FIXME: complete

        //        mission.UnitGroups.Add(uGroup);

        //        GroupID++;
        //        TotalAAValue += aircraftDefinition.AircraftAirToAirRating[(missionObjective.MissionPayload == AircraftPayloadType.A2A) ? 0 : 1];
        //    }

        //    HQDebugLog.Instance.Log();
        //}

        public int AddPlayerFlightGroup(DCSMission mission, MissionTemplate template, MissionTemplatePlayerFlightGroup flightGroupTemplate, DefinitionObjective objectiveDef, DefinitionTheaterAirbase airbaseDef)
        {
            DefinitionUnit aircraftDefinition = Library.Instance.GetDefinition<DefinitionUnit>(flightGroupTemplate.AircraftType);
            if (aircraftDefinition == null) return -1; // Aircraft does not exist

            DCSMissionUnitGroup unitGroup = new DCSMissionUnitGroup(
                "GroupAircraftPlayer", "UnitAircraft", aircraftDefinition.Category,
                LastGroupID, template.ContextPlayerCoalition, airbaseDef.Coordinates,
                Enumerable.Repeat(aircraftDefinition.ID, flightGroupTemplate.Count).ToArray());

            DCSFlightGroupTask fgTask;

            switch (flightGroupTemplate.Task)
            {
                case PlayerFlightGroupTask.CAPEscort:
                    fgTask = DCSFlightGroupTask.CAP; break;
                case PlayerFlightGroupTask.SEADEscort:
                    fgTask = DCSFlightGroupTask.SEAD; break;
                default: //aka PlayerFlightGroupTask.PrimaryMission:
                    fgTask = objectiveDef.FGTasking; break;
            }

            SetupAircraftGroup(
                unitGroup, mission, CallsignFamily.Aircraft, true,
                aircraftDefinition, GetPayloadByPlayerGroupTask(fgTask), airbaseDef.DCSID);

            bool usePlayerInsteadOfClient = (template.GetPlayerCount() < 2) && !template.PreferencesForceClientInSP;

            if (flightGroupTemplate.WingmenAI)
            {
                unitGroup.UnitsSkill = HQSkillToDCSSkill(template.SkillFriendlyAir); // NEXTVERSION: allySkillAir or enemySkillAir according to airbase coalition
                unitGroup.Flags.Add(usePlayerInsteadOfClient ? UnitGroupFlag.FirstUnitIsPlayer : UnitGroupFlag.FirstUnitIsClient);
            }
            else // No AI wingmen: make all units clients
                unitGroup.UnitsSkill = usePlayerInsteadOfClient ? DCSSkillLevel.Player : DCSSkillLevel.Client;

            unitGroup.CustomValues.Add("AirdromeID", HQTools.ValToString(airbaseDef.DCSID));
            //unitGroup.CustomValues.Add("FinalWPIndex", HQTools.ValToString(mission.Waypoints.Count + 2));
            unitGroup.CustomValues.Add("FinalX", HQTools.ValToString(airbaseDef.Coordinates.X));
            unitGroup.CustomValues.Add("FinalY", HQTools.ValToString(airbaseDef.Coordinates.Y));
            unitGroup.CustomValues.Add("FinalAirdromeID", HQTools.ValToString(airbaseDef.DCSID));
            //unitGroup.CustomValues.Add("DCSTask", MGTools.GetDCSTaskNameString(missionObjective.DCSTask));
            //unitGroup.CustomValues.Add("DCSTaskTasks", MGTools.GetDCSTaskAdditionalTasksString(missionObjective.DCSTask, 2));

            //unitGroup.CustomValues.Add("TakeoffAltitude", "13"); // FIXME: fixed value in the Lua file
            //unitGroup.CustomValues.Add("TakeOffAltitudeType", "BARO"); // FIXME: fixed value in the Lua file
            //unitGroup.CustomValues.Add("TakeOffSpeed", "130.0"); // FIXME: fixed value in the Lua file

            switch (flightGroupTemplate.StartLocation)
            {
                default: // PlayerFlightGroupStartLocation.InAir
                    unitGroup.CustomValues.Add("TakeoffAltitude", HQTools.ValToString(20000 * HQTools.FEET_TO_METERS * HQTools.RandomDouble(0.9, 1.1))); // FIXME: altitude by aircraft
                    unitGroup.CustomValues.Add("TakeOffSpeed", HQTools.ValToString(300.0 * HQTools.KNOTS_TO_METERSPERSECOND)); // FIXME: speed by aircraft
                    unitGroup.CustomValues.Add("TakeOffType", "BARO");
                    unitGroup.CustomValues.Add("TakeOffAction", "Turning Point");
                    unitGroup.CustomValues.Add("TakeOffType", "Turning Point");
                    break;
                case PlayerFlightGroupStartLocation.FromRunway:
                    unitGroup.CustomValues.Add("TakeOffAction", "From Runway");
                    unitGroup.CustomValues.Add("TakeOffType", "TakeOff");
                    break;
                case PlayerFlightGroupStartLocation.FromParking:
                    unitGroup.CustomValues.Add("TakeOffAction", "From Parking Area");
                    unitGroup.CustomValues.Add("TakeOffType", "TakeOffParking");
                    break;
                case PlayerFlightGroupStartLocation.FromParkingHot:
                    unitGroup.CustomValues.Add("TakeOffAction", "From Parking Area Hot");
                    unitGroup.CustomValues.Add("TakeOffType", "TakeOffParkingHot");
                    break;
            }

            mission.BriefingFlightPackage.Add(
                new DCSMissionBriefingFlightGroup(
                    unitGroup.Name, unitGroup.Units[0], DCSFlightGroupTask.CAS, airbaseDef.Name, // FIXME: tasking
                    unitGroup.UnitCount, unitGroup.RadioFrequency)); // FIXME: complete

            mission.UnitGroups.Add(unitGroup);
            LastGroupID++;
            return LastGroupID - 1;
        }

        private PlayerFlightGroupPayloadType GetPayloadByPlayerGroupTask(DCSFlightGroupTask task)
        {
            switch (task)
            {
                case DCSFlightGroupTask.AntishipStrike:
                    return PlayerFlightGroupPayloadType.AntiShip;

                case DCSFlightGroupTask.CAP:
                case DCSFlightGroupTask.FighterSweep:
                case DCSFlightGroupTask.Intercept:
                    return PlayerFlightGroupPayloadType.A2A;

                case DCSFlightGroupTask.CAS:
                case DCSFlightGroupTask.GroundAttack:
                case DCSFlightGroupTask.PinpointStrike:
                case DCSFlightGroupTask.RunwayAttack:
                    return PlayerFlightGroupPayloadType.A2G;

                case DCSFlightGroupTask.SEAD:
                    return PlayerFlightGroupPayloadType.SEAD;

                case DCSFlightGroupTask.Escort:
                case DCSFlightGroupTask.Reconnaissance:
                case DCSFlightGroupTask.Transport:
                    return PlayerFlightGroupPayloadType.Default;
            }

            return PlayerFlightGroupPayloadType.Default;
        }

        //public DCSMissionUnitGroup AddNodeFeatureGroup(
        //    DCSMission mission, MissionTemplate template,
        //    DefinitionFeatureUnitGroup unitGroupDefinition, Coordinates location)
        //{
        //    DebugLog.Instance.Log("Adding unit group...");

        //    Coalition groupCoalition = unitGroupDefinition.Flags.Contains(MissionObjectiveUnitGroupFlags.Friendly) ? mission.CoalitionPlayer : mission.CoalitionEnemy;

        //    DCSMissionUnitGroup unitGroup = null;

        //    UnitFamily selectedFamily = HQTools.RandomFrom(unitGroupDefinition.Families);

        //    unitGroup =
        //        DCSMissionUnitGroup.FromCoalitionArmyAndUnitFamily(
        //            unitGroupDefinition.LuaGroup, unitGroupDefinition.LuaUnit,
        //            Library.Instance.GetDefinition<DefinitionCoalition>(groupCoalition == Coalition.Red ? template.ContextCoalitionRed : template.ContextCoalitionBlue),
        //            template.ContextTimePeriod, selectedFamily, unitGroupDefinition.UnitCount.GetValue(),
        //            LastGroupID, groupCoalition, location);


        //    if ((unitGroup == null) || (unitGroup.UnitCount == 0)) // TODO: is group critical or not? If not, simply output a warning
        //        throw new Exception($"Found no valid units to generate mission critical group of {unitGroup.Coalition} {unitGroup.Category}.");

        //    if (unitGroup.IsAircraft)
        //    {
        //        CallsignFamily csFamily = CallsignFamily.Aircraft;

        //        switch (selectedFamily)
        //        {
        //            case UnitFamily.PlaneAWACS:
        //                csFamily = CallsignFamily.AWACS;
        //                break;
        //            case UnitFamily.PlaneTankerBasket:
        //            case UnitFamily.PlaneTankerBoom:
        //                csFamily = CallsignFamily.Tanker;
        //                break;
        //        }

        //        SetupAircraftGroup(unitGroup, mission, csFamily, unitGroupDefinition.Flags.Contains(MissionObjectiveUnitGroupFlags.Friendly));
        //    }

        //    // TODO: embedded air defense
        //    //if (grp.Flags.Contains(MissionObjectiveUnitGroupFlags.AllowAirDefense) && !grp.Flags.Contains(MissionObjectiveUnitGroupFlags.Friendly))
        //    //    unitGroup.AddAirDefenseUnits(Library, coalitions[(int)groupCoalition], template.TimePeriod, Library.Common.EnemyAirDefense[(int)HQTools.ResolveRandomAmount(template.EnemyAirDefense)]);

        //    mission.UnitGroups.Add(unitGroup);

        //    DebugLog.Instance.Log();

        //    LastGroupID++;
        //    return unitGroup; // GroupID - 1;
        //}

        //public void GeneratePlayerFlightGroups(HQMission mission, MissionTemplate template, DefinitionMissionObjective missionObjective)
        //{
        //    HQDebugLog.Instance.Log("Generating player flight groups...");

        //    foreach (MissionTemplatePlayerFlightGroup pfg in template.FlightGroupsPlayers)
        //    {
        //        DefinitionUnit aircraftDefinition = Library.GetDefinition<DefinitionUnit>(pfg.Aircraft);

        //        if ((aircraftDefinition == null) || !aircraftDefinition.AircraftPlayerControllable)
        //            throw new Exception($"Player aircraft \"{pfg.Aircraft}\" not found.");

        //        HQMissionUnitGroup uGroup = new HQMissionUnitGroup(
        //            "GroupAircraftPlayer", "UnitAircraft", aircraftDefinition.Category,
        //            GroupID, template.PlayerCoalition, mission.Airbases[1].Coordinates,
        //            Enumerable.Repeat(aircraftDefinition.ID, pfg.GroupSize).ToArray());

        //        SetupAircraftGroup(
        //            uGroup, mission, CallsignFamily.Aircraft, true,
        //            aircraftDefinition, missionObjective.MissionPayload, mission.Airbases[0].DCSID);

        //        if (pfg.AIWingmen)
        //        {
        //            // set group AI to default allied AI, and add a special flag so the first unit of the group will be a client
        //            uGroup.UnitsSkill = HQSkillToDCSSkill(template.AlliedSkill);
        //            uGroup.Flags.Add(UnitGroupFlag.FirstUnitIsClient);
        //        }
        //        else // No AI wingmen: make all units clients
        //            uGroup.UnitsSkill = DCSSkillLevel.Client;

        //        uGroup.CustomValues.Add("AirdromeID", HQTools.ValToString(mission.Airbases[0].DCSID));
        //        uGroup.CustomValues.Add("FinalWPIndex", HQTools.ValToString(mission.Waypoints.Count + 2));
        //        uGroup.CustomValues.Add("FinalX", HQTools.ValToString(mission.Airbases[1].Coordinates.X));
        //        uGroup.CustomValues.Add("FinalY", HQTools.ValToString(mission.Airbases[1].Coordinates.Y));
        //        uGroup.CustomValues.Add("FinalAirdromeID", HQTools.ValToString(mission.Airbases[1].DCSID));
        //        uGroup.CustomValues.Add("DCSTask", MGTools.GetDCSTaskNameString(missionObjective.DCSTask));
        //        uGroup.CustomValues.Add("DCSTaskTasks", MGTools.GetDCSTaskAdditionalTasksString(missionObjective.DCSTask, 2));

        //        uGroup.CustomValues.Add("TakeoffAltitude", "13"); // FIXME: fixed value in the Lua file
        //        uGroup.CustomValues.Add("TakeOffAltitudeType", "BARO"); // FIXME: fixed value in the Lua file
        //        uGroup.CustomValues.Add("TakeOffSpeed", "130.0"); // FIXME: fixed value in the Lua file

        //        switch (pfg.StartFrom)
        //        {
        //            default: // PlayerFlightGroupStartLocation.InAir
        //                uGroup.CustomValues.Add("TakeoffAltitude", HQTools.ValToString(20000 * HQTools.FEET_TO_METERS * HQTools.RandomDouble(0.9, 1.1))); // FIXME: altitude by aircraft
        //                uGroup.CustomValues.Add("TakeOffSpeed", HQTools.ValToString(300.0 * HQTools.KNOTS_TO_METERSPERSECOND)); // FIXME: speed by aircraft
        //                uGroup.CustomValues.Add("TakeOffType", "BARO");
        //                uGroup.CustomValues.Add("TakeOffAction", "Turning Point");
        //                uGroup.CustomValues.Add("TakeOffType", "Turning Point");
        //                break;
        //            case PlayerFlightGroupStartLocation.FromRunway:
        //                uGroup.CustomValues.Add("TakeOffAction", "From Runway");
        //                uGroup.CustomValues.Add("TakeOffType", "TakeOff");
        //                break;
        //            case PlayerFlightGroupStartLocation.FromParking:
        //                uGroup.CustomValues.Add("TakeOffAction", "From Parking Area");
        //                uGroup.CustomValues.Add("TakeOffType", "TakeOffParking");
        //                break;
        //            case PlayerFlightGroupStartLocation.FromParkingHot:
        //                uGroup.CustomValues.Add("TakeOffAction", "From Parking Area Hot");
        //                uGroup.CustomValues.Add("TakeOffType", "TakeOffParkingHot");
        //                break;
        //        }

        //        foreach (HQMissionWaypoint wp in mission.Waypoints)
        //            uGroup.Waypoints.Add(
        //                new HQMissionWaypoint(
        //                    wp.Coordinates, wp.Name, wp.AltitudeMultiplier * HQTools.RandomDouble(0.9, 1.1), HQTools.RandomDouble(0.9, 1.1)));

        //        mission.BriefingFlightPackage.Add(
        //        new HQMissionBriefingFlightGroup(
        //            uGroup.Name, uGroup.Units[0], DCSAircraftTask.CAS, // FIXME: tasking
        //            uGroup.UnitCount, uGroup.RadioFrequency)); // FIXME: complete

        //        mission.UnitGroups.Add(uGroup);

        //        GroupID++;
        //        TotalAAValue += aircraftDefinition.AircraftAirToAirRating[(missionObjective.MissionPayload == AircraftPayloadType.A2A) ? 0 : 1];
        //    }

        //    HQDebugLog.Instance.Log();
        //}

        public void AddObjectiveUnitGroupsAtEachObjective(DCSMission mission, MissionTemplate template, DefinitionObjective objective, DefinitionCoalition[] coalitions)
        {
            DebugLog.Instance.Log("Generating mission objective unit groups at each objective...");

            int i, j;

            //CommonSettingsEnemyAirDefense airDefense = Library.Common.EnemyAirDefense[(int)HQTools.ResolveRandomAmount(template.EnemyAirDefense)];

            for (i = 0; i < mission.Objectives.Length; i++)
            {
                if (objective.UnitGroups.Length == 0) continue;

                for (j = 0; j < HQTools.EnumCount<ObjectiveUnitGroupRole>(); j++)
                {
                    DefinitionObjectiveUnitGroup grp = objective.UnitGroups[j];
                    if (string.IsNullOrEmpty(grp.LuaGroup)) break; // No units

                    DCSMissionObjectiveLocation obj = mission.Objectives[i];

                    Coalition groupCoalition = grp.Flags.Contains(MissionObjectiveUnitGroupFlags.Friendly) ? mission.CoalitionPlayer : mission.CoalitionEnemy;

                    DCSMissionUnitGroup uGroup = null;

                    List<UnitFamily> availableFamilies = new List<UnitFamily>(grp.Family);

                    int groupID = (j == (int)ObjectiveUnitGroupRole.Target) ? 1001 + i : ++LastGroupID;

                    while (availableFamilies.Count > 0)
                    {
                        UnitFamily selectedFamily = HQTools.RandomFrom(availableFamilies);

                        uGroup =
                            DCSMissionUnitGroup.FromCoalitionArmyAndUnitFamily(
                                grp.LuaGroup, grp.LuaUnit, coalitions[(int)groupCoalition],
                                template.ContextTimePeriod, selectedFamily, grp.Count.GetValue(),
                                groupID, groupCoalition, obj.Coordinates);

                        if (uGroup == null)
                            throw new HQ4DCSException($"Failed to generate required unit group of family {selectedFamily} at objective #{i + 1}. Perhaps no unit type of this type was found for the {groupCoalition} coalition.");

                        if (uGroup.UnitCount > 0) break;

                        availableFamilies.Remove(selectedFamily);
                    }


                    if ((uGroup == null) || (uGroup.UnitCount == 0))
                        throw new Exception($"Found no valid units to generate mission critical group of {uGroup.Coalition} {uGroup.Category}.");

                    if (grp.Flags.Contains(MissionObjectiveUnitGroupFlags.AllowAirDefense))
                        uGroup.AddAirDefenseUnits(
                            coalitions[(int)groupCoalition], template.ContextTimePeriod,
                            Library.Instance.Common.AirDefense[(int)HQTools.ResolveRandomAmount(grp.Flags.Contains(MissionObjectiveUnitGroupFlags.Friendly) ? template.SituationFriendlyAirDefense : template.SituationEnemyAirDefense)]);

                    string hidden;
                    if (grp.Flags.Contains(MissionObjectiveUnitGroupFlags.AlwaysVisible)) hidden = "false";
                    else if (grp.Flags.Contains(MissionObjectiveUnitGroupFlags.AlwaysHidden)) hidden = "true";
                    else if (grp.Flags.Contains(MissionObjectiveUnitGroupFlags.Friendly)) hidden = "false";
                    else if (template.PreferencesEnemiesOnF10Map) hidden = "false";
                    else hidden = "true";

                    uGroup.CustomValues.Add("Hidden", hidden);

                    mission.UnitGroups.Add(uGroup);
                }
            }

            DebugLog.Instance.Log();
        }

        //public void GenerateObjectiveUnitGroupsAtCenter(HQMission mission, MissionTemplate template, MissionObjectiveDefinition missionTask, CoalitionDefinition[] coalitions)
        //{
        //    int i;

        //    HQDebugLog.Log("Generating common mission objective unit groups...");

        //    EnemyAirDefenseSettings airDefense = Library.Common.AirDefense[(int)HQTools.ResolveRandomAmount(template.EnemyAirDefense)];

        //    Coordinates location = (mission.ObjectiveCount > 0) ? mission.ObjectivesCenterPoint : mission.Airbases[0].Coordinates;

        //    for (i = 0; i < missionTask.UnitGroupOnlyOnce.Length; i++)
        //    {
        //        MissionObjectiveUnitGroup grp = missionTask.UnitGroupOnlyOnce[i];
        //        if (string.IsNullOrEmpty(grp.LuaGroup)) break; // No units

        //        Coalition groupCoalition = grp.Flags.Contains(MissionTaskUnitGroupFlags.Friendly) ? mission.CoalitionPlayer : mission.CoalitionEnemy;

        //        MissionHQUnitGroup uGroup = null;

        //        List<UnitFamily> availableFamilies = new List<UnitFamily>(grp.Families);
        //        while (availableFamilies.Count > 0)
        //        {
        //            UnitFamily selectedFamily = HQTools.RandomFrom(availableFamilies);

        //            uGroup =
        //                MissionHQUnitGroup.FromCoalitionArmyAndUnitFamily(
        //                    grp.LuaGroup, grp.LuaUnit, coalitions[(int)groupCoalition],
        //                    template.TimePeriod, selectedFamily, grp.UnitCount.GetValue(),
        //                    (i + 1) * 1000 + j, groupCoalition, location);

        //            if (uGroup.UnitCount > 0) break;

        //            availableFamilies.Remove(selectedFamily);
        //        }

        //        if ((uGroup == null) || (uGroup.UnitCount == 0))
        //            throw new Exception($"Found no valid units to generate mission critical group of {uGroup.Coalition} {uGroup.Category}.");

        //        if (grp.Flags.Contains(MissionTaskUnitGroupFlags.AllowAirDefense) && !grp.Flags.Contains(MissionTaskUnitGroupFlags.Friendly))
        //            uGroup.AddAirDefenseUnits(coalitions[(int)groupCoalition], template.TimePeriod, Library.Common.AirDefense[(int)HQTools.ResolveRandomAmount(template.EnemyAirDefense)]);

        //        mission.UnitGroups.Add(uGroup);
        //    }

        //    HQDebugLog.Log();
        //}

        public void AddEnemyAirDefenseUnits(
            DCSMission mission, MissionTemplate template, DefinitionTheater theater, DefinitionObjective objective, DefinitionCoalition[] coalitions, DefinitionTheaterAirbase airbase,
            out AmountNR selectedEnemyAirDefense)
        {
            DebugLog.Instance.Log("Generating enemy ground air defense units...");

            selectedEnemyAirDefense = HQTools.ResolveRandomAmount(template.SituationEnemyAirDefense);

            DebugLog.Instance.Log(
                $"  Enemy air defense should be {template.SituationEnemyAirDefense.ToString().ToUpperInvariant()}" +
                ((template.SituationEnemyAirDefense == AmountNR.Random) ?
                $" (randomly selected level {selectedEnemyAirDefense.ToString().ToUpperInvariant()})" : ""));

            LibraryCommonSettingsEnemyAirDefense airDefense = Library.Instance.Common.AirDefense[(int)selectedEnemyAirDefense];

            foreach (AirDefenseRange adr in Enum.GetValues(typeof(AirDefenseRange)))
            {
                int adGroupCount = airDefense.InAreaGroupCount[(int)adr].GetValue();
                if (adGroupCount == 0) continue; // no unit groups for this air defense range

                LibraryCommonSettingsEnemyAirDefenseDistance distanceSettings = Library.Instance.Common.AirDefenseDistance[(int)adr];

                for (int i = 0; i < adGroupCount; i++)
                {
                    // Select nodes (1) far enough from the player starting airbase and (2) between min and max distance from an objective
                    DefinitionTheaterSpawnPoint? selNode = theater.GetRandomSpawnPoint(
                        distanceSettings.NodeTypes, null,
                        new MinMaxD(distanceSettings.MinDistanceFromTakeOffLocation, double.MaxValue), airbase.Coordinates,
                        distanceSettings.DistanceFromObjective, HQTools.RandomFrom(mission.Objectives).Coordinates);

                    if (!selNode.HasValue) // No nodes matching search criteria, don't spawn anything
                    {
                        DebugLog.Instance.Log("WARNING: failed to find a node to spawn enemy air defense.");
                        continue;
                    }

                    UnitFamily unitFamily = HQTools.RandomFrom(airDefense.InAreaFamilies[(int)adr]);

                    DCSMissionUnitGroup uGroup = DCSMissionUnitGroup.FromCoalitionArmyAndUnitFamily(
                        "GroupVehicleIdle", "UnitVehicle",
                        coalitions[(int)mission.CoalitionEnemy], template.ContextTimePeriod,
                        unitFamily, airDefense.InAreaGroupSize[(int)adr].GetValue(),
                        LastGroupID, mission.CoalitionEnemy, selNode.Value.Coordinates);

                    if (uGroup == null)
                    {
                        DebugLog.Instance.Log($"WARNING: failed to generate air defense of type {unitFamily} for coalition {mission.CoalitionEnemy}.");
                        continue;
                    }

                    uGroup.Name = GetGroupName(UnitFamily.VehicleSAMMedium);

                    if (uGroup.UnitCount == 0) continue;

                    mission.UnitGroups.Add(uGroup);
                    LastGroupID++;
                }
            }

            DebugLog.Instance.Log();
        }

        public void AddFriendlyAirDefenseUnits(
            DCSMission mission, MissionTemplate template, DefinitionTheater theater, DefinitionObjective objective, DefinitionCoalition[] coalitions, DefinitionTheaterAirbase airbase,
            out AmountNR selectedFriendlyAirDefense)
        {
            DebugLog.Instance.Log("Generating friendly ground air defense units...");

            selectedFriendlyAirDefense = HQTools.ResolveRandomAmount(template.SituationFriendlyAirDefense);

            DebugLog.Instance.Log(
                $"  Enemy air defense should be {template.SituationEnemyAirDefense.ToString().ToUpperInvariant()}" +
                ((template.SituationEnemyAirDefense == AmountNR.Random) ?
                $" (randomly selected level {selectedFriendlyAirDefense.ToString().ToUpperInvariant()})" : ""));

            LibraryCommonSettingsEnemyAirDefense airDefense = Library.Instance.Common.AirDefense[(int)selectedFriendlyAirDefense];

            foreach (AirDefenseRange adr in Enum.GetValues(typeof(AirDefenseRange)))
            {
                int adGroupCount = airDefense.InAreaGroupCount[(int)adr].GetValue();
                if (adGroupCount == 0) continue; // no unit groups for this air defense range

                LibraryCommonSettingsEnemyAirDefenseDistance distanceSettings = Library.Instance.Common.AirDefenseDistance[(int)adr];

                for (int i = 0; i < adGroupCount; i++)
                {
                    // Select nodes near the starting location
                    DefinitionTheaterSpawnPoint? selNode = theater.GetRandomSpawnPoint(
                        distanceSettings.NodeTypes, null,
                        new MinMaxD(0, 10.0), airbase.Coordinates);

                    if (!selNode.HasValue) // No nodes matching search criteria, don't spawn anything
                    {
                        DebugLog.Instance.Log("WARNING: failed to find a spawn point for enemy air defense.");
                        continue;
                    }

                    UnitFamily unitFamily = HQTools.RandomFrom(airDefense.InAreaFamilies[(int)adr]);

                    DCSMissionUnitGroup uGroup = DCSMissionUnitGroup.FromCoalitionArmyAndUnitFamily(
                        "GroupVehicleIdle", "UnitVehicle",
                        coalitions[(int)mission.CoalitionPlayer], template.ContextTimePeriod,
                        unitFamily, airDefense.InAreaGroupSize[(int)adr].GetValue(),
                        LastGroupID, mission.CoalitionPlayer, selNode.Value.Coordinates);

                    if (uGroup == null)
                    {
                        DebugLog.Instance.Log($"WARNING: failed to generate air defense of type {unitFamily} for coalition {mission.CoalitionPlayer}.");
                        continue;
                    }

                    uGroup.Name = GetGroupName(UnitFamily.VehicleSAMMedium);

                    if (uGroup.UnitCount == 0) continue;

                    mission.UnitGroups.Add(uGroup);
                    LastGroupID++;
                }
            }

            DebugLog.Instance.Log();
        }

        public void AddCombatAirPatrolUnits(DCSMission mission, MissionTemplate template, DefinitionTheater theater, DefinitionCoalition[] coalitions, DefinitionTheaterAirbase missionAirbase,
            out AmountNR selectedFriendlyCAP, out AmountNR selectedEnemyCAP)
        {
            DebugLog.Instance.Log("Generating combat air patrols...");
            selectedFriendlyCAP = AmountNR.None; selectedEnemyCAP = AmountNR.None;

            for (int i = 0; i < 2; i++)
            {
                bool friendly = (i == (int)template.ContextPlayerCoalition);

                // Select a enemy CAP intensity if set to Random
                AmountNR capSetting;
                if (friendly)
                {
                    selectedFriendlyCAP = HQTools.ResolveRandomAmount(template.SituationFriendlyCAP);
                    capSetting = selectedFriendlyCAP;
                }
                else
                {
                    selectedEnemyCAP = HQTools.ResolveRandomAmount(template.SituationEnemyCAP);
                    capSetting = selectedEnemyCAP;
                }

                DebugLog.Instance.Log($"  CAP for coalition {(Coalition)i} set to {capSetting.ToString().ToUpperInvariant()}");

                // Get the proper number of aircraft according to Setting.ini
                int capAircraftCount = Library.Instance.Common.CAPCount[(int)capSetting].GetValue();
                if (capAircraftCount <= 0) return; // No aircraft

                string[] availableFighterUnits = coalitions[i].GetUnits(mission.TimePeriod, UnitFamily.PlaneFighter, true, false);

                if (availableFighterUnits.Length == 0)
                {
                    DebugLog.Instance.Log($"  WARNING: No fighters or interceptors found for coalition {(Coalition)i}, could not generate combat air patrols.");
                    continue;
                }

                while (capAircraftCount > 0)
                {
                    List<string> groupUnits = new List<string>();

                    string unit = HQTools.RandomFrom(availableFighterUnits);
                    DefinitionUnit aircraftDefinition = Library.Instance.GetDefinition<DefinitionUnit>(unit);
                    if (aircraftDefinition == null) { capAircraftCount--; continue; }

                    do
                    {
                        groupUnits.Add(unit);
                        capAircraftCount--;
                        if (capAircraftCount <= 0) break;
                        if (groupUnits.Count >= HQTools.RandomFrom(2, 2, 3, 4, 4, 4)) break; // Group is full, stop here
                    } while (true);

                    if (groupUnits.Count == 0) { capAircraftCount--; continue; }

                    DefinitionTheaterSpawnPoint? spawnPoint;
                    if (friendly)
                        // Friendly CAP: select any nodes (aircraft can be spawned anywhere, even over water) located close enough to the
                        // players starting airbase (see HQLibrary.Common.EnemyCAPDistance)
                        spawnPoint = theater.GetRandomSpawnPoint(
                            null, null,
                            new MinMaxD(0, 1), missionAirbase.Coordinates);
                    else
                        // Enemy CAP: select any nodes (aircraft can be spawned anywhere, even over water) located far enough from the
                        // players starting airbase and neither too far or too near of the objective (see HQLibrary.Common.EnemyCAPDistance)
                        spawnPoint = theater.GetRandomSpawnPoint(
                            null, null,
                            new MinMaxD(Library.Instance.Common.EnemyCAPDistance.MinDistanceFromTakeOffLocation, double.MaxValue), missionAirbase.Coordinates,
                            Library.Instance.Common.EnemyCAPDistance.DistanceFromObjective, HQTools.RandomFrom(mission.Objectives).Coordinates);

                    if (!spawnPoint.HasValue) { capAircraftCount--; continue; }

                    DCSMissionUnitGroup unitGroup = new DCSMissionUnitGroup(
                        friendly ? "GroupAircraftCAPFriendly" : "GroupAircraftCAPEnemy",
                        "UnitAircraft",
                        UnitCategory.Plane, LastGroupID, (Coalition)i, spawnPoint.Value.Coordinates,
                        groupUnits.ToArray());


                    SetupAircraftGroup(unitGroup, mission, CallsignFamily.Aircraft, friendly, aircraftDefinition, PlayerFlightGroupPayloadType.A2A);
                    unitGroup.CustomValues.Add("LateActivation", "false"); // TODO: random chance
                    unitGroup.CustomValues.Add("ParkingID", "0"); // TODO: should not be used, remove from Lua file

                    mission.UnitGroups.Add(unitGroup);
                    LastGroupID++;
                }
            }

            DebugLog.Instance.Log();
        }

        /// <summary>
        /// Adds friendly AWACS and tanker aircraft to the mission, if required and available.
        /// </summary>
        /// <param name="mission">Mission to add the aircraft to</param>
        /// <param name="template">Mission template</param>
        /// <param name="playerCoalition">Definition of the player's coalition</param>
        /// <param name="theater">Definition of the mission's theater</param>
        /// <param name="missionAirbase">Definition of the mission's starting airbase</param>
        public void AddFriendlySupportAircraft(DCSMission mission, MissionTemplate template, DefinitionCoalition playerCoalition, DefinitionTheater theater, DefinitionTheaterAirbase missionAirbase)
        {
            DebugLog.Instance.Log("Generating friendly support aircraft...");

            UnitFamily[] familiesToGenerate = new UnitFamily[] { UnitFamily.PlaneAWACS, UnitFamily.PlaneTankerBasket, UnitFamily.PlaneTankerBoom };

            foreach (UnitFamily family in familiesToGenerate)
            {
                // Unit of this family is not required, continue
                if (((family == UnitFamily.PlaneAWACS) && !template.FlightPackageSupportAWACS) ||
                    ((family != UnitFamily.PlaneAWACS) && !template.FlightPackageSupportTanker))
                    continue;

                string unitLua = (family == UnitFamily.PlaneAWACS) ? "GroupPlaneAWACS" : "GroupPlaneTanker";

                DefinitionTheaterSpawnPoint? sp = theater.GetRandomSpawnPoint(null, null, new MinMaxD(20, 40) * HQTools.NM_TO_METERS, mission.MapCenter);
                if (!sp.HasValue)
                {
                    DebugLog.Instance.Log($"WARNING: failed to find a spawn point for friendly {family} aircraft.");
                    continue;
                }

                Coordinates oval1 = mission.MapCenter + Coordinates.CreateRandomInaccuracy(20, 50) * HQTools.NM_TO_METERS;
                Coordinates oval2 = oval1 + Coordinates.CreateRandomInaccuracy(20, 50) * HQTools.NM_TO_METERS;

                DCSMissionUnitGroup unitGroup =
                    DCSMissionUnitGroup.FromCoalitionArmyAndUnitFamily(
                        unitLua, "UnitAircraft",
                        playerCoalition, template.ContextTimePeriod, family,
                        1, LastGroupID, template.ContextPlayerCoalition, oval1);

                unitGroup.CustomValues.Add("X2", HQTools.ValToString(oval2.X));
                unitGroup.CustomValues.Add("Y2", HQTools.ValToString(oval2.Y));

                if (unitGroup == null)
                    DebugLog.Instance.Log($"  Failed to generate {family} aircraft for {template.ContextPlayerCoalition} coalition.");

                SetupAircraftGroup(unitGroup, mission, (family == UnitFamily.PlaneAWACS) ? CallsignFamily.AWACS : CallsignFamily.Tanker, true);

                mission.UnitGroups.Add(unitGroup);
                LastGroupID++;
            }

            DebugLog.Instance.Log();
        }

        /// <summary>
        /// Sets the various parameters and special values required for aircraft (fixed-wing and helicopters) unit groups.
        /// </summary>
        /// <param name="uGroup">The unit group to setup.</param>
        /// <param name="mission">The mission.</param>
        /// <param name="csFamily">The callsign family to use for this group.</param>
        /// <param name="isFriendly">Does this group belong to the player's coalition?</param>
        /// <param name="aircraftDefinition">The definition of the aircraft type. If none is provided (null), the method will try to get one from the unit type name.</param>
        /// <param name="payload">The type of payload to use.</param>
        /// <param name="airdromeID">The airdrome this aircraft group is linked to.</param>
        /// <returns>True if everything went right, false is something went wrong.</returns>
        private bool SetupAircraftGroup(DCSMissionUnitGroup uGroup, DCSMission mission, CallsignFamily csFamily, bool isFriendly, DefinitionUnit aircraftDefinition = null, PlayerFlightGroupPayloadType payload = PlayerFlightGroupPayloadType.Default, int airdromeID = 0)
        {
            if (uGroup.UnitCount == 0) return false;

            if (aircraftDefinition == null)
                aircraftDefinition = Library.Instance.GetDefinition<DefinitionUnit>(uGroup.Units[0]);

            if (aircraftDefinition == null) return false;

            MGCallsign cs = CSGenerator.GetCallsign(csFamily, isFriendly ? mission.CoalitionPlayer : mission.CoalitionEnemy);
            uGroup.Name = cs.GroupName;
            uGroup.CustomValues.Add("Altitude", HQTools.ValToString(aircraftDefinition.AircraftCruiseAltitude));
            uGroup.CustomValues.Add("Callsign", cs.Lua);
            uGroup.CustomValues.Add("Payload", aircraftDefinition.GetPayloadLua(payload));
            uGroup.CustomValues.Add("Speed", HQTools.ValToString(aircraftDefinition.AircraftCruiseSpeed));
            uGroup.RadioFrequency = aircraftDefinition.CommsRadioFrequency;

            for (int i = 0; i < uGroup.UnitCount; i++)
            {
                uGroup.CustomValues.Add(new DCSMissionUnitGroupCustomValueKey("OnBoardNum", i), HQTools.ValToString(GetOnboardNumber()));
                if (airdromeID != -1)
                    uGroup.CustomValues.Add(new DCSMissionUnitGroupCustomValueKey("ParkingID", i), HQTools.ValToString(GetParkingNumber(airdromeID)));
            }

            return true;
        }

        //public void GenerateAIEscortFlightGroups(
        //    HQMission mission, MissionTemplate template,
        //    DefinitionCoalition[] coalitions,
        //    int count, UnitFamily family, string luaGroup, AircraftPayloadType payload, DCSAircraftTask task)
        //{
        //    HQDebugLog.Instance.Log($"Generating AI escort flight groups ({task.ToString()})...");

        //    int[] groupsCount = CreateGroupsFromAircraftCount(count);

        //    foreach (int gCount in groupsCount)
        //    {
        //        HQMissionUnitGroup uGroup = HQMissionUnitGroup.FromCoalitionArmyAndUnitFamily(
        //            Library,
        //            luaGroup, "unitAircraft",
        //            coalitions[(int)mission.CoalitionPlayer], mission.TimePeriod,
        //            family, gCount, GroupID, mission.CoalitionPlayer,
        //            mission.Airbases[0].Coordinates + Coordinates.CreateRandomInaccuracy(1000, 10000));

        //        if (uGroup.UnitCount == 0) continue; // No unit, proceed

        //        DefinitionUnit aircraftDefinition =
        //            (from d in Library.GetAllDefinitions<DefinitionUnit>() where d.ID == uGroup.Units[0] select d).FirstOrDefault();
        //        if (aircraftDefinition == null) continue; // Aircraft definition doesn't exist

        //        SetupAircraftGroup(
        //            uGroup, mission, CallsignFamily.Aircraft, true,
        //            aircraftDefinition, payload, mission.Airbases[0].DCSID);

        //        mission.BriefingFlightPackage.Add(new HQMissionBriefingFlightGroup(uGroup.Name, uGroup.Units[0], task, uGroup.UnitCount));

        //        GroupID++;
        //        TotalAAValue += aircraftDefinition.AircraftAirToAirRating[(payload == AircraftPayloadType.A2A) ? 0 : 1];
        //    }

        //    HQDebugLog.Instance.Log();
        //}

        // FIXME: write a more generic "add flight groups" option

        //public void GenerateAISupportFlightGroups(
        //    HQMission mission, MissionTemplate template,
        //    DefinitionCoalition[] coalitions,
        //    UnitFamily family, string luaGroup,
        //    CallsignFamily csFamily, DCSAircraftTask task)
        //{
        //    HQDebugLog.Instance.Log($"Generating AI support flight groups ({family.ToString()})...");

        //    // Set spawning position near the departure airbase
        //    Coordinates position = mission.Airbases[0].Coordinates + Coordinates.CreateRandomInaccuracy(1000, 5000);

        //    // If there are objectives, move the position to 15% to 30% of the distance between the starting position and the center of all objectives
        //    if (mission.Objectives.Count > 0)
        //        position = Coordinates.Lerp(position, mission.ObjectivesCenterPoint, HQTools.RandomDouble(0.15, 0.3));

        //    // Create a second waypoint 5 to 10 Km away from the original one to allow "racetrack" orbiting.
        //    Coordinates position2 = position + Coordinates.CreateRandomInaccuracy(5000, 10000);

        //    HQMissionUnitGroup uGroup = HQMissionUnitGroup.FromCoalitionArmyAndUnitFamily(
        //        Library,
        //        luaGroup, "UnitAircraft",
        //        coalitions[(int)mission.CoalitionPlayer], mission.TimePeriod,
        //        family, 1, GroupID, mission.CoalitionPlayer, position);

        //    if ((uGroup == null) || (uGroup.UnitCount == 0)) return; // No unit, proceed

        //    DefinitionUnit aircraftDefinition =
        //        (from d in Library.GetAllDefinitions<DefinitionUnit>() where d.DCSID == uGroup.Units[0] select d).FirstOrDefault();
        //    if (aircraftDefinition == null) // Aircraft definition doesn't exist, proceed
        //    { HQDebugLog.Instance.Log($"    WARNING: cannot find aircraft definition \"{uGroup.Units[0]}\""); return; }
        //    uGroup.CustomValues.Add("X2", HQTools.ValToString(position2.X));
        //    uGroup.CustomValues.Add("Y2", HQTools.ValToString(position2.Y));
        //    ////uGroup.RadioFrequency = coalitions[(int)template.PlayerCoalition].TankerFrequency;

        //    SetupAircraftGroup(uGroup, mission, csFamily, true, aircraftDefinition, AircraftPayloadType.Default, mission.Airbases[0].DCSID);

        //    mission.UnitGroups.Add(uGroup);
        //    GroupID++;

        //    string tacan = null;
        //    //if (task == DCSAircraftTask.Tanker) tacan = TANKER_TACAN; // FIXME

        //    mission.BriefingFlightPackage.Add(new HQMissionBriefingFlightGroup(uGroup.Name, uGroup.Units[0], task, 1, uGroup.RadioFrequency, tacan));

        //    HQDebugLog.Instance.Log();
        //}

        private int GetParkingNumber(int airdromeID)
        {
            if (!AirdromeParkingNumber.ContainsKey(airdromeID))
                AirdromeParkingNumber[airdromeID] = 1;
            else
                AirdromeParkingNumber[airdromeID]++;

            return AirdromeParkingNumber[airdromeID];
        }

        private int GetOnboardNumber()
        {
            int number;

            do
            {
                number = HQTools.RandomInt(111, 999);
            } while (UsedOnboardNumbers.Contains(number));

            return number;
        }

        private string GetGroupName(UnitFamily family)
        {
            int randomFakeGroupNameNumber;

            if (UsedFakeGroupNamesNumbers.Count > 250) UsedFakeGroupNamesNumbers.Clear();
            do { randomFakeGroupNameNumber = HQTools.RandomInt(1, 500); }
            while (UsedFakeGroupNamesNumbers.Contains(randomFakeGroupNameNumber));

            string nameTemplate;

            switch (family)
            {
                case UnitFamily.VehicleSAMLong:
                case UnitFamily.VehicleSAMMedium:
                case UnitFamily.VehicleSAMShort:
                case UnitFamily.VehicleSAMShortIR:
                case UnitFamily.VehicleAAA:
                    nameTemplate = Language.GetStringRandom("GroupNames", "AirDefense"); break;
                case UnitFamily.VehicleAPC:
                case UnitFamily.VehicleMBT:
                    nameTemplate = Language.GetStringRandom("GroupNames", "Armor"); break;
                case UnitFamily.VehicleArtillery:
                case UnitFamily.VehicleMissile:
                    nameTemplate = Language.GetStringRandom("GroupNames", "Artillery"); break;
                case UnitFamily.PlaneStrike:
                default:
                    nameTemplate = Language.GetStringRandom("GroupNames", "Default"); break;
                case UnitFamily.VehicleInfantry:
                case UnitFamily.VehicleInfantryMANPADS:
                    nameTemplate = Language.GetStringRandom("GroupNames", "Infantry"); break;
                case UnitFamily.VehicleCommand:
                case UnitFamily.VehicleTransport:
                    nameTemplate = Language.GetStringRandom("GroupNames", "Unarmed"); break;
            }

            nameTemplate = nameTemplate.Replace("$N$", HQTools.ValToString(randomFakeGroupNameNumber));
            nameTemplate = nameTemplate.Replace("$NTH$", Language.GetOrdinalAdjective(randomFakeGroupNameNumber));

            return nameTemplate;
        }

        private int[] CreateGroupsFromAircraftCount(int aircraftCount)
        {
            List<int> acGroups = new List<int>();

            while (aircraftCount > 0)
            {
                if (acGroups.Count == 0) acGroups.Add(0);
                if ((acGroups.Last() == 4) ||
                    ((acGroups.Last() == 3) && HQTools.RandomChance(4)) ||
                    ((acGroups.Last() == 2) && HQTools.RandomChance(3)))
                    acGroups.Add(1);
                else
                    acGroups[acGroups.Count - 1]++;

                aircraftCount--;
            }

            return acGroups.ToArray();
        }

        private DCSSkillLevel HQSkillToDCSSkill(HQSkillLevel aiSkill)
        {
            switch (aiSkill)
            {
                case HQSkillLevel.Average: return DCSSkillLevel.Average;
                case HQSkillLevel.Good: return DCSSkillLevel.Good;
                case HQSkillLevel.High: return DCSSkillLevel.High;
                case HQSkillLevel.Excellent: return DCSSkillLevel.Excellent;
            }

            return HQTools.RandomFrom(new DCSSkillLevel[] { DCSSkillLevel.Average, DCSSkillLevel.Good, DCSSkillLevel.High, DCSSkillLevel.Excellent });
        }
    }
}
