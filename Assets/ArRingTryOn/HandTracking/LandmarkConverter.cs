using System;
using System.Collections.Generic;
using ArRingTryOn.Core;
using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Tasks.Vision.HandLandmarker;

namespace ArRingTryOn.HandTracking
{
    /// <summary>Copies MediaPipe results into plain Core data.</summary>
    internal static class LandmarkConverter
    {
        public static HandFrame ToHandFrame(in HandLandmarkerResult result, CameraView view, long timestampMs)
        {
            return new HandFrame(timestampMs, view.DisplayAspect, ToHands(result, view));
        }

        private static IReadOnlyList<HandLandmarks> ToHands(in HandLandmarkerResult result, CameraView view)
        {
            var detected = result.handLandmarks;
            if (detected == null || detected.Count == 0)
            {
                return Array.Empty<HandLandmarks>();
            }

            var hands = new List<HandLandmarks>(detected.Count);
            for (var i = 0; i < detected.Count; i++)
            {
                var landmarks = detected[i].landmarks;
                if (landmarks == null || landmarks.Count != HandLandmarks.Count)
                {
                    continue;
                }

                var points = new Vec3[HandLandmarks.Count];
                for (var j = 0; j < points.Length; j++)
                {
                    var landmark = landmarks[j];
                    points[j] = view.ToDisplay(new Vec3(landmark.x, landmark.y, landmark.z));
                }

                hands.Add(new HandLandmarks(points, view.ResolveSide(ReadSide(result.handedness, i))));
            }

            return hands;
        }

        private static HandSide ReadSide(List<Classifications> handedness, int handIndex)
        {
            if (handedness == null || handIndex >= handedness.Count)
            {
                return HandSide.Unknown;
            }

            var categories = handedness[handIndex].categories;
            if (categories == null || categories.Count == 0)
            {
                return HandSide.Unknown;
            }

            switch (categories[0].categoryName)
            {
                case "Left": return HandSide.Left;
                case "Right": return HandSide.Right;
                default: return HandSide.Unknown;
            }
        }
    }
}
