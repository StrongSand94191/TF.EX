﻿using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using TF.EX.Domain.Extensions;
using TF.EX.Domain.Models.State.Entity.LevelEntity;
using TF.EX.TowerFallExtensions.MonocleExtensions;

namespace TF.EX.TowerFallExtensions.Entity.LevelEntity
{
    public static class CrackedPlatformExtensions
    {
        public static CrackedPlatform GetState(this TowerFall.CrackedPlatform entity)
        {
            var dynCrackedPlatform = DynamicData.For(entity);
            var actualDepth = dynCrackedPlatform.Get<double>("actualDepth");
            var collidable = dynCrackedPlatform.Get<bool>("Collidable");
            var shakeAlarm = dynCrackedPlatform.Get<Alarm>("shakeAlarm");
            var respawnAlarm = dynCrackedPlatform.Get<Alarm>("respawnAlarm");
            var state = dynCrackedPlatform.Get<TowerFall.CrackedPlatform.States>("state");
            var positionCounter = dynCrackedPlatform.Get<Vector2>("counter");
            var flashCounter = dynCrackedPlatform.Get<float>("flashCounter");
            var flashInterval = dynCrackedPlatform.Get<float>("flashInterval");

            return new CrackedPlatform
            {
                ActualDepth = actualDepth,
                IsCollidable = collidable,
                Position = entity.Position.ToModel(),
                PositionCounter = positionCounter.ToModel(),
                Shake = shakeAlarm.GetState(),
                Respawn = respawnAlarm.GetState(),
                State = state,
                Flash = new Flash(entity.Flashing, flashCounter, flashInterval)
            };
        }

        public static void LoadState(this TowerFall.CrackedPlatform entity, CrackedPlatform toLoad)
        {
            var dynCrackedPlatform = DynamicData.For(entity);

            dynCrackedPlatform.Set("Collidable", toLoad.IsCollidable);
            entity.Position = toLoad.Position.ToTFVector();
            dynCrackedPlatform.Set("counter", toLoad.PositionCounter.ToTFVector());
            dynCrackedPlatform.Set("state", toLoad.State);

            var shakeAlarm = dynCrackedPlatform.Get<Alarm>("shakeAlarm");
            shakeAlarm.LoadState(toLoad.Shake);

            var respawnAlarm = dynCrackedPlatform.Get<Alarm>("respawnAlarm");
            respawnAlarm.LoadState(toLoad.Respawn);

            dynCrackedPlatform.Set("Flashing", toLoad.Flash.IsFlashing);
            dynCrackedPlatform.Set("flashCounter", toLoad.Flash.FlashCounter);
            dynCrackedPlatform.Set("flashInterval", toLoad.Flash.FlashInterval);
        }
    }
}