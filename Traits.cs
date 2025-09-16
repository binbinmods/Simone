using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Obeliskial_Content;
using UnityEngine;
using static Simone.CustomFunctions;
using static Simone.Plugin;
using static Simone.DescriptionFunctions;
using static Simone.CharacterFunctions;
using System.Text;
using TMPro;
using Obeliskial_Essentials;
using System.Data.Common;

namespace Simone
{
    [HarmonyPatch]
    internal class Traits
    {
        // list of your trait IDs

        public static string[] simpleTraitList = ["trait0", "trait1a", "trait1b", "trait2a", "trait2b", "trait3a", "trait3b", "trait4a", "trait4b"];

        public static string[] myTraitList = simpleTraitList.Select(trait => subclassname.ToLower() + trait).ToArray(); // Needs testing

        public static string trait0 = myTraitList[0];
        // static string trait1b = myTraitList[1];
        public static string trait2a = myTraitList[3];
        public static string trait2b = myTraitList[4];
        public static string trait4a = myTraitList[7];
        public static string trait4b = myTraitList[8];

        // public static int infiniteProctection = 0;
        // public static int bleedInfiniteProtection = 0;
        public static bool isDamagePreviewActive = false;

        public static bool isCalculateDamageActive = false;
        public static int infiniteProctection = 0;
        public static bool playedAttackingCard = false;
        public static int stealthGained = 0;

        public static string debugBase = "Binbin - Testing " + heroName + " ";


