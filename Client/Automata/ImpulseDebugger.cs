using RCC.Automata.Internal;
using RCC.Database;
using RCC.Property;
using System.Collections.Generic;
using System.Reflection;

namespace RCC.Automata
{
    internal class ImpulseDebugger : AutomatonBase
    {
        public override void OnInitialize()
        {
            Handler.GetLogger().Info("Automaton 'ImpulseDebugger' initialized.");
        }

        public override void OnGameJoined()
        {
            Handler.GetLogger().Info($"§eImpulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        public override void OnGuildUseFemaleTitles(bool useFemaleTitles)
        {
            Handler.GetLogger().Info($"§eImpulse on {MethodBase.GetCurrentMethod()?.Name} useFemaleTitles: {useFemaleTitles}");
        }

        public override void OnPhraseDownLoad(List<Phrase.PhraseSlot> phrases, List<Phrase.PhraseMemorySlot> memorizedPhrases)
        {
            Handler.GetLogger().Info($"§eImpulse on {MethodBase.GetCurrentMethod()?.Name} phrases: {phrases.Count} memorizedPhrases: {memorizedPhrases.Count}");
        }

        public override void OnGuildUpdatePlayerTitle(bool unblock, int len, List<ushort> titles)
        {
            Handler.GetLogger().Info($"§eImpulse on {MethodBase.GetCurrentMethod()?.Name} unblock: {unblock} len: {len} titles: {titles.Count}");
        }

        public override void OnDeathRespawnPoint(int x, int y)
        {
            Handler.GetLogger().Info($"§eImpulse on {MethodBase.GetCurrentMethod()?.Name} x: {x} y: {y}");
        }

        public override void OnEncyclopediaInit()
        {
            Handler.GetLogger().Info($"§eImpulse on {MethodBase.GetCurrentMethod()?.Name}");
        }

        public override void OnInitInventory(uint serverTick)
        {
            Handler.GetLogger().Info($"§eImpulse on {MethodBase.GetCurrentMethod()?.Name} serverTick: {serverTick}");
        }

        public override void OnUserBars(byte msgNumber, int hp, int sap, int sta, int focus)
        {
            //Handler.GetLogger().Info($"§eImpulse on {MethodBase.GetCurrentMethod()?.Name} msgNumber: {msgNumber} hp: {hp} sap: {sap} sta: {sta} focus: {focus}");
        }

        public override void OnDatabaseInitBank(in uint serverTick, in uint bank, DatabaseManager databaseManager)
        {
            Handler.GetLogger().Info($"§eImpulse on {MethodBase.GetCurrentMethod()?.Name} serverTick: {serverTick} bank: {bank}");
        }

        public override void OnDatabaseUpdateBank(uint serverTick, uint bank, DatabaseManager databaseManager)
        {
            Handler.GetLogger().Info($"§eImpulse on {MethodBase.GetCurrentMethod()?.Name} serverTick: {serverTick} bank: {bank}");
        }

        public override void OnDatabaseResetBank(uint serverTick, uint bank, DatabaseManager databaseManager)
        {
            Handler.GetLogger().Info($"§eImpulse on {MethodBase.GetCurrentMethod()?.Name} serverTick: {serverTick} bank: {bank}");
        }

        public override void OnEntityCreate(in byte slot, in uint form, Change.TNewEntityInfo newEntityInfo)
        {
            //Handler.GetLogger().Info($"§eImpulse on {MethodBase.GetCurrentMethod()?.Name} slot: {slot} form: {form} alias: {newEntityInfo.Alias} dataSetIndex: {newEntityInfo.DataSetIndex}");
        }

        public override void OnEntityRemove(in byte slot)
        {
            //Handler.GetLogger().Info($"§eImpulse on {MethodBase.GetCurrentMethod()?.Name} slot: {slot}");
        }

        public override void OnEntityUpdateVisualProperty(in uint gameCycle, in byte slot, in byte prop, in uint predictedInterval)
        {
            //Handler.GetLogger().Info($"§eImpulse on {MethodBase.GetCurrentMethod()?.Name} gameCycle: {gameCycle} slot: {slot} prop: {prop} predictedInterval: {predictedInterval}");
        }
    }
}
