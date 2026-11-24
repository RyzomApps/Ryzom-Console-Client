///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using API.Entity;
using API.Sheet;
using Client.Database;
using Client.Forage;
using Client.Network;
using Client.Property;
using Client.Strings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading;
using StringWait = Client.Commands.StringWait;

namespace Client.Entity
{
    /// <summary>
    /// Class to manage entities and shapes instances.
    /// </summary>
    /// <author>Guillaume PUZIN</author>
    /// <author>Nevrax France</author>
    /// <date>2001</date>
    public class EntityManager(RyzomClient client) : IEntityManager
    {
        private uint _nbMaxEntity;

        public UserEntity UserEntity { get; set; }

        /// <inheritdoc />
        public IUserEntity GetApiUserEntity() => UserEntity;

        // Contain all entities.
        private Entity[] _entities;

        private readonly Dictionary<uint, Dictionary<uint, Property>> _backupedChanges = new();

        /// <inheritdoc />
        public IEntity[] GetApiEntities() => _entities;

        /// <summary>
        /// Initialize some dynamic parameters.
        /// </summary>
        /// <param name="nbMaxEntity">uint : maximum number of entities allocated</param> 
        public void Initialize(uint nbMaxEntity)
        {
            // Set the maximum number of entities.
            _nbMaxEntity = nbMaxEntity;

            _entities = new Entity[_nbMaxEntity];

            // TODO: Add an observer on the mission database
            // TODO: Add an Observer to the Team database
            // TODO: Add an Observer to the Animal database
        }

        /// <summary>
        /// Create an entity according to the slot and the form.
        /// </summary>
        /// <param name="slot">uint : slot for the entity</param> 
        /// <param name="form">uint32 : form to create the entity</param>
        /// <param name="newEntityInfo">dataset</param>
        /// <returns>CEntityCL : pointer on the new entity</returns> 
        public Entity Create(in byte slot, uint form, PropertyChange.TNewEntityInfo newEntityInfo)
        {
            var sheetId = client.GetSheetIdFactory().SheetId(form);

            // DEBUG
            client.GetLogger().Debug($"(_,{client.GetNetworkManager().GetCurrentServerTick()}) EM:create: slot '{slot}': {sheetId}");

            if (slot >= _nbMaxEntity)
            {
                client.GetLogger().Warn($"EM:create: Cannot create the entity, the slot '{slot}' is invalid.");
                return null;
            }

            // Slot 0 is for the user and so should be allocated only once (at beginning of main loop).
            if (slot == 0 && _entities[0] != null)
            {
                if (newEntityInfo.DataSetIndex != Constants.InvalidClientDatasetIndex)
                {
                    // Store the dataSetId received
                    _entities[0].DataSetId(newEntityInfo.DataSetIndex);
                }

                // Store the alias (although there should not be one for the slot 0!)
                _entities[0].NpcAlias(newEntityInfo.Alias);

                return null;
            }

            // Remove the old one (except the user).
            if (slot != 0 && _entities[slot] != null)
            {
                Remove(slot, false);
            }

            // Check parameter: form
            var entitySheet = client.GetSheetManager().Get(sheetId);

            if (entitySheet == null)
            {
                client.GetLogger().Warn($"EM:create: Attempt on create an entity with a bad form number {form} ({sheetId}) for the slot '{slot}' trying to compute the default one.");

                if (slot != 0)
                {
                    _entities[slot] = new Entity(client);
                    return null;
                }
                else
                {
                    UserEntity = new UserEntity(client) { Pos = client.GetNetworkConnection().GetPropertyDecoder().GetReferencePosition() };
                    _entities[slot] = UserEntity;
                    return null;
                }
            }

            // Create the entity according to the type.
            // TODO: Create the right classes here

            switch (entitySheet.Type)
            {
                case SheetType.RACE_STATS:
                case SheetType.CHAR:
                    if (slot == 0)
                    {
                        UserEntity = new UserEntity(client) { Pos = client.GetNetworkConnection().GetPropertyDecoder().GetReferencePosition() };
                        _entities[slot] = UserEntity;
                    }
                    else
                    {
                        _entities[slot] = new PlayerEntity(client);
                    }

                    break;

                case SheetType.FAUNA:
                    //if (entitySheet is CharacterSheet { R2Npc: false })
                    _entities[slot] = new CharacterEntity(client);
                    //else
                    // CPlayerR2CL
                    //_entities[slot] = new PlayerEntity(_client) { Type = EntityType.NPC };
                    break;

                case SheetType.FLORA:
                    _entities[slot] = new CharacterEntity(client);
                    break;

                case SheetType.FX:
                    // TODO: _entities[slot] = new CFxCL;
                    _entities[slot] = new Entity(client);
                    break;

                case SheetType.ITEM:
                    // TODO: _entities[slot] = new CItemCL;
                    _entities[slot] = new Entity(client);
                    break;

                case SheetType.FORAGE_SOURCE:
                    _entities[slot] = new ForageSourceEntity(client);
                    break;

                default:
                    client.GetLogger().Warn($"Unknown Form Type '{entitySheet.Type}' -> entity not created.");
                    return null;
            }

            // If the entity has been right created.
            if (_entities[slot] != null)
            {
                // Set the sheet Id.
                _entities[slot].SheetId(form);

                // Set the slot.
                _entities[slot].SetSlot(slot, client.GetDatabaseManager());

                // Set the DataSet Index. AFTER slot(), so bar manager is correctly init
                _entities[slot].DataSetId(newEntityInfo.DataSetIndex);

                // Set the Mission Giver Alias
                _entities[slot].NpcAlias(newEntityInfo.Alias);

                // Build the entity from a sheet.
                if (_entities[slot].Build((Sheet.Sheet)entitySheet, client))
                {
                    // Apply properties from backup
                    ApplyBackupProperties(slot);
                }
            }

            client.Plugins.OnEntityCreate(slot);

            return _entities[slot];
        }

