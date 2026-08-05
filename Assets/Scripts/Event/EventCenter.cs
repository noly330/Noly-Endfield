using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endfield
{
    public static class EventCenter
    {
        //in:泛型逆变，使T只能用作方法入参，不能当返回值
        public delegate void EvenetHandle<in T>(T message) where T : struct;
        private static readonly Dictionary<Type,Delegate> DelegateDict = new();

        public static void SubscribeListener<T>(EvenetHandle<T> handle) where T : struct
        {
            if(handle == null)
            {
                Debug.LogError("事件中心新增的监听器为Null");
                return;
            }
            var type = typeof(T);
            if(DelegateDict.TryGetValue(type,out var delegates))
            {
                //把两个委托里挂载的所有方法合并，生成一个全新的【多播委托】
                DelegateDict[type] = Delegate.Combine(delegates,handle);
            }
            else
            {
                DelegateDict[type] = handle;
            }
        }

        public static void UnsubscribeListener<T>(EvenetHandle<T> handle) where T : struct
        {
            var type = typeof(T);
            if(!DelegateDict.TryGetValue(type,out var delegates))
            {
                Debug.LogError($"事件中心没有找到类型为{type}的监听器");
                return;
            }
            DelegateDict[type] = Delegate.Remove(delegates,handle);
            if(DelegateDict[type] == null)
            {
                DelegateDict.Remove(type);
            }
        }
        public static void DispatchMessage<T>(T message) where T : struct
        {
            if(!DelegateDict.TryGetValue(typeof(T),out var delegates))
            {
                // 没有任何人订阅这个事件，直接返回，不用执行后续
                return;
            }
            // 将父类Delegate向下转型为泛型事件委托
            var handle = delegates as EvenetHandle<T>;
            // 传入消息结构体
            handle?.Invoke(message);
        }
    }
}