using System;
using System.Collections.Generic;
using System.Linq;
using ArRingTryOn.Core;
using NUnit.Framework;

namespace ArRingTryOn.Tests
{
    public class OneEuroFilterTests
    {
        private const double FrameSeconds = 1.0 / 30.0;
        private static readonly OneEuroSettings Typical = new OneEuroSettings(1.5f, 4f);

        [Test]
        public void FirstSample_PassesStraightThrough()
        {
            Assert.That(new OneEuroFilter(Typical).Filter(0.42f, 10.0), Is.EqualTo(0.42f));
        }

        [Test]
        public void SteadyInput_StaysExactlySteady()
        {
            var filter = new OneEuroFilter(Typical);

            var outputs = Run(filter, Enumerable.Repeat(0.3f, 90));

            Assert.That(outputs, Is.All.EqualTo(0.3f));
        }

        [Test]
        public void Step_ConvergesWithoutOvershoot()
        {
            var filter = new OneEuroFilter(Typical);
            var input = Enumerable.Repeat(0f, 10).Concat(Enumerable.Repeat(1f, 90));

            var outputs = Run(filter, input);

            for (var i = 1; i < outputs.Count; i++)
            {
                Assert.That(outputs[i], Is.GreaterThanOrEqualTo(outputs[i - 1]), $"frame {i} moved backwards");
                Assert.That(outputs[i], Is.LessThanOrEqualTo(1f), $"frame {i} overshot");
            }

            Assert.That(outputs.Last(), Is.EqualTo(1f).Within(1e-3f));
        }

        [Test]
        public void Step_IsSmoothedRatherThanJumpedTo()
        {
            var filter = new OneEuroFilter(Typical);
            filter.Filter(0f, 0.0);

            var first = filter.Filter(1f, FrameSeconds);

            Assert.That(first, Is.GreaterThan(0f).And.LessThan(1f));
        }

        [Test]
        public void Jitter_IsReducedAroundAFixedPoint()
        {
            var noise = Noise(300, 0.01f, seed: 7);
            var filter = new OneEuroFilter(Typical);

            var outputs = Run(filter, noise.Select(n => 0.5f + n));

            Assert.That(StandardDeviation(outputs.Skip(30)), Is.LessThan(0.4 * StandardDeviation(noise)));
        }

        [Test]
        public void FastMotion_LagsLessThanAPlainLowPass()
        {
            var ramp = Enumerable.Range(0, 60).Select(i => i * 0.05f).ToArray();

            var adaptive = Run(new OneEuroFilter(new OneEuroSettings(1.5f, 4f)), ramp).Last();
            var plain = Run(new OneEuroFilter(new OneEuroSettings(1.5f, 0f)), ramp).Last();

            Assert.That(ramp.Last() - adaptive, Is.LessThan(0.5f * (ramp.Last() - plain)));
        }

        [Test]
        public void SampleThatIsNotNewer_IsIgnored()
        {
            var filter = new OneEuroFilter(Typical);
            filter.Filter(0f, 1.0);
            var settled = filter.Filter(0f, 1.1);

            Assert.That(filter.Filter(5f, 1.1), Is.EqualTo(settled));
            Assert.That(filter.Filter(5f, 0.5), Is.EqualTo(settled));
        }

        [Test]
        public void Reset_StartsAfreshFromTheNextSample()
        {
            var filter = new OneEuroFilter(Typical);
            Run(filter, Enumerable.Repeat(0f, 10));

            filter.Reset();

            Assert.That(filter.Filter(9f, 100.0), Is.EqualTo(9f));
        }

        [Test]
        public void Vec3_SteadyInputStaysSteadyAndStepConvergesWithoutOvershoot()
        {
            var filter = new Vec3OneEuroFilter(Typical);
            var target = new Vec3(0.3f, -0.2f, 0.1f);
            var time = 0.0;
            filter.Filter(new Vec3(0f, 0f, 0f), time);
            var previousDistance = target.Length;

            for (var i = 0; i < 90; i++)
            {
                time += FrameSeconds;
                var output = filter.Filter(target, time);
                var distance = (target - output).Length;
                Assert.That(distance, Is.LessThanOrEqualTo(previousDistance + 1e-6f), $"frame {i}");
                Assert.That(Vec3.Dot(output, target), Is.LessThanOrEqualTo(Vec3.Dot(target, target) + 1e-6f), "overshot");
                previousDistance = distance;
            }

            Assert.That(previousDistance, Is.LessThan(1e-3f));
        }

        [Test]
        public void Quat_StepConvergesWithoutOvershoot()
        {
            var filter = new QuatOneEuroFilter(new OneEuroSettings(1.5f, 1.5f));
            var target = AboutY(90f);
            var time = 0.0;
            filter.Filter(Quat.Identity, time);
            var previousAngle = Quat.Angle(Quat.Identity, target);

            for (var i = 0; i < 90; i++)
            {
                time += FrameSeconds;
                var output = filter.Filter(target, time);
                var angle = Quat.Angle(output, target);
                Assert.That(angle, Is.LessThanOrEqualTo(previousAngle + 1e-4f), $"frame {i}");
                Assert.That(Quat.Angle(Quat.Identity, output), Is.LessThanOrEqualTo((float)(Math.PI / 2) + 1e-4f), "overshot");
                previousAngle = angle;
            }

            Assert.That(previousAngle, Is.LessThan(0.01f));
        }

        [Test]
        public void Quat_SteadyInputStaysSteady()
        {
            var filter = new QuatOneEuroFilter(new OneEuroSettings(1.5f, 1.5f));
            var steady = AboutY(37f);

            var time = 0.0;
            for (var i = 0; i < 60; i++)
            {
                var output = filter.Filter(steady, time);
                Assert.That(Quat.Angle(output, steady), Is.LessThan(1e-3f));
                time += FrameSeconds;
            }
        }

        private static List<float> Run(OneEuroFilter filter, IEnumerable<float> input)
        {
            var outputs = new List<float>();
            var time = 0.0;
            foreach (var sample in input)
            {
                outputs.Add(filter.Filter(sample, time));
                time += FrameSeconds;
            }

            return outputs;
        }

        private static float[] Noise(int count, float amplitude, int seed)
        {
            var random = new Random(seed);
            return Enumerable.Range(0, count).Select(_ => (float)(random.NextDouble() * 2.0 - 1.0) * amplitude).ToArray();
        }

        private static double StandardDeviation(IEnumerable<float> values)
        {
            var list = values.ToList();
            var mean = list.Average();
            return Math.Sqrt(list.Average(v => (v - mean) * (v - mean)));
        }

        internal static Quat AboutY(float degrees)
        {
            var half = degrees * Math.PI / 360.0;
            return new Quat(0f, (float)Math.Sin(half), 0f, (float)Math.Cos(half));
        }
    }
}
