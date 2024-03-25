﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FerngillHelper.Framework
{
    internal class Trees
    {


        public static string GetTreeName(string treeNum)
        {
            var output = treeNum switch
            {
                "1" => "Oak",
                "2" => "Maple",
                "3" => "Pine",
                "6" => "Palm",
                "7" => "Mushroom",
                "8" => "Mahogany",
                "9" => "Palm2",
                _ => ""
            };

            return output;
        }
    }
}