        /// <summary>
        /// Delete an entity.
        /// </summary>
        /// <returns>bool : 'true' if the entity has been correctly removed</returns> 
        public bool Remove(in byte slot, bool warning)
        {
            if (warning)
                client.Plugins.OnEntityRemove(slot, warning);

            //_client.GetLogger().Info($"EntityManager.Remove({slot}, {warning})");

            // TODO: Implementation
            _entities[slot] = null;

            return true;
        }

        /// <summary>
        /// Method to update the visual property 'prop' for the entity in 'slot'.
        /// </summary>
        /// <param name="gameCycle">timestamp</param>
        /// <param name="slot">slot of the entity to update.</param> 
        /// <param name="prop">the property to udapte.</param>
        /// <param name="predictedInterval">prediction</param> 
        public void UpdateVisualProperty(uint gameCycle, byte slot, uint prop, uint predictedInterval)
        {
            // Check parameter : slot.
            if (slot >= _nbMaxEntity)
            {
                client.GetLogger().Warn($"CEntityManager::updateVisualProperty : Slot '{slot}' is not valid.");
                return;
            }

            // Entity still not allocated -> backup values received for the entity.
            if (_entities[slot] == null)
            {
                var propName = $"SERVER:Entities:E{slot}:P{prop}";
                var propty = new Property { GameCycle = gameCycle, Value = 0 };

                if (client.GetDatabaseManager() != null)
                    propty.Value = client.GetDatabaseManager().GetProp(propName);

                client.GetLogger().Debug($"EM:updateVP: backup the property {(PropertyType)prop} as long as the entity {slot} is not allocated.");

                // Entity does not have any changes backuped for the time.
                if (!_backupedChanges.ContainsKey(slot))
                {
                    _backupedChanges.Add(slot, new Dictionary<uint, Property> { { prop, propty } });
                }
                // Entity already have some changes backuped.
                else
                {
                    // This properties is still not backuped for this entity.
                    if (!_backupedChanges[slot].ContainsKey(prop))
                    {
                        _backupedChanges[slot].Add(prop, propty);
                    }
                    // There is already a backuped value
                    else
                    {
                        client.GetLogger().Debug($"EM:updateVP:{slot}: property '{prop}' already backuped.");
                        _backupedChanges[slot][prop] = propty;
                    }
                }
            }
            // Entity already allocated -> apply values.
            else
            {
                // Call the method from the entity to update the visual property.
                _entities[slot].UpdateVisualProperty(gameCycle, prop, predictedInterval, client);

                client.Plugins.OnEntityUpdateVisualProperty(gameCycle, slot, prop, predictedInterval);
            }
        }

