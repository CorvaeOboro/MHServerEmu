using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.PlayableExpanded
{
    /// <summary>
    /// PlayableExpanded
    /// Jubilee - new playable character using the Jubilee Team-Up assets.
    /// Powers: 7 hotbar actives (away-passive variants and the synergy passive are excluded -
    /// those belong to the real Team-Up pet pipeline, which this mod does not touch).
    /// Damage scale per ability is listed below (Team-Up powers are tuned as supplemental
    /// pet DPS, so they need a boost to feel like a main character).
    /// </summary>
    public class ExpandedJubilee : ExpandedCharacter
    {
        private static readonly PrototypeId BodyRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/TeamUps/Jubilee.prototype");

        public override PrototypeId BodyProtoRef => BodyRef;
        public override string DisplayName => "Jubilee";

        // Base attributes
        public override float DamageScale => 3.0f; // fallback for anything not listed below
        public override int ThinkIntervalMs => 20;

        // Hotbar powers in slot order (LMB, RMB, then action keys) and damage scaling.
        protected override ExpandedPowerEntry[] PowerTable => _powerTable;

        private static readonly ExpandedPowerEntry[] _powerTable =
        {
            // Cast times are conservative estimates (ms) so the avatar stays frozen long enough
            // for the full power animation + tail to play before the body snaps back.
            new("Powers/TeamUps/Jubilee/Boom.prototype",           true, 3.0f,  500),  // basic attack - quick firework burst
            new("Powers/TeamUps/Jubilee/HomingMissiles.prototype", true, 3.0f,  900),  // missile volley launch
            new("Powers/TeamUps/Jubilee/TripleWave.prototype",     true, 3.0f,  700),  // triple energy wave
            new("Powers/TeamUps/Jubilee/Hotspot.prototype",        true, 3.0f,  600),  // AOE placement
            new("Powers/TeamUps/Jubilee/Multiglob.prototype",      true, 3.0f,  600),  // multi-projectile burst
            new("Powers/TeamUps/Jubilee/Superglob.prototype",      true, 3.0f,  800),  // charged projectile
            new("Powers/TeamUps/Jubilee/GrandFinale.prototype",    true, 3.0f, 1500), // signature / ultimate
        };
    }
}
