using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sudoku.NET
{
    public partial class MainForm : Form
    {
        List<Cell> cellList = new List<Cell>();
        List<List<Cell>> lines = new List<List<Cell>>();
        List<List<Cell>> columns = new List<List<Cell>>();

        List<Cube> cubes = new List<Cube>();
        List<List<Cube>> cubeLines = new List<List<Cube>>();
        List<List<Cube>> cubeColumns = new List<List<Cube>>();

        Cell selectedCell = null;
        int runCount = 0;
        List<Step> history = new List<Step>();

        public MainForm()
        {
            InitializeComponent();

            var regex = new Regex("btn[1-9]{2}", RegexOptions.Compiled);
            foreach (Control cell in this.Controls)
            {
                if (regex.IsMatch(cell.Name))
                {
                    cellList.Add((Cell)cell);
                }
            }

            cellList.ForEach(cell =>
            {
                //txtOutput.AppendText(btn.Name + Environment.NewLine);
                cell.Text = cell.Name.Replace("btn", "");
                cell.SetPosition(cell.Data);
                cell.Click += btn_Click;
                cell.KeyUp += btn_KeyUp;
                cell.Font = new System.Drawing.Font("Consolas", 15.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            });

            for (int i = 1; i < 10; i++)
            {
                List<Cell> line = new List<Cell>();
                List<Cell> column = new List<Cell>();

                line.AddRange(cellList.Where(c => c.Line == i));
                column.AddRange(cellList.Where(c => c.Column == i));

                lines.Add(line);
                columns.Add(column);
            }

            for (int i = 1; i < 4; i++)
            {
                for (int j = 1; j < 4; j++)
                {
                    List<Cell> cells = new List<Cell>();
                    cells.AddRange(cellList.Where(c => (c.Line - 1) / 3 + 1 == i && (c.Column - 1) / 3 + 1 == j));

                    var cube = new Cube()
                    {
                        Line = i,
                        Column = j,
                        Cells = cells
                    };

                    cubes.Add(cube);
                }
            }

            for (int i = 1; i < 4; i++)
            {
                var line = cubes.Where(c => c.Line == i).ToList();
                var column = cubes.Where(c => c.Column == i).ToList();
                cubeLines.Add(line);
                cubeColumns.Add(column);
            }

            cellList.ForEach(cell =>
            {
                cell.Data = 0;
            });
        }

        private void log(string msg)
        {
            txtOutput.AppendText(msg + Environment.NewLine);
        }

        private void btn_Click(object sender, EventArgs e)
        {
            var cell = (Cell)sender;
            log($"{cell.Name.Replace("btn", "cell")} clicked, data: {cell.Data}, line: {cell.Line}, column: {cell.Column}");
            selectedCell = cell;
        }

        private void btn_KeyUp(object sender, KeyEventArgs e)
        {
            string input = e.KeyData.ToString();
            var regex = new Regex("[D|NumPad][0-9]");
            if (regex.IsMatch(input))
            {
                var num = input.Replace("NumPad", "").Replace("D", "");
                if (selectedCell != null)
                {
                    var cells = new List<Cell>() { selectedCell };
                    var data = selectedCell.Data;

                    var newValue = int.Parse(num);
                    if (selectedCell.Data == newValue)
                    {
                        selectedCell.Data = 0; // click twice to clear
                    }
                    else
                    {
                        selectedCell.Data = newValue;
                    }

                    addToHistory(cells, data);
                }

            }
            else
            {
                log($"{e.KeyCode}, {e.KeyValue}, {e.KeyData}");
            }
        }

        enum method
        {
            lines = 0,
            columns = 1,
            cubes = 2,
            cube_line = 3,
            cube_column = 4
        }

        private List<Cell> fillEmptyCell(List<Cell> cellList, method method)
        {
            List<Cell> newCells = new List<Cell>();

            var emptyCells = cellList.Where(c => c.Data == 0);
            if (emptyCells.Count() == 1)
            {
                var emptyCell = cellList.FirstOrDefault(c => c.Data == 0);
                for (int i = 1; i < 10; i++)
                {
                    if (cellList.Exists(c => c.Data == i))
                    {
                        continue;
                    }

                    if (emptyCell != null)
                    {
                        setNumber(emptyCell, i, method);
                        newCells.Add(emptyCell);
                    }
                    break;
                }
            }
            else
            {
                var missingNumbers = new List<int>();
                for (int i = 1; i < 10; i++)
                {
                    if (cellList.Exists(c => c.Data == i))
                    {
                        continue;
                    }
                    missingNumbers.Add(i);
                }

                foreach (var cell in emptyCells)
                {
                    var possibleNumbers = new List<int>();
                    var line = lines[cell.Line - 1];
                    var column = columns[cell.Column - 1];
                    foreach (var n in missingNumbers)
                    {
                        if (line.Exists(c => c.Data == n) == false && column.Exists(c => c.Data == n) == false)
                        {
                            possibleNumbers.Add(n);
                        }
                    }

                    if (possibleNumbers.Count == 1)
                    {
                        setNumber(cell, possibleNumbers[0], method);
                        newCells.Add(cell);
                    }
                }
            }

            return newCells;
        }

        private void setNumber(Cell cell, int num, method method)
        {
            cell.Data = num;
            log($"{cell.Line} : {cell.Column} -> {num}, method: {method}");
        }

        private List<Cell> fillEmptyCellInCubes(List<Cube> cubes, method method)
        {
            List<Cell> newCells = new List<Cell>();

            for (int i = 1; i < 10; i++)
            {
                var existingCubes = cubes.Where(c => c.Cells.Exists(cell => cell.Data == i)).ToList();
                var count = existingCubes.Count;
                if (count == 2)
                {
                    var existingCells = new List<Cell>();
                    foreach (var cube in existingCubes)
                    {
                        foreach (var cell in cube.Cells)
                        {
                            if (cell.Data == i)
                            {
                                existingCells.Add(cell);
                            }
                        }
                    }

                    var targetCube = cubes.First(c => existingCubes.Contains(c) == false);

                    List<Cell> targetCells;
                    try
                    {
                        if (method == method.cube_line)
                        {
                            var lineNumbers = targetCube.Cells.Select(c => c.Line).Distinct().ToList();
                            var targetLineNumber = lineNumbers.First(n => existingCells.Exists(c => c.Line == n) == false);
                            targetCells = targetCube.Cells.Where(c => c.Line == targetLineNumber && c.Data == 0).ToList();
                        }
                        else
                        {
                            var lineNumbers = targetCube.Cells.Select(c => c.Column).Distinct().ToList();
                            var targetLineNumber = lineNumbers.First(n => existingCells.Exists(c => c.Column == n) == false);
                            targetCells = targetCube.Cells.Where(c => c.Column == targetLineNumber && c.Data == 0).ToList();
                        }
                    }
                    catch (Exception)
                    {
                        log($"incorrect data found: {method}, {i}");
                        continue;
                    }

                    if (targetCells.Count() == 1)
                    {
                        setNumber(targetCells[0], i, method);
                        newCells.Add(targetCells[0]);
                    }
                    else
                    {
                        var possibleCells = targetCells.Where(cell =>
                        {
                            var line = lines[cell.Line - 1];
                            var column = columns[cell.Column - 1];
                            return line.Exists(c => c.Data == i) == false && column.Exists(c => c.Data == i) == false;
                        }).ToList();

                        if (possibleCells.Count == 1)
                        {
                            setNumber(possibleCells[0], i, method);
                            newCells.Add(possibleCells[0]);
                        }
                    }
                }
            }

            return newCells;
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            runCount++;
            log($"--- start to process: {runCount} ---");
            var cells = fillAvailableCells();
            addToHistory(cells, 0);
        }

        private List<Cell> fillAvailableCells()
        {
            List<Cell> newCells = new List<Cell>();

            foreach (var line in lines)
            {
                var nc = fillEmptyCell(line, method.lines);
                newCells.AddRange(nc);
            }

            foreach (var column in columns)
            {
                var nc = fillEmptyCell(column, method.columns);
                newCells.AddRange(nc);
            }

            foreach (var cube in cubes)
            {
                var nc = fillEmptyCell(cube.Cells, method.cubes);
                newCells.AddRange(nc);
            }

            foreach (var cubes in cubeLines)
            {
                var nc = fillEmptyCellInCubes(cubes, method.cube_line);
                newCells.AddRange(nc);
            }

            foreach (var cubes in cubeColumns)
            {
                var nc = fillEmptyCellInCubes(cubes, method.cube_column);
                newCells.AddRange(nc);
            }

            return newCells;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            cellList.ForEach(cell =>
            {
                cell.Data = 0;
            });

            history.Clear();
            txtOutput.Clear();
        }

        private void btnUndo_Click(object sender, EventArgs e)
        {
            undoLastStep();
        }

        private bool validate_all()
        {
            bool validate = true;
            for (int i = 1; i <= 9; i++)
            {
                foreach (var line in lines)
                {
                    int count = line.Count(c => c.Data == i);
                    if (count >= 2)
                    {
                        validate = false;
                        break;
                    }
                }

                if (validate)
                {
                    foreach (var column in columns)
                    {
                        int count = column.Count(c => c.Data == i);
                        if (count >= 2)
                        {
                            validate = false;
                            break;
                        }
                    }
                }

                if (validate)
                {
                    foreach (var cube in cubes)
                    {
                        int count = cube.Cells.Count(c => c.Data == i);
                        if (count >= 2)
                        {
                            validate = false;
                            break;
                        }
                    }
                }

                if (validate == false)
                {
                    break;
                }
            }

            return validate;
        }

        private void btnValidate_Click(object sender, EventArgs e)
        {
            var validate = validate_all();

            if (validate == false)
            {
                log("Invalid number found..");
            }
            else
            {
                log("All looks good..");
            }
        }

        private void btnQueen_Click(object sender, EventArgs e)
        {
            var emptyCells = cellList.Where(c => c.Data == 0);

            var suitableCells = emptyCells.Where(c =>
            {
                var pvList = getPossibleValues(c);
                return pvList.Count == 2;
            });

            if (suitableCells.Count() == 0)
            {
                log("No suitable cell found..");
                return;
            }

            int count = 0;
            foreach (var cell in suitableCells)
            {
                count++;
                log($"Queen check, {count}, cell: {cell.Line}-{cell.Column}");

                var possibleValues = getPossibleValues(cell);
                if (possibleValues.Count != 2)
                {
                    continue;
                }

                cell.Data = possibleValues[0];
                var cells = new List<Cell>() { cell };
                var filled = fillAvailableCells();
                while (filled.Count > 0)
                {
                    cells.AddRange(filled);
                    filled = fillAvailableCells();
                }
                addToHistory(cells, 0);

                if (cells.Count > 1)
                {
                    bool valid = validate_all();
                    if (valid == false)
                    {
                        undoLastStep();

                        cell.Data = possibleValues[1];
                        cells = new List<Cell>() { cell };
                        filled = fillAvailableCells();
                        while (filled.Count > 0)
                        {
                            cells.AddRange(filled);
                            filled = fillAvailableCells();
                        }

                        addToHistory(cells, 0);
                        emptyCells = cellList.Where(c => c.Data == 0);
                        if (emptyCells.Count() == 0)
                        {
                            log("All done..");
                        }
                    }
                    else
                    {
                        emptyCells = cellList.Where(c => c.Data == 0);
                        if (emptyCells.Count() == 0)
                        {
                            log("All done..");
                        }
                        else
                        {
                            undoLastStep();
                        }
                    }
                }
                else
                {
                    undoLastStep();
                }
            }
        }

        private void undoLastStep()
        {
            if (history.Count > 0)
            {
                var lastTracker = history.Last();
                foreach (var cell in lastTracker.Cells)
                {
                    cell.Data = lastTracker.Data;
                }
                history.Remove(lastTracker);
                log($"Undo last step {history.Count + 1}, reset {lastTracker.Cells.Count} cells..");
            }
        }

        private void addToHistory(List<Cell> cells, int data)
        {
            if (cells.Count > 0)
            {
                var tracker = new Step() { Cells = cells, Data = data };
                history.Add(tracker);
                log($"Add new step: {history.Count}");
            }
        }

        private List<int> getPossibleValues(Cell cell)
        {
            List<int> possibleValues = new List<int>();
            if (cell.Data != 0)
            {
                possibleValues.Add(cell.Data);
            }
            else
            {
                var line = lines.First(l => l.Contains(cell));
                var column = columns.First(c => c.Contains(cell));
                var cube = cubes.First(c => c.Cells.Contains(cell));
                for (int i = 1; i <= 9; i++)
                {
                    if (line.Exists(c => c.Data == i) == false && column.Exists(c => c.Data == i) == false && cube.Cells.Exists(c => c.Data == i) == false)
                    {
                        possibleValues.Add(i);
                    }
                }
            }

            return possibleValues;
        }
    }
}
