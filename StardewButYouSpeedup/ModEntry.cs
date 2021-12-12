using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;

namespace StardewButYouSpeedup
{
    public class ModEntry : Mod
    {
		public static ModConfig Config;

		public static float CurrentModifier = 0f;

		private int CurrentTileDistance = 0;

		private int PreviousTileX = -1;
		private int PreviousTileY = -1;

		public override void Entry(IModHelper helper)
        {
			Harmony harmony = new(ModManifest.UniqueID);
			harmony.Patch(
				original: AccessTools.Method(typeof(Farmer), nameof(Farmer.getMovementSpeed)),
				prefix: new HarmonyMethod(typeof(ModEntry), nameof(this.Farmer_getMovementSpeed))
			);

			Config = helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
			CurrentModifier = Config.DefaultModifier;
			PreviousTileX = -1;
			PreviousTileY = -1;
			CurrentTileDistance = 0;
        }

        private static void IncreaseMovespeed()
        {
			if (Config.ModifierOperation == "+")
				CurrentModifier += Config.ModifierIncrement;
			else if (Config.ModifierOperation == "-")
				CurrentModifier -= Config.ModifierIncrement;
			else if (Config.ModifierOperation == "*")
				CurrentModifier *= Config.ModifierIncrement;
			else if (Config.ModifierOperation == "/")
				CurrentModifier /= Config.ModifierIncrement;

			if (Config.ModifierMinimum != -1)
				CurrentModifier = Math.Max(CurrentModifier, Config.ModifierMinimum);
			if (Config.ModifierMaximum != -1)
				CurrentModifier = Math.Min(CurrentModifier, Config.ModifierMaximum);
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
			if (!Context.IsPlayerFree)
				return;

			if (Config.IncreaseWithTileMovement)
            {
				if (PreviousTileX == -1)
					PreviousTileX = Game1.player.getTileX();
				if (PreviousTileY == -1)
					PreviousTileY = Game1.player.getTileY();

				if (Game1.player.getTileX() != PreviousTileX || Game1.player.getTileY() != PreviousTileY)
				{
					CurrentTileDistance += (int)Math.Sqrt(Math.Pow(PreviousTileX - Game1.player.getTileX(), 2) + Math.Pow(PreviousTileY - Game1.player.getTileY(), 2));

					PreviousTileX = Game1.player.getTileX();
					PreviousTileY = Game1.player.getTileY();

					if (CurrentTileDistance >= Config.IncreaseEveryNTiles)
					{
						IncreaseMovespeed();
						CurrentTileDistance = 0;
					}
				}
			}


			if (!Config.IncreaseWithTime || Config.IncreaseEveryNSeconds <= 0)
				return;

			if (e.IsMultipleOf((uint)(60 * Config.IncreaseEveryNSeconds)))
				IncreaseMovespeed();
        }

        public static bool Farmer_getMovementSpeed(Farmer __instance, ref float __result)
        {
			float movementSpeed = 1f;
			if (Game1.CurrentEvent == null || Game1.CurrentEvent.playerControlSequence)
			{
				__instance.movementMultiplier = 0.066f;
				movementSpeed = Math.Max(1f, (__instance.speed + (Game1.eventUp ? 0f : (__instance.addedSpeed + (__instance.isRidingHorse() ? 4.6f : __instance.temporarySpeedBuff)))) * __instance.movementMultiplier * Game1.currentGameTime.ElapsedGameTime.Milliseconds);
				if (__instance.movementDirections.Count > 1)
					movementSpeed = 0.7f * movementSpeed;

				if (Game1.CurrentEvent == null && __instance.hasBuff(19))
					movementSpeed = 0f;
			}
			else
			{
				movementSpeed = Math.Max(1f, __instance.speed + (Game1.eventUp ? (Math.Max(0, Game1.CurrentEvent.farmerAddedSpeed - 2)) : (__instance.addedSpeed + (__instance.isRidingHorse() ? 5f : __instance.temporarySpeedBuff))));
				if (__instance.movementDirections.Count > 1)
					movementSpeed = Math.Max(1, (int)Math.Sqrt(2f * (movementSpeed * movementSpeed)) / 2);
			}

			__result = CurrentModifier > 0f ? movementSpeed * CurrentModifier : movementSpeed;
			return false;
		}
    }
}