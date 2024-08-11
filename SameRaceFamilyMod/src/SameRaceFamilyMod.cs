using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SameRaceFamilyMod
{
    [StaticConstructorOnStartup]
    public static class SameRaceFamilyMod
    {
        private static readonly HashSet<string> HumanLikeRaces = new HashSet<string> { "Human", "Android", "Alien" }; // Add more as needed
        private static readonly HashSet<string> AnimalRaces = new HashSet<string> { "Thrumbo", "Muffalo", "Boomrat" }; // Add more as needed

        static SameRaceFamilyMod()
        {
            var harmony = new Harmony("com.yourname.sameracefamilymod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.Message("SameRaceFamilyMod initialized");
        }

        public static bool AreCompatibleForRelations(Pawn pawn1, Pawn pawn2)
        {
            if (pawn1 == null || pawn2 == null)
                return false;

            // Check for same race category (human-like or animal)
            bool pawn1IsHumanLike = HumanLikeRaces.Contains(pawn1.def.defName);
            bool pawn2IsHumanLike = HumanLikeRaces.Contains(pawn2.def.defName);
            bool pawn1IsAnimal = AnimalRaces.Contains(pawn1.def.defName);
            bool pawn2IsAnimal = AnimalRaces.Contains(pawn2.def.defName);

            if ((pawn1IsHumanLike && !pawn2IsHumanLike) || (pawn1IsAnimal && !pawn2IsAnimal))
            {
                Log.Message($"SameRaceFamilyMod: Preventing relation between {pawn1.Name} ({pawn1.def.defName}) and {pawn2.Name} ({pawn2.def.defName}) due to incompatible race categories.");
                return false;
            }

            // For human-like pawns, check xenotypes if Biotech is active
            if (pawn1IsHumanLike && pawn2IsHumanLike && ModsConfig.BiotechActive)
            {
                if (pawn1.genes?.Xenotype != pawn2.genes?.Xenotype)
                {
                    string xenotype1 = pawn1.genes?.Xenotype?.defName ?? "Unknown";
                    string xenotype2 = pawn2.genes?.Xenotype?.defName ?? "Unknown";
                    Log.Message($"SameRaceFamilyMod: Preventing relation between {pawn1.Name} (Xenotype: {xenotype1}) and {pawn2.Name} (Xenotype: {xenotype2}) due to different xenotypes.");
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(PawnRelationWorker), "InRelation")]
    public static class Patch_PawnRelationWorker_InRelation
    {
        public static void Postfix(PawnRelationWorker __instance, Pawn me, Pawn other, ref bool __result)
        {
            if (__result)
            {
                __result = SameRaceFamilyMod.AreCompatibleForRelations(me, other);
            }
        }
    }
}