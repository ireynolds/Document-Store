using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Linq;

namespace NotepadTheNextVersion.Search
{
    public class SearchResultFile : File, IComparable<SearchResultFile>
    {
        public LinkedList<KeyValuePair<int, string[]>> hits { get; set; }

        public SearchResultFile(File file)
            : base(file.FilePath, file.FileName, file.ParentDirectory, file.DateEdited)
        {
            this.hits = new LinkedList<KeyValuePair<int, string[]>>();
        }

        public void AddHit(int index, string[] surroundingText)
        {
            if (surroundingText.Length != 3)
                throw new ArgumentException();

            hits.AddLast(new KeyValuePair<int, string[]>(index, surroundingText));
        }

        public KeyValuePair<int, string[]> getFirstHit()
        {
            if (hits.Count == 0)
                throw new InvalidOperationException();

            return new KeyValuePair<int, string[]>(hits.ElementAt(0).Key, hits.ElementAt(0).Value);
        }

        public int CompareTo(SearchResultFile other)
        {
            return other.hits.Count - this.hits.Count;
        }

        public int GetNumHits()
        {
            return hits.Count;
        }
    }
}
