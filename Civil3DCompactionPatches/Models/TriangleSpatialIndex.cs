using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DCompactionPatches.Models
{
    public sealed class TriangleSpatialIndex
    {
        private readonly Dictionary<(int X, int Y), List<TriangleData>> _cells = new();
        private readonly double _cellSize;

        public TriangleSpatialIndex(IEnumerable<TriangleData> triangles, double cellSize)
        {
            _cellSize = cellSize;

            foreach (TriangleData tri in triangles)
            {
                int minCellX = ToCell(tri.MinX);
                int maxCellX = ToCell(tri.MaxX);
                int minCellY = ToCell(tri.MinY);
                int maxCellY = ToCell(tri.MaxY);

                for (int x = minCellX; x <= maxCellX; x++)
                {
                    for (int y = minCellY; y <= maxCellY; y++)
                    {
                        var key = (x, y);

                        if (!_cells.TryGetValue(key, out var list))
                        {
                            list = new List<TriangleData>();
                            _cells[key] = list;
                        }

                        list.Add(tri);
                    }
                }
            }
        }

        public IEnumerable<TriangleData> Query(Extents3d ext)
        {
            var result = new HashSet<TriangleData>();

            int minCellX = ToCell(ext.MinPoint.X);
            int maxCellX = ToCell(ext.MaxPoint.X);
            int minCellY = ToCell(ext.MinPoint.Y);
            int maxCellY = ToCell(ext.MaxPoint.Y);

            for (int x = minCellX; x <= maxCellX; x++)
            {
                for (int y = minCellY; y <= maxCellY; y++)
                {
                    if (_cells.TryGetValue((x, y), out var list))
                    {
                        foreach (TriangleData tri in list)
                            result.Add(tri);
                    }
                }
            }

            return result;
        }

        private int ToCell(double value)
        {
            return (int)Math.Floor(value / _cellSize);
        }
    }
}
