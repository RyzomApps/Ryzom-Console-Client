﻿using System;
using System.Collections.Generic;
using API;
using API.Commands;
using Client.Entity;

namespace Client.Commands
{
    public class Assist : CommandBase
    {
        public override IEnumerable<string> GetCmdAliases() { return new string[] { "as" }; }
        public override string CmdName => "assist";
        public override string CmdUsage => "[<name>]";
        public override string CmdDesc => "Targets the target of the targeted entity.";

        public override string Run(IClient handler, string command, Dictionary<string, object> localVars)
        {
            if (!(handler is RyzomClient ryzomClient))
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);

            string entityName = "";

            if (args.Length != 0)
            {
                entityName = string.Join(" ", args);
            }

            var entityManager = ryzomClient.GetNetworkManager().GetEntityManager();

            Entity.Entity entity;
            var user = entityManager.UserEntity;
            if (user == null)
                return "";

            if (entityName != string.Empty)
            {
                entity = entityManager.GetEntityByName(entityName, false, false);
            }
            else
            {
                entity = entityManager.GetEntity(((UserEntity)user).TargetSlot());
            }

            if (entity != null)
            {
                // Select the entity
                ((UserEntity)user).Assist(entity.Slot(), ryzomClient);
            }
            else
            {
                handler.GetLogger().Warn("Entity not found.");
            }

            return "";
        }
    }
}