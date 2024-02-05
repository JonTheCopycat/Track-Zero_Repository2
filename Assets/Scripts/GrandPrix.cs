using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrandPrix
{
    string[] cupNames;
    Cup[] cups;

    public GrandPrix(string[] cupNames)
    {
        this.cupNames = cupNames;
        cups = new Cup[cupNames.Length];
    }

    public string GetCup(int index)
    {
        return cupNames[index];
    }

    public string GetCupTrack(int cupIndex, int trackIndex)
    {
        return cups[cupIndex].GetTrack(trackIndex);
    }

    public void SetCupTracks(int index, string[] tracks)
    {
        cups[index] = new Cup(tracks);
    }

    private class Cup
    {
        string[] allTracks;

        public Cup()
        {
            allTracks = new string[0];
        }

        public Cup(string[] tracks)
        {
            allTracks = tracks;
        }

        public string[] AllTracks()
        {
            return allTracks;
        }

        public string GetTrack(int index)
        {
            if (index >= 0 && index < allTracks.Length)
                return allTracks[index];

            return "";
        }

        public void SetAllTracks(string[] tracks)
        {
            allTracks = tracks;
        }
    }
}
