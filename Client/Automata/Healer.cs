using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Client.Automata.Helper;
using Client.Automata.Internal;

namespace Client.Automata
{
    class Healer : AutomatonBase
    {
        private bool _initialized;
        private bool _active;
        private DateTime _lastCast = DateTime.Now;
        private int _castDuration = 5000;
        private float _maxDistance = 36.0f;

        readonly List<string> _whitelist = new List<string> { "rangulf", "lasabo", "purg", "xeixes", "alexarwe", "elke", "zimbo", "lyretta", "sami", "crusher", "mytrix", "palea", "muddi", "tomstato", "deedlid", "skyway", "palorie", "iak", "risani", "eykim", "minuna", "akura", "aldara", "tayron", "angelwings", "mordok" };
        public int HealThreshold = 90;
        public int HealThresholdMin = 30;

        readonly Dictionary<string, EntityBars> healTargets = new Dictionary<string, EntityBars>();

        public override void OnInitialize()
        {
            Handler.GetLogger().Info("Automaton 'Healer' initialized.");

            RegisterAutomatonCommand("healStart", "Start healing", "", Command);
            RegisterAutomatonCommand("healStop", "Stop healing", "", Command);
        }

        public override void OnGameJoined()
        {
            _initialized = true;
        }

        public override void OnUpdate()
        {
            if (!_active)
                return;

            if (_lastCast.AddMilliseconds(_castDuration) > DateTime.Now)
                return;

            // People with very Low HP
            var toHeal2 = healTargets.Where(m => m.Value.HP * 100d / 128 < HealThresholdMin);

            if (toHeal2.Count() > 0)
            {
                byte slot = toHeal2.Random().Value.Slot;
                //Log.Write("Low healing " + index);
                HealEntityInSlot(slot, true);
                return;
            }

            // People without Priority and Low HP
            var toHeal4 = healTargets.Where(m => m.Value.HP * 100d / 128 < HealThreshold);

            if (toHeal4.Count() > 0)
            {
                byte slot = toHeal4.Random().Value.Slot;

                //Log.Write("Healing " + index);
                HealEntityInSlot(slot);
                return;
            }

            // Sap or Stamina is very low
            var toHeal5 = healTargets.Where(m => (m.Value.Sap * 100d / 128 < HealThreshold * 0.9) || (m.Value.Stamina * 100d / 128 < HealThresholdMin * 0.9));

            if (toHeal5.Count() > 0)
            {
                byte slot = toHeal5.Random().Value.Slot;

                //Log.Write("Healing Sap/Stamina " + index);
                HealEntityInSlot(slot);
                return;
            }
        }

        private void HealEntityInSlot(byte slot, bool lowHp = false)
        {
            var entityManager = Handler.GetNetworkManager().GetEntityManager();
            if (entityManager == null)
                return;

            var user = entityManager.UserEntity;
            if (user == null)
                return;

            var entity = entityManager.GetEntity(slot);
            if (entity == null)
                return;

            Handler.GetNetworkManager().GetEntityManager().UserEntity.Selection(entity.Slot(), Handler);
            Handler.GetNetworkManager().GetEntityManager().UserEntity.SetTargetSlot(entity.Slot());

            if (lowHp)
            {
                // Full HP
                Handler.GetLogger().Info("HP + HP " + entity.GetDisplayName());
                string responseMsg = "";
                _lastCast = DateTime.Now;
                Handler.PerformInternalCommand("executePhrase 8 1", ref responseMsg);
                return;
            }

            if (!healTargets.ContainsKey(entity.GetDisplayName()))
                return;

            var bars = healTargets[entity.GetDisplayName()];

            if (bars.Sap <= bars.Stamina)
            {
                // HP + Sap
                Handler.GetLogger().Info("HP + Sap " + entity.GetDisplayName());
                string responseMsg = "";
                _lastCast = DateTime.Now;
                Handler.PerformInternalCommand("executePhrase 8 0", ref responseMsg);
                return;
            }
            else
            {
                // HP + Stamina
                Handler.GetLogger().Info("HP + Stamina " + entity.GetDisplayName());
                string responseMsg = "";
                _lastCast = DateTime.Now;
                Handler.PerformInternalCommand("executePhrase 8 2", ref responseMsg);
                return;
            }
        }

        public override void OnEntityUpdateBars(uint gameCycle, long prop, byte slot, byte hitPoints, byte stamina, byte sap, byte focus)
        {
            if (!_active)
                return;

            var entityManager = Handler.GetNetworkManager().GetEntityManager();
            if (entityManager == null)
                return;

            var user = entityManager.UserEntity;
            if (user == null)
                return;

            var entity = entityManager.GetEntity(slot);
            if (entity == null)
                return;

            var displayName = entity.GetDisplayName().ToLower();

            if (_whitelist.Contains(displayName))
            {
                // remove if to far away
                if (Vector3.Distance(user.Pos, entity.Pos) > _maxDistance)
                {
                    healTargets.Remove(entity.GetDisplayName());
                    return;
                }

                // add new heal target
                if (healTargets.ContainsKey(entity.GetDisplayName()))
                {
                    healTargets[entity.GetDisplayName()].HP = hitPoints;
                    healTargets[entity.GetDisplayName()].Sap = sap;
                    healTargets[entity.GetDisplayName()].Stamina = stamina;
                    healTargets[entity.GetDisplayName()].Slot = slot;
                }
                else
                {
                    healTargets.Add(entity.GetDisplayName(), new EntityBars()
                    {
                        HP = hitPoints,
                        Sap = sap,
                        Stamina = stamina,
                        Slot = slot,
                    });
                }
            }
        }

        public string Command(string cmd, string[] args)
        {
            if (cmd.IndexOf(" ", StringComparison.Ordinal) != -1)
            {
                cmd = cmd.Substring(0, cmd.IndexOf(" ", StringComparison.Ordinal));
            }

            if (!_initialized)
            {
                Handler.GetLogger().Warn("[Healer] Not initialized.");
                return "";
            }

            switch (cmd.ToLower())
            {
                case "healstart":
                    Handler.GetLogger().Info("[Healer] Starting.");
                    _active = true;
                    return "";

                case "healstop":
                    Handler.GetLogger().Info("[Healer] Stopping.");
                    _active = false;
                    return "";

                default:
                    Handler.GetLogger()?.Warn("CommandBase unknown: " + cmd);
                    return "";
            }
        }
    }
}
