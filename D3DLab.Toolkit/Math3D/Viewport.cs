using D3DLab.ECS;
using D3DLab.ECS.Camera;
using D3DLab.ECS.Ext;
using D3DLab.SDX.Engine;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit.Math3D {
    public class Viewport : IViewport {
        public Vector2 Vector3ToScreen(Vector3 world, CameraState camera, IRenderableSurface window) {
            // Convert the given 3D point to homogenous   
            var screenPosition = new Vector4(world, 1);
            // Push the 3D point through the camera transformation pipeline  
            screenPosition = Vector4.Transform(screenPosition, camera.ViewMatrix);
            screenPosition = Vector4.Transform(screenPosition, camera.ProjectionMatrix);
            screenPosition = screenPosition / screenPosition.W;

            // At this point our screen position is the camera world ranging from  
            // -1 to 1  
            // We need adjust to our actual screen width and height  
            var x = ((screenPosition.X + 1.0f) / 2.0f) * window.Size.Width;
            //don't forget the flipping of the y coordinate, since the origin of the window is in a different corner:
            var y = ((-screenPosition.Y + 1.0f) / 2.0f) * window.Size.Height;

            return new Vector2(x, y);
        }
        public Vector3 ScreenToVector3(Vector2 screen, CameraState camera, IRenderableSurface window) {
            var winW = window.Size.Width;
            var winH = window.Size.Height;

            var c = UnProject(screen, camera, window);

            var plane = new SharpDX.Plane(camera.Position.ToSDXVector3(), camera.LookDirection.ToSDXVector3());
            var ray = new SharpDX.Ray(c.Position.ToSDXVector3(), -c.Direction.ToSDXVector3());
            var inter = plane.Intersects(ref ray, out SharpDX.Vector3 point);

            return new Vector3(point.X, point.Y, point.Z);
        }

        void Unproject(IRenderableSurface window, ref Vector3 source, ref Matrix4x4 matrix, out Vector3 vector) {
            var X = 0;
            var Y = 0;
            var MinDepth = 0f;
            var MaxDepth = 1f;
            vector.X = (((source.X - X) / (window.Size.Width)) * 2f) - 1f;
            vector.Y = -((((source.Y - Y) / (window.Size.Height)) * 2f) - 1f);
            vector.Z = (source.Z - MinDepth) / (MaxDepth - MinDepth);

            float a = (((vector.X * matrix.M14) + (vector.Y * matrix.M24)) + (vector.Z * matrix.M34)) + matrix.M44;
            vector = Vector3.Transform(vector, matrix);

            if (!SharpDX.MathUtil.IsOne(a)) {
                vector = (vector / a);
            }
        }


        public Ray UnProject(Vector2 screen, CameraState camera, IRenderableSurface window) {
            var winW = window.Size.Width;
            var winH = window.Size.Height;

            var matrix = camera.ViewMatrix * camera.ProjectionMatrix;
            Matrix4x4.Invert(matrix, out matrix);

            //по X,Y позицией курсора, по Z: 0-мин. глубина, 1-максимальная глубина.
            var nearSource = new Vector3(screen, 0);
            var farSource = new Vector3(screen, 1);

            Unproject(window, ref nearSource, ref matrix, out var nearPoint);
            Unproject(window, ref farSource, ref matrix, out var farPoint);

            //вычисляем направление и нормируем его.
            var direction = farPoint - nearPoint;
            direction.Normalize();

            return new Ray(nearPoint, direction);
            //return UnProject(camera, winW, winH, screen);
        }

        public Ray UnProject(CameraState camera, float w, float h, Vector2 point2d) {//IAppWindow win,
            var px = (float)point2d.X;
            var py = (float)point2d.Y;

            var viewMatrix = camera.ViewMatrix;
            Vector3 v = new Vector3();

            var matrix = viewMatrix.PsudoInverted();
            //float w = win.Width;
            //float h = win.Height;

            var projMatrix = camera.ProjectionMatrix;
            Vector3 zn;
            v.X = (2 * px / w - 1) / projMatrix.M11;
            v.Y = -(2 * py / h - 1) / projMatrix.M22;
            v.Z = 1 / projMatrix.M33;
            Vector3 zf = Vector3.Transform(v, matrix);

            v.Z = 0;
            zn = Vector3.Transform(v, matrix);

            Vector3 r = zf - zn;
            r.Normalize();

            return new Ray(zn + r * camera.NearPlaneDistance, r);
        }
    }
}
