using Cysharp.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

namespace Cysharp.Threading.TasksTests
{
    public class DelayTest
    {
        [UnityTest]
        public IEnumerator DelayFrame() => UniTask.ToCoroutine(async () =>
        {
            for (int i = 1; i < 5; i++)
            {
                await UniTask.Yield(PlayerLoopTiming.PreUpdate);
                var frameCount = Time.frameCount;
                await UniTask.DelayFrame(i);
                Time.frameCount.Should().Be(frameCount + i);
            }

            for (int i = 1; i < 5; i++)
            {
                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
                var frameCount = Time.frameCount;
                await UniTask.DelayFrame(i);
                Time.frameCount.Should().Be(frameCount + i);
            }
        });

        [UnityTest]
        public IEnumerator DelayFrameZero() => UniTask.ToCoroutine(async () =>
        {
            {
                await UniTask.Yield(PlayerLoopTiming.PreUpdate);
                var frameCount = Time.frameCount;
                await UniTask.DelayFrame(0);
                Time.frameCount.Should().Be(frameCount); // same frame
            }
            {
                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
                var frameCount = Time.frameCount;
                await UniTask.DelayFrame(0);
                Time.frameCount.Should().Be(frameCount + 1); // next frame
            }
        });

#if !UNITY_WEBGL

        [UnityTest]
        public IEnumerator DelayInThreadPool() => UniTask.ToCoroutine(async () =>
        {
            await UniTask.Run(async () =>
            {
                await UniTask.Delay(TimeSpan.FromSeconds(2));
            });
        });

#endif

        [UnityTest]
        public IEnumerator DelayRealtime() => UniTask.ToCoroutine(async () =>
        {
            var now = DateTimeOffset.UtcNow;

            await UniTask.Delay(TimeSpan.FromSeconds(2), DelayType.Realtime);

            var elapsed = DateTimeOffset.UtcNow - now;

            var okay1 = TimeSpan.FromSeconds(1.80) <= elapsed;
            var okay2 = elapsed <= TimeSpan.FromSeconds(2.20);

            okay1.Should().Be(true);
            okay2.Should().Be(true);
        });

        [UnityTest]
        public IEnumerator LoopTest() => UniTask.ToCoroutine(async () =>
        {
            for (int i = 0; i < 20; ++i)
            {
                UniTask.DelayFrame(100).Forget();
                await UniTask.DelayFrame(1);
            }
        });
    }
}