        public static void DoCustomTrait(string _trait, ref Trait __instance)
        {
            // get info you may need
            Enums.EventActivation _theEvent = Traverse.Create(__instance).Field("theEvent").GetValue<Enums.EventActivation>();
            Character _character = Traverse.Create(__instance).Field("character").GetValue<Character>();
            Character _target = Traverse.Create(__instance).Field("target").GetValue<Character>();
            int _auxInt = Traverse.Create(__instance).Field("auxInt").GetValue<int>();
            string _auxString = Traverse.Create(__instance).Field("auxString").GetValue<string>();
            CardData _castedCard = Traverse.Create(__instance).Field("castedCard").GetValue<CardData>();
            Traverse.Create(__instance).Field("character").SetValue(_character);
            Traverse.Create(__instance).Field("target").SetValue(_target);
            Traverse.Create(__instance).Field("theEvent").SetValue(_theEvent);
            Traverse.Create(__instance).Field("auxInt").SetValue(_auxInt);
            Traverse.Create(__instance).Field("auxString").SetValue(_auxString);
            Traverse.Create(__instance).Field("castedCard").SetValue(_castedCard);
            TraitData traitData = Globals.Instance.GetTraitData(_trait);
            List<CardData> cardDataList = [];
            List<string> heroHand = MatchManager.Instance.GetHeroHand(_character.HeroIndex);
            Hero[] teamHero = MatchManager.Instance.GetTeamHero();
            NPC[] teamNpc = MatchManager.Instance.GetTeamNPC();

            if (!IsLivingHero(_character))
            {
                return;
            }
            string traitName = traitData.TraitName;
            string traitId = _trait;


            if (_trait == trait0)
            {
                // trait0:
                // At the end of your turn, apply 1 Mark for every Stealth on you to the lowest HP monster.
                LogDebug($"Handling Trait {traitId}: {traitName}");
                Character lowestHPMonster = GetLowestHealthCharacter(teamNpc);
                if (IsLivingNPC(lowestHPMonster))
                {
                    int nToApply = _character.GetAuraCharges("stealth");
                    lowestHPMonster.SetAuraTrait(_character, "mark", nToApply);
                }
            }


            else if (_trait == trait2a)
            {
                // trait2a
                // Fast on you does not increase speed, but increases Stealth Damage by 3% per charge
                // Handled in GACM
            }



            else if (_trait == trait2b)
            {
                // trait2b:
                LogDebug($"Handling Trait {traitId}: {traitName}");
                // Increase Single Target damage by 50%. 
                // Decrease Damage from cards that target All Monsters, Global, or have Jump by 25%. 

            }

            else if (_trait == trait4a)
            {
                // trait 4a;
                // At the end of your turn, if you didn't play a Small Weapon or Attack this turn, 
                // gain 1 Stealth for every Stealth you gained during the turn.
                LogDebug($"Handling Trait {traitId}: {traitName}");
                if (!playedAttackingCard)
                {
                    _character.SetAuraTrait(_character, "stealth", stealthGained);
                }
            }

            else if (_trait == trait4b)
            {
                // trait 4b:
                // Mark on enemies increases All Damage received by 3 per charge.
                // Handled in GACM
                LogDebug($"Handling Trait {traitId}: {traitName}");
            }

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Trait), "DoTrait")]
        public static bool DoTrait(Enums.EventActivation _theEvent, string _trait, Character _character, Character _target, int _auxInt, string _auxString, CardData _castedCard, ref Trait __instance)
        {
            if ((UnityEngine.Object)MatchManager.Instance == (UnityEngine.Object)null)
                return false;
            Traverse.Create(__instance).Field("character").SetValue(_character);
            Traverse.Create(__instance).Field("target").SetValue(_target);
            Traverse.Create(__instance).Field("theEvent").SetValue(_theEvent);
            Traverse.Create(__instance).Field("auxInt").SetValue(_auxInt);
            Traverse.Create(__instance).Field("auxString").SetValue(_auxString);
            Traverse.Create(__instance).Field("castedCard").SetValue(_castedCard);
            if (Content.medsCustomTraitsSource.Contains(_trait) && myTraitList.Contains(_trait))
            {
                DoCustomTrait(_trait, ref __instance);
                return false;
            }
            return true;
        }



        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), "GlobalAuraCurseModificationByTraitsAndItems")]
        // [HarmonyPriority(Priority.Last)]
        public static void GlobalAuraCurseModificationByTraitsAndItemsPostfix(ref AtOManager __instance, ref AuraCurseData __result, string _type, string _acId, Character _characterCaster, Character _characterTarget)
        {
            // LogInfo($"GACM {subclassName}");

            Character characterOfInterest = _type == "set" ? _characterTarget : _characterCaster;
            string traitOfInterest;
            switch (_acId)
            {
                // trait2a:
                // Fast on you does not increase speed, 
                // but increases Stealth Damage by 3% per charge

                // trait2b:

                // trait 4a;

                // trait 4b:
                // Mark on enemies increases All Damage received by 3 per charge.

                case "fast":
                    traitOfInterest = trait2a;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.ThisHero))
                    {
                        __result.CharacterStatModified = Enums.CharacterStat.None;
                        __result.CharacterStatAbsoluteValuePerStack = 0;
                    }
                    break;
                case "stealth":
                    traitOfInterest = trait2b;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.ThisHero))
                    {
                        __result.AuraDamageIncreasedPercentPerStack += 2 * characterOfInterest.GetAuraCharges("fast");
                    }
                    break;
                case "mark":
                    traitOfInterest = trait4b;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.ThisHero))
                    {
                        __result.AuraDamageIncreasedPerStack = 3;
                    }
                    break;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterItem), nameof(CharacterItem.CalculateDamagePrePostForThisCharacter))]
        public static void CalculateDamagePrePostForThisCharacterPrefix()
        {
            isDamagePreviewActive = true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterItem), nameof(CharacterItem.CalculateDamagePrePostForThisCharacter))]
        public static void CalculateDamagePrePostForThisCharacterPostfix()
        {
            isDamagePreviewActive = false;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(MatchManager), nameof(MatchManager.SetDamagePreview))]
        public static void SetDamagePreviewPrefix()
        {
            isDamagePreviewActive = true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MatchManager), nameof(MatchManager.SetDamagePreview))]
        public static void SetDamagePreviewPostfix()
        {
            isDamagePreviewActive = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), nameof(Character.SetEvent))]
        public static void SetEventPostfix(
            Character __instance,
            Enums.EventActivation theEvent,
            Character target = null,
            int auxInt = 0,
            string auxString = "")
        {
            if (theEvent == Enums.EventActivation.BeginTurnAboutToDealCards)
            {

                playedAttackingCard = false;
                stealthGained = 0;
            }
            if (theEvent == Enums.EventActivation.CastCard &&
                (__instance.CardCasted.HasCardType(Enums.CardType.Attack) ||
                __instance.CardCasted.HasCardType(Enums.CardType.Small_Weapon)))
            {

                playedAttackingCard = true;
            }

            if (theEvent == Enums.EventActivation.AuraCurseSet && auxString == "stealth" && (target?.HaveTrait(trait4a) ?? false))
            {
                stealthGained += auxInt;
            }

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "DamageWithCharacterBonus")]
        public static void DamageWithCharacterBonusPrefix(
            ref int value,
            Enums.DamageType DT,
            Enums.CardClass CC,
            int energyCost,
            // ref int __result,
            Character __instance,
            CardData ___cardCasted)
        {
            string traitOfInterest = trait2b;
            // Increase Single Target damage by 50%. 
            // Decrease Damage from cards that target All Monsters, Global, or have Jump by 25%.
            if (IsLivingHero(__instance) && __instance.HaveTrait(traitOfInterest))
            {
                if (___cardCasted == null || MatchManager.Instance == null)
                {
                    return;
                }
                // Single hit and Special cards do 50% bonus damage for every energy spent.
                bool isSingleHit = ___cardCasted != null && ___cardCasted.TargetType == Enums.CardTargetType.Single;
                bool isAreaOrJump = ___cardCasted != null &&
                        (___cardCasted.TargetType == Enums.CardTargetType.Global ||
                        ___cardCasted.TargetType == Enums.CardTargetType.SingleSided ||
                        (___cardCasted.EffectRepeat > 1 && ___cardCasted.EffectRepeatTarget == Enums.EffectRepeatTarget.NoRepeat));

                if (isAreaOrJump)
                {
                    // int energy = MatchManager.Instance.energyJustWastedByHero;
                    // __result[1] += 50f * energy;
                    float multiplier = 0.75f;
                    value = Mathf.RoundToInt(multiplier * value);
                }
                else if (isSingleHit)
                {
                    // int energy = MatchManager.Instance.energyJustWastedByHero;
                    // __result[1] += 50f * energy;
                    float multiplier = 0.5f;
                    value += Mathf.RoundToInt(multiplier * value);
                }


            }
        }

    }
}

