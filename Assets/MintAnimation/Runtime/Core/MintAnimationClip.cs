﻿using System;

namespace MintAnimation.Core
{

    public delegate T MintGetter<out T>();

    public delegate void MintSetter<in T>(T rNewValue);

    public class MintAnimationClip<T>
    {
        /// <summary>
        /// 请使用Create方法构建MintAnimation
        /// </summary>
        protected MintAnimationClip() {}

        public MintAnimationClip(MintGetter<T> mintGetter, MintSetter<T> mintSetter , MintAnimationDataBase<T> mintAnimationInfo) {
            _getter = mintGetter;
            _setter = mintSetter;
            AnimationInfo = mintAnimationInfo;
            Init();
        }

        public Action                                           OnComplete;

        public MintAnimationDataBase<T>                         AnimationInfo;

        private MintGetter<T>                                   _getter;
        private MintSetter<T>                                   _setter;

        private float                                           _nowTime;
        private bool                                            _isPause;

        private int                                             _nowLoopCount;
        private float                                           _backTime;

        public void Init()
        {
            _nowTime = 0;
            _isPause = true;
            _backTime = AnimationInfo.Options.Duration / 2;
            register();
        }

        public void Play() {
            _isPause = false;
        }
        public void Pause() {
            _isPause = true;
        }
        public void Stop() {
            _nowTime = AnimationInfo.Options.Duration;
            setAnimationValue();
            _isPause = true;
            unregister();
        }

        private bool updateAnimation(float deltaTime) {
            if (_isPause) return false;
            setAnimationValue();
            if (_nowTime >= AnimationInfo.Options.Duration) {
                _nowLoopCount++;
                if (AnimationInfo.Options.IsLoop)
                {
                    if (AnimationInfo.Options.LoopCount == -1 || _nowLoopCount < AnimationInfo.Options.LoopCount)
                    {
                        _nowTime = 0;
                        return true;
                    }
                }
                OnComplete?.Invoke();
                Stop();
            }
            else _nowTime += deltaTime;
            return true;
        }
        private void setAnimationValue()
        {
            if (AnimationInfo.Options.IsBack)
            {
                if (_nowTime <= _backTime)
                    _setter.Invoke(AnimationInfo.GetProgress(_nowTime * 2));
                else
                    _setter.Invoke(AnimationInfo.GetProgress(AnimationInfo.Options.Duration - ((_nowTime - _backTime) * 2)));
            }
            else
            {
                _setter.Invoke(AnimationInfo.GetProgress(_nowTime));
            }
        }

        private void register() {
            switch (AnimationInfo.Options.DriveType)
            {
                case DriveEnum.Custom:
                    if (AnimationInfo.Options.CustomDrive != null) {
                        AnimationInfo.Options.CustomDrive.AddDriveAction(updateAnimation, AnimationInfo.Options.UpdaterTypeEnum);
                    }
                    break;
                case DriveEnum.Globa:
                    MintDriveComponentSinge.Instance.AddDriveAction(updateAnimation, AnimationInfo.Options.UpdaterTypeEnum);
                    break;
            }
        }
        private void unregister() {
            switch (AnimationInfo.Options.DriveType)
            {
                case DriveEnum.Custom:
                    if (AnimationInfo.Options.CustomDrive != null)
                    {
                        AnimationInfo.Options.CustomDrive.RemoveDriveAction(updateAnimation , AnimationInfo.Options.UpdaterTypeEnum);
                    }
                    break;
                case DriveEnum.Globa:
                    MintDriveComponentSinge.Instance.RemoveDriveAction(updateAnimation, AnimationInfo.Options.UpdaterTypeEnum);
                    break;
            }
        }

        public static MintAnimationClip<float> Create(MintGetter<float> mintGetter, MintSetter<float> mintSetter, float endvalue, float duration)
        {
            MintAnimationDataBase<float> mintAnimationInfo = new MintAnimtaionDataFloat();
            mintAnimationInfo.Options.EaseType = MintEaseMethod.Linear;
            mintAnimationInfo.StartValue = mintGetter.Invoke();
            mintAnimationInfo.EndValue = endvalue;
            mintAnimationInfo.Options.Duration = duration;
            var a = new MintAnimationClip<float>(mintGetter, mintSetter, mintAnimationInfo);
            return a;
        }
    }
}