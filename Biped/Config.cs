using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using YamlDotNet;
using YamlDotNet.RepresentationModel;

namespace Biped
{
    class Config
    {
        public int Left;
        public int Middle;
        public int Right;
       

        public void Load(string path)
        {
            var file = File.OpenText(path);

            var yaml = new YamlStream();

            yaml.Load(file);

            var mapping =
                (YamlMappingNode)yaml.Documents[0].RootNode;

            var children = mapping.Children;

            var l = children[new YamlScalarNode("left")];
            var m = children[new YamlScalarNode("middle")];
            var r = children[new YamlScalarNode("right")];

            Left = Convert.ToUInt16(l.ToString(), 16);
            Middle = Convert.ToUInt16(m.ToString(), 16);
            Right = Convert.ToUInt16(r.ToString(), 16);


        }
    }
}
