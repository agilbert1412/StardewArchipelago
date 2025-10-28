using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Packets;
using Microsoft.Xna.Framework;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.Utilities.Interfaces;
using StardewValley;
using StardewArchipelago.GameModifications.CodeInjections;
using StardewModdingAPI.Events;

namespace StardewArchipelago.GameModifications.MoveLink
{
    public static class MovementInjections
    {
        private static ILogger _logger;
        private static ArchipelagoClient _archipelago;
        private static List<ReceivedMovement> _queuedMovements;
        private static float _totalDistanceTravelled;

        private const float TILE_FACTOR = 128f;
        private const float EPSILON = 0.1f;
        private const float TRAVEL_THRESHOLD = 40f;

        public static void Initialize(ILogger logger, ArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
            _queuedMovements = new List<ReceivedMovement>();
            _totalDistanceTravelled = 0;
        }

        public static void AddMoveLinkMovement(float time, float x, float y)
        {
            if (!FoolManager.ShouldPrank() || time <= 0 || (Math.Abs(x) < EPSILON && Math.Abs(y) < EPSILON))
            {
                return;
            }

            _queuedMovements.Add(new ReceivedMovement(time, x, y));
        }

        public static void UpdateMove(UpdateTickedEventArgs eventArgs)
        {
            if (!FoolManager.ShouldPrank())
            {
                return;
            }

            const uint moveLinkSendInterval = 15;
            const uint moveLinkApplyInterval = 1;
            if (eventArgs.IsMultipleOf(moveLinkSendInterval))
            {
                SendMoveLink(moveLinkSendInterval);
            }
            if (eventArgs.IsMultipleOf(moveLinkApplyInterval))
            {
                ApplyMoveLink(moveLinkApplyInterval);
            }
        }

        public static void SendMoveLink(uint elapsedFrames)
        {
            try
            {
                if (_logger == null || _archipelago == null || !FoolManager.ShouldPrank())
                {
                    return;
                }

                var farmer = Game1.player;
                var session = _archipelago.GetSession();
                var slot = $"{session.ConnectionInfo.Slot}-{ModEntry.Instance.UniqueIdentifier}";

                var movementSpeed = farmer.getMovementSpeed();
                var xVelocity = GetXVelocity(farmer, movementSpeed) / TILE_FACTOR * elapsedFrames;
                var yVelocity = GetYVelocity(farmer, movementSpeed) / TILE_FACTOR * elapsedFrames;
                var timespan = elapsedFrames / 60f;

                if (Math.Abs(xVelocity) < EPSILON && Math.Abs(yVelocity) < EPSILON)
                {
                    return;
                }

                _archipelago.SendMoveLinkPacket(slot, timespan, xVelocity, yVelocity);

                //_logger.LogInfo($"Sent {ArchipelagoClient.MOVE_LINK_TAG} packet{Environment.NewLine}" +
                //                $"  slot: {slot}{Environment.NewLine}" +
                //                $"  timespan: {timespan}{Environment.NewLine}" +
                //                $"  x: {xVelocity}{Environment.NewLine}" +
                //                $"  y: {yVelocity}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SendMoveLink)}:\n{ex}");
                return;
            }
        }
        private static float GetXVelocity(Farmer farmer, float movementSpeed)
        {
            var xVelocity = 0f;
            if (farmer.movementDirections.Contains(1))
            {
                xVelocity += movementSpeed;
            }
            if (farmer.movementDirections.Contains(3))
            {
                xVelocity -= movementSpeed;
            }
            return xVelocity;
        }
        private static float GetYVelocity(Farmer farmer, float movementSpeed)
        {
            var yVelocity = 0f;
            if (farmer.movementDirections.Contains(0))
            {
                yVelocity -= movementSpeed;
            }
            if (farmer.movementDirections.Contains(2))
            {
                yVelocity += movementSpeed;
            }
            return yVelocity;
        }

