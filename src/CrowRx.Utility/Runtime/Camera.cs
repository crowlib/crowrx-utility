using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;


namespace CrowRx.Utility
{
    public static class CameraUtility
    {
        public static async UniTask<Camera> TransitionAsync(this Camera originalCamera, Camera targetCamera, float durationInSec, CancellationToken token)
        {
            List<UniTask> tasks = new()
            {
                UniTask.Create(
                    async cancellationToken =>
                    {
                        Transform orgTrans = originalCamera.transform;
                        Transform targetTrans = targetCamera.transform;

                        float beginTime = Time.time;

                        Vector3 beginPosition = orgTrans.position;
                        Vector3 endPosition = targetTrans.position;

                        Quaternion beginRotation = orgTrans.rotation;
                        Quaternion endRotation = targetTrans.rotation;

                        Color beginBackgroundColor = originalCamera.backgroundColor;
                        Color endBackgroundColor = targetCamera.backgroundColor;

                        float beginFar = originalCamera.farClipPlane;
                        float endFar = targetCamera.farClipPlane;

                        float beginNear = originalCamera.nearClipPlane;
                        float endNear = targetCamera.nearClipPlane;

                        float beginAspect = originalCamera.aspect;
                        float endAspect = targetCamera.aspect;

                        Rect beginRect = originalCamera.rect;
                        Rect endRect = targetCamera.rect;

                        while (!cancellationToken.IsCancellationRequested)
                        {
                            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate, cancellationToken);

                            float currentTime = Time.time;
                            if (currentTime - beginTime >= durationInSec)
                            {
                                break;
                            }

                            float lerpRate = (currentTime - beginTime) / durationInSec;

                            orgTrans.SetPositionAndRotation(Vector3.Lerp(beginPosition, endPosition, lerpRate), Quaternion.Lerp(beginRotation, endRotation, lerpRate));
                            originalCamera.backgroundColor = Color.Lerp(beginBackgroundColor, endBackgroundColor, lerpRate);
                            originalCamera.farClipPlane = Mathf.Lerp(beginFar, endFar, lerpRate);
                            originalCamera.nearClipPlane = Mathf.Lerp(beginNear, endNear, lerpRate);
                            originalCamera.aspect = Mathf.Lerp(beginAspect, endAspect, lerpRate);

                            Rect tRect = originalCamera.rect;
                            tRect.position = Vector2.Lerp(beginRect.position, endRect.position, lerpRate);
                            tRect.size = Vector2.Lerp(beginRect.size, endRect.size, lerpRate);
                            originalCamera.rect = tRect;
                        }

                        orgTrans.SetPositionAndRotation(endPosition, endRotation);
                        originalCamera.backgroundColor = endBackgroundColor;
                        originalCamera.farClipPlane = endFar;
                        originalCamera.nearClipPlane = endNear;
                        originalCamera.aspect = endAspect;
                        originalCamera.rect = endRect;
                    },
                    token)
            };

            if (targetCamera.orthographic != originalCamera.orthographic)
            {
                tasks.Add(
                    UniTask.Create(
                        async cancellationToken =>
                        {
                            float beginTime = Time.time;

                            Matrix4x4 beginProjection = originalCamera.projectionMatrix;
                            Matrix4x4 endProjection = targetCamera.projectionMatrix;

                            if (targetCamera.orthographic)
                            {
                                originalCamera.orthographicSize = targetCamera.orthographicSize;
                            }
                            else
                            {
                                originalCamera.fieldOfView = targetCamera.fieldOfView;
                            }

                            while (!cancellationToken.IsCancellationRequested)
                            {
                                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate, cancellationToken);

                                float currentTime = Time.time;
                                if (currentTime - beginTime >= durationInSec)
                                {
                                    break;
                                }

                                float lerpRate = (currentTime - beginTime) / durationInSec;

                                Matrix4x4 buffer = originalCamera.projectionMatrix;
                                Mathm.Matrix4X4Lerp(ref buffer, beginProjection, endProjection, lerpRate);
                                originalCamera.projectionMatrix = buffer;
                            }

                            originalCamera.projectionMatrix = endProjection;
                            originalCamera.orthographic = targetCamera.orthographic;
                        },
                        token));
            }
            else
            {
                if (targetCamera.orthographic)
                {
                    tasks.Add(
                        UniTask.Create(
                            async cancellationToken =>
                            {
                                float beginTime = Time.time;

                                float beginOrthographicSize = originalCamera.orthographicSize;
                                float endOrthographicSize = targetCamera.orthographicSize;

                                while (!cancellationToken.IsCancellationRequested)
                                {
                                    await UniTask.Yield(PlayerLoopTiming.PostLateUpdate, cancellationToken);

                                    float currentTime = Time.time;
                                    if (currentTime - beginTime >= durationInSec)
                                    {
                                        break;
                                    }

                                    float lerpRate = (currentTime - beginTime) / durationInSec;

                                    originalCamera.orthographicSize = Mathf.Lerp(beginOrthographicSize, endOrthographicSize, lerpRate);
                                }

                                originalCamera.orthographicSize = endOrthographicSize;
                            },
                            token));
                }
                else
                {
                    tasks.Add(
                        UniTask.Create(
                            async cancellationToken =>
                            {
                                float beginTime = Time.time;

                                float beginFov = originalCamera.fieldOfView;
                                float endFov = targetCamera.fieldOfView;

                                while (!cancellationToken.IsCancellationRequested)
                                {
                                    await UniTask.Yield(PlayerLoopTiming.PostLateUpdate, cancellationToken);

                                    float currentTime = Time.time;
                                    if (currentTime - beginTime >= durationInSec)
                                    {
                                        break;
                                    }

                                    float lerpRate = (currentTime - beginTime) / durationInSec;

                                    originalCamera.fieldOfView = Mathf.Lerp(beginFov, endFov, lerpRate);
                                }

                                originalCamera.fieldOfView = endFov;
                            },
                            token));
                }
            }

