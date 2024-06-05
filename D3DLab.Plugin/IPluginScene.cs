using System.Numerics;

using D3DLab.ECS;

namespace D3DLab.Plugin {
    public struct ArrowDetails {
        public Vector3 Axis;
        public Vector3 Orthogonal;
        public Vector3 Center;
        public Vector4 Color;
    }
    public struct CylinderDetails {
        public Vector3 Axis;
        public Vector3 Start;
        public Vector4 Color;
        public float Length;
        public float Radius;
    }
    public struct PointDetails {
        public Vector3 Center;
        public Vector4 Color;
        public float Radius;
    }

    public interface IPluginScene {
        public IContextState Context { get; }

        GameObject DrawPoint(string key, PointDetails details);
        GameObject DrawPoint(ElementTag tag, PointDetails details);
        GameObject DrawArrow(string key, ArrowDetails ad);
        GameObject DrawPolyline(string key, Vector3[] margin, Vector4 green);
        GameObject DrawCylinder(string key, CylinderDetails cyl);
        //GameObject DrawBox(string key, CylinderDetails cyl);

        void DrawObject(GameObject obj);
        void MoveCameraToEntity(GameObject obj);
        GraphicEntity GetWorld();
        void SetContinuouslyRender(bool enable);
    }
}