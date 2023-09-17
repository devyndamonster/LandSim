SELECT 
    T.XCoord,
    T.YCoord,
    T.TerrainType,
    T.Height,
    T.VegetationLevel,
    C.ConsumableId,
    A.AgentId
FROM TerrainTiles T
LEFT JOIN Consumables C ON C.XCoord = T.XCoord AND C.YCoord = T.YCoord
LEFT JOIN Agents A ON A.XCoord = T.XCoord AND A.YCoord = T.YCoord
