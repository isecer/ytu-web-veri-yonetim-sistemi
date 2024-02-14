using System.Collections.Generic;

namespace BiskaUtil
{
    public class SelectedTableRoll
    {
        public string RoleName { get; set; }
        public string TableIdName { get; set; }
        public int TableId { get; set; }
        public int? SelectedId { get; set; }
        public List<int> RefTableIDs { get; set; }
    }
}
