using Archipelago.MultiClient.Net.Packets;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.GameModifications.CodeInjections;
using StardewArchipelago.GameModifications.EntranceRandomizer;
using StardewArchipelago.GameModifications.MoveLink;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewArchipelago.GameModifications.MultiplayerVision
{
    public static class MultiplayerVisionInjections
    {
        private static ILogger _logger;
        private static StardewArchipelagoClient _archipelago;
        private static Dictionary<string, VisiblePlayer> _visiblePlayers;
        private static Dictionary<string, PlayerAppearance> _playerAppearances;

        private static DateTime _timeLastBroadcast;
        private static DateTime _timeLastBroadcastAppearance;
        private static VisiblePlayer _lastBroadcast;
        private const float EPSILON = 0.1f;

        public static void Initialize(ILogger logger, StardewArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
            _visiblePlayers = new Dictionary<string, VisiblePlayer>();
            _playerAppearances = new Dictionary<string, PlayerAppearance>();
            _timeLastBroadcast = DateTime.MinValue;
            _timeLastBroadcastAppearance = DateTime.MinValue;
            _lastBroadcast = null;
        }

        public static void OnUpdateTicked(UpdateTickedEventArgs eventArgs)
        {
            if (eventArgs.IsMultipleOf(VisiblePlayer.TICK_FREQUENCY_WHEN_MOVING))
            {
                SendMultiplayerVision();
            }
        }

        public static void SendMultiplayerVision()
        {
            try
            {
                if (_logger == null || _archipelago == null || !ModEntry.Instance.Config.MultiplayerVision)
                {
                    return;
                }

                var timeSinceLastBroadcast = DateTime.Now - _timeLastBroadcast;
                if (timeSinceLastBroadcast.TotalSeconds <= VisiblePlayer.UPDATE_FREQUENCY_WHEN_MOVING_SECONDS)
                {
                    return;
                }

                var currentMap = Game1.currentLocation.Name;

                var farmer = Game1.player;
                var session = _archipelago.GetSession();
                var slotName = _archipelago.GetPlayerName();
                var identifier = $"{session.ConnectionInfo.Slot}-{ModEntry.Instance.UniqueIdentifier}";

                var movementSpeed = farmer.getMovementSpeed();
                var xVelocity = GetXVelocity(farmer, movementSpeed);
                var yVelocity = GetYVelocity(farmer, movementSpeed);

                var anyoneOnSameMap = _visiblePlayers.Any(x => x.Value.IsCloseEnough());
                double updateFrequency = VisiblePlayer.UPDATE_FREQUENCY_DIFFERENT_MAP_SECONDS;
                if (anyoneOnSameMap)
                {
                    if (_lastBroadcast != null && PlayerPositionsAreSame(Game1.player.Position, xVelocity, yVelocity, _lastBroadcast))
                    {
                        updateFrequency = Math.Min(updateFrequency, VisiblePlayer.UPDATE_FREQUENCY_WHEN_IMMOBILE_SECONDS);
                    }
                    else
                    {
                        updateFrequency = Math.Min(updateFrequency, VisiblePlayer.UPDATE_FREQUENCY_WHEN_MOVING_SECONDS);
                    }
                }
                if (_lastBroadcast != null && _lastBroadcast.MapName != currentMap)
                {
                    updateFrequency = Math.Min(updateFrequency, VisiblePlayer.UPDATE_FREQUENCY_WHEN_MOVING_SECONDS);
                }

                if (timeSinceLastBroadcast.TotalSeconds <= updateFrequency)
                {
                    return;
                }

                var timeSinceLastBroadcastAppearance = DateTime.Now - _timeLastBroadcastAppearance;
                var includeAppearance = timeSinceLastBroadcastAppearance.TotalSeconds >= VisiblePlayer.BROADCAST_APPEARANCE_SECONDS;

                var visiblePlayer = new VisiblePlayer()
                {
                    UniqueIdentifier = identifier,
                    MapName = currentMap,
                    Position = Game1.player.Position,
                    Velocity = new Vector2(xVelocity, yVelocity),
                    FacingDirection = farmer.FacingDirection,
                    IsGlowing = farmer.isGlowing,
                    XOffset = farmer.xOffset,
                    YOffset = farmer.yOffset,
                    IsSitting = farmer.IsSitting(),
                    IsRidingHorse = farmer.isRidingHorse(),
                    Rotation = farmer.rotation,
                };

                if (includeAppearance)
                {
                    _timeLastBroadcastAppearance = DateTime.Now;
                    // _logger.LogWarning($"{DateTime.Now} - Broadcasting appearance...");
                    var appearance = new PlayerAppearance()
                    {
                        IsMale = farmer.IsMale,

                        Skin = farmer.skin.Value,
                        Accessory = farmer.accessory.Value,

                        Hair =  farmer.getHair(),
                        HairColorRed = farmer.hairstyleColor.R,
                        HairColorGreen = farmer.hairstyleColor.G,
                        HairColorBlue = farmer.hairstyleColor.B,

                        EyeColorRed = farmer.newEyeColor.R,
                        EyeColorGreen = farmer.newEyeColor.G,
                        EyeColorBlue = farmer.newEyeColor.B,

                        PantsColorRed = farmer.pantsColor.R,
                        PantsColorGreen = farmer.pantsColor.G,
                        PantsColorBlue = farmer.pantsColor.B,

                        HatId = farmer.hat.Value?.ItemId ?? "",
                        ShirtId = farmer.GetShirtId(),
                        PantsId = farmer.GetPantsId(),
                        ShoesId = farmer.shoes.Value,
                    };
                    visiblePlayer.Appearance = appearance;
                }

                _logger.LogInfo($"Performing a MultiplayerVision update (appearance: {includeAppearance})");
                _timeLastBroadcast = DateTime.Now;
                _lastBroadcast = visiblePlayer;
                _archipelago.SendMultiplayerVisionPacket(visiblePlayer);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SendMultiplayerVision)}:\n{ex}");
                return;
            }
        }

        private static bool PlayerPositionsAreSame(Vector2 playerPosition, float xVelocity, float yVelocity, VisiblePlayer lastBroadcast)
        {
            return FloatAreSame(playerPosition.X, lastBroadcast.Position.X) &&
                   FloatAreSame(playerPosition.Y, lastBroadcast.Position.Y) &&
                   FloatAreSame(xVelocity, lastBroadcast.Velocity.X) &&
                   FloatAreSame(yVelocity, lastBroadcast.Velocity.X);
        }

        private static bool FloatAreSame(float floatA, float floatB)
        {
            return Math.Abs(floatA - floatB) <= 0.01f;
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

        // public override void draw(SpriteBatch b)
        public static void Draw_DrawOtherPlayers_Postfix(Farmer __instance, SpriteBatch b)
        {
            try
            {
                DrawAndUpdateOtherPlayers(b, 1);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Draw_DrawOtherPlayers_Postfix)}:\n{ex}");
                return;
            }
        }

        public static void DrawAndUpdateOtherPlayers(SpriteBatch spriteBatch, uint elapsedFrames)
        {
            if (!ModEntry.Instance.Config.MultiplayerVision || !_visiblePlayers.Any())
            {
                return;
            }

            foreach (var visiblePlayerId in _visiblePlayers.Keys.ToArray())
            {
                var visiblePlayer = _visiblePlayers[visiblePlayerId];
                if (visiblePlayer.DrawAndUpdate(spriteBatch, elapsedFrames))
                {
                    _visiblePlayers.Remove(visiblePlayerId);
                }
            }
        }

        public static void HandleBouncePacket(BouncePacket bouncePacket)
        {
            if (!ModEntry.Instance.Config.MultiplayerVision || _logger == null)
            {
                return;
            }

            if (!bouncePacket.Tags.Contains(StardewArchipelagoClient.MULTIPLAYER_VISION_TAG))
            {
                return;
            }

            var identifier = bouncePacket.Data.TryGetValue("identifier", out var identifierValue) ? identifierValue.ToObject<string>() : "";
            var mapName = bouncePacket.Data.TryGetValue("mapName", out var mapNameValue) ? mapNameValue.ToObject<string>() : "";
            if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(mapName) || identifier.Contains(ModEntry.Instance.UniqueIdentifier))
            {
                return;
            }

            var positionX = bouncePacket.Data.TryGetValue("positionX", out var positionXValue) ? positionXValue.ToObject<float>() : 0f;
            var positionY = bouncePacket.Data.TryGetValue("positionY", out var positionYValue) ? positionYValue.ToObject<float>() : 0f;
            var velocityX = bouncePacket.Data.TryGetValue("velocityX", out var velocityXValue) ? velocityXValue.ToObject<float>() : 0f;
            var velocityY = bouncePacket.Data.TryGetValue("velocityY", out var velocityYValue) ? velocityYValue.ToObject<float>() : 0f;
            var facingDirection = bouncePacket.Data.TryGetValue("facingDirection", out var facingDirectionValue) ? facingDirectionValue.ToObject<int>() : 0;
            var isGlowing = bouncePacket.Data.TryGetValue("isGlowing", out var isGlowingValue) ? isGlowingValue.ToObject<int>() >= 1 : false;
            var xOffset = bouncePacket.Data.TryGetValue("xOffset", out var xOffsetValue) ? xOffsetValue.ToObject<float>() : 0f;
            var yOffset = bouncePacket.Data.TryGetValue("yOffset", out var yOffsetValue) ? yOffsetValue.ToObject<float>() : 0f;
            var isSitting = bouncePacket.Data.TryGetValue("isSitting", out var isSittingValue) ? isSittingValue.ToObject<int>() >= 1 : false;
            var isRidingHorse = bouncePacket.Data.TryGetValue("isRidingHorse", out var isRidingHorseValue) ? isRidingHorseValue.ToObject<int>() >= 1 : false;
            var rotation = bouncePacket.Data.TryGetValue("rotation", out var rotationValue) ? rotationValue.ToObject<float>() : 0f;

            var visiblePlayer = new VisiblePlayer()
            {
                UniqueIdentifier = identifier,
                MapName = mapName,
                Position = new Vector2(positionX, positionY),
                Velocity = new Vector2(velocityX, velocityY),
                FacingDirection = facingDirection,
                IsGlowing = isGlowing,
                XOffset = xOffset,
                YOffset = yOffset,
                IsSitting = isSitting,
                IsRidingHorse = isRidingHorse,
                Rotation = rotation,
            };

            var appearance = TryGetAppearance(bouncePacket.Data);
            if (appearance != null)
            {
                _playerAppearances[identifier] = appearance;
            }

            if (_playerAppearances.ContainsKey(identifier))
            {
                visiblePlayer.Appearance = _playerAppearances[identifier];
            }

            if (_visiblePlayers.ContainsKey(identifier))
            {
                //if (appearance == null)
                //{
                //    visiblePlayer.Farmer = _visiblePlayers[identifier].Farmer;
                //}
                _visiblePlayers[identifier] = visiblePlayer;
            }
            else
            {
                _visiblePlayers.Add(identifier, visiblePlayer);
            }

            //_logger.LogInfo($"Received {ArchipelagoClient.MULTIPLAYER_VISION_TAG} packet{Environment.NewLine}" +
            //               $"  identifier: {identifier}{Environment.NewLine}" +
            //               $"  mapName: {mapName}{Environment.NewLine}" +
            //               $"  positionX: {positionX}{Environment.NewLine}" +
            //               $"  positionY: {positionY}");
        }

        private static PlayerAppearance TryGetAppearance(Dictionary<string, JToken> bouncePacketData)
        {
            PlayerAppearance appearance = null;

            if (!bouncePacketData.TryGetValue("isMale", out var isMaleValue))
            {
                return appearance;
            }

            var isMale = isMaleValue.ToObject<int>() >= 1;

            var skin = bouncePacketData.TryGetValue("skin", out var skinValue) ? skinValue.ToObject<int>() : 0;
            var accessory = bouncePacketData.TryGetValue("accessory", out var accessoryValue) ? accessoryValue.ToObject<int>() : 0;

            var hair = bouncePacketData.TryGetValue("hair", out var hairValue) ? hairValue.ToObject<int>() : 0;
            var hairColorRed = bouncePacketData.TryGetValue("hairColorRed", out var hairColorRedValue) ? hairColorRedValue.ToObject<int>() : 0;
            var hairColorGreen = bouncePacketData.TryGetValue("hairColorGreen", out var hairColorGreenValue) ? hairColorGreenValue.ToObject<int>() : 0;
            var hairColorBlue = bouncePacketData.TryGetValue("hairColorBlue", out var hairColorBlueValue) ? hairColorBlueValue.ToObject<int>() : 0;

            var eyeColorRed = bouncePacketData.TryGetValue("eyeColorRed", out var eyeColorRedValue) ? eyeColorRedValue.ToObject<int>() : 0;
            var eyeColorGreen = bouncePacketData.TryGetValue("eyeColorGreen", out var eyeColorGreenValue) ? eyeColorGreenValue.ToObject<int>() : 0;
            var eyeColorBlue = bouncePacketData.TryGetValue("eyeColorBlue", out var eyeColorBlueValue) ? eyeColorBlueValue.ToObject<int>() : 0;

            var hatId = bouncePacketData.TryGetValue("hatId", out var hatIdValue) ? hatIdValue.ToObject<string>() : "";
            var shirtId = bouncePacketData.TryGetValue("shirtId", out var shirtIdValue) ? shirtIdValue.ToObject<string>() : "";
            var pantsId = bouncePacketData.TryGetValue("pantsId", out var pantsIdValue) ? pantsIdValue.ToObject<string>() : "";
            var shoesId = bouncePacketData.TryGetValue("shoesId", out var shoesIdValue) ? shoesIdValue.ToObject<string>() : "";

            var pantsColorRed = bouncePacketData.TryGetValue("pantsColorRed", out var pantsColorRedValue) ? pantsColorRedValue.ToObject<int>() : 0;
            var pantsColorGreen = bouncePacketData.TryGetValue("pantsColorGreen", out var pantsColorGreenValue) ? pantsColorGreenValue.ToObject<int>() : 0;
            var pantsColorBlue = bouncePacketData.TryGetValue("pantsColorBlue", out var pantsColorBlueValue) ? pantsColorBlueValue.ToObject<int>() : 0;

            appearance = new PlayerAppearance()
            {
                IsMale = isMale,

                Skin = skin,
                Accessory = accessory,

                Hair = hair,
                HairColorRed = hairColorRed,
                HairColorGreen = hairColorGreen,
                HairColorBlue = hairColorBlue,

                EyeColorRed = eyeColorRed,
                EyeColorGreen = eyeColorGreen,
                EyeColorBlue = eyeColorBlue,

                PantsColorRed = pantsColorRed,
                PantsColorGreen = pantsColorGreen,
                PantsColorBlue = pantsColorBlue,

                HatId = hatId,
                ShirtId = shirtId,
                PantsId = pantsId,
                ShoesId = shoesId,
            };

            return appearance;
        }
    }
}
