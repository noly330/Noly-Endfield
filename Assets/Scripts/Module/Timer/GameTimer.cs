using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Endfield.Module.Timer
{
    public enum TimerState{NotWorked,DoWorking,DoneWorked}  //没有工作，正在工作，已经完成工作
    public class GameTimer
    {
        private float _startTime;
        private Action _task;
        private TimerState _timerState;
        public TimerState timerState => _timerState;
        private bool _isStopTime;
        private bool _isRealTime;
        public bool isRealTime => _isRealTime;

        public GameTimer()
        {
            InitTimer();
        }

        public void InitTimer() 
    {
        _startTime = 0;
        _task = null;
        _isStopTime = true;
        _timerState= TimerState.NotWorked;
        _isRealTime = false;

    }

    public void StartTimer(bool isRealTime,float startTime,Action task)
    {
        _isRealTime = isRealTime;
        _startTime =startTime;
        _task = task;
        _isStopTime = false;
        _timerState = TimerState.DoWorking;
    }

    /// <summary>
    /// 游戏时间更新
    /// </summary>
    public void UpdateTimer()
    {
        if (_isRealTime) { return; }
        if (_isStopTime == true) { return; }

        _startTime-=Time.deltaTime;
        if (_startTime <= 0)
        { 
           _task?.Invoke();
           _isStopTime = true;
            _timerState = TimerState.DoneWorked;
        }
    }
    /// <summary>
    /// 现实中的时间更新
    /// </summary>
    public void UpdateRealTimer()
    {
        if (!_isRealTime) { return; }
        if (_isStopTime == true) { return; }

        _startTime -= Time.unscaledDeltaTime;
        if (_startTime <= 0)
        {
            _task?.Invoke();
            _isStopTime = true;
            _timerState = TimerState.DoneWorked;
        }
    }
        
    }
}