            try
            {
                await UniTask.WhenAll(tasks);
            }
            catch (Exception e)
            {
                if (e is not OperationCanceledException)
                {
                    Log.Exception(e);
                    throw;
                }
            }

            return originalCamera;
        }

        public static Observable<Camera> TransitionObservable(this Camera org, Camera target, float durationInSec) =>
            Observable.Defer(() =>
                {
                    TimeSpan duration = TimeSpan.FromSeconds(durationInSec);

                    Transform orgTrans = org.transform;
                    Transform targetTrans = target.transform;

                    float beginTime = Time.time;

                    Vector3 beginPosition = orgTrans.position;
                    Vector3 endPosition = targetTrans.position;

                    Quaternion beginRotation = orgTrans.rotation;
                    Quaternion endRotation = targetTrans.rotation;

                    Color beginBackgroundColor = org.backgroundColor;
                    Color endBackgroundColor = target.backgroundColor;

                    float beginFar = org.farClipPlane;
                    float endFar = target.farClipPlane;

                    float beginNear = org.nearClipPlane;
                    float endNear = target.nearClipPlane;

                    float beginAspect = org.aspect;
                    float endAspect = target.aspect;

                    Rect beginRect = org.rect;
                    Rect endRect = target.rect;

                    Observable<float> updateCamera = org.UpdateAsObservable()
                        .Take(duration)
                        .Select(_ => (Time.time - beginTime) / durationInSec);

                    Observable<float> updateCommon = updateCamera.Publish().RefCount()
                        .Do(t =>
                        {
                            orgTrans.SetPositionAndRotation(Vector3.Lerp(beginPosition, endPosition, t), Quaternion.Lerp(beginRotation, endRotation, t));
                            org.backgroundColor = Color.Lerp(beginBackgroundColor, endBackgroundColor, t);
                            org.farClipPlane = Mathf.Lerp(beginFar, endFar, t);
                            org.nearClipPlane = Mathf.Lerp(beginNear, endNear, t);
                            org.aspect = Mathf.Lerp(beginAspect, endAspect, t);

                            Rect tRect = org.rect;
                            tRect.position = Vector2.Lerp(beginRect.position, endRect.position, t);
                            tRect.size = Vector2.Lerp(beginRect.size, endRect.size, t);
                            org.rect = tRect;
                        })
                        .LastAsync()
                        .ToObservable();

                    Observable<float> updateByProjection;

                    if (target.orthographic != org.orthographic)
                    {
                        Matrix4x4 beginProjection = org.projectionMatrix;
                        Matrix4x4 endProjection = target.projectionMatrix;

                        updateByProjection = updateCamera.Publish().RefCount()
                            .Do(t =>
                            {
                                Matrix4x4 buffer = org.projectionMatrix;
                                Mathm.Matrix4X4Lerp(ref buffer, beginProjection, endProjection, t);
                                org.projectionMatrix = buffer;
                            });

                        if (target.orthographic)
                            org.orthographicSize = target.orthographicSize;
                        else
                            org.fieldOfView = target.fieldOfView;
                    }
                    else
                    {
                        if (target.orthographic)
                        {
                            float beginOrthographicSize = org.orthographicSize;
                            float endOrthographicSize = target.orthographicSize;

                            updateByProjection = updateCamera.Publish().RefCount()
                                .Do(t => org.orthographicSize = Mathf.Lerp(beginOrthographicSize, endOrthographicSize, t));
                        }
                        else
                        {
                            float beginFov = org.fieldOfView;
                            float endFov = target.fieldOfView;

                            updateByProjection = updateCamera.Publish().RefCount()
                                .Do(t => org.fieldOfView = Mathf.Lerp(beginFov, endFov, t));
                        }
                    }

                    return Observable.Merge(updateCommon, updateByProjection)
                        .LastAsync()
                        .ToObservable();
                })
                .Select(_ =>
                {
                    org.orthographic = target.orthographic;
                    //org.CopyFrom(target);
                    return org;
                });

        public static void TransitionCamera(this Camera org, Camera target, float duration, Action<Camera> onComplete) =>
            org.TransitionObservable(target, duration).Subscribe(onComplete);

        public static async UniTask UpdateUIPositionFromWorldPositionAsync(
            Camera gameCamera,
            Camera uiCamera,
            Transform worldTransform,
            RectTransform targetTransform,
            Vector2 offset,
            CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await UniTask.Yield(PlayerLoopTiming.PreUpdate, cancellationToken);

                if (!UpdateUIPositionFromWorldPosition(gameCamera, uiCamera, worldTransform, targetTransform, offset))
                {
                    break;
                }
            }
        }

        public static bool UpdateUIPositionFromWorldPosition(
            Camera gameCamera,
            Camera uiCamera,
            Transform worldTransform,
            RectTransform targetTransform,
            Vector2 offset)
        {
            if (!gameCamera || !worldTransform || !targetTransform)
            {
                return false;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    (RectTransform)targetTransform.parent,
                    gameCamera.WorldToScreenPoint(worldTransform.position),
                    uiCamera,
                    out Vector2 lp))
            {
                return false;
            }

            targetTransform.localPosition = new Vector3
            {
                x = lp.x + offset.x,
                y = lp.y + offset.y,
                z = 0f
            };

            return true;
        }
    }
}