﻿namespace TF.EX.Domain.Models.State
{
    public class Miasma
    {
        public float Counter { get; set; }
        public int CoroutineTimer { get; set; }
        public double ActualDepth { get; set; } = Constants.MIASMA_CUSTOM_DEPTH;

        public static Miasma Default()
        {
            return new Miasma
            {
                Counter = Constants.DEFAULT_MIASMA_COUNTER,
                CoroutineTimer = Constants.DEFAULT_COUROUTINE_TIMER,
            };
        }
    }
}
