using D3DLab.ECS;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Toolkit.Components {
    [Obsolete("Remake to using GeometryPool")]
    /// <summary>
    /// DO NOT USE GeometryComponent OUTSIDE
    /// APPROACH OF STORING GEO WILL BE CHANGED 
    /// </summary>
    public class GeometryComponent : GraphicComponent {
        public Vector3[] Positions { get; private set; }
        public int[] Indices { get; private set; }
        public Vector3[] Normals { get; private set; }
        public Vector2[] TexCoor { get; private set; }

        public GeometryComponent(Vector3[] positions, Vector3[] normals, int[] indices) {
            Positions = positions;
            Normals = normals;
            Indices = indices;
            TexCoor = new Vector2[positions.Length]; 
            IsModified = true;
        }
        public GeometryComponent(Vector3[] positions, Vector3[] normals, int[] indices, Vector2[] texCoor) {
            Positions = positions;
            Normals = normals;
            Indices = indices;
            TexCoor = texCoor;

            IsModified = true;
        }
    }
}
