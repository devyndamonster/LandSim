using LandSim.Areas.Agents.Models;
using LandSim.Areas.Configuration.Models;
using LandSim.Areas.Map.Models;
using LandSim.Extensions;

namespace Agent.Boar
{
    public class BoarService
    {
        public AgentAction GetAction(AgentContext context)
        {
            if (context.Agent is null) throw new NullReferenceException("Recieved agent was null");

            var closestConsumable = context.Consumables
                .OrderBy(consumable => Math.Abs(consumable.XCoord - context.Agent.XCoord) + Math.Abs(consumable.YCoord - context.Agent.YCoord))
                .FirstOrDefault();

            ILocation destination = context.Agent;
            if(closestConsumable is not null && !context.Agent.IsAt(closestConsumable))
            {
                destination = GetNextMoveTarget(context.Agent, closestConsumable, context.TerrainTiles, context.SimulationConfig ?? new SimulationConfig());
            }

            var agentAction = destination switch
            {
                var dest when dest.XCoord < context.Agent.XCoord => AgentActionType.MoveLeft,
                var dest when dest.XCoord > context.Agent.XCoord => AgentActionType.MoveRight,
                var dest when dest.YCoord < context.Agent.YCoord => AgentActionType.MoveUp,
                var dest when dest.YCoord > context.Agent.YCoord => AgentActionType.MoveDown,
                _ when closestConsumable is not null && context.Agent.IsAt(closestConsumable) => AgentActionType.Eat,
                _ => (AgentActionType)new Random().Next(1, 5)
            };

            return new AgentAction
            {
                AgentId = context.Agent.AgentId,
                ActionType = agentAction
            };
        }

        private TerrainTile GetNextMoveTarget(ILocation currentLocation, ILocation destination, IEnumerable<TerrainTile> tiles, SimulationConfig config)
        {
            var nodes = tiles.Select(tile => new LocationNode(tile)).ToArray();
            var bounds = Bounds.FromLocations(nodes);
            var grid = nodes.MapLocationsToBoundedGrid(bounds);
            var startNode = new LocationNode(tiles.First(currentLocation.IsAt));
            var endNode = new LocationNode(tiles.First(destination.IsAt));

            var path = CalculatePath(startNode, endNode, grid, config);
            return path.First().Tile;
        }

        //Calculate path using A* algorithm
        private List<LocationNode> CalculatePath(LocationNode start, LocationNode end, LocationNode?[,] grid, SimulationConfig config)
        {
            var open = new List<LocationNode>();
            var closed = new List<LocationNode>();
            open.Add(start);

            while (open.Any())
            {
                var current = open.OrderBy(node => node.FCost).First();
                open.Remove(current);
                closed.Add(current);

                if (current.IsAt(end))
                {
                    var path = new List<LocationNode>();
                    while (!current!.IsAt(start))
                    {
                        path.Add(current!);
                        current = current!.Parent;
                    }
                    path.Reverse();
                    return path;
                }

                foreach(var neighbor in grid.GetImmediateNeighbors(current))
                {
                    if(!neighbor.Tile.IsWalkable() || closed.Any(neighbor.IsAt))
                    {
                        continue;
                    }

                    var heightIncrease = MathF.Max(0, neighbor.Tile.Height - current.Tile.Height);
                    var heightCost = heightIncrease * config.MovementHungerCost;
                    var vegitationCost = neighbor.Tile.VegetationLevel * config.VegitationMovementHungerCost;
                    var baseMovementCost = GetDistance(current, neighbor) * config.MovementHungerCost;

                    var movementCost = current.GCost + baseMovementCost + heightCost + vegitationCost;
                    if(movementCost < neighbor.GCost || !open.Any(neighbor.IsAt))
                    {
                        neighbor.GCost = movementCost;
                        neighbor.HCost = GetDistance(neighbor, end) * config.MovementHungerCost;
                        neighbor.Parent = current;

                        if(!open.Any(neighbor.IsAt))
                        {
                            open.Add(neighbor);
                        }
                    }
                }
            }

            return new List<LocationNode>();
        }

        private int GetDistance(LocationNode a, LocationNode b)
        {
            var distanceX = Math.Abs(a.XCoord - b.XCoord);
            var distanceY = Math.Abs(a.YCoord - b.YCoord);

            return distanceX + distanceY;
        }
        
    }

    public class LocationNode : ILocation
    {
        public TerrainTile Tile { get; init; }

        public LocationNode? Parent { get; set; }

        public float GCost { get; set; }

        public float HCost { get; set; }

        public float FCost => GCost + HCost;

        public int XCoord { get => Tile.XCoord; init => throw new NotImplementedException(); }

        public int YCoord { get => Tile.YCoord; init => throw new NotImplementedException(); }

        public LocationNode(TerrainTile tile)
        {
            Tile = tile;
        }
    }
}
