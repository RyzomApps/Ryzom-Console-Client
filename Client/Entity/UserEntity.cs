///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Numerics;
using API;
using API.Entity;
using Client.Messages;
using Client.Network;
using Client.Stream;

namespace Client.Entity
{
    public class UserEntity : Entity, IUserEntity
    {
        private byte _selection;

        /// <summary>
        /// Ryzom Client
        /// </summary>
        private IClient _client;

        public UserEntity(IClient client)
        {
            _client = client;
        }

        /// <inheritdoc />
        public void Selection(byte slot)
        {
            //allows reselection in Ring client: even if the selected slots is equal to the selection,
            //the client must send the messages.
            if (_selection == slot)
            {
                return;
            }

            // The selection will be the entity to watch
            //WatchedEntitySlot = slot;
            //disableFollow();

            // Send the entity selected to the server.
            _client.GetApiNetworkManager().PushTarget(slot);

            // Target the slot on client, don't wait NetWork response
            //targetSlot(slot);
            //_TargetSlotNoLag = slot;
            //
            //if (ClientCfg.R2EDEnabled)
            //{
            //	R2.getEditor().inGameSelection(slot);
            //}


            // Change the current selection so un color the current selection.
            //CEntityCL sel = EntitiesMngr.entity(_Selection);
            //if (sel != null)
            //{
            //	sel.visualSelectionStop(); // Blink off == restore to normal
            //}

            // Set the entity selected
            _selection = slot;

            // Update visual selection and interface
            //if (sel != null && sel.isForageSource())
            //{
            //	sel.buildInSceneInterface(); // remove focus on previous selected source
            //}
            //sel = EntitiesMngr.entity(_Selection);
            //if (sel != null)
            //{
            //	sel.visualSelectionStart();
            //	if (sel.isForageSource())
            //	{
            //		sel.buildInSceneInterface(); // set focus on new selected source
            //	}
            //}


            // **** Update Target interface
            //get the new target slot and set it in the database
            //CInterfaceManager pIM = CInterfaceManager.getInstance();
            //NLGUI.CDBManager.getInstance().getDbProp("UI:VARIABLES:TARGET:SLOT").setValue64(slot);

            // Get the new target UID, and set in Database
            //uint tgtSlot = _Selection;
            //uint tgtEntityId = CLFECOMMON.INVALID_CLIENT_DATASET_INDEX;
            //CEntityCL entity = null;
            //if (tgtSlot != CLFECOMMON.INVALID_SLOT)
            //{
            //	entity = EntitiesMngr.entity(tgtSlot);
            //	if (entity != null)
            //	{
            //		tgtEntityId = entity.dataSetId();
            //	}
            //}
            //
            //// Set the User Target
            //CCDBNodeLeaf prop = NLGUI.CDBManager.getInstance().getDbProp("UI:VARIABLES:TARGET:UID", false);
            //if (prop != null)
            //{
            //	prop.setValue32(tgtEntityId);
            //}
            //
            //// Bar Manager. Update now the Target View (so it takes VP if data available or 0... but result is immediate)
            //CBarManager.getInstance().setLocalTarget(tgtEntityId);

            // **** Update DB for InGameMenu
            // clear the entries for mission option
            //for (uint k = 0; k < NUM_MISSION_OPTIONS; ++k)
            //{
            //    CCDBNodeLeaf missionOption = NLGUI.CDBManager.getInstance().getDbProp(toString("LOCAL:TARGET:CONTEXT_MENU:MISSIONS_OPTIONS:%d:TITLE", (int)k), false);
            //    if (missionOption != null)
            //    {
            //        missionOption.setValue32(0);
            //    }
            //    CCDBNodeLeaf playerGiftNeeded = NLGUI.CDBManager.getInstance().getDbProp(toString("LOCAL:TARGET:CONTEXT_MENU:MISSIONS_OPTIONS:%d:PLAYER_GIFT_NEEDED", (int)k), false);
            //    if (playerGiftNeeded != null)
            //    {
            //        playerGiftNeeded.setValue32(0);
            //    }
            //}
            // TODO ULU : Add RP tags
            //
            //// update pvp tags
            //if ((tgtSlot != CLFECOMMON.INVALID_SLOT) && entity != null)
            //{
            //    CPlayerCL pPlayer = entity as CPlayerCL;
            //
            //    if (pPlayer != null)
            //    {
            //        /*// Pvp Mode
            //        CViewBitmap * tagMode = dynamic_cast<CViewBitmap*>(CWidgetManager::getInstance()->getElementFromId("ui:interface:target:pvp_tags:mode"));
            //        if (tagMode)
            //        {
            //            if (pPlayer->getPvpMode()&PVP_MODE::PvpFaction)
            //                tagMode->setTexture("pvp_orange.tga");
            //            else if (pPlayer->getPvpMode()&PVP_MODE::PvpFactionFlagged)
            //                tagMode->setTexture("pvp_red.tga");
            //            else
            //                tagMode->setTexture("alpha_10.tga");
            //        }
            //*/
            //        /*// Pvp available actions (attack, heal, both)
            //        CViewBitmap * tagMode = dynamic_cast<CViewBitmap*>(CWidgetManager::getInstance()->getElementFromId("ui:interface:target:pvp_tags:actions"));
            //        if (tagMode)
            //        {
            //            if (pPlayer->getPvpMode()&PVP_MODE::PvpFaction)
            //                tag->setTexture("pvp_orange.tga");
            //            else if (pPlayer->getPvpMode()&PVP_MODE::PvpFactionFlagged)
            //                tag->setTexture("pvp_red.tga");
            //            else
            //                tag->setTexture("alpha_10.tga");
            //        }*/
            //
            //    }
            //}
            //
            //// clear web page
            //prop = NLGUI.CDBManager.getInstance().getDbProp("LOCAL:TARGET:CONTEXT_MENU:WEB_PAGE_URL", false);
            //if (prop != null)
            //{
            //    prop.setValue32(0);
            //}
            //prop = NLGUI.CDBManager.getInstance().getDbProp("LOCAL:TARGET:CONTEXT_MENU:WEB_PAGE_TITLE", false);
            //if (prop != null)
            //{
            //    prop.setValue32(0);
            //}
            //
            //// clear mission ring
            //for (uint k = 0; k < BOTCHATTYPE.MaxR2MissionEntryDatabase; ++k)
            //{
            //    prop = NLGUI.CDBManager.getInstance().getDbProp(toString("LOCAL:TARGET:CONTEXT_MENU:MISSION_RING:%d:TITLE", k), false);
            //    if (prop != null)
            //    {
            //        prop.setValue32(0);
            //    }
            //}
            //
            //// clear programs
            //prop = NLGUI.CDBManager.getInstance().getDbProp("LOCAL:TARGET:CONTEXT_MENU:PROGRAMMES", false);
            //if (prop != null)
            //{
            //    prop.setValue32(0);
            //}
            //prop = NLGUI.CDBManager.getInstance().getDbProp("SERVER:TARGET:CONTEXT_MENU:PROGRAMMES");
            //if (prop != null)
            //{
            //    prop.setValue32(0);
            //}
            //// increment db counter for context menu
            //pIM.incLocalSyncActionCounter();
        }