        /// <summary>
        /// Writes the properties that were already received before the entity was created to the entity
        /// </summary>
        public void ApplyBackupProperties(uint slot)
        {
            if (!_backupedChanges.TryGetValue(slot, out var change))
                return;

            foreach (var (key, property) in change)
            {
                _entities[slot].UpdateVisualProperty(property.GameCycle, key, 0, client);

                client.Plugins.OnEntityUpdateVisualProperty(property.GameCycle, (byte)slot, key, 0);
            }

            _backupedChanges.Remove(slot);
        }

        /// <inheritdoc />
        public IEntity GetEntityByName(string name, bool caseSensitive, bool complete)
        {
            var minDistance = float.MaxValue;
            IEntity minEntity = null;

            foreach (var entity in _entities)
            {
                if (entity != null && entity.GetDisplayName().Contains(name, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
                {
                    if (UserEntity == null)
                    {
                        client.Log.Info($"Found entity '{name}' and user entity is null.");
                        return entity;
                    }

                    // Try to get the closest
                    var distance = Vector3.Distance(entity.Pos, UserEntity.Pos);

                    if (distance < minDistance && distance < 500)
                    {
                        minEntity = entity;
                        minDistance = distance;
                    }
                    else
                    {
                        client.Log.Debug($"Found entity '{name}' in a distance of {distance:0.0} m (User: {UserEntity.Pos} - Entity: {entity.Pos}).");
                    }
                }
            }

            return minEntity;
        }

        public Entity GetEntity(byte slot)
        {
            return _entities[slot];
        }

        /// <summary>
        /// Replace control characters and non-breaking spaces so the JSON stays valid.
        /// </summary>
        private static string SanitizeText(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";
            var sb = new System.Text.StringBuilder(s.Length);
            foreach (var c in s)
            {
                if (c == '\u00A0')
                    sb.Append(' ');           // NBSP -> space (comes from localized names)
                else if (char.IsControl(c))
                    sb.Append(' ');           // other control chars -> space
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Max time WarmUpNpcNames() spends pumping the network for the name texts.
        /// </summary>
        public const uint NpcNameWaitMs = 2000;

        /// <summary>
        /// Ask the string manager for the name and guild texts of the server NPCs in the
        /// vision, wait a short time for the answers and collect them, keyed by npc alias.
        /// A server NPC has no localizing key in its entity properties: its name and guild
        /// are plain string ids (PROPERTY_NAME_STRING_ID, PROPERTY_GUILD_NAME_ID) whose text
        /// the server only sends through the string manager (STRING_MANAGER:STRING_RQ/RESP).
        /// Without this request the cache holds nothing and the dump writes empty names.
        /// Dynamic string ids (top bit set) are never requested, the server pushes them by
        /// itself; only their cache is read.
        /// Blocking, must not be called from a network callback: it pumps the network
        /// reception itself, like StringWait.WaitAll() and ItemDetail() do.
        /// </summary>
        /// <param name="timeoutMs">max time to wait for the answers</param>
        /// <param name="onlyAliases">restrict to these npc aliases (null: all NPC entities
        /// in the vision). Ids are still taken from the entities, unknown aliases give nothing.</param>
        /// <returns>texts found for every npc alias of the vision, empty when unresolved.
        /// Aliases whose entity disappeared while pumping can still be in the result.</returns>
        public Dictionary<uint, (string Name, string Guild)> WarmUpNpcNames(uint timeoutMs = NpcNameWaitMs, ICollection<uint> onlyAliases = null)
        {
            var result = new Dictionary<uint, (string Name, string Guild)>();

            var stringManager = client?.GetStringManager();
            var networkManager = client?.GetNetworkManager();
            if (stringManager == null || networkManager == null)
                return result;

            // Alias -> name and guild string ids of the entities, 0 when unknown.
            // The name id comes from the entity when it has one, otherwise from its SERVER
            // database node (the visual property can have arrived before the entity did).
            var aliasNameId = new Dictionary<uint, uint>();
            var aliasGuildId = new Dictionary<uint, uint>();

            var entities = _entities;
            for (var slot = 1; slot < entities.Length; ++slot)
            {
                var entity = entities[slot];
                if (entity == null)
                    continue;

                // The alias is an optional block of the entity creation message: only the
                // NPCs the server marks with one have it, players and plain entities do not.
                var alias = entity.NpcAlias();
                if (alias == 0)
                    continue;

                if (onlyAliases != null && !onlyAliases.Contains(alias))
                    continue;

                uint nameId = entity._nameId;
                uint guildId = 0;

                // Fallbacks: the visual property of the slot can have arrived before the
                // entity object existed, and Entity._guildNameId is private, so the SERVER
                // database node is read in both cases.
                var slotNode = GetSlotDbNode((byte)slot);
                if (slotNode != null)
                {
                    if (nameId == 0)
                        nameId = ReadStringIdProp(slotNode, PropertyType.NameStringID);
                    guildId = ReadStringIdProp(slotNode, PropertyType.GuildNameID);
                }

                if (nameId != 0 && !aliasNameId.ContainsKey(alias))
                    aliasNameId[alias] = nameId;
                if (guildId != 0 && !aliasGuildId.ContainsKey(alias))
                    aliasGuildId[alias] = guildId;
            }

            // Text of a string id from the cache of the string manager, empty when
            // unresolved. A dyn id is only read, requesting it would be answered never.
            // A static id goes through GetString(), which is harmless: it queues a request
            // for an id that has none pending, and every static id of the vision was asked
            // for above.
            string LocalizedText(uint stringId)
            {
                if (stringId == 0)
                    return "";

                string text;

                if (StringWait.IsDynId(stringId))
                {
                    if (!stringManager.GetDynString(stringId, out var dynText, networkManager))
                        return "";
                    text = dynText;
                }
                else
                {
                    if (!stringManager.GetString(stringId, out var staticText, networkManager))
                        return "";
                    text = staticText;
                }

                // GetLocalizedName() indexes the first char, so only non-empty texts
                return string.IsNullOrEmpty(text) ? "" : stringManager.GetLocalizedName(text);
            }

            var pendingIds = new HashSet<uint>();

            // GetString() returns false with an empty result when the text is not cached
            // yet: it queues one STRING_MANAGER:STRING_RQ for the id and never queues the
            // same id twice, so it both requests and reads the cache. Dynamic ids are left
            // out, a STRING_RQ on them is never answered (the server pushes them).
            void AskString(uint stringId)
            {
                if (stringId == 0 || StringWait.IsDynId(stringId))
                    return;

                if (!stringManager.GetString(stringId, out _, networkManager))
                    pendingIds.Add(stringId);
            }

            foreach (var id in aliasNameId.Values)
                AskString(id);

            foreach (var id in aliasGuildId.Values)
                AskString(id);

            // Wait for the answers, pumping the network reception like StringWait.WaitAll()
            // does: Update() dispatches the incoming STRING_RESP, Send() flushes the queued
            // STRING_RQ while the main thread is blocked here.
            var watch = Stopwatch.StartNew();

            while (pendingIds.Count > 0 && watch.ElapsedMilliseconds < timeoutMs)
            {
                networkManager.Update();
                networkManager.Send(networkManager.GetNetworkConnection().GetCurrentServerTick());
                Thread.Sleep(10);

                pendingIds.RemoveWhere(id => stringManager.GetString(id, out _, networkManager));
            }

            watch.Stop();

            // Collect the texts: prefer the entity's own values (its string callback
            // localizes and strips title and shard), fall back on the cached text
            // (also for aliases whose entity is gone or shares a name string id, whose
            // callback only ever reaches one of them).
            var aliases = new HashSet<uint>(aliasNameId.Keys);
            aliases.UnionWith(aliasGuildId.Keys);

            foreach (var alias in aliases)
            {
                var name = "";
                var guild = "";

                // The values of the entity itself first: its string callback localizes the
                // text and strips title and shard. Several entities can share one alias, and
                // one name string id can serve several aliases: scan all of them and keep the
                // first text that is there.
                for (var slot = 1; slot < entities.Length; ++slot)
                {
                    var entity = entities[slot];
                    if (entity == null || entity.NpcAlias() != alias)
                        continue;

                    if (name.Length == 0)
                        name = entity.GetDisplayName();
                    if (guild.Length == 0)
                        guild = entity.GetGuildName();

                    if (name.Length > 0 && guild.Length > 0)
                        break;
                }

                // Fallback on the cache: aliases whose entity vanished while pumping, and
                // entities that never got the callback because another entity owns it.
                if (name.Length == 0)
                    name = LocalizedText(aliasNameId.TryGetValue(alias, out var nameId) ? nameId : 0);

                if (guild.Length == 0)
                    guild = LocalizedText(aliasGuildId.TryGetValue(alias, out var guildId) ? guildId : 0);

                result[alias] = (name, guild);
            }

            return result;
        }

        /// <summary>
        /// Write a file with the position of all entities.
        /// Without a .json extension the legacy StartCommands format is written.
        /// A .json file name produces a full JSON dump of every entity in the vision.
        /// </summary>
        /// <param name="file">output file, a name ending in .json writes the JSON dump</param>
        /// <param name="npcNames">texts resolved by <see cref="WarmUpNpcNames"/>, keyed by npc
        /// alias: used for entities whose own name or guild is still empty and written
        /// as an extra 'npcNames' object of the JSON dump (null: only the entity values)</param>
        public void WriteEntities(string file = "entities.txt", Dictionary<uint, (string Name, string Guild)> npcNames = null)
        {
            using var outStream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Read);
            using var writer = new StreamWriter(outStream, new System.Text.UTF8Encoding(false));

            var json = file.EndsWith(".json", StringComparison.OrdinalIgnoreCase);

            if (!json)
            {
                // Legacy format, unchanged behaviour
                writer.Write("StartCommands = {\n");
                var nb = _entities.Length;
                for (var i = 1; i < nb; ++i)
                {
                    var sheet = _entities[i] != null ? client.GetApiSheetIdFactory()?.SheetId(_entities[i].SheetId()) : null;

                    var strTmp = _entities[i] == null
                        ? $"// {i}\n"
                        : $"\"{(sheet != null ? sheet.ToString() : _entities[i].SheetId().ToString())}\",\t\"{_entities[i].Pos.X}\", \"{_entities[i].Pos.Y}\", \"{_entities[i].Pos.Z}\", \"{_entities[i].Front.X}\", \"{_entities[i].Front.Y}\", \"{_entities[i].Front.Z}\",\t// {i} {_entities[i].GetDisplayName()} {_entities[i].GetTitle()} {_entities[i].GetEntityType()}\n";
                    writer.Write(strTmp);
                }
                writer.Write("};\n");
                return;
            }

            // JSON dump: one object per occupied slot
            writer.Write("{\n  \"entities\": [\n");
            var first = true;
            var nbAll = _entities.Length;
            for (var i = 1; i < nbAll; ++i)
            {
                var e = _entities[i];
                if (e == null)
                    continue;

                var sheet = client.GetApiSheetIdFactory()?.SheetId(e.SheetId());

                // Heading as angle in degrees: atan2 of the front vector, 0 = +Y (game north), clockwise
                var headingDeg = Math.Atan2(e.Front.X, e.Front.Y) * 180.0 / Math.PI;
                if (headingDeg < 0)
                    headingDeg += 360.0;

                var (vpa, vpb, vpc) = GetVisualProperties((byte)i);

                // VP bitfields as hex strings (JSON has no hex literals); null only when
                // the DB node was never received, a real 0 is written as "0x000...0".
                static string Hex(ulong? v) => v.HasValue ? "\"0x" + v.Value.ToString("X16") + "\"" : "null";

                var alias = e.NpcAlias();

                // Name and guild of server NPCs are texts held in the string manager
                // cache only: take them from the warm-up result when the entity has
                // none itself (its string callback does not fire for every NPC,
                // because several entities can share one name string id).
                var name = SanitizeText(e.GetDisplayName());
                var guild = SanitizeText(e.GetGuildName());

                if (alias != 0 && npcNames != null && npcNames.TryGetValue(alias, out var resolved))
                {
                    if (name.Length == 0 && !string.IsNullOrEmpty(resolved.Name))
                        name = SanitizeText(resolved.Name);

                    if (guild.Length == 0 && !string.IsNullOrEmpty(resolved.Guild))
                        guild = SanitizeText(resolved.Guild);
                }

                var fields = new List<string>
                {
                    $"\"slot\": {i}",
                    $"\"datasetId\": {e.DataSetId()}",
                    $"\"npcAlias\": {alias}",
                    $"\"sheet\": \"{JsonEscape(sheet != null ? sheet.ToString() : e.SheetId().ToString())}\"",
                    $"\"name\": \"{JsonEscape(name)}\"",
                    $"\"title\": \"{JsonEscape(SanitizeText(e.GetTitle()))}\"",
                    $"\"guild\": \"{JsonEscape(guild)}\"",
                    $"\"type\": \"{e.GetEntityType()}\"",
                    $"\"pos\": [\"{e.Pos.X}\", \"{e.Pos.Y}\", \"{e.Pos.Z}\"]",
                    $"\"heading\": {headingDeg.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
                    $"\"front\": [\"{e.Front.X}\", \"{e.Front.Y}\", \"{e.Front.Z}\"]",
                    $"\"dead\": {(e.IsDead() ? "true" : "false")}",
                    $"\"selectable\": {(e.Properties.IsSelectable ? "true" : "false")}",
                    $"\"attackable\": {(e.Properties.IsAttackable ? "true" : "false")}",
                    $"\"harvestable\": {(e.Properties.IsHarvestable ? "true" : "false")}",
                    $"\"lootable\": {(e.Properties.IsLootable ? "true" : "false")}",
                    $"\"vpa\": {Hex(vpa)}",
                    $"\"vpb\": {Hex(vpb)}",
                    $"\"vpc\": {Hex(vpc)}"
                };

                if (!first)
                    writer.Write(",\n");
                first = false;

                writer.Write("    {" + string.Join(", ", fields) + "}");
            }
            writer.Write("\n  ]");

            // Every text the warm-up resolved, keyed by npc alias. It also contains
            // aliases whose entity is no longer in the vision (the network pump of
            // the warm-up can remove entities while waiting), so no resolved text
            // gets lost. Aliases without any text are left out: they are already
            // visible as an entity with an empty name and its npcAlias.
            if (npcNames is { Count: > 0 })
            {
                var aliases = new List<uint>(npcNames.Keys);
                aliases.Sort();

                var npcLines = new List<string>();

                foreach (var alias in aliases)
                {
                    var info = npcNames[alias];

                    if (string.IsNullOrEmpty(info.Name) && string.IsNullOrEmpty(info.Guild))
                        continue;

                    npcLines.Add($"    \"{alias}\": {{\"name\": \"{JsonEscape(SanitizeText(info.Name))}\", \"guild\": \"{JsonEscape(SanitizeText(info.Guild))}\"}}");
                }

                if (npcLines.Count > 0)
                {
                    writer.Write(",\n  \"npcNames\": {\n");
                    writer.Write(string.Join(",\n", npcLines));
                    writer.Write("\n  }");
                }
            }

            writer.Write("\n}\n");
        }


        // Helper: SERVER database branch of an entity slot, null if never received
        private DatabaseNodeBranch GetSlotDbNode(byte slot)
        {
            var db = client?.GetDatabaseManager()?.GetServerDb();
            if (db == null) return null;

            var nodeRoot = db.GetNode(0) as DatabaseNodeBranch;
            if (nodeRoot == null) return null;

            return nodeRoot.GetNode(slot) as DatabaseNodeBranch;
        }

        // Helper: numeric visual property of a slot as string id (0 if not received)
        private static uint ReadStringIdProp(DatabaseNodeBranch slotNode, PropertyType prop)
        {
            if (slotNode?.GetNode((ushort)prop) is not DatabaseNodeLeaf leaf)
                return 0;

            return (uint)(leaf.GetValue64() & 0xffffffffL);
        }

        // Helper: raw VPA/VPB/VPC of a slot, or null if not received yet
        private (ulong? vpa, ulong? vpb, ulong? vpc) GetVisualProperties(byte slot)
        {
            var slotNode = GetSlotDbNode(slot);
            if (slotNode == null) return (null, null, null);

            ulong? Get(ushort node)
            {
                if (slotNode.GetNode(node) is not DatabaseNodeLeaf leaf) return null;
                var v = leaf.GetValue64();
                return v == 0 ? (ulong?)null : (ulong?)v; // 0 = noch nie empfangen
            }

            return (Get((ushort)PropertyType.Vpa), Get((ushort)PropertyType.Vpb), Get((ushort)PropertyType.Vpc));
        }

        private static string JsonEscape(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";
            var sb = new System.Text.StringBuilder(s.Length);
            foreach (var c in s)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 32)
                            sb.AppendFormat("\\u{0:x4}", (int)c);
                        else
                            sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }


        /// <summary>
        /// Get an entity by dataset index. Returns null if the entity is not found.
        /// </summary>
        /// <param name="compressedIndex">The dataset index to search for.</param>
        /// <returns>An instance of <see cref="Entity"/> if found; otherwise, null.</returns>
        public IEntity GetEntityByCompressedIndex(uint compressedIndex)
        {
            if (compressedIndex == Constants.InvalidClientDatasetIndex)
                return null;

            for (uint i = 0; i < _nbMaxEntity; i++)
            {
                if (_entities[i] != null && _entities[i].DataSetId() == compressedIndex)
                {
                    return _entities[i];
                }
            }

            return null;
        }
    }
}