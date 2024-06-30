using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architecture
{
    public class Chunk
    {
        private int width;
        private int height;
        private Block[,] blocks;

        public Chunk(int width, int height)
        {
            this.width = width;
            this.height = height;
            blocks = new Block[width, height];
        }

        public Block GetBlock(int x, int y)
        {
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                return blocks[x, y];
            }
            return null;
        }

        public void SetBlock(int x, int y, Block block)
        {
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                blocks[x, y] = block;
            }
        }
    }

}
