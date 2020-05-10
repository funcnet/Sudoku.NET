using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku.NET
{
    public class Step
    {
        public List<Cell> Cells { get; set; }
        public int Data { get; set; }
    }
}
