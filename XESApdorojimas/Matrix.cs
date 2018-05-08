using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XESApdorojimas
{
    class Matrix
    {
        public List<MatrixElement> Elements { get; private set; } = new List<MatrixElement>();

        public MatrixElement GetElement(string column, string row)
        {
            var existing = Elements.Where(x => x.Column == column && x.Row == row).FirstOrDefault();
            if(existing == null)
            {
                existing = new MatrixElement() { Row = row, Column = column };
                Elements.Add(existing);
            }
            return existing;           
        }
    }

    class MatrixElement
    {
        public string Column { get; set; }
        public string Row { get; set; }

        /// <summary>
        /// Column event cycling, e.g. aaaa
        /// </summary>
        public int CycleCount { get; set; }
        /// <summary>
        /// abab cycling
        /// </summary>
        public int ColumnCycling { get; set; }
        /// <summary>
        /// ab sequence
        /// </summary>
        public int Follows { get; set; }
        /// <summary>
        /// a...b sequence
        /// </summary>
        public int EventuallyFollows { get; set; }
        //pakeisti/prideti
        public int r3 { get; set; }

    }

    
}
