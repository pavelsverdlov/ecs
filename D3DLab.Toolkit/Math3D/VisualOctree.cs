#define OCTREEDEBUG

using D3DLab.ECS;
using D3DLab.Toolkit.D3Objects;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace D3DLab.Toolkit.Math3D {
    public sealed class VisualOctreeOutOfBoxException : Exception {
        public VisualOctreeOutOfBoxException() : base("Item is out off bounds.") {
        }
    }

    /**
     * alternative 
     * https://github.com/Nition/UnityOctree
     * https://github.com/mcserep/NetOctree/tree/master/Octree
     * https://www.wobblyduckstudios.com/Code/OctTree.cs
     * https://www.gamedev.net/articles/programming/general-and-gameplay-programming/introduction-to-octrees-r3529/
     */
    public class VisualOctree<T> {
        readonly OctreeNode<T> root;
        readonly Dictionary<T, OctreeItem<T>> items;
        readonly List<VisualPolylineObject> drawedDebug;
        public AxisAlignedBox Bounds { get { return root.Bounds; } }

        public VisualOctree(AxisAlignedBox box, int MaximumChildren) {
            root = OctreeNode<T>.CreateRoot(ref box, MaximumChildren);
            items = new Dictionary<T, OctreeItem<T>>();
            drawedDebug = new List<VisualPolylineObject>();
        }

        public bool Add(AxisAlignedBox box, T item) {
            var oitem = new OctreeItem<T>(ref box, item);

            if (root.Add(ref box, oitem)) {
                items.Add(item, oitem);
                return true;
            }
            return false;
        }
        public void Remove(T item) {
            items[item].SelfRemove();
            items.Remove(item);
        }

        public bool TryRemove(T item) {
            if (items.ContainsKey(item)) {
                Remove(item);
                return true;
            }
            return false;
        }

        public IEnumerable<OctreeItem<T>> GetColliding(Ray ray, Func<T, bool> predicate) {
            var result = new HashSet<OctreeItem<T>>();
            root.GetColliding(result, ref ray, float.PositiveInfinity, predicate);
            return result;
        }

        public IEnumerable<OctreeItem<T>> GetColliding(AxisAlignedBox box, Func<T, bool> predicate) {
            var result = new HashSet<OctreeItem<T>>();
            root.GetColliding(result, ref box, predicate);
            return result;
        }
        public bool HasCollision(AxisAlignedBox box, Func<T, bool> predicate) {
            var result = new HashSet<OctreeItem<T>>();
            root.GetColliding(result, ref box, predicate);
            return result.Any();
        }


        public void Clear() {
            foreach (var i in items) {
                i.Value.SelfRemove();
            }
            items.Clear();
            root.Clear();
        }


        public void Draw(IContextState context) {
            ClearDrew(context);
            root.Draw(context, drawedDebug);
        }
        public void ClearDrew(IContextState context) {
            foreach (var b in drawedDebug) {
                b.Cleanup(context);
            }
            drawedDebug.Clear();
        }
    }

    public class OctreeNode<T> {
        private readonly Guid key;
        public static OctreeNode<T> CreateRoot(ref AxisAlignedBox box, int maximumChildren) {
            var node = new OctreeNode<T>(ref box, maximumChildren, null);
            return node;
        }

        const int NumChildNodes = 8;

        public AxisAlignedBox Bounds { get; }
        public OctreeNode<T> Parent { get; }
        public int MaximumChildren { get; }
        public OctreeNode<T>[] Nodes { get { return octants; } }

        OctreeNode<T>[] octants;
        readonly HashSet<OctreeItem<T>> items;

        public bool IsLeaf() { return !octants.Any(); }
        public bool IsEmpty() { return !items.Any(); }

        public bool IsRoot => Parent == null;

        public OctreeNode(ref AxisAlignedBox box, int maximumChildren, OctreeNode<T> parent) {
            key = Guid.NewGuid();
            Bounds = box;
            octants = Array.Empty<OctreeNode<T>>();
            MaximumChildren = maximumChildren;
            items = new HashSet<OctreeItem<T>>();
            Parent = parent;
        }


        OctreeNode<T>[] BuildNodes() {
            Vector3 dimensions = Bounds.Maximum - Bounds.Minimum;
            // Vector3 half = dimensions * 0.25f;
            Vector3 center = Bounds.Center;
            var m_region = Bounds;
            var octant = new AxisAlignedBox[NumChildNodes];
            octant[0] = new AxisAlignedBox(m_region.Minimum, center);
            octant[1] = new AxisAlignedBox(new Vector3(center.X, m_region.Minimum.Y, m_region.Minimum.Z), new Vector3(m_region.Maximum.X, center.Y, center.Z));
            octant[2] = new AxisAlignedBox(new Vector3(center.X, m_region.Minimum.Y, center.Z), new Vector3(m_region.Maximum.X, center.Y, m_region.Maximum.Z));
            octant[3] = new AxisAlignedBox(new Vector3(m_region.Minimum.X, m_region.Minimum.Y, center.Z), new Vector3(center.X, center.Y, m_region.Maximum.Z));
            octant[4] = new AxisAlignedBox(new Vector3(m_region.Minimum.X, center.Y, m_region.Minimum.Z), new Vector3(center.X, m_region.Maximum.Y, center.Z));
            octant[5] = new AxisAlignedBox(new Vector3(center.X, center.Y, m_region.Minimum.Z), new Vector3(m_region.Maximum.X, m_region.Maximum.Y, center.Z));
            octant[6] = new AxisAlignedBox(center, m_region.Maximum);
            octant[7] = new AxisAlignedBox(new Vector3(m_region.Minimum.X, center.Y, center.Z), new Vector3(center.X, m_region.Maximum.Y, m_region.Maximum.Z));
            return octant.Select(x => new OctreeNode<T>(ref x, MaximumChildren, this)).ToArray();
        }

        bool RebuildTree() {
            if (Bounds.Diagonal < 4) {
                // node is too small
                return false;
            }
            var sw = new Stopwatch();
            sw.Start();
            octants = BuildNodes();
            //var nodes = new HashSet<OctreeNode<T>>();
            //sorting existing items 
            var old = items.ToList();
            items.Clear();
            for (int i = 0; i < old.Count; i++) {
                old[i].RemoveOwner(this);
                AddIntoNodes(old[i]);
            }
            sw.Stop();
            // Log.Debug($"RebuildTree time: {sw.ElapsedMilliseconds} ms");
            return true;
        }

        void AddIntoNodes(OctreeItem<T> item) {
            var added = false;
            var box = item.Bound;
            for (int j = 0; j < octants.Length; j++) {
                var node = octants[j];

                var cross = node.Bounds.Contains(item.Bound);

                if (cross == AlignedBoxContainmentType.Contains) {
                    added = node.Add(ref box, item);
                    break;//if whole item inside octant, do not check the rest of octants
                }
                if (cross == AlignedBoxContainmentType.Intersects) {
                    //if item Intersects, ignore it 
                }
            }
            if (!added) {
                //if can't add to any suboctants will add in parent octant
                AddItem(item);
            }
            //items.Add(item);
        }

        public bool Add(ref AxisAlignedBox box, OctreeItem<T> item) {
            if (this.Bounds.Contains(ref box) == AlignedBoxContainmentType.Disjoint) {
                return false;
            }

            if (items.Count >= MaximumChildren && IsLeaf()) {
                if (this.Bounds.Contains(item.Bound) == AlignedBoxContainmentType.Contains) {
                    //split nodes if can
                    var builded = RebuildTree();
                    if (builded) {
                        //if octants were created
                        //try to add into suboctants
                        AddIntoNodes(item);
                    } else {
                        //if rebuild failed it means we can't add into suboctants
                        //so add item into current node because it Contains() == AlignedBoxContainmentType.Contains
                        AddItem(item);
                    }
                    return true;//always true, item was added in current octant or suboctants for sure!
                }
                return false;//can't contain whole item so upper/bigger octant must contain it
            }
            if (!IsLeaf()) {//try to add into some suboctants
                AddIntoNodes(item);
            } else {//till saved items less than max count just put in the current node
                AddItem(item);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetColliding(HashSet<OctreeItem<T>> result, ref Ray ray, float maxDistance, Func<T, bool> predicate) {
            if (!Bounds.Intersects(ref ray, out float distance) || distance > maxDistance) {
                return;
            }

            foreach (var item in items) {
                if (item.Bound.Intersects(ref ray, out distance) && distance <= maxDistance && predicate(item.Item)) {
                    result.Add(item);
                }
            }

            for (var j = 0; j < octants.Length; j++) {
                octants[j].GetColliding(result, ref ray, maxDistance, predicate);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetColliding(HashSet<OctreeItem<T>> result, ref AxisAlignedBox box, Func<T, bool> predicate) {
            if (!Bounds.Intersects(ref box)) {
                return;
            }
            foreach (var item in items) {
                if (item.Bound.Intersects(ref box) && predicate(item.Item)) {
                    result.Add(item);
                }
            }

            for (var j = 0; j < octants.Length; j++) {
                octants[j].GetColliding(result, ref box, predicate);
            }
        }

        public void Draw(IContextState context, List<VisualPolylineObject> drawed) {
            if (IsLeaf()) {
                drawed.Add(VisualPolylineObject.CreateBox(context, ElementTag.New(),
                    Bounds.GetCornersBox(), V4Colors.Yellow));
            } else {
                var nodes = Nodes.ToArray();
                for (int i = 0; i < nodes.Length; i++) {
                    nodes[i].Draw(context, drawed);
                }
            }
            foreach (var i in items.ToList()) {
                drawed.Add(VisualPolylineObject.CreateBox(context, ElementTag.New("DEBUG_BOX_"),
                     i.Bound.GetCornersBox(), V4Colors.Blue));
            }
        }

        public void Remove(OctreeItem<T> item) {
            items.Remove(item);
        }
        
        public void Clear() {
            foreach (var n in octants) {
                n.Clear();
            }
            octants = Array.Empty<OctreeNode<T>>();
            items.Clear();
        }

        public void MergeUp() {
            var current = this;
            while (!current.IsRoot && current.Merge()) {
                current = current.Parent;
            }
        }

        bool Merge() {
            var hasOctants = octants.Any();
            var hasItems = items.Any();
            if (!hasItems && !hasOctants) {
                return true;
            }
            if (!hasItems && hasOctants) {
                var mergedAll = true;
                foreach (var oc in octants) {
                    mergedAll = oc.Merge() && mergedAll;
                }
                if (mergedAll) {
                    octants = Array.Empty<OctreeNode<T>>();
                    return true;
                }
            }
            return false;
        }

        void AddItem(OctreeItem<T> item) {
            item.SetOwner(this);
            items.Add(item);
        }
    }

    public class OctreeItem<T> {
        OctreeNode<T> owner;
        public AxisAlignedBox Bound { get; }
        public T Item { get; }
        public OctreeItem(ref AxisAlignedBox box, T item) {
            Item = item;
            Bound = box;
        }

        public void SetOwner(OctreeNode<T> node) {
            if (owner != null) {
                throw new Exception("Item can have only one owner.");
            }
            owner = node;
        }
        public void RemoveOwner(OctreeNode<T> node) {
            owner = null;
        }
        public void SelfRemove() {
            //var temp = owners.ToArray();
            owner.Remove(this);
            owner.MergeUp();
            owner = null;
        }
    }
}
