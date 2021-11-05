using RCC.Automata.Internal;
using RCC.Config;
using System;
using System.Numerics;

namespace RCC.Automata
{
    class Follower : AutomatonBase
    {
        private bool _initialized;
        private bool _active;
        private DateTime _lastMove = DateTime.Now;

        public override void OnInitialize()
        {
            if (!ClientConfig.SendPosition)
            {
                Handler.GetLogger().Warn("You need to enable 'SendPosition' to use Automaton 'Follower'.");
                return;
            }

            Handler.GetLogger().Info("Automaton 'Follower' initialized.");

            RegisterAutomatonCommand("goto", "Move to an entity", "", Command);
            RegisterAutomatonCommand("stop", "Stop the movement", "", Command);
        }

        public override void OnEntityUpdateBars(uint gameCycle, long prop, byte slot, byte hitPoints, byte stamina, byte sap, byte focus)
        {
            var entityManager = Handler.GetNetworkManager().GetEntityManager();
            if (entityManager == null)
                return;

            var user = entityManager.UserEntity;
            if (user == null)
                return;

            if (user.TargetSlot() != slot)
                return;

            var entity = entityManager.GetEntity(slot);
            if (entity == null)
                return;

            Handler.GetLogger().Info($"{entity.GetDisplayName()} hp: {hitPoints} sta: {stamina} sap: {sap} foc: {focus}");
        }

        public override void OnUpdate()
        {
            float duration = (float)Math.Min((DateTime.Now - _lastMove).TotalMilliseconds, 1000);
            _lastMove = DateTime.Now;

            if (!_initialized || !_active)
                return;

            var entityManager = Handler.GetNetworkManager().GetEntityManager();
            if (entityManager == null)
            {
                _active = false;
                Handler.GetLogger().Warn("[Follower] Entity manager is not initialized.");
                return;
            }

            var user = entityManager.UserEntity;
            if (user == null) return;

            var target = entityManager.GetEntity(user.TargetSlot());

            if (target == null)
            {
                _active = false;
                Handler.GetLogger().Info("[Follower] No target. Stopping.");
                return;
            }

            if (target.Pos == Vector3.Zero || user.Pos == Vector3.Zero)
                return;

            var dist = Vector3.Distance(target.Pos, user.Pos);

            if (dist > 250 || dist < 4.5f)
                return;

            user.Dir = Vector3.Normalize(target.Pos - user.Pos);
            user.Front = user.Dir;

            user.Pos += user.Dir * (duration * ClientConfig.Run / 1000f);
        }

        public override void OnGameJoined()
        {
            _initialized = true;
        }

        public string Command(string cmd, string[] args)
        {
            if (cmd.IndexOf(" ", StringComparison.Ordinal) != -1)
            {
                cmd = cmd.Substring(0, cmd.IndexOf(" ", StringComparison.Ordinal));
            }

            if (!_initialized)
            {
                Handler.GetLogger().Warn("[Follower] Not initialized.");
                return "";
            }

            switch (cmd.ToLower())
            {
                case "goto":
                    Handler.GetLogger().Info("[Follower] Starting.");
                    _active = true;
                    return "";

                case "stop":
                    Handler.GetLogger().Info("[Follower] Stopping.");
                    _active = false;
                    return "";

                default:
                    Handler.GetLogger()?.Warn("CommandBase unknown: " + cmd);
                    return "";
            }
        }
    }
}
