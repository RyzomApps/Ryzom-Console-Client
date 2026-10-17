///////////////////////////////////////////////////////////////////
// Helper: server rejects bot-chat commands silently when the
// player is farther than MaxTalkingDist (8 m) from the NPC.
///////////////////////////////////////////////////////////////////
using System;
using Client.Network;
using Client.Entity;

namespace Client.Commands
{
    /// <summary>
    /// Client-side check mirroring CCharacter::startBotChat on the server:
    /// the bot must be within MaxTalkingDist (8 m) and be the current target,
    /// otherwise the server silently drops BOTCHAT messages.
    /// </summary>
    public static class BotChatProximity
    {
        /// <summary>MaxTalkingDistSquare from constants.h = 64 (8 m).</summary>
        public const float MaxTalkingDistance = 8.0f;

        /// <summary>
        /// Returns null if the target entity is a bot within talking distance,
        /// otherwise an error text describing the problem.
        /// </summary>
        public static string Check(RyzomClient ryzomClient)
        {
            var entityManager = ryzomClient.GetNetworkManager().GetEntityManager();
            if (entityManager == null)
                return "Entity manager is null.";

            var user = entityManager.UserEntity;
            if (user == null)
                return "User entity is null.";

            var entity = entityManager.GetEntity(user.TargetSlot());
            if (entity == null)
                return "No target entity. Target the NPC first with 'target <bot>'.";

            var dx = entity.Pos.X - user.Pos.X;
            var dy = entity.Pos.Y - user.Pos.Y;
            var dist = (float)Math.Sqrt(dx * dx + dy * dy);

            if (dist > MaxTalkingDistance)
                return $"Too far from the bot: {dist:0.0} m (max {MaxTalkingDistance:0} m). " +
                       "Walk up to the bot before using this command - the server silently drops it otherwise.";
            return null;
        }
    }
}
