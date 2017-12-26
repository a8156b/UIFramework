using System.Collections.Generic;
using UnityEngine;

namespace VBM {
    public class ListModel {
        public event System.Action<Model> elementAdded;
        public event System.Action<int, Model> elementInserted;
        public event System.Action<int> elementRemoved;
        public event System.Action<int, int> elementRemovRanged;
        public event System.Action elementCleared;
        public event System.Action<int, int> elementWraped;
        protected List<Model> list = new List<Model>();

        public int Count { get { return list.Count; } }

        public Model this[int index] {
            get { return list[index]; }
            set { list[index] = value; }
        }

        public void Add(Model item) {
            list.Add(item);
            if (elementAdded != null)
                elementAdded(item);
        }

        public void AddRange(IEnumerable<Model> collection) {
            list.AddRange(collection);
            if (elementAdded != null) {
                foreach (Model item in collection)
                    elementAdded(item);
            }
        }

        public void Insert(int index, Model item) {
            list.Insert(index, item);
            if (elementInserted != null)
                elementInserted(index, item);
        }

        public bool Remove(Model item) {
            int index = list.IndexOf(item);
            if (index != -1) {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public void RemoveAt(int index) {
            list.RemoveAt(index);
            if (elementRemoved != null)
                elementRemoved(index);
        }

        public void RemoveRange(int index, int count) {
            list.RemoveRange(index, count);
            if (elementRemovRanged != null)
                elementRemovRanged(index, count);
        }

        public void Sort(int index, int count, IComparer<Model> comparer) {
            list.Sort(index, count, comparer);
        }

        public void Sort(IComparer<Model> comparer) {
            list.Sort(comparer);
        }

        public void Sort() {
            list.Sort();
        }

        public bool Contains(Model item) {
            return list.Contains(item);
        }

        public Model Find(System.Predicate<Model> match) {
            return list.Find(match);
        }

        public void Clear() {
            list.Clear();
            if (elementCleared != null)
                elementCleared();
        }

        public int BinarySearch(int index, int count, Model item, IComparer<Model> comparer) {
            return list.BinarySearch(index, count, item, comparer);
        }

        public int BinarySearch(Model item) {
            return list.BinarySearch(item);
        }

        public int BinarySearch(Model item, IComparer<Model> comparer) {
            return list.BinarySearch(item, comparer);
        }
    }
}