using LandSim.Areas.Agents.Models;
using LandSim.Areas.Configuration.Models;
using LandSim.Areas.Map.Enums;
using LandSim.Areas.Map.Models;
using LandSim.Extensions;
using System.Linq;

namespace Agent.Boar
{
    public class BoarService
    {
        public AgentAction GetAction(AgentContext context)
        {
            if (context.Agent is null) throw new NullReferenceException("Recieved agent was null");

            var random = new Random();
            var shortTermMemory = ShortTermMemory.FromCompressedString(context.Agent.ShortTermMemory) ?? new ShortTermMemory();
            if(shortTermMemory.WanderDestination is null || shortTermMemory.WanderStepsRemaining <= 0 || context.Agent.IsAt(shortTermMemory.WanderDestination))
            {
                shortTermMemory = shortTermMemory with
                {
                    //TODO: We should update this per move
                    WanderDestination = new Destination
                    {
                        XCoord = random.Next(-20, 21),
                        YCoord = random.Next(-20, 21)
                    },
                    WanderStepsRemaining = 10
                };
            }
            else
            {
                shortTermMemory = shortTermMemory with
                {
                    WanderStepsRemaining = shortTermMemory.WanderStepsRemaining - 1
                };
            }

            var closestConsumable = context.Consumables
                .OrderBy(consumable => Math.Abs(consumable.XCoord - context.Agent.XCoord) + Math.Abs(consumable.YCoord - context.Agent.YCoord))
                .FirstOrDefault();

            ILocation destination = context.Agent switch
            {
                var agent when closestConsumable is not null => closestConsumable!,
                _ => shortTermMemory.WanderDestination!
            };

            ILocation nextMoveTarget = context.Agent.IsAt(destination) 
                ? destination 
                : GetNextMoveTarget(context.Agent, destination, context.TerrainTiles, context.SimulationConfig ?? new SimulationConfig());

            var agentAction = nextMoveTarget switch
            {
                var dest when dest.XCoord < context.Agent.XCoord => AgentActionType.MoveLeft,
                var dest when dest.XCoord > context.Agent.XCoord => AgentActionType.MoveRight,
                var dest when dest.YCoord < context.Agent.YCoord => AgentActionType.MoveUp,
                var dest when dest.YCoord > context.Agent.YCoord => AgentActionType.MoveDown,
                Consumable => AgentActionType.Eat,
                _ => AgentActionType.None
            };

            return new AgentAction
            {
                AgentId = context.Agent.AgentId,
                ActionType = agentAction,
                UpdatedShortTermMemory = shortTermMemory.ToCompressedString()
            };
        }

        private ILocation GetNextMoveTarget(ILocation currentLocation, ILocation destination, IEnumerable<TerrainTile> tiles, SimulationConfig config)
        {
            var nodes = tiles
                .Select(tile => new LocationNode(tile))
                .Concat(tiles.Any(destination.IsAt) 
                    ? Enumerable.Empty<LocationNode>() 
                    : new LocationNode[] { GetDefaultLocationNode(destination.XCoord, destination.YCoord) })
                .ToArray();

            var bounds = Bounds.FromLocations(nodes);
            var grid = nodes.MapLocationsToBoundedGrid(bounds, GetDefaultLocationNode);

            var startNode = nodes.First(currentLocation.IsAt);
            var endNode = nodes.First(destination.IsAt);

            var path = CalculatePath(startNode, endNode, grid, config);
            return path.FirstOrDefault()?.Tile ?? currentLocation;
        }

        private LocationNode GetDefaultLocationNode(int xCoord, int yCoord) => new LocationNode(new TerrainTile 
        { 
            XCoord = xCoord, 
            YCoord = yCoord, 
            TerrainType = TerrainType.Sand 
        });

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
