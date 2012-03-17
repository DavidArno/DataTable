﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;

namespace DataAccess
{
    // This is ROW major order, since it reads each row one at a time.
    // This is ideal for large read-only files.
    public class StreamingDataTable : DataTable
    {
        private readonly string _filename;
        private string[] _names;

        public StreamingDataTable(string filename)
        {
            _filename = filename;
        }

        public override IEnumerable<string> ColumnNames
        {
            get
            {
                if (_names == null)
                {
                    using (TextReader sr = this.OpenText())
                    {
                        // First get columns.
                        string header = sr.ReadLine();
                        char ch = Reader.GuessSeparateFromHeaderRow(header);
                        _names = Reader.split(header, ch);
                    }
                }
                return _names;
            }
        }

        private TextReader OpenText()
        {
            return new StreamReader(_filename);
        }

        public override IEnumerable<Row> Rows
        {
            get
            {
                int columnCount = this.ColumnNames.Count();
                TextReader sr = this.OpenText();

                string header = sr.ReadLine(); // skip past header
                char chSeparator = Reader.GuessSeparateFromHeaderRow(header);

                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] parts = Reader.split(line, chSeparator);
                    if (parts.Length != columnCount)
                    {
                        continue; // skip malformed input
                    }

                    yield return new RowFromStreamingTable(parts, this);
                }

            }

        }

        // Representation of a row for this table.
        private class RowFromStreamingTable : Row
        {
            readonly string[] _values;
            readonly DataTable _table;

            internal RowFromStreamingTable(string[] values, DataTable table)
            {
                _values = values;
                _table = table;
            }
            public override string[] Values
            {
                get
                {
                    return _values;
                }
            }

            public override IEnumerable<string> ColumnNames
            {
                get { return _table.ColumnNames; }
            }
        }
    }

}