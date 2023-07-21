﻿using TF.EX.Domain.Models.State.Entity.LevelEntity;
using TF.EX.Domain.Models.State.Entity.LevelEntity.Arrows;
using TF.EX.Domain.Models.State.Entity.LevelEntity.Chest;
using TF.EX.Domain.Models.State.Entity.LevelEntity.Player;

namespace TF.EX.Domain.Models.State.Entity
{
    public class Entity
    {
        public ICollection<Player> Players { get; set; } = new List<Player>();
        public ICollection<PlayerCorpse> PlayerCorpses { get; set; } = new List<PlayerCorpse>();
        public ICollection<Arrow> Arrows { get; set; } = new List<Arrow>();
        public ICollection<Chest> Chests { get; set; } = new List<Chest>();
        public ICollection<Pickup> Pickups { get; set; } = new List<Pickup>();
        public ICollection<Lantern> Lanterns { get; set; } = new List<Lantern>();
        public ICollection<Chain> Chains { get; set; } = new List<Chain>();
        public ICollection<JumpPad> JumpPads { get; set; } = new List<JumpPad>();
        public LavaControl LavaControl { get; set; }

        public HUD.HUD Hud { get; set; } = new HUD.HUD();

        public Orb.Orb Orb { get; set; } = new Orb.Orb();

    }
}