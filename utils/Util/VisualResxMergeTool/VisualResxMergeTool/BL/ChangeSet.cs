using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualResxMergeTool.BL
{
    public class ChangeSet
    {
        public List<ModifiedNode> modified = new List<ModifiedNode>();

        public List<ResNode> deleted = new List<ResNode>();

        public List<ResNode> added = new List<ResNode>();
    }
}
