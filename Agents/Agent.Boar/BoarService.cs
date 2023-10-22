using LandSim.Areas.Agents.Models;
using LandSim.Areas.Configuration.Models;
using LandSim.Areas.Map.Enums;
using LandSim.Areas.Map.Models;
using LandSim.Extensions;
using System.Linq;

namespace Agents.Boar
{
    public class BoarService
    {
        public AgentAction GetAction(AgentContext context)
        {
            if (context.Agent is null) throw new NullReferenceException("Recieved agent was null");

            var closestConsumable = context.Consumables
                .OrderBy(context.Agent.DistanceTo)
                .FirstOrDefault();

            var closestWaterSource = context.TerrainTiles
                .Where(tile => tile.TerrainType == TerrainType.Water)
                .OrderBy(context.Agent.DistanceTo)
                .FirstOrDefault();

            var random = new Random();
            var shortTermMemory = ShortTermMemory.FromCompressedString(context.Agent.ShortTermMemory) ?? new ShortTermMemory();

            //Update destinations relative to what our previous action was
            (int deltaX, int deltaY) = shortTermMemory.PreviousAction switch
            {
                AgentActionType.MoveLeft => (-1, 0),
                AgentActionType.MoveRight => (1, 0),
                AgentActionType.MoveUp => (0, -1),
                AgentActionType.MoveDown => (0, 1),
                _ => (0, 0)
            };

            shortTermMemory = shortTermMemory with
            {
                WanderDestination = shortTermMemory.WanderDestination switch
                {
                    var dest when dest is null || shortTermMemory.WanderStepsRemaining <= 0 || context.Agent.IsAt(dest) => new Destination
                    {
                        XCoord = random.Next(-20, 21),
                        YCoord = random.Next(-20, 21)
                    },
                    var dest => new Destination
                    {
                        XCoord = dest!.XCoord - deltaX,
                        YCoord = dest.YCoord - deltaY
                    }
                },
                WaterSource = shortTermMemory.WaterSource switch
                {
                    var dest when closestWaterSource is not null 
                        && (dest is null || context.Agent.DistanceTo(dest) > context.Agent.DistanceTo(closestWaterSource)) => new Destination 
                    { 
                        XCoord = closestWaterSource.XCoord, 
                        YCoord = closestWaterSource.YCoord
                    },
                    var dest when dest is not null => new Destination
                    {
                        XCoord = dest.XCoord - deltaX,
                        YCoord = dest.YCoord - deltaY
                    },
                    var dest => dest
                },
                WanderStepsRemaining = shortTermMemory.WanderStepsRemaining <= 0 ? 10 : shortTermMemory.WanderStepsRemaining - 1
            };

            var closestReproductionTarget = context.Agents
                .Where(agent => 
                    agent.AgentId != context.Agent.AgentId 
                    && agent.AgentOwnerId == context.Agent.AgentOwnerId 
                    && agent.ReproductionCooldown <= 0)
                .OrderBy(context.Agent.DistanceTo)
                .FirstOrDefault();

            ILocation destination = context.Agent switch
            {
                { Thirst: < 0.25f } when shortTermMemory.WaterSource is not null => shortTermMemory.WaterSource,
                { Thirst: < 0.25f } when shortTermMemory.WaterSource is null => shortTermMemory.WanderDestination,
                { Hunger: < 0.25f } when closestConsumable is not null => closestConsumable,
                { ReproductionCooldown: <= 0 } when closestReproductionTarget is not null => closestReproductionTarget,
                var agent when closestConsumable is not null => closestConsumable,
                _ => shortTermMemory.WanderDestination
            };

            var otherAgents = context.Agents.Where(agent => agent.AgentId != context.Agent.AgentId);

            ILocation nextMoveTarget = context.Agent.IsAt(destination) 
                ? destination 
                : GetNextMoveTarget(context.Agent, destination, context.TerrainTiles, otherAgents, context.SimulationConfig ?? new SimulationConfig());

            var agentAction = nextMoveTarget switch
            {
                _ when closestReproductionTarget is not null && context.Agent.ReproductionCooldown <= 0 && context.Agent.IsNextTo(closestReproductionTarget) => AgentActionType.Reproduce,
                var dest when dest.XCoord < context.Agent.XCoord => AgentActionType.MoveLeft,
                var dest when dest.XCoord > context.Agent.XCoord => AgentActionType.MoveRight,
                var dest when dest.YCoord < context.Agent.YCoord => AgentActionType.MoveUp,
                var dest when dest.YCoord > context.Agent.YCoord => AgentActionType.MoveDown,
                Consumable => AgentActionType.Eat,
                var dest when closestWaterSource is not null && context.Agent.IsNextTo(closestWaterSource) => AgentActionType.Drink,
                _ => AgentActionType.None
            };

            shortTermMemory = shortTermMemory with
            {
                PreviousAction = agentAction
            };

            return new AgentAction
            {
                AgentId = context.Agent.AgentId,
                ActionType = agentAction,
                UpdatedShortTermMemory = shortTermMemory.ToCompressedString()
            };
        }

        private ILocation GetNextMoveTarget(ILocation currentLocation, ILocation destination, IEnumerable<TerrainTile> tiles, IEnumerable<ILocation> obstacles, SimulationConfig config)
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

            var path = CalculatePath(startNode, endNode, grid, obstacles, config);
            return path.FirstOrDefault()?.Tile ?? currentLocation;
        }

        private LocationNode GetDefaultLocationNode(int xCoord, int yCoord) => new LocationNode(new TerrainTile 
        { 
            XCoord = xCoord, 
            YCoord = yCoord, 
            TerrainType = TerrainType.Sand 
        });

        //Calculate path using A* algorithm
        private List<LocationNode> CalculatePath(LocationNode start, LocationNode end, LocationNode?[,] grid, IEnumerable<ILocation> obstacles, SimulationConfig config)
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

                var immediateNeightbors = grid.GetImmediateNeighbors(current);

                foreach(var neighbor in immediateNeightbors)
                {
                    if(!neighbor.IsAt(end) && (!neighbor.Tile.IsWalkable() || closed.Any(neighbor.IsAt)))
                    {
                        continue;
                    }

                    //If there is an obstacle directly in front of our direction of travel, prefer to go to the right
                    var directionOfTravel = current.Parent?.Difference(current) ?? (0, 0);

                    var nextDestinationInDirection = new Destination
                    {
                        XCoord = current.XCoord + directionOfTravel.deltaX,
                        YCoord = current.YCoord + directionOfTravel.deltaY
                    };

                    var obstacleInDirectionOfTravel = obstacles.FirstOrDefault(nextDestinationInDirection.IsAt);

                    //Get tile to the right of current based on direction of travel
                    var nextDestinationToRight = new Destination
                    {
                        XCoord = current.XCoord + directionOfTravel.deltaY,
                        YCoord = current.YCoord - directionOfTravel.deltaX
                    };

                    var prioritizeRight = obstacleInDirectionOfTravel is not null;
                    var rightPrioValue = !prioritizeRight ? 0 : neighbor.IsAt(nextDestinationToRight) ? 0 : 1;

                    var heightIncrease = MathF.Max(0, neighbor.Tile.Height - current.Tile.Height);
                    var heightCost = heightIncrease * config.MovementHungerCost;
                    var vegitationCost = neighbor.Tile.VegetationLevel * config.VegitationMovementHungerCost;
                    var baseMovementCost = GetDistance(current, neighbor) * config.MovementHungerCost;

                    var movementCost = current.GCost + baseMovementCost + heightCost + vegitationCost + rightPrioValue;
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
