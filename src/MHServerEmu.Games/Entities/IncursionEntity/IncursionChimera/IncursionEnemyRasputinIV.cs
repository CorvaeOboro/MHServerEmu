using MHServerEmu.Games.GameData;

namespace MHServerEmu.Games.Entities.IncursionEntity
{
    /// <summary>
    /// Incursion
    /// Rasputin IV Invader
    /// Powers: 39 / 47 , five-gift chimera mutant created by Mister Sinister mixing collosus , quentin quire , kitty pryde , unus the untouchable , x23
    /// + organic steel = collosus ( Osmium Armor ,  Colossal Roar) , possibly emma frost ( Diamond Form ) 
    /// + telepathic = psylocke pink psy powers Psi-Strike Psi-Crush , emma frost (Telepathic Torment , Kneel Before Me , Sudden Dread), jean grey ( Psychic Hammer and Mind Crush  Psi Shield , Telepathic Illusion , Telekinetic Throw , Tactical Destruction , Psychokinetic Barrier)
    /// + intagible = kitty pryde ( Soulsword, Disciplined Strike , Logan-Style , Shadowcat's Pirouette, Just a Phase)
    /// + force field = invisible woman (Spheroid Typhoon , Shield Dome  , "Resonating Wave , Force Pillar , Steamroll , Wall of Force)
    /// + regenerative = x23 ( Trigger Scent ?) , wolverine?, not very compatible powers since we disabled regen  , should have dashes but doesnt have claws
    /// + weilds SoulSword magiks sword ( Soul Slashing , Soul Shockwave , Soul Cleaving ,Seven League Step )  , psylocke "Psi-Thrust" and "Kirisute Gomen" Tsunami Slash , Falling Lotus Strike ( kid omega pink color ) , 
    /// Damage scale per ability is listed below.
    /// NOTE: experimental , currently not included in random Incursions - she needs per ability animation overrides
    /// </summary>
    public class IncursionEnemyRasputinIV : IncursionEnemyAvatar
    {
        // Rasputin IV uses Magik's avatar due to the Soul Sword abilities , animations from other characters play Tpose - need to fix with per ability animation ovverides 
        private static readonly PrototypeId AvatarRef =
            GameDatabase.GetPrototypeRefByName("Entity/Characters/Avatars/Shipping/Magik.prototype");

        private static readonly PrototypeId CostumeRef =
            GameDatabase.GetPrototypeRefByName("Entity/Items/Costumes/Prototypes/Magik/SoulArmor.prototype");
        // Costumes: MarvelNOW, NewMutants, PhoenixForce, SoulArmor

        public IncursionEnemyRasputinIV(Game game) : base(game) { }

        public override PrototypeId RenderAvatarRef => AvatarRef;
        public override PrototypeId RenderCostumeRef => CostumeRef;

        // Rasputin IV Invader - Custom Display name
        // ЯΛSPЦTI₦ IV //  example glyphs = ЦGGΣЯ₦Λ
        public override string InvaderDisplayName => "ЯΛSPЦTI₦ IV";

        // Base Incursion Attributes
        protected override int ThinkIntervalMs => 250;
        protected override float AttackRange => 150.0f;
        protected override float ChaseRange => 5000.0f;
        protected override float GlobalAttackCooldownMs => 1500.0f;
        protected override float PerPowerCooldownMs => 10000.0f;
        protected override float DamageScale => 0.05f; // this is fallback if some secondary effect is not listed below

        // Powers Available and Damage Scaling
        protected override IncursionPowerEntry[] PowerTable => _powerTable;

