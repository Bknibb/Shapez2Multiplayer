using Core.Collections;
using Core.Collections.Scoped;
using HarmonyLib;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.UI;

namespace Shapez2Multiplayer
{
    public static class Extensions
    {
        public static List<KeyValuePair<TKey, TValue>> GetAllKVPs<TKey, TValue>(this DashMap<TKey, TValue> dashMap) where TKey : IEquatable<TKey>
        {
            var type = typeof(DashMap<TKey, TValue>);
            MethodInfo DashMapLockShards = AccessTools.Method(type, "LockShards");
            MethodInfo DashMapUnlockShards = AccessTools.Method(type, "UnlockShards");
            FieldInfo DashMapShards = AccessTools.Field(type, "Shards");
            List<KeyValuePair<TKey, TValue>> keyValuePairs = new List<KeyValuePair<TKey, TValue>>();
            DashMapLockShards.Invoke(dashMap, new object[] { });
            var Shards = (ScopedList<ScopedDictionary<TKey, TValue>>)DashMapShards.GetValue(dashMap);
            foreach (ScopedDictionary<TKey, TValue> scopedDictionary in Shards)
            {
                foreach (var kvp in scopedDictionary)
                {
                    keyValuePairs.Add(kvp);
                }
            }
            DashMapUnlockShards.Invoke(dashMap, new object[] { });
            return keyValuePairs;
        }
        public static bool TryScheduleActionNoDetection(this PlayerActionManager playerActionManager, IPlayerAction action)
        {
            Shapez2Multiplayer.ActionDetection = false;
            var result = playerActionManager.TryScheduleAction(action);
            Shapez2Multiplayer.ActionDetection = true;
            return result;
        }
        public static readonly MethodInfo HUDDialogCloseInfo = AccessTools.Method(typeof(HUDDialog), "Close");
        public static void Close(this HUDDialog dialog)
        {
            HUDDialogCloseInfo.Invoke(dialog, new object[] { });
        }
        public static string GetGenericFriendlyName(this Type type)
        {
            if (!type.IsGenericType) return type.Name;

            var genericArgs = string.Join(", ", type.GetGenericArguments().Select(GetGenericFriendlyName));
            return $"{type.Name.Split('`')[0]}<{genericArgs}>";
        }
        public static readonly FieldInfo HUDMenuButtonUIButtonInfo = AccessTools.Field(typeof(HUDMenuButton), "UIButton");
        public static void SetInteractable(this HUDMenuButton button, bool interactable)
        {
            ((Button)HUDMenuButtonUIButtonInfo.GetValue(button)).interactable = interactable;
        }
        public static async Task RefreshAsync(this Lobby lobby)
        {
            TaskCompletionSource<bool> resultWaiter = new TaskCompletionSource<bool>();
            Action<Lobby> eventHandler = (Lobby queriedLobby) =>
            {
                if (lobby.Id != queriedLobby.Id) return;
                resultWaiter.SetResult(true);
            };

            SteamMatchmaking.OnLobbyDataChanged += eventHandler;
            lobby.Refresh();
            var result = await resultWaiter.Task;
            SteamMatchmaking.OnLobbyDataChanged -= eventHandler;
        }
    }
}
