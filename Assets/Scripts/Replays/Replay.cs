using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cars;

namespace Replays
{
    [Serializable]
    public class Replay
    {
        ReplayFrame[] frames;
        string track;
        Car car;
        float totalTime;
        float[] allLapTimes;

        public Replay()
        {
            frames = new ReplayFrame[0];
            track = string.Empty;
            car = null;
            totalTime = -1;
            allLapTimes = new float[0];
        }
        public Replay(List<ReplayFrame> recordedFrames, string track, Car car, float t, float[] allT)
        {
            frames = recordedFrames.ToArray();
            this.track = track;
            this.car = car;
            totalTime = t;
            allLapTimes = allT;
        }

        public Replay(ReplayFrame[] recordedFrames, string track, Car car, float t, float[] allT)
        {
            frames = recordedFrames;
            this.track = track;
            this.car = car;
            totalTime = t;
            allLapTimes = allT;
        }

        public ReplayFrame[] GetReplayFrames()
        {
            return frames;
        }

        public string GetTrack()
        {
            return track;
        }

        public Car GetCar()
        {
            return car;
        }

        public float GetTotalTime()
        {
            return totalTime;
        }

        public float[] GetAllLaps()
        {
            return allLapTimes;
        }

        public ReplayFrame GetFrameAtTime(float time)
        {
            int frameInQuestion = 0;

            for (int i = 0; i < frames.Length; i++)
            {
                if (frames[i].GetTime() < time)
                {
                    frameInQuestion = i;
                }
                else
                {
                    break;
                }
            }

            //edge case: if the time given is greater than every frame in the replay
            if (frameInQuestion >= frames.Length - 1)
            {
                return frames[frames.Length - 1];
            }

            //interpolate between the two frames
            float interpolationFactor;
            if ((frames[frameInQuestion + 1].GetTime() - frames[frameInQuestion].GetTime()) > 0)
            {
                interpolationFactor = (time - frames[frameInQuestion].GetTime()) / (frames[frameInQuestion + 1].GetTime() - frames[frameInQuestion].GetTime());
            }
            else
            {
                interpolationFactor = 0;
            }

            float newAcc = Mathf.Lerp(frames[frameInQuestion].GetAcceleration(), frames[frameInQuestion + 1].GetAcceleration(), interpolationFactor);
            float newBrakes = Mathf.Lerp(frames[frameInQuestion].GetBrakes(), frames[frameInQuestion + 1].GetBrakes(), interpolationFactor);
            float newSteer = Mathf.Lerp(frames[frameInQuestion].GetSteering(), frames[frameInQuestion + 1].GetSteering(), interpolationFactor);
            Vector3 newPosition = Vector3.Lerp(frames[frameInQuestion].GetPosition(), frames[frameInQuestion + 1].GetPosition(), interpolationFactor);
            Quaternion newRotation = Quaternion.Lerp(frames[frameInQuestion].GetRotation(), frames[frameInQuestion + 1].GetRotation(), interpolationFactor).normalized;

            ReplayFrame madeUpFrame = new ReplayFrame(
                time,
                newPosition,
                newRotation,
                newAcc,
                newBrakes,
                newSteer,
                frames[frameInQuestion].GetEBrake(),
                frames[frameInQuestion].GetBoost(),
                frames[frameInQuestion].GetDrift(),
                frames[frameInQuestion].GetRespawn()
                );

            return madeUpFrame;
        }
    }
}
