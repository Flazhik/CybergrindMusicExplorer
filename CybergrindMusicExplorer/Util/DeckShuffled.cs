using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CybergrindMusicExplorer.Util
{
    public class DeckShuffled<T> : IEnumerable<T>
    {
        private List<T> _current;

        public DeckShuffled(IEnumerable<T> target) => _current = Randomize(target).ToList();

        public void Reshuffle()
        {
            if (_current.Count <= 1)
                return;
            IEnumerable<T> source1 = _current.Take(Mathf.FloorToInt(_current.Count / 2));
            IEnumerable<T> source2 = _current.Skip(Mathf.FloorToInt(_current.Count / 2));
            _current = Randomize(source1).Concat(Randomize(source2)).ToList();
        }

        private static IEnumerable<T> Randomize(IEnumerable<T> source)
        {
            T[] arr = source.ToArray();
            for (int i = arr.Length - 1; i > 0; --i)
            {
                int swapIndex = Random.Range(0, i + 1);
                yield return arr[swapIndex];
                arr[swapIndex] = arr[i];
            }

            yield return arr[0];
        }

        public IEnumerator<T> GetEnumerator() => _current.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _current.GetEnumerator();
    }
}