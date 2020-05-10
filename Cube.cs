using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku.NET
{
    public class Cube
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public List<Cell> Cells {get;set;}
    }
}