        public static void ApplyMoveLink(uint elapsedFrames)
        {
            if (!FoolManager.ShouldPrank() || !_queuedMovements.Any() || (Game1.eventUp && Game1.CurrentEvent?.isFestival == false))
            {
                return;
            }

            var farmer = Game1.player;
            var currentLocation = Game1.currentLocation;
            var timespan = elapsedFrames / 60f;

            var currentMove = _queuedMovements[0];

            var x = currentMove.X;
            var y = currentMove.Y;

            while (currentMove.TimeRemaining <= 0)
            {
                _queuedMovements.RemoveAt(0);
                if (!_queuedMovements.Any())
                {
                    return;
                }
                currentMove = _queuedMovements[0];
                x = currentMove.X;
                y = currentMove.Y;
            }

            while (Math.Abs(currentMove.X) < EPSILON && Math.Abs(currentMove.Y) < EPSILON)
            {
                _queuedMovements.RemoveAt(0);
                if (!_queuedMovements.Any())
                {
                    return;
                }

                currentMove = _queuedMovements[0];
                x += currentMove.X;
                y += currentMove.Y;
            }
            if (currentMove.TimeRemaining <= timespan)
            {
                _queuedMovements.RemoveAt(0);
            }
            else if (currentMove.TimeRemaining > timespan)
            {
                var factor = timespan / currentMove.TimeRemaining;
                var factoredX = x * factor;
                var factoredY = y * factor;
                x = factoredX;
                y = factoredY;
                currentMove.X -= factoredX;
                currentMove.Y -= factoredY;
                currentMove.TimeRemaining -= timespan;
            }

            var hasTravelledEnough = _totalDistanceTravelled >= TRAVEL_THRESHOLD;
            _totalDistanceTravelled += (Math.Abs(x) + Math.Abs(y));
            if (!hasTravelledEnough && _totalDistanceTravelled >= TRAVEL_THRESHOLD)
            {
                Game1.chatBox?.addMessage($"April's Fool!", Color.Orange);
                Game1.chatBox?.addMessage($"Type !!fish to disable MoveLink", Color.Orange);
            }
            MovePlayer(farmer, currentLocation, x * TILE_FACTOR, y * TILE_FACTOR);
        }
        
        private static void MovePlayer(Farmer farmer, GameLocation currentLocation, float x, float y)
        {
            var warpBoundingBox = farmer.GetBoundingBox();
            warpBoundingBox.X += (int)Math.Ceiling(x);
            warpBoundingBox.Y += (int)Math.Ceiling(y);
            var w = Game1.currentLocation.isCollidingWithWarp(warpBoundingBox, farmer);
            if (w != null && farmer.IsLocalPlayer)
            {
                if (Game1.eventUp)
                {
                    bool? isFestival = Game1.CurrentEvent?.isFestival;
                    if (isFestival.HasValue && isFestival.GetValueOrDefault())
                    {
                        Game1.CurrentEvent.TryStartEndFestivalDialogue(farmer);
                        return;
                    }
                }
                farmer.warpFarmer(w, farmer.FacingDirection);
                return;
            }

            var boundingBox = farmer.GetBoundingBox();
            var floorRectangle = new Rectangle(boundingBox.X + (int)Math.Floor(x), boundingBox.Y + (int)Math.Floor(y), boundingBox.Width, boundingBox.Height);
            var ceilingRectangle = new Rectangle(boundingBox.X + (int)Math.Ceiling(x), boundingBox.Y + (int)Math.Ceiling(y), boundingBox.Width, boundingBox.Height);
            var position = Rectangle.Union(floorRectangle, ceilingRectangle);
            if (!currentLocation.isCollidingPosition(position, Game1.viewport, true, -1, false, farmer))
            {
                farmer.position.X += x;
                farmer.position.Y += y;
            }
        }

        public static void HandleBouncePacket(BouncePacket bouncePacket)
        {
            if (!FoolManager.ShouldPrank())
            {
                return;
            }

            if (!bouncePacket.Tags.Contains(ArchipelagoClient.MOVE_LINK_TAG))
            {
                return;
            }

            var slot = bouncePacket.Data.TryGetValue("slot", out var slotValue) ? slotValue.ToObject<string>() : "";
            if (string.IsNullOrWhiteSpace(slot) || slot.Contains(ModEntry.Instance.UniqueIdentifier))
            {
                return;
            }
            var timespan = bouncePacket.Data.TryGetValue("timespan", out var timeValue) ? timeValue.ToObject<float>() : 0.25f;
            var x = bouncePacket.Data.TryGetValue("x", out var xValue) ? xValue.ToObject<float>() : 0f;
            var y = bouncePacket.Data.TryGetValue("y", out var yValue) ? yValue.ToObject<float>() : 0f;

            AddMoveLinkMovement(timespan, x, y);

            //_logger.LogInfo($"Received {ArchipelagoClient.MOVE_LINK_TAG} packet{Environment.NewLine}" +
            //               $"  slot: {slot}{Environment.NewLine}" +
            //               $"  timespan: {timespan}{Environment.NewLine}" +
            //               $"  x: {x}{Environment.NewLine}" +
            //               $"  y: {y}");
        }
    }
}
