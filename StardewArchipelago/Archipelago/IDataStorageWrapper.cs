using System;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net.Enums;

namespace StardewArchipelago.Archipelago
{
    public interface IDataStorageWrapper<T> where T : struct
    {

        void Set(Scope scope, string key, T value);

        T? Read(Scope scope, string key);

        Task<T?> ReadAsync(Scope scope, string key);

        Task<T?> ReadAsync(Scope scope, string key, Action<T> callback);

        bool Add(Scope scope, string key, T amount);

        bool Subtract(Scope scope, string key, T amount, bool dontGoBelowZero);
        bool Multiply(Scope scope, string key, int multiple);
        bool DivideByTwo(Scope scope, string key);
    }
}