        public void Attack()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Angle in the world to the front vector
        /// </summary>
        private float FrontYaw() { return (float)Math.Atan2(Front.Y, Front.X); }

        /// <summary>
        /// Send the position and orientation to the server.
        /// </summary>
        public bool SendToServer(BitMemoryStream out2, GenericMessageHeaderManager genericMessageHeaderManager, IClient client)
        {
            if (!genericMessageHeaderManager.PushNameToStream("POSITION", out2))
                throw new Exception("UE:sendToServer: unknown message named 'POSITION'.");

            // Backup the position sent.
            //if (_Primitive) _Primitive->getGlobalPosition(_LastGPosSent, dynamicWI);

            // Send Position & Orientation
            //CPositionMsg positionMsg;

            // hack for CPositionMsg
            var x = (int)(Pos.X * 1000);
            var y = (int)(Pos.Y * 1000);
            var z = (int)(Pos.Z * 1000);

            var heading = FrontYaw();

            out2.Serial(ref x);
            out2.Serial(ref y);
            out2.Serial(ref z);
            out2.Serial(ref heading);

            return true;
        }

        /// <summary>
        /// the entity target in the slot become the target of your current one.
        /// </summary>
        public void Assist(byte slot)
        {
            // Check the current target
            if (slot == Constants.InvalidSlot || slot == Slot())
            {
                return;
            }

            // Check the target
            var target = _client.GetApiNetworkManager().GetApiEntityManager().GetApiEntities()[slot];

            if (target == null)
            {
                return;
            }

            // Check the new slot.
            var newSlot = target.TargetSlot();

            if (newSlot == Constants.InvalidSlot || newSlot == Slot())
            {
                return;
            }

            // Select the new target.
            Selection(newSlot);
        }

        //private static bool wellPosition = false;
        //private bool wellP = false;

        public bool MsgForCombatPos(BitMemoryStream out2, GenericMessageHeaderManager genericMessageHeaderManager)
        {
            //// Is the user well Placed
            //CEntityCL target = EntitiesMngr.entity(TargetSlot());
            //
            //if (target)
            //
            //    wellP = target.isPlacedToFight(Pos(), Front(), AttackRadius() + ClientCfg.AttackDist);
            //
            //
            //if (wellPosition != wellP)
            //{
            //    wellPosition = wellP;
            //
            //    // Send state to server.
            //    if (genericMessageHeaderManager.PushNameToStream("COMBAT:VALIDATE_MELEE", out2))
            //    {
            //        byte flag = (byte)(wellP ? 1 : 0);
            //        out2.Serial(ref flag);
            //        return true;
            //    }
            //    else
            //    {
            //        throw new Exception("UE:msgForCombatPos: unknown message named 'COMBAT:TOGGLE_COMBAT_FLOAT_MODE'.");
            //    }
            //}

            // Do not send the msg.
            return false;
        }

        public void CorrectPos(Vector3 dest)
        {
            Debug.Print($"UE:correctPos: new user position {dest.X} {dest.Y} {dest.Z}");

            Pos = dest;

            // Change the user poisition.
            //PacsPos(dest);
            //// Update the primitive useful to check the user position.
            //_LastPositionValidated = dest;
            //// Get the Global position
            //if (_Primitive)
            //{
            //    // this is the last PACS position validated too
            //    _Primitive.getGlobalPosition(_LastGPosValidated, dynamicWI);
            //    // consider this is the last position sent to server (since actually received!)
            //    _Primitive.getGlobalPosition(_LastGPosSent, dynamicWI);
            //    // Set the new position of the 'check' primitive
            //    if (_CheckPrimitive)
            //    {
            //        _CheckPrimitive.setGlobalPosition(_LastGPosSent, dynamicWI);
            //    }
            //}
            //else
            //{
            //    nlwarning("UE:correctPos: the entity has a Null primitive.");
            //}
            //
            //// Correct the pos of the mount, if riding
            //if (isRiding())
            //{
            //    if (_Mount < EntitiesMngr.entities().size())
            //    {
            //        CEntityCL mount = EntitiesMngr.entities()[_Mount];
            //        if (mount != null)
            //        {
            //            mount.pacsPos(dest);
            //        }
            //    }
            //}

        }
    }
}
