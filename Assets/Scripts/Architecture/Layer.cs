using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architecture
{
    public class Layer
    {
        private int chunkWidth;
        private int chunkHeight;
        private Dictionary<Vector2Int, Chunk> chunks;

        public Layer(int chunkWidth, int chunkHeight)
        {
            this.chunkWidth = chunkWidth;
            this.chunkHeight = chunkHeight;
            chunks = new Dictionary<Vector2Int, Chunk>();
        }

        public Chunk GetOrCreateChunk(int chunkX, int chunkY)
        {
            Vector2Int chunkPos = new Vector2Int(chunkX, chunkY);
            if (!chunks.ContainsKey(chunkPos))
            {
                chunks[chunkPos] = new Chunk(chunkWidth, chunkHeight);
            }
            return chunks[chunkPos];
        }

        public Block GetBlock(int chunkX, int chunkY, int blockX, int blockY)
        {
            return GetOrCreateChunk(chunkX, chunkY).GetBlock(blockX, blockY);
        }

        public void SetBlock(int chunkX, int chunkY, int blockX, int blockY, Block block)
        {
            GetOrCreateChunk(chunkX, chunkY).SetBlock(blockX, blockY, block);
        }
    }

}
