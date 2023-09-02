﻿using Newtonsoft.Json;
using System.ComponentModel;
using TF.EX.Domain.Models.State;
using TowerFall;

namespace TF.EX.Domain.Models
{
    public class Replay
    {
        public ReplayInfo Informations { get; set; }

        public List<Record> Record { get; set; } = new List<Record>();

        public List<Record> Desynchs { get; set; } = new List<Record>();
    }

    public class ReplayInfo
    {
        public int Id { get; set; }
        public PlayerDraw PlayerDraw { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(ReplayVersion.Unknown)]
        public ReplayVersion Version { get; set; }

        public string Name { get; set; }

        public TowerFall.Modes Mode { get; set; }

        public IEnumerable<ArcherInfo> Archers { get; set; } = Enumerable.Empty<ArcherInfo>();

        public TimeSpan MatchLenght { get; set; } = TimeSpan.Zero;
    }

    public class ArcherInfo
    {
        public int Index { get; set; }
        public ArcherData.ArcherTypes Type { get; set; }
        public bool HasWon { get; set; }
        public int Score { get; set; }

        public string NetplayName { get; set; }

    }

    public class Record
    {
        public List<Input> Inputs { get; set; }

        public GameState GameState { get; set; }
    }

    public enum ReplayVersion
    {
        Unknown,
        V1
    }

}
