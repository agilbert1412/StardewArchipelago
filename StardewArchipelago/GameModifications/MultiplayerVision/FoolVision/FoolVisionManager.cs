using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewArchipelago.GameModifications.CodeInjections;
using StardewModdingAPI.Events;
using StardewValley;

namespace StardewArchipelago.GameModifications.MultiplayerVision.FoolVision
{
    public class FoolVisionManager
    {
        public static FoolVisionManager Instance = new FoolVisionManager();
        public List<FoolPlayerPath> FoolPlayerPaths { get; set; }
        public FoolPlayerPath CurrentRecordingPath { get; set; }
        public List<ActiveFoolPlayer> ActiveFishPlayers { get; set; }
        private Random _random;

        private FoolVisionManager()
        {
            _random = new Random();
            FoolPlayerPaths = new List<FoolPlayerPath>();
            CurrentRecordingPath = null;
            ActiveFishPlayers = new List<ActiveFoolPlayer>();
        }

        public void Update(UpdateTickedEventArgs updateTickedEventArgs)
        {
            if (!FoolManager.ShouldPrank())
            {
                //return;
            }
            UpdateRecording(updateTickedEventArgs);
            SpawnFishPlayer(updateTickedEventArgs);
            UpdateFishPlayers(updateTickedEventArgs);
        }

        private void UpdateRecording(UpdateTickedEventArgs updateTickedEventArgs)
        {
            if (!updateTickedEventArgs.IsMultipleOf(5))
            {
                return;
            }

            if (CurrentRecordingPath == null || CurrentRecordingPath.MapName != Game1.player.currentLocation.Name)
            {
                FinishRecording();
            }

            RecordCurrentStep();
        }

        private void RecordCurrentStep()
        {
            var farmer = Game1.player;

            var position = farmer.Position;
            var movementSpeed = farmer.getMovementSpeed();
            var xVelocity = MultiplayerVisionInjections.GetXVelocity(farmer, movementSpeed);
            var yVelocity = MultiplayerVisionInjections.GetYVelocity(farmer, movementSpeed);
            var velocity = new Vector2(xVelocity, yVelocity);

            CurrentRecordingPath.AddDataPoint(position, velocity);
        }

        private void FinishRecording()
        {
            if (CurrentRecordingPath != null)
            {
                if (CurrentRecordingPath.DataPoints.Count >= 4)
                {
                    FoolPlayerPaths.Add(CurrentRecordingPath);
                }
                CurrentRecordingPath = null;
            }

            CurrentRecordingPath = new FoolPlayerPath(Game1.player.currentLocation.Name);
        }

        private void SpawnFishPlayer(UpdateTickedEventArgs updateTickedEventArgs)
        {
            if (FoolPlayerPaths.Count < 2)
            {
                return;
            }

            var defaultRatePerMinute = 0.1;
            var numberRecordings = FoolPlayerPaths.Count;
            var ratePerMinute = defaultRatePerMinute * numberRecordings;
            var ratePerSecond = ratePerMinute / 60;
            var ticksBetweenSpawns = 60 / ratePerSecond;
            var ticksBetweenSpawnsCapped = (uint)Math.Round(Math.Max(1, ticksBetweenSpawns));

            if (!updateTickedEventArgs.IsMultipleOf(ticksBetweenSpawnsCapped))
            {
                return;
            }

            SpawnFishPlayer();
        }

        private void SpawnFishPlayer()
        {
            var chosenPath = FoolPlayerPaths[_random.Next(0, FoolPlayerPaths.Count)];
            var activeFishPlayer = new ActiveFoolPlayer(chosenPath);
            ActiveFishPlayers.Add(activeFishPlayer);
        }

        private void UpdateFishPlayers(UpdateTickedEventArgs updateTickedEventArgs)
        {
            throw new System.NotImplementedException();
        }
    }

}
