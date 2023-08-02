using D3DLab.ECS;
using D3DLab.Toolkit.Math3D;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit.D3Objects {
    public class CoordinateSystemLinesGameObject : MultiVisualObject {
        ElementTag lines;
        ElementTag[] arrows;

        public static CoordinateSystemLinesGameObject Create(IContextState context, bool visible = true, Vector3 center = new Vector3()) {
            var llength = 100;
            var obj = new CoordinateSystemLinesGameObject();
            var points = new[] {
                center - Vector3.UnitX * llength, center + Vector3.UnitX * llength,
                center- Vector3.UnitY  * llength, center + Vector3.UnitY * llength,
                center- Vector3.UnitZ  * llength, center + Vector3.UnitZ * llength,
            };
            var color = new[] {
                V4Colors.Red, V4Colors.Red,
                V4Colors.Green, V4Colors.Green,
                V4Colors.Blue, V4Colors.Blue,
            };
            obj.lines = VisualPolylineObject.Create(context, new ElementTag("Coordinate System"),
                points, color, visible).Tag;
            var lenght = 20.0f;
            var radius = 5.0f;

            obj.arrows = new ElementTag[3];
            obj.arrows[0] = ArrowGameObject.Create(context, new ElementTag("Arrow_Z"), new ArrowData {
                axis = Vector3.UnitZ,
                orthogonal = Vector3.UnitX,
                center = center + Vector3.UnitZ * (llength - lenght + 5),
                lenght = lenght,
                radius = radius,
                color = V4Colors.Blue
            }, visible).Tag;
            obj.arrows[1] = ArrowGameObject.Create(context, new ElementTag("Arrow_X"), new ArrowData {
                axis = Vector3.UnitX,
                orthogonal = Vector3.UnitY,
                center = center + Vector3.UnitX * (llength - lenght + 5),
                lenght = lenght,
                radius = radius,
                color = V4Colors.Red,
            }, visible).Tag;
            obj.arrows[2] = ArrowGameObject.Create(context, new ElementTag("Arrow_Y"), new ArrowData {
                axis = Vector3.UnitY,
                orthogonal = Vector3.UnitZ,
                center = center + Vector3.UnitY * (llength - lenght + 5),
                lenght = lenght,
                radius = radius,
                color = V4Colors.Green,
            }, visible).Tag;

            obj.tags.AddRange(obj.arrows);
            obj.tags.Add(obj.lines);

            return obj;
        }

        public CoordinateSystemLinesGameObject() : base(typeof(CoordinateSystemLinesGameObject).Name) {

        }


    }
}
