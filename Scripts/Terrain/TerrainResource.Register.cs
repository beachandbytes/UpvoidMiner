// Copyright (C) by Upvoid Studios
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>

using System;
using Engine;
using Engine.Universe;
using Engine.Scripting;
using System.Collections.Generic;

namespace UpvoidMiner
{
    /// <summary>
    /// In this part of the class, all resources are registered.
    /// </summary>
    public partial class TerrainResource
    {
        /// <summary>
        /// Registers all the resources.
        /// Currently only implemented for Host.
        /// </summary>
        public static void RegisterResources(TerrainEngine _terrain)
        {
            terrain = _terrain;

            // Register some simple solid materials.

            // Dirt
            addResource(new VegetatedTerrainResource("Dirt", "Terrain/Dirt", "Particles/Terrain/Dirt"));

            // Stones
            for (int i = 1; i <= 14; ++i)
                addResource(new SolidTerrainResource("Stone." + i.ToString("00"), "Terrain/Rock" + i.ToString("00"), "Particles/Terrain/Rock" + i.ToString("00")));

            // Wood
            addResource(new SolidTerrainResource("Wood", "Terrain/Wood", "Particles/Terrain/Wood"));

            // Ores + Metals
            addResource(new SolidTerrainResource("Coal", "Terrain/Coal", "Particles/Terrain/Coal"));
            addResource(new SolidTerrainResource("Copper", "Terrain/Copper", "Particles/Terrain/Copper"));
            addResource(new SolidTerrainResource("Iron", "Terrain/Iron", "Particles/Terrain/Iron"));
            addResource(new SolidTerrainResource("Gold", "Terrain/Gold", "Particles/Terrain/Gold"));

            // Rares
            addResource(new SolidTerrainResource("AoiCrystal", "Terrain/AoiCrystal", "Particles/Terrain/AoiCrystal"));
            addResource(new SolidTerrainResource("FireRock", "Terrain/FireRock", "Particles/Terrain/FireRock"));
            addResource(new SolidTerrainResource("AlienRock", "Terrain/AlienRock", "Particles/Terrain/AlienRock"));
        }

        /// <summary>
        /// The associated terrain engine
        /// </summary>
        private static TerrainEngine terrain;
        /// <summary>
        /// Mapping from material index to terrain resource.
        /// </summary>
        private static Dictionary<int, TerrainResource> indexToResource = new Dictionary<int, TerrainResource>();
        /// <summary>
        /// Mapping from material name to terrain resource.
        /// </summary>
        private static Dictionary<string, TerrainResource> nameToResource = new Dictionary<string, TerrainResource>();
        /// <summary>
        /// Gets a terrain resource based on material index
        /// </summary>
        public static TerrainResource FromIndex(int idx)
        {
            TerrainResource res;
            if (indexToResource.TryGetValue(idx, out res))
                return res;
            else
                return null;
        }
        /// <summary>
        /// Gets a terrain resource based on material name
        /// </summary>
        public static TerrainResource FromName(string name)
        {
            TerrainResource res;
            if (nameToResource.TryGetValue(name, out res))
                return res;
            else
                return null;
        }

        /// <summary>
        /// Adds a resource to the global dictionary.
        /// </summary>
        private static void addResource(TerrainResource res)
        {
            indexToResource.Add(res.Index, res);
            nameToResource.Add(res.Name, res);
        }
    }
}

