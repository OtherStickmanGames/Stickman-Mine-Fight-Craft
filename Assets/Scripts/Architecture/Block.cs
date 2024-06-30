using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architecture
{
    public class Block
    {
        public int ID { get; set; }
        public bool IsSolid { get; set; }
        public Sprite Sprite { get; set; }

        public Block(int id, bool isSolid, Sprite sprite)
        {
            ID = id;
            IsSolid = isSolid;
            Sprite = sprite;
        }

        public Block()
        {

        }
    }
}
