///////////////////////////////////////////////////////////////////
// Based on 'Ryzom - MMORPG Framework' (GNU AGPL)
// http://dev.ryzom.com/projects/ryzom/
// Copyright 2010 Winch Gate Property Limited
// STRING:CHAT_SHARE (client -> server) and the incoming
// STRING:CHAT_SHARE_RESULT (server -> client) for the Ryzom
// Console Client. See ryzomcore docs/chat.md, chat_message.h and
// net_manager.cpp impulseChatShareResult (commit 3c29303).
///////////////////////////////////////////////////////////////////
using API;
using API.Commands;
using API.Chat;
using Client.Stream;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Client.Commands
{
    /// <summary>
    /// Send a chat message with structured metadata (chat sharing).
    /// Usage (at least one structured reference or a quote is required):
    ///   ChatShare <text> item <slotId>         link an inventory item (packed slotId)
    ///   ChatShare <text> here                  link your own position
    ///   ChatShare <text> map <x> <y>          link a map point, mm coordinates
    ///   ChatShare <text> map <x> <y> landmark <name>   ... as a saveable landmark
    ///   ChatShare <text> map <x> <y> continent <name>  ... with a same continent check
    ///   ChatShare <text> quote <messageId>     quote an earlier message
    ///   ChatShare <text> phrase <membank> <memindex>  phrase from your memory, sent as KnownPhrase
    ///   ChatShare <text> phrasesheet <sheetId>  rolemaster phrase sheet id, decimal or 0x hex
    ///   ChatShare <text> knownphrase <index>   raw known phrase index (server vector slot)
    ///   ChatShare <text> macro <name> <action> [params] | <action> [params]   share a macro
    ///   ChatShare reaction <messageId> <emoji> [remove]   react to an earlier message
    ///   ChatShare <text> mention <name>        player name (resolved by the IOS)
    ///   ChatShare <text> mention team|guild|all  group mention (scope reference)
    /// A tell is sent as: ChatShare tell <name> <text ...>
    /// Other channels: ChatShare say|shout|team|guild|region|universe <text ...>
    ///   ChatShare dyn                                     list your channels
    ///   ChatShare dyn <name|index|id> <text ...>           share in one channel
    /// Plain text has no metadata and is not sent by this command; use the normal
    /// chat or /tell for plain messages.
    /// </summary>
    public class ChatShare : CommandBase
    {
        // Limits from CHAT_MESSAGE (chat_message.h).
        private const int MaxTextLength = 255;
        private const int MaxReferences = 8;
        private const int MessageIdLength = 36;
        private const int MaxReceiverLength = 255;
        private const int MaxEmojiNameLength = 64;   // CHAT_MESSAGE::MaxEmojiNameLength
        private const int MaxMacroCommands = 32;     // CHAT_MESSAGE::MaxMacroCommands

        /// <summary>Visible text the item reference points at.</summary>
        private const string ItemPlaceholder = "[item]";
        private const string PositionPlaceholder = "[pos]";
        private const string PhrasePlaceholder = "[phrase]";
        private const string MacroPlaceholder = "[macro]";

        /// <summary>CEntityId::Unknown (entity_id.cpp:35): UNKNOWN_TYPE = 0xFF in the
        /// Type field (bits 16-23 of the 64-bit id), everything else 0.
        /// CHAT_MESSAGE::isValidTarget requires exactly this value for every
        /// channel except dyn_chat, for tell included.</summary>
        private const ulong UnknownEntityId = 0x00FF0000UL;

        /// <summary>
        /// Channel prefix of the command -> CChatGroup::TGroupType (chat_group.h).
        /// say=0 shout=1 team=2 guild=3 universe=6 tell=7 region=11; arround(9) is
        /// refused by CHAT_MESSAGE::isValidTarget, so say is used for local chat.
        /// </summary>
        private static readonly Dictionary<string, ChatGroupType> ChannelModes =
            new Dictionary<string, ChatGroupType>(StringComparer.OrdinalIgnoreCase)
        {
            { "say", ChatGroupType.Say },
            { "shout", ChatGroupType.Shout },
            { "team", ChatGroupType.Team },
            { "guild", ChatGroupType.Guild },
            { "region", ChatGroupType.Region },
            { "universe", ChatGroupType.Universe },
            { "tell", ChatGroupType.Tell },
        };

        // CChatMessageReference::TType (chat_message.h)
        public enum RefType : byte
        {
            Item = 0,
            KnownPhrase = 1,
            PhraseSheet = 2,
            Position = 3,
            MapPosition = 4,
            Mention = 5,
            Macro = 6,
            Reaction = 7
        }

        // CChatMessagePosition::TKind
        public enum PosKind : byte
        {
            PlayerPosition = 0,
            MapPosition = 1,
            UserLandMark = 2
        }

        /// <summary>Mirror of CChatMessagePosition (chat_message.h). X/Y are millimetres.</summary>
        public class ChatMessagePosition
        {
            public PosKind Kind = PosKind.MapPosition;
            public int X;
            public int Y;
            public string FlagName = "";          // ucstring, only for UserLandMark
            public byte R = 255, G = 255, B = 255, A = 255; // NLMISC::CRGBA
            public string Place = "";              // std::string, filled by the server
            public string Region = "";
            public string Continent = "";          // only this one is read by the server
            public string SenderName = "";         // ucstring
            public uint Timestamp;

            public void Serial(BitMemoryStream f)
            {
                int kind = (int)Kind;
                f.Serial(ref kind);       // serialEnum: sint32
                f.Serial(ref X);          // sint32
                f.Serial(ref Y);          // sint32
                f.Serial(ref FlagName);   // ucstring
                f.Serial(ref R);          // CRGBA: four uint8
                f.Serial(ref G);
                f.Serial(ref B);
                f.Serial(ref A);
                f.Serial(ref Place, false);     // std::string: UTF-8 byte count
                f.Serial(ref Region, false);
                f.Serial(ref Continent, false);
                f.Serial(ref SenderName);       // ucstring
                f.Serial(ref Timestamp);        // uint32
            }

            /// <summary>CChatMessagePosition::isValid (server side).</summary>
            public bool IsValid()
            {
                if (Kind != PosKind.MapPosition && Kind != PosKind.UserLandMark)
                    return false;
                if (FlagName.Length > MaxTextLength ||
                    (Kind == PosKind.UserLandMark ? FlagName.Length == 0 : FlagName.Length != 0))
                    return false;
                foreach (var c in FlagName)
                    if (c < 32 || c == 127)
                        return false;
                return Place.Length <= MaxTextLength &&
                       Region.Length <= MaxTextLength &&
                       Continent.Length <= MaxTextLength &&
                       SenderName.Length <= MaxReceiverLength;
            }
        }

        /// <summary>Mirror of CChatMessageReaction (chat_message.h).</summary>
        public class ChatMessageReaction
        {
            public string MessageId = "";   // std::string, 36 chars
            public string Emoji = "";       // std::string, 1..64 bytes, no ':' and no control codes
            public bool Remove;

            public void Serial(BitMemoryStream f)
            {
                f.Serial(ref MessageId, false);
                f.Serial(ref Emoji, false);
                f.Serial(ref Remove);
            }

            /// <summary>CChatMessageReaction::isValid (server side).</summary>
            public bool IsValid()
            {
                if (MessageId.Length != MessageIdLength || Emoji.Length == 0 || Emoji.Length > MaxEmojiNameLength)
                    return false;
                foreach (var c in Emoji)
                    if (c < 32 || c == 127 || c == ':')
                        return false;
                return true;
            }
        }

        /// <summary>Mirror of CChatMessageMacro (chat_message.h). Strings are std::string, not ucstring.</summary>
        public class ChatMessageMacro
        {
            public class CCommand
            {
                public string Name = "";
                public string Params = "";

                public void Serial(BitMemoryStream f)
                {
                    f.Serial(ref Name, false);
                    f.Serial(ref Params, false);
                }
            }

            public string Name = "";
            public string DispText = "";
            public byte BitmapBack;
            public byte BitmapIcon;
            public byte BitmapOver;
            public readonly List<CCommand> Commands = new List<CCommand>();

            public void Serial(BitMemoryStream f)
            {
                f.Serial(ref Name, false);
                f.Serial(ref DispText, false);
                f.Serial(ref BitmapBack);
                f.Serial(ref BitmapIcon);
                f.Serial(ref BitmapOver);
                var count = (byte)Commands.Count; // serialBounded(MaxMacroCommands = 32)
                f.Serial(ref count);
                foreach (var c in Commands)
                    c.Serial(f);
            }

            /// <summary>CChatMessageMacro::isValid (server side). Sizes only, no character filter.</summary>
            public bool IsValid()
            {
                if (Name.Length == 0 || Name.Length > MaxTextLength ||
                    DispText.Length > MaxTextLength ||
                    Commands.Count == 0 || Commands.Count > MaxMacroCommands)
                    return false;
                foreach (var c in Commands)
                    if (c.Name.Length == 0 || c.Name.Length > MaxTextLength || c.Params.Length > MaxTextLength)
                        return false;
                return true;
            }
        }

        /// <summary>Mirror of CChatMessageReference. Item/KnownPhrase/PhraseSheet/Mention carry Value only.</summary>
        public class ChatMessageReference
        {
            public ushort Start;
            public ushort Length;
            public RefType Type;
            public uint Value;
            public ChatMessagePosition PositionValue;
            public ChatMessageReaction ReactionValue;
            public ChatMessageMacro MacroValue;

            public void Serial(BitMemoryStream f)
            {
                f.Serial(ref Start);
                f.Serial(ref Length);
                // NeL serialEnum = sint32 (stream.h).
                int type = (int)Type;
                f.Serial(ref type);
                Type = (RefType)type;
                switch (Type)
                {
                    case RefType.Item:
                    case RefType.KnownPhrase:
                    case RefType.PhraseSheet:
                    case RefType.Mention:
                        f.Serial(ref Value);
                        break;
                    case RefType.Position:
                        // No payload: the EGS uses the sender's own position.
                        break;
                    case RefType.MapPosition:
                        (PositionValue ?? new ChatMessagePosition()).Serial(f);
                        break;
                    case RefType.Reaction:
                        (ReactionValue ?? new ChatMessageReaction()).Serial(f);
                        break;
                    case RefType.Macro:
                        (MacroValue ?? new ChatMessageMacro()).Serial(f);
                        break;
                    default:
                        throw new NotSupportedException($"Reference type {Type} not supported by this client.");
                }
            }
        }

        /// <summary>Mirror of CChatMessageRequest (chat_message.h, version 1).</summary>
        public class ChatMessageRequest
        {
            public const uint Version = 1;

            public string Text = "";
            public string QuoteMessageId = "";
            public uint ClientRequestId;
            public readonly List<ChatMessageReference> References = new List<ChatMessageReference>();

            public void Serial(BitMemoryStream f)
            {
                var version = (byte)Version;
                f.Serial(ref version);
                f.Serial(ref Text);              // ucstring
                f.Serial(ref QuoteMessageId, false); // std::string: UTF-8 byte count
                f.Serial(ref ClientRequestId);
                var count = (byte)References.Count; // serialBounded: uint8
                f.Serial(ref count);
                foreach (var r in References)
                    r.Serial(f);
            }
        }

        /// <summary>CSheetId is (shortId &lt;&lt; 8) | type, stored big endian in its 32 bit word.</summary>
        private static bool TryParseSheetId(string text, out uint sheetId)
        {
            sheetId = 0;
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                return uint.TryParse(text.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out sheetId);
            return uint.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out sheetId);
        }

        private const byte DynChatGroupEntityType = 10; // RYZOMID::dynChatGroup (ryzom_entity_id.h)

        private sealed class DynChannel
        {
            public int Slot;
            public ulong Id;
            public uint NameId;
            public string Name = "";
            public bool WriteRight;
        }

        internal static bool IsDynChatPrefix(string text)
        {
            return text.Equals("dyn", StringComparison.OrdinalIgnoreCase) ||
                   text.Equals("dynchat", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>CEntityId type field (bits 16-23) must be RYZOMID::dynChatGroup for
        /// CHAT_MESSAGE::isValidTarget to accept a dyn_chat share.</summary>
        private static bool IsDynChatGroupId(ulong id) => ((id >> 16) & 0xFF) == DynChatGroupEntityType;

        /// <summary>Read the player's SERVER:DYN_CHAT:CHANNEL0..7 slots. NAME is a
        /// string id, ID is the full CEntityId the share must send.</summary>
        private static bool TryLoadDynChannels(RyzomClient client, out List<DynChannel> channels, out string error)
        {
            channels = new List<DynChannel>();
            error = "";
            var db = client.GetDatabaseManager();
            if (db == null)
            {
                error = "Player chat channels are not available yet.";
                return false;
            }

            for (var i = 0; i < Constants.MaxDynChanPerPlayer; i++)
            {
                var idNode = db.GetServerNode($"SERVER:DYN_CHAT:CHANNEL{i}:ID", false);
                var nameNode = db.GetServerNode($"SERVER:DYN_CHAT:CHANNEL{i}:NAME", false);
                if (idNode == null || nameNode == null)
                    continue;

                var id = unchecked((ulong)idNode.GetValue64());
                var nameId = unchecked((uint)nameNode.GetValue32());
                if (id == 0 || nameId == 0) // NAME 0: slot not available
                    continue;

                var rightNode = db.GetServerNode($"SERVER:DYN_CHAT:CHANNEL{i}:WRITE_RIGHT", false);
                channels.Add(new DynChannel
                {
                    Slot = i,
                    Id = id,
                    NameId = nameId,
                    WriteRight = rightNode != null && rightNode.GetValue8() != 0
                });
            }

            if (channels.Count == 0)
            {
                error = "You are not in any player chat channel. Join one in game first; " +
                        "the client tracks up to 8 in SERVER:DYN_CHAT.";
                return false;
            }
            return true;
        }

        private static string ResolveDynChannelName(RyzomClient client, DynChannel channel)
        {
            var name = StringWait.WaitString(client.GetStringManager(), channel.NameId, client.GetNetworkManager());
            if (string.IsNullOrEmpty(name) || name == $"TEXT {channel.NameId}")
                return $"channel {channel.Slot}";
            return name;
        }

        private static void ResolveDynChannelNames(RyzomClient client, List<DynChannel> channels)
        {
            var ids = new List<uint>(channels.Count);
            foreach (var c in channels)
                ids.Add(c.NameId);

            var names = StringWait.WaitAll(client.GetStringManager(), ids, client.GetNetworkManager());
            foreach (var c in channels)
            {
                if (names.TryGetValue(c.NameId, out var name) &&
                    !string.IsNullOrEmpty(name) && name != $"TEXT {c.NameId}")
                    c.Name = name;
                else
                    c.Name = $"channel {c.Slot}";
            }
        }

        private static string FormatDynChannels(List<DynChannel> channels)
        {
            var sb = new StringBuilder();
            foreach (var c in channels)
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append('#').Append(c.Slot).Append(" '")
                  .Append(string.IsNullOrEmpty(c.Name) ? $"channel {c.Slot}" : c.Name).Append('\'')
                  .Append(c.WriteRight ? " (write)" : " (read-only)");
            }
            return sb.ToString();
        }

        private static bool TryParseDynChannelId(string text, out ulong id)
        {
            id = 0;
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                return ulong.TryParse(text[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out id);
            return ulong.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out id);
        }

        /// <summary>Resolve a dyn chat selector: slot index 0..7, channel name, or raw
        /// CEntityId (decimal or 0x hex). The selected id is the full 64-bit channel id.</summary>
        internal static bool TryResolveDynChannel(RyzomClient client, string selector,
            out ulong channelId, out string channelName, out string error)
        {
            channelId = 0;
            channelName = "";
            error = "";
            if (!TryLoadDynChannels(client, out var channels, out error))
                return false;

            // The listing shows #0..; accept the leading # on the numeric forms too.
            var numericSelector = selector.Length > 1 && selector[0] == '#' ? selector[1..] : selector;
            DynChannel chosen = null;
            if (int.TryParse(numericSelector, NumberStyles.Integer, CultureInfo.InvariantCulture, out var slot) &&
                slot >= 0 && slot < Constants.MaxDynChanPerPlayer)
            {
                chosen = channels.Find(c => c.Slot == slot);
            }
            if (chosen == null && TryParseDynChannelId(numericSelector, out var rawId))
            {
                chosen = channels.Find(c => c.Id == rawId);
            }

            if (chosen == null)
            {
                ResolveDynChannelNames(client, channels);
                foreach (var c in channels)
                {
                    if (!string.Equals(c.Name, selector, StringComparison.OrdinalIgnoreCase))
                        continue;
                    chosen = c;
                    if (c.WriteRight)
                        break;
                }
                if (chosen == null)
                {
                    error = $"No player chat channel matches '{selector}'. Available: {FormatDynChannels(channels)}.";
                    return false;
                }
            }
            else
            {
                chosen.Name = ResolveDynChannelName(client, chosen);
            }

            if (!IsDynChatGroupId(chosen.Id))
            {
                error = $"Player chat channel '{chosen.Name}' has an invalid channel id; rejoin it.";
                return false;
            }
            if (!chosen.WriteRight)
            {
                error = $"Player chat channel '{chosen.Name}' is read-only.";
                return false;
            }

            channelId = chosen.Id;
            channelName = chosen.Name;
            return true;
        }

        /// <summary>Last sent request id, echoed by CHAT_SHARE_RESULT.</summary>
        public uint LastRequestId { get; private set; }

        /// <summary>Last sent text; restore it in your input on rejected results.</summary>
        public string LastText { get; private set; } = "";

        /// <summary>Set by 'ChatShare debug'; appends a request hex dump to the send response.</summary>
        private static bool DebugDump;

        public override CommandCategory CmdCategory => CommandCategory.Chat;
        public override string CmdName => "ChatShare";
        public override string CmdUsage =>
            "debug | <text> <reference> | reaction <messageId> <emoji> [remove] | " +
            "[say|shout|team|guild|region|universe <text> <reference>] | " +
            "dyn <name|index|id> <text> <reference> | tell <name> <text> <reference>";
        public override string CmdDesc => "Send a chat message with structured metadata (STRING:CHAT_SHARE). " +
            "<reference> is one of: item <slotId>, phrase <membank> <memindex>, phrasesheet <sheetId>, " +
            "knownphrase <index>, macro <name> <action> [params...] | <action> ..., here, " +
            "map <x> <y> [landmark <name>] [continent <name>], quote <messageId>, " +
            "mention <name|team|guild|all>. At least one reference or quote is required; plain text " +
            "uses the normal chat or /tell. Without a channel prefix the current channel is used " +
            "(around is sent as say); player chat channels use 'dyn <name|index|id>' " +
            "(run 'ChatShare dyn' to list them). Run 'ChatShare debug' to toggle a request hex dump.";

        public override bool CmdAsync => true;

        public override bool Run(IClient handler, string command, out string responseMsg, Dictionary<string, object> localVars)
        {
            responseMsg = "";
            if (handler is not RyzomClient client)
                throw new Exception("Command handler is not a Ryzom client.");

            var args = GetArgs(command);
            if (args.Length < 1)
            {
                responseMsg = $"Usage: {CmdUsage}";
                return false;
            }

            if (args.Length == 1 && string.Equals(args[0], "debug", StringComparison.OrdinalIgnoreCase))
            {
                DebugDump = !DebugDump;
                responseMsg = $"ChatShare debug dump {(DebugDump ? "enabled" : "disabled")}.";
                return true;
            }

            var request = new ChatMessageRequest();
            byte targetGroup = (byte)ChatGroupType.Say;
            ulong dynamicChannelId = UnknownEntityId; // CEntityId::Unknown: type 0xFF at bits 16-23
            var receiver = "";
            var sb = new StringBuilder();
            ChatMessageReference lastPos = null;
            ChatMessageReaction reaction = null;
            var note = "";   // resolution detail for the final message
            var channelNote = ""; // dyn chat channel name, kept apart from phrase/macro notes

            var start = 0;
            if (IsDynChatPrefix(args[0]))
            {
                if (args.Length == 1)
                {
                    if (!TryLoadDynChannels(client, out var channels, out var listError))
                    {
                        responseMsg = listError;
                        return false;
                    }
                    ResolveDynChannelNames(client, channels);
                    responseMsg = $"Player chat channels: {FormatDynChannels(channels)}. " +
                                  "Use ChatShare dyn <name|index|id> <text ...>.";
                    return true;
                }
                if (args.Length < 3)
                {
                    responseMsg = "Usage: ChatShare dyn <name|index|id> <text ...>";
                    return false;
                }
                targetGroup = (byte)ChatGroupType.DynChat;
                if (!TryResolveDynChannel(client, args[1], out dynamicChannelId, out var dynName, out var dynError))
                {
                    responseMsg = dynError;
                    return false;
                }
                channelNote = $"player chat channel '{dynName}'";
                start = 2;
            }
            else if (args.Length >= 2 && ChannelModes.TryGetValue(args[0], out var mode))
            {
                if (mode == ChatGroupType.Tell)
                {
                    targetGroup = (byte)ChatGroupType.Tell;
                    receiver = args[1];
                    start = 2;
                }
                else
                {
                    // say/shout/team/guild/region/universe: no receiver, no dynamic channel id.
                    targetGroup = (byte)mode;
                    start = 1;
                }
            }
            else
            {
                // The official client shares in its current chat mode. Around has no share
                // target (isValidTarget refuses it), so local chat is sent as say.
                if (client.Channel == ChatGroupType.DynChat)
                {
                    if (client.DynamicChannelId == UnknownEntityId)
                    {
                        responseMsg = "No player chat channel is selected; use " +
                                      "'Channel dyn <name|index|id>' or 'ChatShare dyn <name|index|id> <text ...>'.";
                        return false;
                    }
                    targetGroup = (byte)ChatGroupType.DynChat;
                    dynamicChannelId = client.DynamicChannelId;
                    channelNote = "active player chat channel";
                }
                else
                {
                    targetGroup = (byte)(client.Channel switch
                    {
                        ChatGroupType.Shout => ChatGroupType.Shout,
                        ChatGroupType.Team => ChatGroupType.Team,
                        ChatGroupType.Guild => ChatGroupType.Guild,
                        ChatGroupType.Region => ChatGroupType.Region,
                        ChatGroupType.Universe => ChatGroupType.Universe,
                        _ => ChatGroupType.Say
                    });
                }
            }

            for (var i = start; i < args.Length; i++)
            {
                switch (args[i].ToLowerInvariant())
                {
                    case "item" when i + 1 < args.Length:
                    {
                        // Packed slotId: inventory << 17 | slot, exactly as the server expects.
                        var slot = (uint)Convert.ToInt32(args[++i], 10);
                        var refStart = (ushort)sb.Length;
                        sb.Append(ItemPlaceholder).Append(' ');
                        request.References.Add(new ChatMessageReference
                        {
                            Start = refStart,
                            Length = (ushort)ItemPlaceholder.Length, // span must be non-empty
                            Type = RefType.Item,
                            Value = slot
                        });
                        break;
                    }
                    case "quote" when i + 1 < args.Length:
                        request.QuoteMessageId = args[++i];
                        break;
                    case "phrase" when i + 2 < args.Length:
                    {
                        // RefType.KnownPhrase addressed by memory line (bank) and slot (index).
                        // MemorySlot.Id is the known phrase id, i.e. the index the server
                        // uses in CCharacter::getKnownPhrases().
                        if (!uint.TryParse(args[++i], NumberStyles.Integer, CultureInfo.InvariantCulture, out var memBank) ||
                            !uint.TryParse(args[++i], NumberStyles.Integer, CultureInfo.InvariantCulture, out var memIndex))
                        {
                            responseMsg = "phrase needs a memory line and slot: phrase <membank> <memindex>.";
                            return false;
                        }
                        var phraseManager = client.GetPhraseManager();
                        var memories = phraseManager.GetMemories();
                        if (memBank >= memories.Count || memIndex >= Phrase.PhraseManager.PHRASE_MAX_MEMORY_SLOT)
                        {
                            responseMsg = $"Memory line {memBank} has no slot {memIndex} " +
                                          $"(lines 0..{memories.Count - 1}, slots 0..{Phrase.PhraseManager.PHRASE_MAX_MEMORY_SLOT - 1}).";
                            return false;
                        }
                        var phraseId = phraseManager.GetPhraseIdFromMemory(memBank, memIndex);
                        if (phraseId == 0)
                        {
                            responseMsg = $"Memory line {memBank} slot {memIndex} holds no phrase.";
                            return false;
                        }
                        var phraseName = phraseManager.GetPhrase(phraseId).Name;
                        var refStart = (ushort)sb.Length;
                        sb.Append(PhrasePlaceholder).Append(' ');
                        request.References.Add(new ChatMessageReference
                        {
                            Start = refStart,
                            Length = (ushort)PhrasePlaceholder.Length,
                            Type = RefType.KnownPhrase,
                            Value = phraseId
                        });
                        note = string.IsNullOrEmpty(phraseName)
                            ? $"phrase id {phraseId} from memory {memBank}/{memIndex}"
                            : $"phrase '{phraseName}' (id {phraseId}) from memory {memBank}/{memIndex}";
                        break;
                    }
                    case "phrasesheet" when i + 1 < args.Length:
                    {
                        // RefType.PhraseSheet: the raw CSheetId, (shortId << 8) | type.
                        // The server checks existence and rolemaster-ness, never ownership.
                        if (!TryParseSheetId(args[++i], out var sheetId))
                        {
                            responseMsg = "phrasesheet needs a sheet id in decimal or 0x hex form.";
                            return false;
                        }
                        var refStart = (ushort)sb.Length;
                        sb.Append(PhrasePlaceholder).Append(' ');
                        request.References.Add(new ChatMessageReference
                        {
                            Start = refStart,
                            Length = (ushort)PhrasePlaceholder.Length,
                            Type = RefType.PhraseSheet,
                            Value = sheetId
                        });
                        break;
                    }
                    case "knownphrase" when i + 1 < args.Length:
                    {
                        // RefType.KnownPhrase: index into the sender's own phrase list.
                        var refStart = (ushort)sb.Length;
                        sb.Append(PhrasePlaceholder).Append(' ');
                        request.References.Add(new ChatMessageReference
                        {
                            Start = refStart,
                            Length = (ushort)PhrasePlaceholder.Length,
                            Type = RefType.KnownPhrase,
                            Value = (uint)Convert.ToInt32(args[++i], 10)
                        });
                        break;
                    }
                    case "macro" when i + 2 < args.Length:
                    {
                        // Rest of the line: macro <name> <action> [params...] | <action> [params...]
                        // Server validation is size only, so any action name and any params go through.
                        var macro = new ChatMessageMacro { Name = args[++i] };
                        macro.DispText = macro.Name;
                        ChatMessageMacro.CCommand cmd = null;
                        for (i++; i < args.Length; i++)
                        {
                            if (args[i] == "|")
                            {
                                cmd = null;
                                continue;
                            }
                            if (cmd == null)
                            {
                                cmd = new ChatMessageMacro.CCommand { Name = args[i] };
                                macro.Commands.Add(cmd);
                            }
                            else
                            {
                                cmd.Params = cmd.Params.Length == 0 ? args[i] : cmd.Params + " " + args[i];
                            }
                        }
                        if (!macro.IsValid())
                        {
                            responseMsg = $"Macro invalid: name 1..{MaxTextLength} bytes, 1..{MaxMacroCommands} commands, " +
                                          $"action names non-empty, params at most {MaxTextLength} bytes.";
                            return false;
                        }
                        var refStart = (ushort)sb.Length;
                        sb.Append(MacroPlaceholder).Append(' ');
                        request.References.Add(new ChatMessageReference
                        {
                            Start = refStart,
                            Length = (ushort)MacroPlaceholder.Length,
                            Type = RefType.Macro,
                            MacroValue = macro
                        });
                        note = $"macro '{macro.Name}' with {macro.Commands.Count} command(s)";
                        break;
                    }
                    case "reaction" when i + 2 < args.Length:
                    {
                        var msgId = args[++i];
                        var emoji = args[++i];
                        var remove = i + 1 < args.Length &&
                                     string.Equals(args[i + 1], "remove", StringComparison.OrdinalIgnoreCase);
                        if (remove)
                            i++;
                        reaction = new ChatMessageReaction { MessageId = msgId, Emoji = emoji, Remove = remove };
                        break;
                    }
                    case "here":
                    {
                        // Server fills the position from your own character.
                        var refStart = (ushort)sb.Length;
                        sb.Append(PositionPlaceholder).Append(' ');
                        request.References.Add(new ChatMessageReference
                        {
                            Start = refStart,
                            Length = (ushort)PositionPlaceholder.Length,
                            Type = RefType.Position
                        });
                        break;
                    }
                    case "map" when i + 2 < args.Length:
                    {
                        if (!int.TryParse(args[i + 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var mx) ||
                            !int.TryParse(args[i + 2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var my))
                        {
                            responseMsg = "map needs two coordinates in millimetres: map <x> <y>.";
                            return false;
                        }
                        i += 2;
                        var refStart = (ushort)sb.Length;
                        sb.Append(PositionPlaceholder).Append(' ');
                        lastPos = new ChatMessageReference
                        {
                            Start = refStart,
                            Length = (ushort)PositionPlaceholder.Length,
                            Type = RefType.MapPosition,
                            PositionValue = new ChatMessagePosition
                            {
                                Kind = PosKind.MapPosition,
                                X = mx,
                                Y = my
                            }
                        };
                        request.References.Add(lastPos);
                        break;
                    }
                    case "landmark" when i + 1 < args.Length:
                    {
                        if (lastPos == null)
                        {
                            responseMsg = "landmark must follow a map reference.";
                            return false;
                        }
                        lastPos.PositionValue.Kind = PosKind.UserLandMark;
                        lastPos.PositionValue.FlagName = args[++i];
                        break;
                    }
                    case "continent" when i + 1 < args.Length:
                    {
                        if (lastPos == null)
                        {
                            responseMsg = "continent must follow a map reference.";
                            return false;
                        }
                        // Leave it empty to skip the server's same continent check.
                        lastPos.PositionValue.Continent = args[++i];
                        break;
                    }
                    case "mention" when i + 1 < args.Length:
                    {
                        var name = args[++i];
                        var refStart = (ushort)sb.Length;
                        sb.Append('@').Append(name).Append(' ');
                        // TScope: Player=0 is rejected in a reference; a player mention is
                        // plain text, the IOS resolves it from the message text.
                        var scope = name.ToLowerInvariant() switch
                        {
                            "team" => 1u,
                            "guild" => 2u,
                            "all" => 3u,
                            _ => 0u
                        };
                        if (scope != 0)
                        {
                            request.References.Add(new ChatMessageReference
                            {
                                Start = refStart,
                                Length = (ushort)(name.Length + 1), // '@' + name
                                Type = RefType.Mention,
                                Value = scope
                            });
                        }
                        break;
                    }
                    default:
                        sb.Append(args[i]).Append(' ');
                        break;
                }
            }

            request.Text = sb.ToString().TrimEnd();

            if (reaction != null)
            {
                // A reaction is a message of its own: one reference spanning the whole text,
                // no quote id (CChatMessageRequest::isValid rejects anything else).
                if (request.References.Count != 0 || request.Text.Length != 0)
                {
                    responseMsg = "A reaction goes alone: ChatShare reaction <messageId> <emoji> [remove].";
                    return false;
                }
                if (!reaction.IsValid())
                {
                    responseMsg = $"Reaction invalid: message id must be {MessageIdLength} characters, " +
                                  $"the emoji 1 to {MaxEmojiNameLength} bytes without ':' or control codes.";
                    return false;
                }
                request.Text = ":" + reaction.Emoji + ":";
                request.References.Add(new ChatMessageReference
                {
                    Start = 0,
                    Length = (ushort)request.Text.Length,
                    Type = RefType.Reaction,
                    ReactionValue = reaction
                });
            }

            // ChatShare carries structured metadata; plain text belongs to the normal
            // chat (say/shout/team/...) and /tell. A quote alone is structured too.
            if (request.References.Count == 0 && string.IsNullOrEmpty(request.QuoteMessageId))
            {
                responseMsg = "ChatShare needs a structured reference or a quote (item, phrase, " +
                              "phrasesheet, knownphrase, macro, here, map, quote, mention or reaction). " +
                              "Plain text uses the normal chat or /tell <name> <text>.";
                return false;
            }

            // Client side limits of CChatMessageRequest::isValid.
            if (request.Text.Length == 0)
            {
                responseMsg = "The text must not be empty.";
                return false;
            }
            if (request.Text.Length > MaxTextLength)
            {
                responseMsg = $"Text too long: {request.Text.Length} characters, the limit is {MaxTextLength}.";
                return false;
            }
            if (request.References.Count > MaxReferences)
            {
                responseMsg = $"Too many references: {request.References.Count}, the limit is {MaxReferences}.";
                return false;
            }
            if (!string.IsNullOrEmpty(request.QuoteMessageId) && request.QuoteMessageId.Length != MessageIdLength)
            {
                responseMsg = $"A message id has {MessageIdLength} characters, got {request.QuoteMessageId.Length}.";
                return false;
            }
            foreach (var r in request.References)
            {
                if (r.Type != RefType.MapPosition)
                    continue;
                if (r.PositionValue == null || !r.PositionValue.IsValid())
                {
                    responseMsg = "A landmark needs 1 to 255 printable characters and no control codes;" +
                                  " a map position needs valid coordinates.";
                    return false;
                }
            }

            // ClientRequestId: monotonous counter, echoed by CHAT_SHARE_RESULT.
            request.ClientRequestId = (uint)Environment.TickCount & 0x7FFFFFFF;

            var bms = new BitMemoryStream();
            const string msgType = "STRING:CHAT_SHARE";
            if (!client.GetNetworkManager().GetMessageHeaderManager().PushNameToStream(msgType, bms))
            {
                responseMsg = $"Unknown message named '{msgType}'. Is msg.xml up to date?";
                return false;
            }

            // The dump is built in its own byte-aligned stream: the header can end
            // mid-byte, so reading the payload from the shared stream shows shifted
            // bytes even though the wire data is correct.
            var dumpPayload = DebugDump ? new BitMemoryStream() : null;
            if (dumpPayload != null)
            {
                dumpPayload.Serial(ref targetGroup);
                dumpPayload.Serial(ref dynamicChannelId, 64);
                dumpPayload.Serial(ref receiver, false);
                request.Serial(dumpPayload);
            }

            bms.Serial(ref targetGroup);
            bms.Serial(ref dynamicChannelId, 64);
            bms.Serial(ref receiver, false); // std::string: UTF-8 byte count
            request.Serial(bms);
            var debugDump = dumpPayload != null
                ? FormatDebugDump(request, targetGroup, dynamicChannelId, receiver, dumpPayload)
                : null;
            client.GetNetworkManager().Push(bms);

            LastRequestId = request.ClientRequestId;
            LastText = request.Text;
            responseMsg = $"CHAT_SHARE sent (request id {request.ClientRequestId}), " +
                          $"{request.References.Count} reference(s).";
            if (channelNote.Length > 0)
                responseMsg += " " + channelNote + ".";
            if (note.Length > 0)
                responseMsg += " " + note + ".";
            responseMsg += " Waiting for CHAT_SHARE_RESULT.";
            if (debugDump != null)
                responseMsg += "\n" + debugDump;
            return true;
        }

        /// <summary>Hex dump of the CHAT_SHARE payload (without the message-name header).</summary>
        private static string FormatDebugDump(ChatMessageRequest request, byte targetGroup,
            ulong dynamicChannelId, string receiver, BitMemoryStream payload)
        {
            var dump = new StringBuilder();
            dump.Append("CHAT_SHARE debug: mode=").Append(targetGroup)
                .Append(" dyn=0x").Append(dynamicChannelId.ToString("X16"))
                .Append(" receiver='").Append(receiver).Append('\'')
                .Append(" requestId=").Append(request.ClientRequestId)
                .Append(" textChars=").Append(request.Text.Length)
                .Append(" refs=").Append(request.References.Count);
            foreach (var reference in request.References)
            {
                dump.Append("\n  ref type=").Append(reference.Type)
                    .Append(" start=").Append(reference.Start)
                    .Append(" length=").Append(reference.Length)
                    .Append(" value=").Append(reference.Value);
            }

            var bytes = payload.Buffer();
            var end = payload.Length;
            dump.Append("\n  payload ").Append(end).Append(" bytes:");
            for (var i = 0; i < end; i += 16)
            {
                dump.Append("\n  ").Append(i.ToString("X4")).Append("  ");
                for (var j = i; j < Math.Min(i + 16, end); j++)
                    dump.Append(bytes[j].ToString("X2")).Append(' ');
            }
            return dump.ToString();
        }
    }
}