        private static readonly IncursionPowerEntry[] _powerTable =
        {
            // Organic steel - Colossus 
            new("Powers/Player/Colossus/ArmoringPunch.prototype",                      false,  0.05f),
            new("Powers/Player/Colossus/Punch.prototype",                              false,  0.1022f), // 2026-06-05
            new("Powers/Player/Colossus/MetalCharge.prototype",                        false,  0.1975f), // 2026-06-05
            new("Powers/Player/Colossus/Shockwave.prototype",                          true,  0.0856f), // 2026-06-05
            new("Powers/Player/Colossus/DeathFromAbove.prototype",                     false,  0.0885f), // 2026-06-08
            new("Powers/Player/Colossus/MetalRegeneration.prototype",                  false,  0.0312f), // 2026-06-05
            new("Powers/Player/Colossus/MagikEldritchArmor.prototype",                 false,  0.05f),

            // Organic steel - Emma Frost 
            new("Powers/Player/EmmaFrost/Update/DiamondArmorCondition.prototype",      false,  0.05f),
            new("Powers/Player/EmmaFrost/Update/DiamondSweepKick.prototype",           false,  0.1101f), // 2026-06-05
            new("Powers/Player/EmmaFrost/Update/DiamondWhirlwind.prototype",           false,  0.0843f), // 2026-06-07

            // Telepathic - Emma Frost  + Jean Grey
            new("Powers/Player/EmmaFrost/Update/Drain.prototype",                      true,  0.0434f), // 2026-06-05
            new("Powers/Player/EmmaFrost/Update/AoEFear.prototype",                    true,  0.1146f), // 2026-06-05
            new("Powers/Player/EmmaFrost/Update/PsychicSpear.prototype",               false,  0.0740f), // 2026-06-07
            new("Powers/Player/EmmaFrost/Update/KneelBeforeMe.prototype",                  true,  0.05f),
            new("Powers/Player/JeanGrey/Rework/PsychicHammerJean.prototype",           true,  0.0820f), // 2026-06-06
            new("Powers/Player/JeanGrey/Rework/ImplosionJean.prototype",               false,  0.05f),
            new("Powers/Player/JeanGrey/Rework/NeuralNetworkJean.prototype",           false,  0.0780f), // 2026-06-06
            new("Powers/Player/JeanGrey/PsiShield.prototype",                          true,  0.05f),
            new("Powers/Player/JeanGrey/Rework/TelepathicIllusionJean.prototype",      true,  0.05f),
            new("Powers/Player/JeanGrey/Rework/TKTossJean.prototype",                  true,  0.05f),
            new("Powers/Player/JeanGrey/Rework/DamageMaelstrom.prototype",             true,  0.0269f), // 2026-06-03
            new("Powers/Player/JeanGrey/Rework/ForcePushJean.prototype",               true,  0.1209f), // 2026-06-03

            // Intangible - Kitty Pryde
            new("Powers/Player/KittyPryde/PhaseOut.prototype",                         false,  0.05f),
            new("Powers/Player/KittyPryde/PhaseDash.prototype",                        false,  0.1362f), // 2026-06-08
            new("Powers/Player/KittyPryde/DeathFromBelow.prototype",                   false,  0.0369f), // 2026-06-08
            new("Powers/Player/KittyPryde/NoCollisionPassive.prototype",               false, 0.05f),

            // Force field - Invisible Woman 
            new("Powers/Player/InvisibleWoman/BoomerangBubble.prototype",              true,  0.1584f), // 2026-06-03
            new("Powers/Player/InvisibleWoman/DefenseHotspot.prototype",               false,  0.008f),
            new("Powers/Player/InvisibleWoman/ShockwavePBAoE.prototype",               true,  0.03f),
            new("Powers/Player/InvisibleWoman/ForceWall.prototype",                    true,  0.05f),
            new("Powers/Player/InvisibleWoman/OrbStorm.prototype",                       true,  0.1129f), // 2026-06-03
            new("Powers/Player/InvisibleWoman/Pancake.prototype",                      true,  0.126f), // 2026-06-03

            // Regenerative - Wolverine  X23 
            new("Powers/Player/Wolverine/RapidRegeneration.prototype",                 false,  0.05f),
            new("Powers/Player/Wolverine/BerserkerBarrage.prototype",                  false,  0.0436f), // 2026-06-07
            new("Powers/Player/Wolverine/Lunge.prototype",                             false,  0.1050f), // 2026-06-07
            new("Powers/Player/Wolverine/Frenzy.prototype",                            false,  0.02f),
            new("Powers/Player/X23/BasicBloody.prototype",                             false,  0.1295f), // 2026-06-03
            new("Powers/Player/X23/FuriousLunge.prototype",                            false,  0.1793f), // 2026-06-03
            new("Powers/Player/X23/UltTriggerScent.prototype",                         true,  0.006f),
            new("Powers/Player/X23/BladeSpin.prototype",                               false,  0.0381f), // 2026-06-03

            // Soulsword - Magik 
            new("Powers/Player/Magik/SoulswordBasic.prototype",                        true,  0.1446f), // 2026-06-08
            new("Powers/Player/Magik/SoulswordWideSlash.prototype",                    true,  0.0815f), // 2026-06-08
            new("Powers/Player/Magik/SoulCone.prototype",                              true,  0.0987f), // 2026-06-03
            new("Powers/Player/Magik/Teleport.prototype",                              false,  0.1253f), // 2026-06-08
            new("Powers/Player/Magik/Ultimate.prototype",                              false,  0.0059f), // 2026-06-03

            // Bladework - Psylocke , Deadpool  Blade 
            new("Powers/Player/Psylocke/PsiKatanaCone.prototype",                      true,  0.1102f), // 2026-06-03
            new("Powers/Player/Psylocke/DashBackstab.prototype",                       false,  0.1141f), // 2026-06-03
            new("Powers/Player/Deadpool/Rework/OmnislashTeleport.prototype",           false, 0.0473f), // 2026-06-06
            new("Powers/Player/Blade/Helichopter.prototype",                           false,  0.0887f), // 2026-06-05
            new("Powers/Player/Blade/SwordDash.prototype",                             false, 0.1378f), // 2026-06-05

            // Shared 
            new("Powers/Player/TravelPower/MagikFlight.prototype",                     false, 0.05f),
            new("Powers/Blueprints/Conditions/CCReactCondition.prototype",             false, 0.05f),
            new("Powers/Player/Active/ResurrectAnimOnly.prototype",                    false, 0.05f),
            new("Powers/Player/Active/ResurrectOtherEntityPower.prototype",            false, 0.05f),
            new("Powers/Player/HealthAndEnduranceOnHitEffect.prototype",               false, 0.05f),
            new("Powers/Player/OutOfCombatHealingOverTime.prototype",                  false, 0.05f),
            new("Powers/Player/Passive/StatsPassive.prototype",                        false, 0.05f),
        };
    }
}
