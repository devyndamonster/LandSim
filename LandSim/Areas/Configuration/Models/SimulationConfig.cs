namespace LandSim.Areas.Configuration.Models
{
    public class SimulationConfig
    {
        public int SimulationConfigId { get; set; }

        public float ConsumableSpawnChance { get; set; } = 0.001f;

        public float ConsumableHungerIncrease { get; set; } = 0.1f;

        public float ConsumableVegitationSpawnThreshold { get; set; } = 0.95f;

        public float BaseHungerCost { get; set; } = 0.01f;

        public float MovementHungerCost { get; set; } = 0.01f;

        public float ClimbHungerCost { get; set; } = 0.5f;

        public float AgentSpawnChange { get; set; } = 0.0001f;

        public float BaseThirstCost { get; set; } = 0.01f;

        public float DrinkThirstIncrease { get; set; } = 0.1f;

        public float VegitationMovementHungerCost { get; set; } = 0.02f;

        public float VegitationSpawnChance { get; set; } = 0.0001f;

        public float VegitationSpreadChance { get; set; } = 0.1f;

        public float VegitationGrowthRate { get; set; } = 0.01f;

        public float VegitationDecreaseFromMovement { get; set; } = 0.1f;

        public float ReproductionCooldownRate { get; set; } = 0.01f;


    }
}
