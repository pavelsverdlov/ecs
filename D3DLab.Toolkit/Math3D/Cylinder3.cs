using D3DLab.ECS.Ext;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit.Math3D {
    struct Cylinder3 {
        class Result {
            public List<Vector3> point = new List<Vector3>();
            public float[] parameter = new float[2];
            public int numIntersections;
            public bool intersect;
        }

        public float Height;
        public float Radius;
        public Vector3 AxisDirection;
        public Vector3 Origin;

        public AlignedBoxContainmentType Contains(Vector3 v0, Vector3 v1, Vector3 v2) {
            var out0 = IsOutside(v0, this);
            var out1 = IsOutside(v1, this);
            var out2 = IsOutside(v2, this);

            if (out0 && out1 && out2) {
                return AlignedBoxContainmentType.Disjoint;
            }
            if (out0 || out1 || out2) {
                return AlignedBoxContainmentType.Intersects;
            }
            return AlignedBoxContainmentType.Contains;
        }

        public bool Intersects(Vector3 v0, Vector3 v1, Vector3 v2, out List<Vector3> trianglePoints) {
            var edge1 = v1 - v0;
            var edge2 = v2 - v1;
            var edge3 = v0 - v2;

            var intersec = new[] { false, false, false };
            trianglePoints = new List<Vector3>();

            intersec[0] = EndgeIntersec(v0, edge1, this, ref trianglePoints);
            intersec[1] = EndgeIntersec(v1, edge2, this, ref trianglePoints);
            intersec[2] = EndgeIntersec(v2, edge3, this, ref trianglePoints);

            return trianglePoints.Any();
        }


        static bool EndgeIntersec(Vector3 v, Vector3 edge, Cylinder3 c, ref List<Vector3> pp) {
            if (Math.Abs(Math.Abs(Vector3.Dot(edge.Normalized(), -c.AxisDirection)) - 1) < 0.001) {
                //edge is colinear to cylinder asis
                return false;
            }
            var res = LineIntersection(v, edge, c);
            if (IsOutside(v, c)) {
                pp.Add(v);
            }
            if (res.point.Any()) {
                pp.Add(res.point[0]);
                return true;
            }
            return false;
        }

        static Result LineIntersection(Vector3 lineOrigin, Vector3 line, Cylinder3 cylinder) {
            var result = new Result();
            var direction = line.Normalized();
            var length = line.Length();

            RayIntersection(lineOrigin, direction, cylinder, result);

            for (int i = 0; i < result.numIntersections; ++i) {
                if (length >= result.parameter[i] && result.parameter[i] > 0) {
                    result.point.Add(lineOrigin + result.parameter[i] * direction);
                }
            }
            return result;
        }

        static void RayIntersection(Vector3 lineOrigin, Vector3 lineDirection, Cylinder3 cylinder, Result result) {

            // Initialize the result as if there is no intersection.  If we discover
            // an intersection, these values will be modified accordingly.
            result.intersect = false;
            result.numIntersections = 0;

            // Create a coordinate system for the cylinder.  In this system, the
            // cylinder segment center C is the origin and the cylinder axis direction
            // W is the z-axis.  U and V are the other coordinate axis directions.
            // If P = x*U+y*V+z*W, the cylinder is x^2 + y^2 = r^2, where r is the
            // cylinder radius.  The end caps are |z| = h/2, where h is the cylinder
            // height.

            Vector3[] basis = new Vector3[3];  // {W, U, V}
            basis[0] = cylinder.AxisDirection;
            ComputeOrthogonalComplement(1, basis, false);
            var halfHeight = 0.5f * cylinder.Height;
            var rSqr = cylinder.Radius * cylinder.Radius;

            // Convert incoming line origin to capsule coordinates.
            Vector3 diff = lineOrigin - cylinder.Origin;
            var P = new Vector3(
                Vector3.Dot(basis[1], diff), Vector3.Dot(basis[2], diff),
                Vector3.Dot(basis[0], diff));

            // Get the z-value, in cylinder coordinates, of the incoming line's
            // unit-length direction.
            var dz = Vector3.Dot(basis[0], lineDirection);
            if (Math.Abs(dz) == 1f) {
                // The line is parallel to the cylinder axis.  Determine whether the
                // line intersects the cylinder end disks.
                var radialSqrDist = rSqr - P[0] * P[0] - P[1] * P[1];
                if (radialSqrDist >= 0f) {
                    // The line intersects the cylinder end disks.
                    result.intersect = true;
                    result.numIntersections = 2;
                    if (dz > 0f) {
                        result.parameter[0] = -P[2] - halfHeight;
                        result.parameter[1] = -P[2] + halfHeight;
                    } else {
                        result.parameter[0] = P[2] - halfHeight;
                        result.parameter[1] = P[2] + halfHeight;
                    }
                }
                // else:  The line is outside the cylinder, no intersection.
                return;
            }

            // Convert the incoming line unit-length direction to cylinder
            // coordinates.
            var D = new Vector3(Vector3.Dot(basis[1], lineDirection), Vector3.Dot(basis[2], lineDirection), dz);
            float a0, a1, a2, discr, root, inv, tValue;

            if (D[2] == 0f) {
                // The line is perpendicular to the cylinder axis.
                if (Math.Abs(P[2]) <= halfHeight) {
                    // Test intersection of line P+t*D with infinite cylinder
                    // x^2+y^2 = r^2.  This reduces to computing the roots of a
                    // quadratic equation.  If P = (px,py,pz) and D = (dx,dy,dz),
                    // then the quadratic equation is
                    //   (dx^2+dy^2)*t^2 + 2*(px*dx+py*dy)*t + (px^2+py^2-r^2) = 0
                    a0 = P[0] * P[0] + P[1] * P[1] - rSqr;
                    a1 = P[0] * D[0] + P[1] * D[1];
                    a2 = D[0] * D[0] + D[1] * D[1];
                    discr = a1 * a1 - a0 * a2;
                    if (discr > 0) {
                        // The line intersects the cylinder in two places.
                        result.intersect = true;
                        result.numIntersections = 2;
                        root = (float)Math.Sqrt(discr);
                        inv = 1f / a2;
                        result.parameter[0] = (-a1 - root) * inv;
                        result.parameter[1] = (-a1 + root) * inv;
                    } else if (discr == 0) {
                        // The line is tangent to the cylinder.
                        result.intersect = true;
                        result.numIntersections = 1;
                        result.parameter[0] = -a1 / a2;
                        // Used by derived classes.
                        result.parameter[1] = result.parameter[0];
                    }
                    // else: The line does not intersect the cylinder.
                }
                // else: The line is outside the planes of the cylinder end disks.
                return;
            }

            // Test for intersections with the planes of the end disks.
            inv = 1f / D[2];

            var t0 = (-halfHeight - P[2]) * inv;
            var xTmp = P[0] + t0 * D[0];
            var yTmp = P[1] + t0 * D[1];
            if (xTmp * xTmp + yTmp * yTmp <= rSqr) {
                // Plane intersection inside the top cylinder end disk.
                result.parameter[result.numIntersections++] = t0;
            }

            var t1 = (+halfHeight - P[2]) * inv;
            xTmp = P[0] + t1 * D[0];
            yTmp = P[1] + t1 * D[1];
            if (xTmp * xTmp + yTmp * yTmp <= rSqr) {
                // Plane intersection inside the bottom cylinder end disk.
                result.parameter[result.numIntersections++] = t1;
            }

            if (result.numIntersections < 2) {
                // Test for intersection with the cylinder wall.
                a0 = P[0] * P[0] + P[1] * P[1] - rSqr;
                a1 = P[0] * D[0] + P[1] * D[1];
                a2 = D[0] * D[0] + D[1] * D[1];
                discr = a1 * a1 - a0 * a2;
                if (discr > 0) {
                    root = (float)Math.Sqrt(discr);
                    inv = 1f / a2;
                    tValue = (-a1 - root) * inv;
                    if (t0 <= t1) {
                        if (t0 <= tValue && tValue <= t1) {
                            result.parameter[result.numIntersections++] = tValue;
                        }
                    } else {
                        if (t1 <= tValue && tValue <= t0) {
                            result.parameter[result.numIntersections++] = tValue;
                        }
                    }

                    if (result.numIntersections < 2) {
                        tValue = (-a1 + root) * inv;
                        if (t0 <= t1) {
                            if (t0 <= tValue && tValue <= t1) {
                                result.parameter[result.numIntersections++] = tValue;
                            }
                        } else {
                            if (t1 <= tValue && tValue <= t0) {
                                result.parameter[result.numIntersections++] = tValue;
                            }
                        }
                    }
                    // else: Line intersects end disk and cylinder wall.
                } else if (discr == 0) {
                    tValue = -a1 / a2;
                    if (t0 <= t1) {
                        if (t0 <= tValue && tValue <= t1) {
                            result.parameter[result.numIntersections++] = tValue;
                        }
                    } else {
                        if (t1 <= tValue && tValue <= t0) {
                            result.parameter[result.numIntersections++] = tValue;
                        }
                    }
                }
                // else: Line does not intersect cylinder wall.
            }
            // else: Line intersects both top and bottom cylinder end disks.
            if (result.numIntersections == 2) {
                result.intersect = true;
                if (result.parameter[0] > result.parameter[1]) {
                    //std::swap(result.parameter[0], result.parameter[1]);
                    var p0 = result.parameter[0];
                    result.parameter[0] = result.parameter[1];
                    result.parameter[1] = p0;
                }
            } else if (result.numIntersections == 1) {
                result.intersect = true;
                // Used by derived classes.
                result.parameter[1] = result.parameter[0];
            }
        }

        static bool IsOutside(Vector3 v, Cylinder3 cylinder) {
            var centerProject = ProjectOnPlane(v, -cylinder.AxisDirection, cylinder.Origin);
            var pv = v - centerProject;
            if (pv.Length() >= cylinder.Radius) {
                return true;
            }
            return false;
        }
        static Vector3 ProjectOnPlane(Vector3 planeOrigin, Vector3 planeAxis, Vector3 pt) {
            var vv = pt - planeOrigin;
            var dist = Vector3.Dot(vv, planeAxis);
            return pt - planeAxis * dist;
        }
        static float ComputeOrthogonalComplement(int numInputs, Vector3[] v, bool robust) {
            if (numInputs == 1) {
                if (Math.Abs(v[0][0]) > Math.Abs(v[0][1])) {
                    v[1] = new Vector3(-v[0][2], 0f, +v[0][0]);
                } else {
                    v[1] = new Vector3(0f, +v[0][2], -v[0][1]);
                }
                numInputs = 2;
            }

            if (numInputs == 2) {
                v[2] = Vector3.Cross(v[0], v[1]);
                return Orthonormalize(3, v, robust);
            }

            return 0f;
        }
        static float Orthonormalize(int numInputs, Vector3[] v, bool robust) {
            if (v.Any() && 1 <= numInputs && numInputs <= 3) {
                var v0 = v[0];
                var minLength = Normalize(ref v0, robust);
                v[0] = v0;
                for (int i = 1; i < numInputs; ++i) {
                    for (int j = 0; j < i; ++j) {
                        float dot = Vector3.Dot(v[i], v[j]);
                        v[i] -= v[j] * dot;
                    }
                    var vi = v[i];
                    var length = Normalize(ref vi, robust);
                    v[i] = vi;
                    if (length < minLength) {
                        minLength = length;
                    }
                }
                return minLength;
            }

            return 0f;
        }
        static float Normalize(ref Vector3 v, bool robust) {
            if (robust) {
                var maxAbsComp = (float)MathF.Abs(v[0]);
                for (int i = 1; i < 3; ++i) {
                    var absComp = (float)MathF.Abs(v[i]);
                    if (absComp > maxAbsComp) {
                        maxAbsComp = absComp;
                    }
                }

                float length;
                if (maxAbsComp > 0f) {
                    v /= maxAbsComp;
                    length = (float)Math.Sqrt(Vector3.Dot(v, v));
                    v /= length;
                    length *= maxAbsComp;
                } else {
                    length = 0f;
                    for (int i = 0; i < 3; ++i) {
                        v[i] = 0f;
                    }
                }
                return length;
            } else {
                var length = (float)Math.Sqrt(Vector3.Dot(v, v));
                if (length > 0f) {
                    v /= length;
                } else {
                    for (int i = 0; i < 3; ++i) {
                        v[i] = 0f;
                    }
                }
                return length;
            }
        }

    }
}
