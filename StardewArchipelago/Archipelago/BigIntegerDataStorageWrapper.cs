using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StardewArchipelago.Archipelago
{
    public class BigIntegerDataStorageWrapper : IDataStorageWrapper<BigInteger>
    {
        private readonly ILogger _logger;
        private readonly ArchipelagoSession _session;

        public BigIntegerDataStorageWrapper(ILogger logger, ArchipelagoSession session)
        {
            _logger = logger;
            _session = session;
        }

        public void Set(Scope scope, string key, BigInteger value)
        {
            var token = JToken.FromObject(value);
            _session.DataStorage[scope, key] = token;
        }

        public BigInteger? Read(Scope scope, string key)
        {
            try
            {
                _session.DataStorage[scope, key].Initialize(JToken.FromObject(0));
                var bigIntegerValue = _session.DataStorage[scope, key].To<BigInteger>();
                return bigIntegerValue;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Reading BigInteger from DataStorage key [{key}].{Environment.NewLine}Message: {ex.Message}{Environment.NewLine}Stack Trace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<BigInteger?> ReadAsync(Scope scope, string key)
        {
            return await ReadAsync(scope, key, null);
        }

        public async Task<BigInteger?> ReadAsync(Scope scope, string key, Action<BigInteger> callback)
        {
            try
            {
                _session.DataStorage[scope, key].Initialize(0);
                var bigIntegerValue = await _session.DataStorage[scope, key].GetAsync<BigInteger>();
                callback?.Invoke(bigIntegerValue);
                return bigIntegerValue;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Reading BigInteger from DataStorage key [{key}].{Environment.NewLine}Message: {ex.Message}{Environment.NewLine}Stack Trace: {ex.StackTrace}");
                return null;
            }
        }

        public bool Add(Scope scope, string key, BigInteger amount)
        {
            try
            {
                _session.Socket.SendPacket(
                    new EnergyLinkSetPacket
                    {
                        Key = key,
                        DefaultValue = 0,
                        Slot = _session.ConnectionInfo.Slot,
                        Operations = new OperationSpecification[]
                        {
                            new() { OperationType = OperationType.Add, Value = JToken.Parse(amount.ToString()) },
                        },
                    }
                );

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding {amount} to DataStorage key [{key}].{Environment.NewLine}Message: {ex.Message}{Environment.NewLine}Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        public bool Subtract(Scope scope, string key, BigInteger amount, bool dontGoBelowZero)
        {
            try
            {
                var operations = new List<OperationSpecification>
                {
                    new() { OperationType = OperationType.Add, Value = JToken.Parse("-" + amount) },
                };

                if (dontGoBelowZero)
                {
                    operations.Add(new() { OperationType = OperationType.Max, Value = 0 });
                }

                _session.Socket.SendPacket(
                    new EnergyLinkSetPacket
                    {
                        Key = key,
                        DefaultValue = 0,
                        Slot = _session.ConnectionInfo.Slot,
                        Operations = operations.ToArray(),
                    }
                );

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error subtracting {amount} from DataStorage key [{key}].{Environment.NewLine}Message: {ex.Message}{Environment.NewLine}Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        public bool Multiply(Scope scope, string key, int multiple)
        {
            try
            {
                _session.DataStorage[scope, key] *= multiple;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error multiplying by {multiple} in DataStorage key [{key}].{Environment.NewLine}Message: {ex.Message}{Environment.NewLine}Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        public bool DivideByTwo(Scope scope, string key)
        {
            try
            {
                _session.DataStorage[scope, key] += new OperationSpecification { OperationType = OperationType.RightShift };
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error dividing by 2 in DataStorage key [{key}].{Environment.NewLine}Message: {ex.Message}{Environment.NewLine}Stack Trace: {ex.StackTrace}");
                return false;
            }
        }
    }

    class EnergyLinkSetPacket : SetPacket
    {
        [JsonProperty("slot")] public int Slot { get; set; }
    }

}
