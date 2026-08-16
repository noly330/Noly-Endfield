using System;
using System.Collections.Generic;
using UnityEngine;
using Endfield.Module.Timer;
using Endfield.Core;

namespace Endfield
{
    public class TimerManager : SingletonMono<TimerManager>
    {
        [SerializeField] private int _timerCount = 50;

        private Queue<GameTimer> _notWorkTimers = new Queue<GameTimer>();
        private List<GameTimer> _isWorkingTimers = new List<GameTimer>();


        protected void Start()
        {
            for (int i = 0; i < _timerCount; i++)
            {
                CreateTimer();
            }
        }

        private void Update()
        {
            UpdateTimers();
        }
        private void CreateTimer()
        {
            var timer = new GameTimer();
            _notWorkTimers.Enqueue(timer);
        }

        private void UpdateTimers()
        {
            if(_isWorkingTimers.Count == 0)  
                return;
            // 倒序遍历：清理 DoneWorked 定时器时 RemoveAt 不影响已遍历下标，
            // 修复正序 Remove 会跳过相邻定时器、导致冲刺冷却等定时器偶发不触发的问题
            for(int i = _isWorkingTimers.Count - 1; i >= 0; i--)
            {
                if(_isWorkingTimers[i].timerState == TimerState.DoWorking)
                {
                    if (_isWorkingTimers[i].isRealTime)
                    {
                        _isWorkingTimers[i].UpdateRealTimer();
                    }
                    else
                    {
                        _isWorkingTimers[i].UpdateTimer();
                    }
                }
                else if(_isWorkingTimers[i].timerState == TimerState.DoneWorked)
                {
                    _isWorkingTimers[i].InitTimer();
                    _notWorkTimers.Enqueue(_isWorkingTimers[i]);
                    _isWorkingTimers.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 开启一个游戏时间定时器,直接开始计时，不返回定时器对象
        /// </summary>
        /// <param name="time"></param>
        /// <param name="action"></param>
        public void GetOneTimer(float time, Action action)
        {
            if (_notWorkTimers.Count == 0)
            {
                CreateTimer();
            }
            GameTimer gameTimer = _notWorkTimers.Dequeue();
            gameTimer.StartTimer(false, time, action);
            _isWorkingTimers.Add(gameTimer);
        }
        /// <summary>
        /// 开启一个游戏时间定时器并且返回定时器对象
        /// </summary>
        /// <param name="time"></param>
        /// <param name="action"></param>
        public GameTimer GetTimer(float time, Action action)
        {
            if (_notWorkTimers.Count == 0) { CreateTimer(); }
            GameTimer gameTimer = _notWorkTimers.Dequeue();
            gameTimer.StartTimer(false, time, action);
            _isWorkingTimers.Add(gameTimer);
            return gameTimer;
        }
        /// <summary>
        /// 开启一个真是时间定时器并且返回定时器对象
        /// </summary>
        /// <param name="time"></param>
        /// <param name="action"></param>
        public GameTimer GetRealTimer(float time, Action action)
        {
            if (_notWorkTimers.Count == 0) { CreateTimer(); }
    
            GameTimer gameTimer = _notWorkTimers.Dequeue();
            gameTimer.StartTimer(true, time, action);
            _isWorkingTimers.Add(gameTimer);
            return gameTimer;
        }

        /// <summary>
        /// 关闭指定计时器
        /// </summary>
        /// <param name="gameTimer"></param>
        public void UnregisterTimer(GameTimer gameTimer)
        {
            if(gameTimer == null)  return; 
            if(gameTimer.timerState != TimerState.DoWorking)  return;
            gameTimer.InitTimer();
            _isWorkingTimers.Remove(gameTimer);
            _notWorkTimers.Enqueue(gameTimer);
        }
    }
}