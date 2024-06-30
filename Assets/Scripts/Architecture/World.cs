using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architecture
{
    public class World
    {
        private List<Layer> layers;

        public World(int chunkWidth, int chunkHeight, int numLayers)
        {
            layers = new List<Layer>();
            for (int i = 0; i < numLayers; i++)
            {
                layers.Add(new Layer(chunkWidth, chunkHeight));
            }
        }

        public Block GetBlock(int layerIndex, int chunkX, int chunkY, int blockX, int blockY)
        {
            if (layerIndex >= 0 && layerIndex < layers.Count)
            {
                return layers[layerIndex].GetBlock(chunkX, chunkY, blockX, blockY);
            }
            return null;
        }

        public void SetBlock(int layerIndex, int chunkX, int chunkY, int blockX, int blockY, Block block)
        {
            if (layerIndex >= 0 && layerIndex < layers.Count)
            {
                layers[layerIndex].SetBlock(chunkX, chunkY, blockX, blockY, block);
            }
        }

        public int NumLayers
        {
            get { return layers.Count; }
        }
    }

}

