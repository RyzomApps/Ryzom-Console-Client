///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace Client.Property
{
    /// <summary>
    /// These properties belong to players, npc
    /// Note: due to some hardcoding in client interface scripts, DO NOT change these values.
    /// </summary>
    public enum PropertyType : byte
    {
        // main root
        Position = 0,
        PositionY = 1,
        PositionZ = 2,

        Orientation = 3,

        // discrete root
        Sheet = 4,
        Behaviour = 5,
        NameStringID = 6,
        TargetID = 7,
        Mode = 8,
        Vpa = 9,
        Vpb = 10,
        Vpc = 11,
        EntityMountedID = 12,
        RiderEntityID = 13,
        Contextual = 14,
        Bars = 15,
        TargetList = 16,
        GuildSymbol = 20,
        GuildNameID = 21,
        VisualFx = 22,
        EventFactionID = 23,
        PvpMode = 24,
        PvpClan = 25,
        OwnerPeople = 26,
        OutpostInfos = 27,

        /*
        * DO LEAVE ENOUGH ROOM FOR FUTURE PROPERTIES !
        */
        AddNewEntity = 32,
        RemoveOldEntity,
        ConnectionReady,
        LagDetected,
        ProbeReceived,

        InvalidPropIndex = 0xFF,
    }
}