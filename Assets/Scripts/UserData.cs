using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class User
{
    static User data;
    public static User Data
    {
        get
        {
            if (data == null)
            {
                data = new();
            }

            return data;
        }

        set => data = value;
    }

   
    public int golda;    

    
}
