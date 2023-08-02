using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using D3DLab.ECS.Common;
using D3DLab.ECS.Components;

namespace D3DLab.Toolkit {
    public static class CameraUtils {
        public static OrthographicCameraComponent FocusCameraOnBox(OrthographicCameraComponent current,
            AxisAlignedBox box, SurfaceSize surfaceSize) {
            var newone = OrthographicCameraComponent.Clone(current);

            var size = box.Size();
            var aspectRatio = surfaceSize.Width / surfaceSize.Height;

            var move = Math.Max(Math.Abs(newone.LookDirection.X * size.X),
            Math.Max(Math.Abs(newone.LookDirection.Y * size.Y), Math.Abs(newone.LookDirection.Z * size.Z)));

            newone.Position = box.Center + newone.LookDirection * -move * 10;
            newone.RotatePoint = box.Center;
            newone.Width = Math.Max(size.X, Math.Max(size.Y, size.Z)) * aspectRatio;
            newone.Scale = 1;

            return newone;
        }

    }
}
