using System;
using System.Numerics;
using API.Plugins;

namespace HackePeter
{
    /// <summary>
    /// Handle events for all Player related events
    /// </summary>
    /// <author>bierdosenhalter</author>
    internal class Listener : ListenerBase
    {
        private readonly Main _plugin;
        private bool _isIngame;
        private bool _active;
        private DateTime _lastMove = DateTime.Now;
        private string _targetName = "";

        // TODO: Use value from client config
        public double RunSpeed = 6.0;

        public Listener(Main plugin)
        {
            _plugin = plugin;
        }

        public override void OnInitialize()
        {
            // TODO: move some config values to the API
            //if (!ClientConfig.SendPosition)
            //{
            //    _plugin.GetLogger().Warn("You need to enable 'SendPosition' to use Automaton 'HackePeter'.");
            //    return;
            //}

            _plugin.RegisterCommand("goto", "Move to an entity", "", Command);
            _plugin.RegisterCommand("stop", "Stop the movement", "", Command);
        }

        public override void OnUpdate()
        {
            var duration = (float)Math.Min((DateTime.Now - _lastMove).TotalMilliseconds, 1000);
            _lastMove = DateTime.Now;

            if (!_isIngame || !_active || _targetName == "")
                return;

            var entityManager = _plugin.GetClient().GetApiNetworkManager().GetApiEntityManager();
            if (entityManager == null)
            {
                _active = false;
                _plugin.GetLogger().Error("Entity manager is not initialized.");
                return;
            }

            var user = entityManager.GetApiUserEntity();
            if (user == null)
            {
                _active = false;
                _plugin.GetLogger().Error("User entity is not initialized.");
                return;
            }

            var target = entityManager.GetEntityByName(_targetName, false, false);
            if (target == null)
            {
                _plugin.GetLogger().Warn("Cannot find entity with name '" + _targetName + "'.");
                return;
            }

            if (target.Pos == Vector3.Zero || user.Pos == Vector3.Zero)
            {
                _plugin.GetLogger().Warn("User or target position is empty.");
                return;
            }

            var dist = Vector3.Distance(target.Pos, user.Pos);

            if (dist > 250 || dist < 4.5f)
            {
                //_plugin.GetLogger().Info($"Target distance of {dist:0.0} m is not in the range to move.");
                return;
            }

            user.Dir = Vector3.Normalize(target.Pos - user.Pos);
            user.Front = user.Dir;
            user.Pos += user.Dir * (duration * (float)(RunSpeed / 1000f));

            _plugin.GetLogger().Info($"New user position is {user.Pos} with a speed of {RunSpeed}. Last update was {duration} ms ago.");
        }

        public override void OnGameJoined()
        {
            _isIngame = true;
        }

        public string Command(string cmd, string[] args)
        {
            if (cmd.IndexOf(" ", StringComparison.Ordinal) != -1)
            {
                cmd = cmd.Substring(0, cmd.IndexOf(" ", StringComparison.Ordinal));
            }

            if (!_isIngame)
            {
                _plugin.GetLogger().Warn("Not initialized.");
                return "";
            }

            switch (cmd.ToLower())
            {
                case "goto":
                    if (args.Length == 0)
                    {
                        var entityManager = _plugin.GetClient().GetApiNetworkManager().GetApiEntityManager();
                        if (entityManager == null)
                        {
                            _active = false;
                            _plugin.GetLogger().Warn("Entity manager is not initialized.");
                            return "";
                        }

                        var user = entityManager.GetApiUserEntity();
                        if (user == null) return "";

                        var target = entityManager.GetApiEntities()[user.TargetSlot()];

                        if (target == null)
                        {
                            _active = false;
                            _plugin.GetLogger().Info("No target. Stopping.");
                            return "";
                        }

                        _targetName = target.GetDisplayName();
                    }
                    else
                    {
                        _targetName = string.Join(" ", args);
                    }

                    _plugin.GetLogger().Info("Following " + _targetName);

                    _active = true;
                    return "";

                case "stop":
                    _plugin.GetLogger().Info("Stopping.");
                    _active = false;
                    return "";

                default:
                    _plugin.GetLogger()?.Warn("CommandBase unknown: " + cmd);
                    return "";
            }
        }
    }
}
