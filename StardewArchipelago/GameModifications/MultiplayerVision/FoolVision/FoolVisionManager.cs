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
        public const uint UPDATE_RATE_TICKS = 5;

        public static FoolVisionManager Instance = new FoolVisionManager();
        public List<FoolPlayerPath> FoolPlayerPaths { get; set; }
        public FoolPlayerPath CurrentRecordingPath { get; set; }
        public List<ActiveFoolPlayer> ActiveFoolPlayers { get; set; }
        private Random _random;

        private FoolVisionManager()
        {
            _random = new Random();
            FoolPlayerPaths = new List<FoolPlayerPath>();
            CurrentRecordingPath = null;
            ActiveFoolPlayers = new List<ActiveFoolPlayer>();
        }

        public void Update(UpdateTickedEventArgs updateTickedEventArgs)
        {
            if (!FoolManager.ShouldPrank())
            {
                //return;
            }
            UpdateRecording(updateTickedEventArgs);
            SpawnFoolPlayer(updateTickedEventArgs);
            UpdateFoolPlayers(updateTickedEventArgs);
        }

        private void UpdateRecording(UpdateTickedEventArgs updateTickedEventArgs)
        {
            if (!updateTickedEventArgs.IsMultipleOf(UPDATE_RATE_TICKS))
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
            var facingDirection = farmer.FacingDirection;

            CurrentRecordingPath.AddDataPoint(position, velocity, facingDirection);
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

        private void SpawnFoolPlayer(UpdateTickedEventArgs updateTickedEventArgs)
        {
            if (FoolPlayerPaths.Count < 2)
            {
                return;
            }

            var defaultRatePerMinute = 10d; //0.1;
            var numberRecordings = FoolPlayerPaths.Count;
            var ratePerMinute = defaultRatePerMinute * numberRecordings;
            var ratePerSecond = ratePerMinute / 60;
            var ticksBetweenSpawns = 60 / ratePerSecond;
            var ticksBetweenSpawnsCapped = (uint)Math.Round(Math.Max(1, ticksBetweenSpawns));

            if (!updateTickedEventArgs.IsMultipleOf(ticksBetweenSpawnsCapped))
            {
                return;
            }

            SpawnFoolPlayer();
        }

        private void SpawnFoolPlayer()
        {
            var chosenPath = FoolPlayerPaths[_random.Next(0, FoolPlayerPaths.Count)];
            var activeFoolPlayer = new ActiveFoolPlayer(chosenPath, _random);
            ActiveFoolPlayers.Add(activeFoolPlayer);
        }

        private void UpdateFoolPlayers(UpdateTickedEventArgs updateTickedEventArgs)
        {
            if (!updateTickedEventArgs.IsMultipleOf(UPDATE_RATE_TICKS))
            {
                return;
            }

            for (var i = ActiveFoolPlayers.Count - 1; i >= 0; i--)
            {
                var activeFoolPlayer = ActiveFoolPlayers[i];
                activeFoolPlayer.CurrentPathIndex++;
                if (activeFoolPlayer.CurrentPathIndex >= activeFoolPlayer.CurrentPath.DataPoints.Count)
                {
                    MultiplayerVisionInjections.RemoveVisiblePlayer(activeFoolPlayer.UniqueId);
                    ActiveFoolPlayers.RemoveAt(i);
                    continue;
                }
                if (activeFoolPlayer.CurrentPath.MapName != Game1.currentLocation.Name)
                {
                    continue;
                }
                var visiblePlayer = activeFoolPlayer.CreateVisiblePlayer();
                MultiplayerVisionInjections.AddVisiblePlayer(activeFoolPlayer.UniqueId, visiblePlayer);
            }
        }
    }

}
