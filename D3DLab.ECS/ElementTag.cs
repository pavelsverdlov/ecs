using System;
using System.Collections.Generic;

namespace D3DLab.ECS {
    public readonly struct ElementTag : IEquatable<ElementTag> {
        public static ElementTag Empty = new ElementTag(string.Empty);
        public static ElementTag New() => new ElementTag(Guid.NewGuid().ToString());
        public static ElementTag New(string prefix) => new ElementTag(string.Concat(prefix,"_",Guid.NewGuid().ToString()));

        readonly string tag;      
        public ElementTag(string tag) {
            this.tag = tag;
        }
        public ElementTag WithPrefix(string prefix) => new ElementTag(string.Concat(prefix, "_", tag));

        public bool IsEmpty => Empty.tag == tag || tag == null;

        public override bool Equals(object obj) {
            return obj is ElementTag && Equals((ElementTag)obj);
        }
        public bool Equals(ElementTag other) {
            return tag == other.tag;
        }

        //public override int GetHashCode() {
        //    var hashCode = -1778964077;
        //    //hashCode = hashCode * -1521134295 + base.GetHashCode();
        //    hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(tag);
        //    return hashCode;
        //}

        public override int GetHashCode() {
            return HashCode.Combine(tag);
        }

        public override string ToString() {
            return tag;
        }
        public static bool operator ==(ElementTag x, ElementTag y) {
            return x.Equals(y);
        }
        public static bool operator !=(ElementTag x, ElementTag y) {
            return !x.Equals(y);
        }
    }
}
